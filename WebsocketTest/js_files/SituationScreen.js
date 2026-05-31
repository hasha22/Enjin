const ws = new WebSocket("wss://enjin--enjin--qpbmsj2bcc7n.code.run/");

const roomCode = sessionStorage.getItem("roomCode");
const clientId = sessionStorage.getItem("clientId");

ws.onopen = () => {
  console.log("Reconnected to server from SituationScreen");

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

  switch (msg.type) {
    case "reconnect_success":
      setStatus("Reconnected. Look at the main screen.");

      sessionStorage.setItem("roomCode", data.room);
      sessionStorage.setItem("playerName", data.playerName);
      sessionStorage.setItem("clientId", data.clientId);
      sessionStorage.setItem("playerState", data.playerState);

      if (data.character) {
        sessionStorage.setItem("character", JSON.stringify(data.character));
      }
      break;

    case "reconnect_failed":
      setStatus("Reconnection failed: " + data.reason);
      window.location.href = "index.html";
      break;

    case "voting_started":
      if (data.phase === "voting") {
        window.location.href = "VotingScreen.html";
      }
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
  const logElement = document.getElementById("log");
  if (logElement) {
    logElement.innerHTML += "<p>" + text + "</p>";
  }
}

function setStatus(text) {
  const statusElement = document.getElementById("status");
  if (statusElement) {
    statusElement.innerText = text;
  }
}