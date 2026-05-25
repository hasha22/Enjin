const ws = new WebSocket("wss://enjin--enjin--qpbmsj2bcc7n.code.run/");

const roomCode = localStorage.getItem("roomCode");
const clientId = localStorage.getItem("clientId");

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

      localStorage.setItem("roomCode", data.room);
      localStorage.setItem("playerName", data.playerName);
      localStorage.setItem("clientId", data.clientId);
      localStorage.setItem("playerState", data.playerState);

      if (data.character) {
        localStorage.setItem("character", JSON.stringify(data.character));
      }
      renderCharacter();

      break;

    case "reconnect_failed":
      console.log("Reconnect failed:", data.reason);
      window.location.href = "index.html";
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
  const savedCharacter = localStorage.getItem("character");

  if (!savedCharacter) {
    console.log("No character found in localStorage");
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