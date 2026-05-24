// Setup
const ws = new WebSocket("wss://enjin--enjin--qpbmsj2bcc7n.code.run/");

const roomCode = localStorage.getItem("roomCode");
const clientId = localStorage.getItem("clientId");


ws.onopen = () => {
  console.log("Reconnected to server");   
    if (roomCode && clientId) {
        ws.send(JSON.stringify({
            type: "reconnect_request",
            room: roomCode,
            clientId: clientId
            }));
    } else {
        setStatus("Missing saved player data");
        window.location.href = "index.html";
            }
        
    };      


ws.onmessage = (event) => {
  const msg = JSON.parse(event.data);
  const data = msg.data ? JSON.parse(msg.data) : {};
    switch(msg.type)
    {        case "reconnect_success":
            setStatus("Reconnected to room: " + data.room);
            log("Reconnected as " + data.playerName);   
            localStorage.setItem("roomCode", data.room);
            localStorage.setItem("playerName", data.playerName);
            localStorage.setItem("clientId", data.clientId);
            localStorage.setItem("playerState", data.playerState);
            break;
            
        case "reconnect_failed":
        setStatus("Reconnection failed: " + data.reason);
        log("Reconnection failed: " + data.reason);
        window.location.href = "index.html";
        break;
        case "error":
            log("Server error");
            break;
    }
};

ws.onclose = () => {
  console.log("Connection closed");
  setStatus("Connection lost");
};


function log(text) {
  document.getElementById("log").innerHTML += "<p>" + text + "</p>";
}

function setStatus(text) {
  document.getElementById("status").innerText = text;
}