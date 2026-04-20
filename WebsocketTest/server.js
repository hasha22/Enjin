const WebSocket = require("ws");

const wss = new WebSocket.Server({ port: 5040 });

/**
 * Room model:
 * {
 *   roomCode: "ABCD",
 *   host: WebSocket | null,
 *   players: Set<WebSocket>,
 *   gameState: "lobby" | "voting" | "results" | string,
 *   phase: number,
 *   timer: number,
 *   disconnectedPlayers: Map<string, { clientId: string|null, playerName: string, disconnectedAt: number }>
 * }
 */
const rooms = new Map();
const socketMeta = new Map(); // ws -> { role, roomCode, clientId, playerName }

const DEFAULT_STATE = "lobby";
const DEFAULT_PHASE = 0;
const DEFAULT_TIMER = 90;

function normalizeRoomCode(code) {
  if (typeof code !== "string") return null;
  const normalized = code.trim().toUpperCase();
  return normalized || null;
}

function normalizePlayerName(name) {
  if (typeof name !== "string") return null;
  const normalized = name.trim();
  if (!normalized) return null;
  return normalized.slice(0, 24);
}

function normalizeState(state) {
  if (typeof state !== "string") return null;
  const normalized = state.trim();
  return normalized || null;
}

function toReconnectKey(clientId, playerName) {
  if (typeof clientId === "string" && clientId.trim()) return `id:${clientId.trim()}`;
  return `name:${(playerName || "").toLowerCase()}`;
}

function send(ws, payload) {
  if (ws && ws.readyState === WebSocket.OPEN) {
    ws.send(JSON.stringify(payload));
  }
}

function sendError(ws, message) {
  send(ws, { type: "error", message });
}

function getOrCreateRoom(roomCode) {
  let room = rooms.get(roomCode);
  if (!room) {
    room = {
      roomCode,
      host: null,
      players: new Set(),
      gameState: DEFAULT_STATE,
      phase: DEFAULT_PHASE,
      timer: DEFAULT_TIMER,
      disconnectedPlayers: new Map()
    };
    rooms.set(roomCode, room);
    console.log(`[room:${roomCode}] created`);
  }
  return room;
}

function roomHasPlayerName(room, playerName, ignoreSocket = null) {
  const wanted = playerName.toLowerCase();
  for (const playerSocket of room.players) {
    if (ignoreSocket && playerSocket === ignoreSocket) continue;
    const existingName = socketMeta.get(playerSocket)?.playerName;
    if (typeof existingName === "string" && existingName.toLowerCase() === wanted) {
      return true;
    }
  }
  return false;
}

function broadcastToPlayers(room, payload) {
  room.players.forEach((playerSocket) => send(playerSocket, payload));
}

function buildRoomInfo(room) {
  const players = Array.from(room.players).map((playerSocket) => {
    const meta = socketMeta.get(playerSocket) || {};
    return {
      clientId: meta.clientId || null,
      playerName: meta.playerName || null
    };
  });

  return {
    type: "room_info",
    room: room.roomCode,
    roomCode: room.roomCode, // compatibility alias
    hasHost: !!room.host,
    playerCount: room.players.size,
    players,
    playerNames: players.map((x) => x.playerName).filter(Boolean),
    gameState: room.gameState,
    phase: room.phase,
    timer: room.timer
  };
}

function publishRoomInfo(room) {
  const payload = buildRoomInfo(room);
  if (room.host) send(room.host, payload);
  broadcastToPlayers(room, payload);
}

function upsertSocketMeta(ws, meta) {
  socketMeta.set(ws, meta);
}

function removeSocketFromRoom(ws) {
  const meta = socketMeta.get(ws);
  if (!meta || !meta.roomCode) return;

  const room = rooms.get(meta.roomCode);
  socketMeta.delete(ws);
  if (!room) return;

  if (meta.role === "host" && room.host === ws) {
    room.host = null;
    broadcastToPlayers(room, {
      type: "host_disconnected",
      room: room.roomCode,
      roomCode: room.roomCode
    });
    console.log(`[room:${room.roomCode}] host disconnected`);
  }

  if (meta.role === "player") {
    room.players.delete(ws);
    const reconnectKey = toReconnectKey(meta.clientId, meta.playerName);
    room.disconnectedPlayers.set(reconnectKey, {
      clientId: meta.clientId || null,
      playerName: meta.playerName || null,
      disconnectedAt: Date.now()
    });

    if (room.host) {
      send(room.host, {
        type: "player_left",
        room: room.roomCode,
        roomCode: room.roomCode,
        clientId: meta.clientId || null,
        playerName: meta.playerName || null,
        playerCount: room.players.size
      });
    }
    console.log(`[room:${room.roomCode}] player disconnected (${room.players.size} active)`);
  }

  if (!room.host && room.players.size === 0) {
    rooms.delete(room.roomCode);
    console.log(`[room:${room.roomCode}] removed (empty)`);
    return;
  }

  publishRoomInfo(room);
}

function registerHost(ws, roomCode, clientId) {
  removeSocketFromRoom(ws);
  const room = getOrCreateRoom(roomCode);

  if (room.host && room.host !== ws) {
    send(room.host, { type: "host_replaced", room: roomCode, roomCode });
    try {
      room.host.close();
    } catch (_) {
      // best effort
    }
  }

  room.host = ws;
  upsertSocketMeta(ws, { role: "host", roomCode, clientId: clientId || null, playerName: null });

  send(ws, {
    type: "host_registered",
    room: roomCode,
    roomCode,
    gameState: room.gameState,
    phase: room.phase,
    timer: room.timer,
    playerCount: room.players.size
  });

  publishRoomInfo(room);
  console.log(`[room:${roomCode}] host registered`);
}

function joinRoom(ws, roomCode, clientId, playerName) {
  const room = getOrCreateRoom(roomCode);
  const reconnectKey = toReconnectKey(clientId, playerName);
  const isReconnect = room.disconnectedPlayers.has(reconnectKey);

  if (roomHasPlayerName(room, playerName, ws)) {
    send(ws, {
      type: "join_room_failed",
      room: roomCode,
      roomCode,
      reason: "playerName already exists in this room"
    });
    return;
  }

  removeSocketFromRoom(ws);
  room.players.add(ws);
  room.disconnectedPlayers.delete(reconnectKey);
  upsertSocketMeta(ws, { role: "player", roomCode, clientId: clientId || null, playerName });

  const successPayload = {
    type: "join_room_success",
    room: roomCode,
    roomCode, // compatibility alias
    playerName,
    clientId: clientId || null,
    gameState: room.gameState,
    phase: room.phase,
    timer: room.timer
  };
  send(ws, successPayload);
  send(ws, { ...successPayload, type: "joined_room", state: room.gameState }); // legacy alias

  if (room.host) {
    send(room.host, {
      type: isReconnect ? "player_reconnected" : "player_joined",
      room: roomCode,
      roomCode,
      playerCount: room.players.size,
      clientId: clientId || null,
      playerName
    });
  }

  publishRoomInfo(room);
  console.log(`[room:${roomCode}] player ${isReconnect ? "reconnected" : "joined"} (${room.players.size} active) name=${playerName}`);
}

function forwardPlayerToHost(ws, room, meta, msg) {
  if (!room.host) {
    sendError(ws, "Host not connected for this room");
    return;
  }

  send(room.host, {
    type: msg.type || "player_input",
    room: room.roomCode,
    roomCode: room.roomCode,
    clientId: meta.clientId || null,
    playerName: meta.playerName || null,
    payload: msg.payload || {},
    eventType: msg.eventType || msg.type || "unknown"
  });
}

console.log("Server running on ws://localhost:5040");

wss.on("connection", (ws) => {
  console.log("New connection");

  ws.on("message", (raw) => {
    let msg;
    try {
      msg = JSON.parse(raw);
    } catch (_) {
      sendError(ws, "Invalid JSON payload");
      return;
    }

    const incomingRoom = normalizeRoomCode(msg.room || msg.roomCode);

    // Backward compatibility aliases
    if (msg.type === "host") msg = { type: "host_register", room: incomingRoom || "DEFAULT" };
    if (msg.type === "join_room") msg = { ...msg, type: "join_room_request", room: incomingRoom };
    if (msg.type === "slider_submit") msg = { type: "player_input", eventType: "slider_submit", payload: { value: msg.value, label: msg.label } };

    if (msg.type === "host_register") {
      const roomCode = normalizeRoomCode(msg.room || msg.roomCode);
      if (!roomCode) return sendError(ws, "roomCode is required for host_register");
      return registerHost(ws, roomCode, msg.clientId || null);
    }

    if (msg.type === "join_room_request") {
      const roomCode = normalizeRoomCode(msg.room || msg.roomCode);
      const playerName = normalizePlayerName(msg.playerName);
      if (!roomCode) return send(ws, { type: "join_room_failed", reason: "roomCode is required" });
      if (!playerName) return send(ws, { type: "join_room_failed", room: roomCode, roomCode, reason: "playerName is required" });
      return joinRoom(ws, roomCode, msg.clientId || null, playerName);
    }

    const meta = socketMeta.get(ws);
    if (!meta) return sendError(ws, "Join or register before sending routed messages");

    const room = rooms.get(meta.roomCode);
    if (!room) return sendError(ws, "Room not found");

    const playerToHostTypes = new Set(["player_input", "player_ready", "player_unready", "game_start_requested"]);
    if (meta.role === "player" && playerToHostTypes.has(msg.type)) {
      return forwardPlayerToHost(ws, room, meta, msg);
    }

    const hostToPlayersTypes = new Set(["host_broadcast", "lobby_updated", "game_started"]);
    if (meta.role === "host" && hostToPlayersTypes.has(msg.type)) {
      return broadcastToPlayers(room, {
        type: msg.type,
        room: room.roomCode,
        roomCode: room.roomCode,
        payload: msg.payload || {},
        eventType: msg.eventType || msg.type
      });
    }

    if (msg.type === "state_update") {
      if (meta.role !== "host") return sendError(ws, "Only host can update state");

      const nextState = normalizeState(msg.state || msg.gameState);
      if (!nextState) return sendError(ws, "state must be a non-empty string");

      room.gameState = nextState;
      if (typeof msg.phase === "number") room.phase = msg.phase;
      if (typeof msg.timer === "number") room.timer = msg.timer;

      const payload = {
        type: "state_update",
        room: room.roomCode,
        roomCode: room.roomCode,
        gameState: room.gameState,
        state: room.gameState, // compatibility alias
        phase: room.phase,
        timer: room.timer
      };

      send(room.host, payload);
      broadcastToPlayers(room, payload);
      return publishRoomInfo(room);
    }

    sendError(ws, `Unknown or unauthorized message type: ${msg.type}`);
  });

  ws.on("close", () => removeSocketFromRoom(ws));
});