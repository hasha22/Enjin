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
  const data = typeof msg.data === "string"
    ? JSON.parse(msg.data)
    : (msg.data || {});

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
  const fullImage = document.getElementById("fullImage");
  const background = document.getElementById("Background");

  const box1Text = document.getElementById("box1Text");
  const box2Text = document.getElementById("box2Text");
  const modalDescription = document.getElementById("modalDescription");

   if (background) {
    background.style.backgroundColor = character.backgroundColor;
   }

  if (profileImage) {
    profileImage.src = character.faceImage;
  }

  if (characterCircle) {
    characterCircle.style.backgroundColor = character.backgroundColor;
  }

  if (fullImage) {
    fullImage.src = character.fullImage;  
  }
  if (box1Text) {
    box1Text.textContent = character.box1Text;
  }

  if (box2Text) {
    box2Text.textContent = character.box2Text;
    box2Text.style.backgroundColor = character.backgroundColor;
  }

  if (modalDescription) {
    modalDescription.textContent = character.modalDescription;
  }
}


renderCharacter();