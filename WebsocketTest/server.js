// Setup
const WebSocket = require("ws");
const wss = new WebSocket.Server({ port: 5085 });
console.log("Server running on ws://localhost:5085");

const rooms = new Map();

// Player states
const PLAYER_STATE = {
  WAITING: "waiting",
  VOTING: "voting",
  MAKING_CHOICE: "making_choice"
};

// Core logic
function registerHost(clientSocket, roomCode) 
{
  const room = getRoom(roomCode);
  room.host = clientSocket;

  send(clientSocket, "host_registered", {
    room: roomCode
  });

  console.log(`[room:${roomCode}] host registered`);
}

function joinRoom(clientSocket, roomCode, playerName, clientId) 
{
  const room = rooms.get(roomCode);

  if (!room || !room.host) 
  {
    send(clientSocket, "join_room_failed", {  
      reason: "Room not found"
    });
    return;
  }

  // Duplicate name check
  for (const p of room.players) 
  {
    if (p.playerName === playerName.toLowerCase()) 
    {
      send(clientSocket, "join_room_failed", {
        reason: "Name already taken"
      });
      return;
    }
  }

  // Adding players
  const player = 
  {
    socket: clientSocket,
    playerName: playerName.toLowerCase(),
    clientId: clientId,
    playerState: PLAYER_STATE.WAITING,
    connected: true
  };
  room.players.add(player);
  console.log(`Player joined: ${playerName} (total: ${room.players.size})`);

  send(clientSocket, "join_room_success", {
    room: roomCode,
    playerName,
    clientId,
    playerState: player.playerState
  });

  // Notify Unity host
  if (room.host) 
  {
    send(room.host, "player_joined", {
      playerName,
      playerID: clientId,
      playerState: player.playerState
    });
  }

  console.log(`[Room: ${roomCode}] Player joined: ${playerName}`);
}


//Connection Handling
wss.on("connection", (clientSocket) => {
  console.log("New client connected");

  clientSocket.on("message", (raw) => {
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

      return registerHost(clientSocket, roomCode);
    }

    // Client Connection
    if (msg.type === "join_room_request") 
    {
      const roomCode = normalize(msg.room);
      const playerName = normalizeName(msg.playerName);
      const clientId = msg.clientId;

      if (!roomCode || !playerName) 
      {
        send(clientSocket, "join_room_failed", {
          reason: "RoomCode and playerName required"
        });
        return;
      }

      return joinRoom(clientSocket, roomCode, playerName, clientId);
    }
  });

  // Closing Connection
  clientSocket.on("close", () => {
    for (const room of rooms.values()) {
      if (room.host === clientSocket) {
        room.host = null;
        console.log(`Host disconnected`);
      }

      for (const player of room.players) 
      {
        if (player.socket === clientSocket) 
        {
          player.connected = false;
          console.log(`Player temporarily disconnected: ${player.playerName}`);
        }
      }
    }
  });
});

//Helpers
function send(clientSocket, type, dataObj = {}) 
{
  if (clientSocket.readyState !== WebSocket.OPEN) return;

  clientSocket.send(JSON.stringify({
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