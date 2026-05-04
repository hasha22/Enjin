//Setup & Connection
const ws = new WebSocket("ws://localhost:5085");
let joinedRoomCode = null;
const clientId = getClientId();

ws.onopen = () => {
  setStatus("Connected. Enter room code and click Join Room.");
};

ws.onclose = () => {
  setStatus("Disconnected from server");
  joinedRoomCode = null;
};

ws.onmessage = (event) => {
  const msg = JSON.parse(event.data);

  switch(msg.type)
  {
    case "join_room_success":
        joinedRoomCode = msg.room;
        setStatus("Joined room: " + joinedRoomCode);
        log("Joined as " + msg.playerName);
        break;
    case "join_room_failed":
      joinedRoomCode = null;
      log("Join failed");
      break;
    case "error":
      log("Server error");
      break;
  }
};

//Functions
function joinRoom() {

  const roomCode = document.getElementById("roomCode").value.trim().toUpperCase();
  const playerName = document.getElementById("playerName").value.trim();

  if (!roomCode) return log("Room code is required");
  if (!playerName) return log("Player name is required");

  ws.send(JSON.stringify({
    type: "join_room_request",
    room: roomCode,
    clientId,
    playerName
  }));
}

//Helpers
function getClientId() {
  let id = localStorage.getItem("clientId");
  if (!id) {
    id = crypto.randomUUID();
    localStorage.setItem("clientId", id);
  }
  return id;
}

function log(text) {
  document.getElementById("log").innerHTML += "<p>" + text + "</p>";
}

function setStatus(text) {
  document.getElementById("status").innerText = text;
}