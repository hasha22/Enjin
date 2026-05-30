const ws = new WebSocket("wss://enjin--enjin--qpbmsj2bcc7n.code.run/");

const roomCode = sessionStorage.getItem("roomCode");
const clientId = sessionStorage.getItem("clientId");

ws.onopen = () => {
  console.log("Connected from WaitingScreen");

  if (roomCode && clientId) {
    ws.send(JSON.stringify({
      type: "reconnect_request",
      room: roomCode,
      clientId: clientId
    }));
  } else {
    window.location.href = "index.html";
  }
};

ws.onmessage = (event) => {
  const msg = JSON.parse(event.data);
  const data = msg.data ? JSON.parse(msg.data) : {};

  switch (msg.type) {
    case "reconnect_success":
      console.log("Reconnected on WaitingScreen");

      sessionStorage.setItem("roomCode", data.room);
      sessionStorage.setItem("playerName", data.playerName);
      sessionStorage.setItem("clientId", data.clientId);
      sessionStorage.setItem("playerState", data.playerState);

      if (data.character) {
        sessionStorage.setItem("character", JSON.stringify(data.character));
      }
      renderCharacter();

      break;

    case "reconnect_failed":
      console.log("Reconnect failed:", data.reason);
      window.location.href = "index.html";
      break;

    case "show_scenario":
      sessionStorage.setItem("roomCode", data.room);
      sessionStorage.setItem("playerName", data.playerName);
      sessionStorage.setItem("clientId", data.clientId);
      sessionStorage.setItem("playerState", data.playerState);

      window.location.href = "SituationScreen.html";
      break;

  
    case "error":
      console.log("Server error");
      break;
  }
};

ws.onclose = () => {
  console.log("Socket closed on WaitingScreen");
};

function renderCharacter() {
  const savedCharacter = sessionStorage.getItem("character");

  if (!savedCharacter) {
    console.log("No character found in sessionStorage");
    return;
  }

  const character = JSON.parse(savedCharacter);

  const profileImage = document.getElementById("profileImage");
  const characterCircle = document.getElementById("characterCircle");

  if (profileImage) {
    profileImage.src = character.faceImage;
  }

  if (characterCircle) {
    characterCircle.style.backgroundColor = character.backgroundColor;
  }
}

renderCharacter();