// Setup
const WebSocket = require("ws");
const wss = new WebSocket.Server({ port: 5085 });

const rooms = new Map();

// Player states
const PLAYER_STATE = {
  WAITING: "waiting",
  VOTING: "voting",
  MAKING_CHOICE: "making_choice",
  VIEWING_CHARACTER: "viewing_character",
  VIEWING_SCENARIO: "viewing_scenario",
  DISCUSSING: "discussing"
};

const DISCONNECT_TIMEOUT_MS = 10000;

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
    connected: true,
    disconnectTimer: null,
    character: null,
  };
  room.players.add(player);
  console.log(`Player joined: ${playerName} (total: ${room.players.size})`);

  send(clientSocket, "join_room_success", {
    room: roomCode,
    playerName,
    clientId,
    playerState: player.playerState,
  });

  // Notify Unity host
  if (room.host) 
  {
    send(room.host, "player_joined", {
      playerName,
      playerID: clientId,
    });
  }
  console.log(`[Room: ${roomCode}] Player joined: ${playerName}`);
}


//Connection Handling
wss.on("connection", (clientSocket) => {
  console.log("New client connected");

  clientSocket.on("message", (raw) => {

    console.log("RAW message received:", raw.toString());
    
    let msg;

    try 
    { msg = JSON.parse(raw); } 
    catch 
    {
      console.log("Couldn't parse message")
      return;
    }

    // Host connection - WORKS
    if (msg.type === "host_register") 
    {
      const roomCode = normalize(msg.room || msg.roomCode);
      const payload = msg.data;

      console.log("Host registered with id:", payload.hostClientId);

      if (!roomCode) return;

      return registerHost(clientSocket, roomCode);
    }

    // Client Connection - WORKS
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


    // Reconnection Attempt - REVIEW THIS
    if (msg.type === "reconnect_request")
    {
      const roomCode = normalize(msg.room);
      const clientId = msg.clientId;
      if (!roomCode || !clientId)
      {
        send(clientSocket, "reconnect_failed", {
          reason: "RoomCode and clientId required"
        });
        return;
      } 

      return reconnectPlayer(clientSocket, roomCode, clientId);
    }
    //ACCOUNTED FOR
    if (msg.type === "start_game_request")
    {
      const roomCode = normalize(msg.room);
      const payload = msg.data;
      if (!roomCode)
      {
        send(clientSocket, "start_game_failed", {
          reason: "RoomCode required"
        });
        return;
      }
      console.log("Game started by host:", payload.hostClientId);

      return startGame(clientSocket, roomCode);
    }
    //ACCOUNTED FOR
    if (msg.type === "show_scenario_request")
      {
        const roomCode = normalize(msg.room);
        const payload = msg.data;

        if (!roomCode)
        {
          send(clientSocket, "show_scenario_failed", {
            reason: "RoomCode required"
          });
          return;
        }
        console.log("Showing scenario from host:", payload.hostClientId);
        return showScenario(clientSocket, roomCode);
      }
    //REVISE
    if (msg.type === "start_voting_request") {
        const roomCode = normalize(msg.room || msg.roomCode);
        const payload = msg.data;

        if (!roomCode) {
          send(clientSocket, "start_voting_failed", {
            reason: "Room code is missing"
          });
          return;
        }
        console.log("Attempting to start voting round on host ", payload.hostClientId);

        startVoting(clientSocket, roomCode, payload.votingRound);
        return;
      }
      //REVIEW THIS
      if (msg.type === "submit_vote") {
        const roomCode = normalize(msg.room || msg.roomCode);
        const clientId = msg.clientId;
        const voteValue = msg.voteValue;
        const voteType = msg.voteType;
        const roundNumber = Number(msg.roundNumber || 0);

        if (!roomCode || !clientId) {
          send(clientSocket, "vote_failed", {
            reason: "Room code and clientId are required"
          });
          return;
        }

        submitVote(clientSocket, roomCode, clientId, voteValue, voteType);
        return;
      }
      //ACCOUNTED FOR
      if(msg.type === "character_info")
      {
        console.log("Received character_info on server.");

        const roomCode = normalize(msg.room || msg.roomCode);

         const payload = parsePayload(msg.data);
        assignCharacterInfo(
          roomCode,
          payload.playerID,
          payload.characterName,
          payload.characterDescription,
          payload.keyword1,
          payload.keyword2
        );
        return;
      }
      if (msg.type === "start_discussion_request")
      {
        const roomCode = normalize(msg.room || msg.roomCode);
       
        if (!roomCode)        {
          send(clientSocket, "start_discussion_failed", {
            reason: "Room code is missing"  
          });
          return;
        }
        startDiscussion(clientSocket, roomCode);
        return;
      }

      if (msg.type === "show_enjin_update_screen")
      {
        const roomCode = normalize(msg.room || msg.roomCode);
     

        if (!roomCode)
        {
          send(clientSocket, "show_enjin_update_failed", {
            reason: "Room code is missing"
          });
          return;
        }
        showEnjinUpdateScreen(clientSocket, roomCode);
        return;
      }

      if (msg.type === "show_outcome_screen")
      {
        const roomCode = normalize(msg.room || msg.roomCode);
        
        if (!roomCode)        {
          send(clientSocket, "show_outcome_screen_failed", {
            reason: "Room code is missing"  
          });
          return;
        }
        showOutcomeScreen(clientSocket, roomCode);
        return;
      }

      if (msg.type === "show_waiting_situation_screen")
      {
        const roomCode = normalize(msg.room || msg.roomCode);

        if (!roomCode)
        {
          send(clientSocket, "show_waiting_situation_screen_failed", {
            reason: "Room code is missing"
          });
          return;
        }

        showWaitingSituationScreen(clientSocket, roomCode);
        return;
      }


      if (msg.type === "skip_discussion")
      {
        const roomCode = normalize(msg.room || msg.roomCode);
        const clientId = msg.clientId;

        if (!roomCode)
        {
          send(clientSocket, "skip_discussion_failed", {
            reason: "Room code is missing"
          });
          return;
        }

        skipDiscussionTurn(clientSocket, roomCode, clientId);
        return;
      }

      if (msg.type === "send_current_speaker_id") {
        const roomCode = normalize(msg.room || msg.roomCode);
        const payload = parsePayload(msg.data);

        if (!roomCode || !payload.playerID) {
          send(clientSocket, "send_current_speaker_id_failed", {
            reason: "Room code and playerID are required"
          });
          return;
        }

        sendCurrentSpeakerID(clientSocket, roomCode, payload.playerID);
        return;
      }

      if (msg.type === "player_screen_command")
      {
        const roomCode = normalize(msg.room || msg.roomCode);
        const payload = parsePayload(msg.data);

        if (!roomCode)
        {
          send(clientSocket, "player_screen_command_failed", {
            reason: "Room code is missing"
          });
          return;
        }

        sendPlayerScreenCommand(clientSocket, roomCode, payload);
        return;
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

        if (room.host) 
        {
          send(room.host, "player_disconnected", {
            playerName: player.playerName,
            playerID: player.clientId
          });
        }

        player.disconnectTimer = setTimeout(() => {
          if (!player.connected) 
          {
            room.players.delete(player);

            console.log(`Player removed after timeout: ${player.playerName}`);

            if (room.host) 
            {
              send(room.host, "player_removed", {
                playerName: player.playerName,
                playerID: player.clientId
              });
            }
          }
        }, DISCONNECT_TIMEOUT_MS);
      }
    }}
  });
});
function assignCharacterInfo(roomCode, playerID, characterName, characterDescription, keyword1, keyword2)
{
    const room = rooms.get(roomCode);

    if (!room)
        return;

    const player =
        [...room.players].find(
            p => p.clientId === playerID
        );

        if (!player)
    {
        console.log(
            `Player ${playerID} not found`
        );
        return;
    }

    player.character =
    {
        characterName,
        characterDescription,
        keyword1,
        keyword2
    };

    console.log(
    "Player socket state:",
    player.socket.readyState
);

    send(player.socket, "character_info",
    {
        characterName,
        characterDescription,
        keyword1,
        keyword2
    });
     console.log(
        `Assigned ${characterName} to ${player.playerName}`
    );

}
function sendPlayerScreenCommand(hostSocket, roomCode, payload)
{
  const room = rooms.get(roomCode);

  if (!room)
  {
    send(hostSocket, "player_screen_command_failed", {
      reason: "Room not found"
    });
    return;
  }

  if (room.host !== hostSocket)
  {
    send(hostSocket, "player_screen_command_failed", {
      reason: "Only host can control player screens"
    });
    return;
  }

  if (!payload || typeof payload.screenId !== "string" || payload.screenId.trim() === "")
  {
    send(hostSocket, "player_screen_command_failed", {
      reason: "screenId is required"
    });
    return;
  }
  const connectedPlayers = [...room.players].filter(player => player.connected);

  if (connectedPlayers.length === 0)
  {
    send(hostSocket, "player_screen_command_failed", {
      reason: "No connected players in the room"
    });
    return;
  }

  room.lastPlayerScreen = {
    screenId: payload.screenId,
    playerState: payload.playerState || PLAYER_STATE.WAITING,
    roundNumber: payload.roundNumber || 0,
    totalRounds: payload.totalRounds || 0,
    voteType: payload.voteType || "",
    votingDuration: payload.votingDuration || 0,
    currentSpeakerPlayerID: payload.currentSpeakerPlayerID || "",
    currentSpeakerName: payload.currentSpeakerName || ""
  };
  for (const player of connectedPlayers)
  {
    player.playerState = room.lastPlayerScreen.playerState;

    send(player.socket, "player_screen_changed", {
      ...room.lastPlayerScreen,
      room: roomCode,
      playerName: player.playerName,
      clientId: player.clientId,
      isCurrentSpeaker: player.clientId === room.lastPlayerScreen.currentSpeakerPlayerID,
      character: player.character
    });
  }

  send(hostSocket, "player_screen_command_success", {
    ...room.lastPlayerScreen,
    room: roomCode,
    playerCount: connectedPlayers.length
  });

  console.log(
    `[Room: ${roomCode}] Sent player screen ${payload.screenId} to ${connectedPlayers.length} players`
  );
}

//Helpers
function send(clientSocket, type, dataObj = {}) 
{
  if (clientSocket.readyState !== WebSocket.OPEN) return;

  console.log(
        "SENDING TO CLIENT:", JSON.stringify(dataObj)
    );

  clientSocket.send(JSON.stringify({
    type,
    data: JSON.stringify(dataObj) 
  }));
}
function parsePayload(payload)
{
  if (!payload) return {};

  if (typeof payload === "string")
  {
    try
    {
      return JSON.parse(payload);
    }
    catch
    {
      return {};
    }
  }

  if (typeof payload === "object") return payload;

  return {};
}

function getRoom(roomCode) 
{
  if (!rooms.has(roomCode)) {
    rooms.set(roomCode, {
      host: null,
      players: new Set(),
        currentRound: 0,
        roundVotes: {},
        lastPlayerScreen: null
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

// Reconnection logic
function reconnectPlayer(clientSocket, roomCode, clientId) 
{
  const room = rooms.get(roomCode);

  if (!room) 
  {
    send(clientSocket, "reconnect_failed", {
      reason: "Room not found"
    });
    return;
  }

  for (const player of room.players) 
  {
    if (player.clientId === clientId) 
    {
      player.socket = clientSocket;
      player.connected = true;

      if (player.disconnectTimer) 
      {
        clearTimeout(player.disconnectTimer);
        player.disconnectTimer = null;
      }

      send(clientSocket, "reconnect_success", {
        room: roomCode,
        playerName: player.playerName,
        clientId: player.clientId,
        playerState: player.playerState,
        character: player.character,
        lastPlayerScreen: room.lastPlayerScreen
      });

      if (room.host) 
      {
        send(room.host, "player_reconnected", {
          playerName: player.playerName,
          playerID: player.clientId,
          playerState: player.playerState
        
        });
      }
      return;
    }
  }

  send(clientSocket, "reconnect_failed", {
    reason: "Player not found"
  });
}

// Game start logic
function startGame(clientSocket, roomCode)
{  const room = rooms.get(roomCode);

  if (!room) 
  {
    send(clientSocket, "start_game_failed", {
      reason: "Room not found"
    });
    return;
  } 
  if (room.host !== clientSocket)
  {
    send(clientSocket, "start_game_failed", {
      reason: "Only host can start the game"
    });
    return;
  }
   const connectedPlayers = [...room.players].filter(player => player.connected);

  if (connectedPlayers.length === 0)
  {
    send(clientSocket, "start_game_failed", {
      reason: "No connected players in the room"
    });
    return;
  }
  // Notify all players that the game is starting
  for (const player of connectedPlayers)
  {
    player.playerState = PLAYER_STATE.VIEWING_CHARACTER;
    if (player.connected)
    {
      console.log("sent game_started request");
      send(player.socket, "game_started");
    }
  }
  send(room.host, "start_game_success", {
  room: roomCode
});

  console.log(`[Room: ${roomCode}] Game started`);
};

function showScenario(clientSocket, roomCode)
{
  const room = rooms.get(roomCode);

  const connectedPlayers = [...room.players].filter(player => player.connected);

  if (connectedPlayers.length === 0)
  {
    send(clientSocket, "show_scenario_failed", {
      reason: "No connected players in the room"
    });
    return;
  }

  for (const player of connectedPlayers)
  {
    player.playerState = PLAYER_STATE.VIEWING_SCENARIO;

    send(player.socket, "show_scenario");
  }

  send(room.host, "show_scenario_success", {
    room: roomCode
  });

  console.log(`[Room: ${roomCode}] Scenario shown`);
}



function startVoting(hostSocket, roomCode, voteType) {
  const room = rooms.get(roomCode);

  const connectedPlayers = [...room.players].filter(player => player.connected);

  if (connectedPlayers.length === 0) {
    send(hostSocket, "start_voting_failed", {
      reason: "No connected players in the room"
    });
    return;
  }

  for (const player of connectedPlayers) {
    player.playerState = PLAYER_STATE.VOTING;

    if (player.socket.readyState === WebSocket.OPEN) {
      send(player.socket, "voting_started", {
        voteType: voteType
      });
    }
  }

  send(hostSocket, "start_voting_success", {
    room: roomCode,
  });

  console.log(`[Room: ${roomCode}] Voting started`);
}


function submitVote(clientSocket, roomCode, clientId, voteValue, voteType) {
  const room = rooms.get(roomCode);
  const player = [...room.players].find(player => player.clientId === clientId);
  const roundNumber = room.currentRound;

  if (typeof voteValue !== "string" || voteValue.trim() === "") {
    send(clientSocket, "vote_failed", {
      reason: "Vote value is invalid"
    });
    return;
  }

  if (voteType !== "first_vote" && voteType !== "second_vote") {
    send(clientSocket, "vote_failed", {
      reason: "Vote type is invalid"
    });
    return;
  }

  player.playerState = PLAYER_STATE.WAITING;

  send(clientSocket, "vote_saved", {
    room: roomCode,
    currentRound: roundNumber,
    roundNumber: roundNumber,
    voteType: voteType,
    voteValue: voteValue,
    playerState: player.playerState
  });

  const messageType = voteType === "first_vote"
    ? "player_vote_1"
    : "player_vote_2";

  send(room.host, messageType, {
    playerID: clientId,
    playerVote: voteValue,

  });

  console.log(
    `[Room: ${roomCode}] ${player.playerName} submitted ${voteType}: ${voteValue} in round ${roundNumber}`
  );
}

function startDiscussion(clientSocket, roomCode)
{  const room = rooms.get(roomCode);

  const connectedPlayers = [...room.players].filter(player => player.connected);
  if (connectedPlayers.length === 0)
  {
    send(clientSocket, "start_discussion_failed", {
      reason: "No connected players in the room"
    });
    return;
  } 
  for (const player of connectedPlayers)
  {player.playerState = PLAYER_STATE.DISCUSSING;
    if (player.connected)
    {
      console.log("sent start_discussion request");
      send(player.socket, "discussion_started");
    }
  }
  send(room.host, "start_discussion_success", {
  room: roomCode
});
  console.log(`[Room: ${roomCode}] Discussion started`);
} 

function showEnjinUpdateScreen(clientSocket, roomCode)
{
  const room = rooms.get(roomCode);

  if (!room)
  {
    send(clientSocket, "show_enjin_update_screen_failed", {
      reason: "Room not found"
    });
    return;
  }

  const connectedPlayers = [...room.players].filter(player => player.connected);

  if (connectedPlayers.length === 0)
  {
    send(clientSocket, "show_enjin_update_screen_failed", {
      reason: "No connected players in the room"
    });
    return;
  }

  for (const player of connectedPlayers)
  {
    console.log("sent show_enjin_update_screen request");
    send(player.socket, "show_enjin_update_screen");
  }

  send(room.host, "show_enjin_update_screen_success", {
    room: roomCode
  });
}

function showOutcomeScreen(clientSocket, roomCode)
{  
  const room = rooms.get(roomCode);

  if (!room)    
  {
    send(clientSocket, "show_outcome_screen_failed", {
      reason: "Room not found"
    });
    return;
  }
  const connectedPlayers = [...room.players].filter(player => player.connected);
  if (connectedPlayers.length === 0)
  {
    send(clientSocket, "show_outcome_screen_failed", {
      reason: "No connected players in the room"
    });
    return;
  }
  for (const player of connectedPlayers)
  {    console.log("sent show_outcome_screen request");
    send(player.socket, "show_outcome_screen");
  }
  send(room.host, "show_outcome_screen_success", {
  room: roomCode});
} 

function showWaitingSituationScreen(clientSocket, roomCode)
{
  const room = rooms.get(roomCode);

  if (!room)
  {
    send(clientSocket, "show_waiting_situation_screen_failed", {
      reason: "Room not found"
    });
    return;
  }

  const connectedPlayers = [...room.players].filter(player => player.connected);

  if (connectedPlayers.length === 0)
  {
    send(clientSocket, "show_waiting_situation_screen_failed", {
      reason: "No connected players in the room"
    });
    return;
  }

  for (const player of connectedPlayers)
  {
    console.log("sent show_waiting_situation_screen request");
    send(player.socket, "show_waiting_situation_screen");
  }

  send(room.host, "show_waiting_situation_screen_success", {
    room: roomCode
  });
}

function skipDiscussionTurn(clientSocket, roomCode, clientId) {
  const room = rooms.get(roomCode);

  if (!room || !room.host) {
    send(clientSocket, "skip_discussion_failed", {
      reason: "Room not found"
    });
    return;
  }

  send(room.host, "player_skip", {
    playerID: clientId
  });

  send(clientSocket, "skip_discussion_success", {
    playerID: clientId
  });
}

function sendCurrentSpeakerID(hostSocket, roomCode, currentSpeakerPlayerID) {
  const room = rooms.get(roomCode);

  if (!room || room.host !== hostSocket) {
    send(hostSocket, "send_current_speaker_id_failed", {
      reason: "Room not found or sender is not host"
    });
    return;
  }

  const connectedPlayers = [...room.players].filter(player => player.connected);
  const speaker = connectedPlayers.find(player => player.clientId === currentSpeakerPlayerID);

  for (const player of connectedPlayers) {
    send(player.socket, "current_speaker_changed", {
      currentSpeakerPlayerID: currentSpeakerPlayerID,
      currentSpeakerName: speaker ? speaker.playerName : "",
      isCurrentSpeaker: player.clientId === currentSpeakerPlayerID
    });
  }
}