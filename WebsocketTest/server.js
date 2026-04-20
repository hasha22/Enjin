// Setup
const WebSocket = require("ws");
const wss = new WebSocket.Server({ port: 5040 });
console.log("Server running on ws://localhost:5040");

const rooms = new Map();

// Core logic
function registerHost(ws, roomCode) 
{
  const room = getRoom(roomCode);
  room.host = ws;

  send(ws, "host_registered", {
    room: roomCode
  });

  console.log(`[room:${roomCode}] host registered`);
}

function joinRoom(ws, roomCode, playerName, clientId) 
{
  const room = getRoom(roomCode);

  // Duplicate name check
  for (const p of room.players) 
  {
    if (p.playerName === playerName.toLowerCase()) 
    {
      send(ws, "join_room_failed", {
        reason: "Name already taken"
      });
      return;
    }
  }

  // Adding players
  const player = 
  {
    ws,
    playerName: playerName.toLowerCase(),
    clientId: clientId
  };
  room.players.add(player);

  send(ws, "join_room_success", {
    room: roomCode,
    playerName,
    clientId
  });

  // Notify Unity host
  if (room.host) 
  {
    send(room.host, "player_joined", {
      playerName,
      playerID: clientId 
    });
  }

  console.log(`[Room: ${roomCode}] Player joined: ${playerName}`);
}


//Connection Handling
wss.on("connection", (ws) => {
  console.log("Client connected");

  ws.on("message", (raw) => {
    let msg;

    try 
    { msg = JSON.parse(raw); } 
    catch 
    {
      console.log("Couldn't parse message")
      return;
    }

    // Host connection
    if (msg.type === "host_register") 
    {
      const roomCode = normalize(msg.room || msg.roomCode);

      if (!roomCode) return;

      return registerHost(ws, roomCode);
    }

    // Client Connection
    if (msg.type === "join_room_request") 
    {
      const roomCode = normalize(msg.room);
      const playerName = normalizeName(msg.playerName);
      const clientId = msg.clientId;

      if (!roomCode || !playerName) 
      {
        send(ws, "join_room_failed", {
          reason: "RoomCode and playerName required"
        });
        return;
      }

      return joinRoom(ws, roomCode, playerName, clientId);
    }
  });

  // Closing Connection
  ws.on("close", () => {
    for (const room of rooms.values()) {
      if (room.host === ws) {
        room.host = null;
      }

      for (const player of room.players) 
      {
        if (player.ws === ws) 
        {
          room.players.delete(player);
        }
      }
    }
  });
});

//Helpers
function send(ws, type, dataObj = {}) 
{
  if (ws.readyState !== WebSocket.OPEN) return;

  ws.send(JSON.stringify({
    type,
    data: JSON.stringify(dataObj) 
  }));
}
function getRoom(roomCode) 
{
  if (!rooms.has(roomCode)) {
    rooms.set(roomCode, {
      host: null,
      players: new Set()
    });
  }
  return rooms.get(roomCode);
}
function normalize(str) 
{
  return typeof str === "string" ? str.trim().toUpperCase() : null;
}

function normalizeName(str) 
{
  return typeof str === "string" ? str.trim().slice(0, 24) : null;
}