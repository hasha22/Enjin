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
  VIEWING_SCENARIO: "viewing_scenario"
};

const CHARACTERS = [
  {
    id: "char1",
    name: "Character 1",
    faceImage: "CharFace1.png",
    fullImage: "Char1.png",
    backgroundColor: "#FF6B6B",
    box1Text: "Productivity",
    box2Text: "AI Usage",
    modalDescription: "You think Enjin shouldn’t be discussed, it should be used. Moreover, you think that if Enjin isn’t used, the sector will fall behind. You care more about what you create, than that you created it. Think about how you're going to push for Enjin implementation within the company.  How can Enjin boost productivity? How can it make people's lives more efficient? Get in character! ",
    active: true
  },
  {
    id: "char2",
    name: "Character 2",
    faceImage: "CharFace2.png",
    fullImage: "Char2.png",
    backgroundColor: "#4D96FF",
    box1Text: "Box 1 text for character 2",
    box2Text: "Box 2 text for character 2",
    modalDescription: "Description text for character 2.",
    active: true
  },

  // Future characters
  {
    id: "char3",
    name: "Character 3",
    faceImage: null,
    fullImage: null,
    backgroundColor: "#CCCCCC",
    active: false
  },
  {
    id: "char4",
    name: "Character 4",
    faceImage: null,
    fullImage: null,
    backgroundColor: "#CCCCCC",
    active: false
  },
  {
    id: "char5",
    name: "Character 5",
    faceImage: null,
    fullImage: null,
    backgroundColor: "#CCCCCC",
    active: false
  },
  {
    id: "char6",
    name: "Character 6",
    faceImage: null,
    fullImage: null,
    backgroundColor: "#CCCCCC",
    active: false
  }
];

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

    // Host connection
    if (msg.type === "host_register") 
    {
      const roomCode = normalize(msg.room || msg.roomCode);
      const payload = msg.data;

      console.log("Host registered with id:", payload.hostClientId);

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


      // Reconnection Attempt
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

    if (msg.type === "start_voting_request") {
        const roomCode = normalize(msg.room || msg.roomCode);
        const payload = msg.data;

        if (!roomCode) {
          send(clientSocket, "start_voting_failed", {
            reason: "Room code is missing"
          });
          return;
        }
        console.log("Succesfully starting the voting round on host ", payload.hostClientId);

        startVoting(clientSocket, roomCode, roundNumber, voteType);
        return;
      }

      if (msg.type === "submit_vote") {
        const roomCode = normalize(msg.room || msg.roomCode);
        const clientId = msg.clientId;
        const submitReason = msg.submitReason || "manual";
        const voteValue = msg.voteValue;
        const voteType = msg.voteType;

        if (!roomCode || !clientId) {
          send(clientSocket, "vote_failed", {
            reason: "Room code and clientId are required"
          });
          return;
        }

        submitVote(clientSocket, roomCode, clientId, voteValue, submitReason, voteType);
        return;
      }

      if(msg.type === "character_info")
      {
        console.log("Received character_info on server.");

        const roomCode = normalize(msg.room || msg.roomCode);
        const payload = msg.data;
        assignCharacterInfo(roomCode, payload.playerID, payload.characterName, payload.characterDescription, payload.keyword1, payload.keyword2);
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
      players: new Set(),
        currentRound: 0,
        roundVotes: {}
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
        character: player.character
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
      send(player.socket, "game_started", {
        nextPage: "CharacterScreen.html",
        playerState: PLAYER_STATE.VIEWING_CHARACTER,
        room: roomCode,
        playerName: player.playerName,
        clientId: player.clientId,
        character: player.character,
        message: "The game has started!"

        
      });
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

  if (!room)
  {
    send(clientSocket, "show_scenario_failed", {
      reason: "Room not found"
    });
    return;
  }

  if (room.host !== clientSocket)
  {
    send(clientSocket, "show_scenario_failed", {
      reason: "Only host can show scenario"
    });
    return;
  }

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

    send(player.socket, "show_scenario", {
      nextPage: "SituationScreen.html",
      playerState: player.playerState,
      room: roomCode,
      playerName: player.playerName,
      clientId: player.clientId,
      message: "Look at the main screen"
    });
  }

  send(room.host, "show_scenario_success", {
    room: roomCode
  });

  console.log(`[Room: ${roomCode}] Scenario shown`);
}



function startVoting(hostSocket, roomCode, requestedRoundNumber, voteType) {
  const room = rooms.get(roomCode);

  if (!room) {
    send(hostSocket, "start_voting_failed", {
      reason: "Room not found"
    });
    return;
  }

  if (room.host !== hostSocket) {
    send(hostSocket, "start_voting_failed", {
      reason: "Only host can start voting"
    });
    return;
  }

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
        room: roomCode,
        playerName: player.playerName,
        clientId: player.clientId,
        playerState: player.playerState,
        character: player.character,
        voteType: voteType
      });
    }
  }

  send(hostSocket, "start_voting_success", {
    room: roomCode,
  });

  console.log(`[Room: ${roomCode}] Voting started`);
}


function submitVote(clientSocket, roomCode, clientId, voteValue, submitReason, voteType) {
  const room = rooms.get(roomCode);

  if (!room) {
    send(clientSocket, "vote_failed", {
      reason: "Room not found"
    });
    return;
  }

  const player = [...room.players].find(player => player.clientId === clientId);

  if (!player) {
    send(clientSocket, "vote_failed", {
      reason: "Player not found"
    });
    return;
  }

  if (player.socket !== clientSocket) {
    send(clientSocket, "vote_failed", {
      reason: "This socket does not belong to this player"
    });
    return;
  }

  const roundNumber = room.currentRound;

  if (!roundNumber || roundNumber <= 0) {
    send(clientSocket, "vote_failed", {
      reason: "Voting has not started yet"
    });
    return;
  }

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

  if (!room.host || room.host.readyState !== WebSocket.OPEN) {
    send(clientSocket, "vote_failed", {
      reason: "Unity host is not connected"
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
    playerName: player.playerName,
    playerID: clientId,
    playerVote: voteValue
  });

  console.log(
    `[Room: ${roomCode}] ${player.playerName} submitted ${voteType}: ${voteValue} in round ${roundNumber}`
  );
}