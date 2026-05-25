//Setup & Connection
const ws = new WebSocket("wss://enjin--enjin--qpbmsj2bcc7n.code.run");// Create a new WebSocket connection to the server at ws://localhost:5085
let joinedRoomCode = null; // Variable to store the room code of the room that the client has joined, initialized to null
const clientId = getClientId();// Get the client's unique ID using the getClientId helper function and store it in the clientId variable

ws.onopen = () => {
  setStatus("Connected. Enter room code and click Join Room."); // When the WebSocket connection is opened, set the status text to prompt the user to enter a room code and join a room
};

ws.onclose = () => {
  setStatus("Disconnected from server"); // When the WebSocket connection is closed, set the status text to indicate that the client has been disconnected from the server
  joinedRoomCode = null;
};

ws.onmessage = (event) => {
  const msg = JSON.parse(event.data);
  const data = msg.data ? JSON.parse(msg.data) : {};

  switch(msg.type)
  {
    case "join_room_success":
        joinedRoomCode = data.room;
        setStatus("Joined room: " + joinedRoomCode);
        log("Joined as " + data.playerName);


        localStorage.setItem("roomCode", data.room);
        localStorage.setItem("playerName", data.playerName);
        localStorage.setItem("clientId", data.clientId);
        localStorage.setItem("playerState", data.playerState);

        window.location.href = "WaitingScreen.html";
        
        break;
    case "join_room_failed":
      joinedRoomCode = null;
      log("Join failed"); // reason?
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
  document.getElementById("log").innerHTML += "<p>" + text + "</p>"; // Append a new paragraph containing the provided text to the inner HTML of the element with the ID "log"
}

function setStatus(text) {
  document.getElementById("status").innerText = text; // Set the inner text of the element with the ID "status" to the provided text
}