// PLAYTEST VERSION
// One HTML page, one WebSocket, no reconnect between screens.

const SERVER_URL = "wss://enjin--enjin--qpbmsj2bcc7n.code.run/";

let ws = null;
let joinedRoomCode = null;
let clientId = getClientId();

let currentRound = 1;
let votingDuration = 60;
let votingStartedAt = Date.now();

let currentVoteType = "first_vote";

let hasSubmittedVote = false;
let countdownInterval = null;
let currentScreenId = "joinScreen";

connectWebSocket();
setupVotingControls();
hideCharacterWidgetIfNeeded();

function connectWebSocket() {
  ws = new WebSocket(SERVER_URL);

  ws.onopen = () => {
    console.log("Connected to server from playtest page");
    setStatus("Connected. Enter room code and click Join Room.", "joinScreen");
  };

  ws.onclose = () => {
    console.log("Socket closed on playtest page");
    setStatus("Disconnected from server");
    joinedRoomCode = null;
  };

  ws.onerror = (error) => {
    console.log("WebSocket error:", error);
    setStatus("WebSocket error. Check console.");
  };

  ws.onmessage = (event) => {
    
    const { type, data } = parseServerMessage(event);

    console.log("Message from server:", type, data);

    switch (type) {
      case "join_room_success":
        handleJoinRoomSuccess(data);
        break;
      case "join_room_failed":
        joinedRoomCode = null;
        log("Join failed: " + (data.reason || "Unknown reason"));
        setStatus("Join failed: " + (data.reason || "Unknown reason"));
        break;

      case "game_started":
        handleGameStarted(data);
        break;

      case "show_scenario":
        handleShowScenario(data);
        break;

      case "voting_started":
        handleVotingStarted(data);
        break;

      case "vote_saved":
        handleVoteSaved(data);
        break;

      case "vote_failed":
        handleVoteFailed(data);
        break;
      case "character_info":
        handleCharacterInfo(data);
        break;
      case "error":
        log("Server error");
        break;

      default:
        console.log("Unhandled message type:", type, data);
        break;
    }
  };
}
function handleCharacterInfo(data)
{
  const character = buildCharacterObject(data);

  sessionStorage.setItem(
        "character",
        JSON.stringify(character)
    );

    renderCharacterWidgetSafely();
}
function buildCharacterObject(data)
{
    let faceImage = "";
    let fullImage = "";
    let backgroundColor = "#99C998";

    switch(data.characterName)
    {
        case "AIArtist":
            faceImage = "AIArtist_portrait.png";
            fullImage = "AIArtist.png";
            backgroundColor = "#99C998";
            break;
        case "CulturalOrganizer":
            faceImage = "CulturalOrganizer_portrait.png";
            fullImage = "CulturalOrganizer.png";
            backgroundColor = "#7EA5D8";
            break;
        case "EthicalAdvisor":
            faceImage = "EthicalAdvisor_portrait.png";
            fullImage = "EthicalAdvisor.png";
            backgroundColor = "#f735ea";
            break;
        case "FinanceEmployee":
            faceImage = "FinanceEmployee_portrait.png";
            fullImage = "FinanceEmployee.png";
            backgroundColor = "#FFD700";
            break;
        case "UIDesigner":
            faceImage = "UIDesigner_portrait.png";
            fullImage = "UIDesigner.png";
            backgroundColor = "#088F8F";
            break;
    }

    return {
        faceImage,
        fullImage,
        box1Text: data.keyword1,
        box2Text: data.keyword2,
        modalDescription: data.characterDescription,
        backgroundColor
    };
}

function parseServerMessage(event) {

  let msg;

  try {
    msg = typeof event.data === "string"
      ? JSON.parse(event.data)
      : event.data;
  }
  catch(error) {
    console.error("FAILED TO PARSE EVENT.DATA", error);
    return { type: null, data: {} };
  }

  console.log("PARSED MSG:", msg);

  let data = {};

  if (typeof msg.data === "string" && msg.data.length > 0) {
    try {
      data = JSON.parse(msg.data);
    }
    catch(error) {
      console.warn("Could not parse msg.data:", msg.data);
      data = {};
    }
  }
  else if (msg.data && typeof msg.data === "object") {
    data = msg.data;
  }

  return {
    type: msg.type,
    data: data
  };
}

function joinRoom() {
  const roomInput = document.getElementById("roomCode");
  const nameInput = document.getElementById("playerName");

  const roomCode = roomInput.value.trim().toUpperCase();
  const playerName = nameInput.value.trim();

  if (!roomCode) {
    return log("Room code is required");
  }

  if (!playerName) {
    return log("Player name is required");
  }

  if (!ws || ws.readyState !== WebSocket.OPEN) {
    return log("WebSocket is not open yet. Wait a second and try again.");
  }

  sessionStorage.setItem("roomCode", roomCode);
  sessionStorage.setItem("playerName", playerName);
  sessionStorage.setItem("clientId", clientId);

  ws.send(JSON.stringify({
    type: "join_room_request",
    room: roomCode,
    clientId: clientId,
    playerName: playerName
  }));

  setStatus("Joining room...", "joinScreen");
}

function handleJoinRoomSuccess(data) {
  joinedRoomCode = data.room;

  savePlayerData(data);

  setStatus("Connected to lobby! Waiting for game to start", "connectedScreen");
  log("Joined as " + (data.playerName || sessionStorage.getItem("playerName")), "connectedScreen");

  showScreen("connectedScreen");
}

function handleGameStarted(data) {

  setStatus("Character received. Waiting for the next step.", "characterScreen");
  showScreen("characterScreen");
}

function handleShowScenario(data) {

  setStatus("The current situation is being explained there.", "situationScreen");
  showScreen("situationScreen");
}

function handleVotingStarted(data) {

  resetVotingScreen();
  showScreen("votingScreen");
}

function handleVoteSaved(data) {
  console.log("Vote saved:", data);
  hasSubmittedVote = true;
  showWaitingForOthersScreen();
}

function handleVoteFailed(data) {
  console.log("Vote failed:", data.reason);

  hasSubmittedVote = false;

  const submitVoteBtn = document.getElementById("submitVoteBtn");

  if (submitVoteBtn) {
    submitVoteBtn.disabled = false;
    submitVoteBtn.innerText = "Submit Vote";
  }

  setStatus("Vote failed: " + (data.reason || "Unknown reason"), "votingScreen");
}

function savePlayerData(data) {
  if (data.room) {
    joinedRoomCode = data.room;
    sessionStorage.setItem("roomCode", data.room);
  }

  if (data.playerName) {
    sessionStorage.setItem("playerName", data.playerName);
  }

  if (data.clientId) {
    clientId = data.clientId;
    sessionStorage.setItem("clientId", data.clientId);
  }
}

function setupVotingControls() {
  const voteSlider = document.getElementById("voteSlider");
  const voteValue = document.getElementById("voteValue");
  const submitVoteBtn = document.getElementById("submitVoteBtn");

  if (voteSlider && voteValue) {
    voteValue.innerText = voteSlider.value;

    voteSlider.addEventListener("input", () => {
      voteValue.innerText = voteSlider.value;
    });
  }

  if (submitVoteBtn) {
    submitVoteBtn.addEventListener("click", () => {
      submitVote("manual");
    });
  }
}

function resetVotingScreen() {
  hasSubmittedVote = false;

  const voteSlider = document.getElementById("voteSlider");
  const voteValue = document.getElementById("voteValue");
  const submitVoteBtn = document.getElementById("submitVoteBtn");
  const votingContent = document.getElementById("votingContent");
  const waitingContent = document.getElementById("waitingContent");

  if (voteSlider && voteValue) {
    voteSlider.value = 3;
    voteValue.innerText = voteSlider.value;
  }

  if (submitVoteBtn) {
    submitVoteBtn.disabled = false;
    submitVoteBtn.innerText = "Submit Vote";
  }

  if (votingContent) {
    votingContent.style.display = "flex";
  }

  if (waitingContent) {
    waitingContent.style.display = "none";
  }

  setStatus("Choose your position on the scale", "votingScreen");
}

function mapSliderValueToFirstVote(sliderValue) {
  const value = Number(sliderValue);

  if (value === 1) return "mostly_disagree";
  if (value === 2) return "disagree";
  if (value === 3) return "neutral";
  if (value === 4) return "agree";
  if (value === 5) return "mostly_agree";

  return "neutral";
}

function submitVote(submitReason) {
  if (hasSubmittedVote) {
    return;
  }

  const voteSlider = document.getElementById("voteSlider");
  const submitVoteBtn = document.getElementById("submitVoteBtn");

  if (!voteSlider) {
    return;
  }

  if (!ws || ws.readyState !== WebSocket.OPEN) {
    setStatus("WebSocket is not open. Vote was not sent.", "votingScreen");
    return;
  }

  hasSubmittedVote = true;

  const vote = mapSliderValueToFirstVote(voteSlider.value);
  const roomCode = joinedRoomCode;

  if (submitVoteBtn) {
    submitVoteBtn.disabled = true;
    submitVoteBtn.innerText = "Saving...";
  }

  setStatus("Saving your vote...", "votingScreen");

  ws.send(JSON.stringify({
    type: "submit_vote",
    room: roomCode,
    clientId: clientId,
    roundNumber: currentRound,
    voteType: currentVoteType,
    voteValue: vote,
  }));
}

function showWaitingForOthersScreen() {
  const votingContent = document.getElementById("votingContent");
  const waitingContent = document.getElementById("waitingContent");

  if (votingContent) {
    votingContent.style.display = "none";
  }

  if (waitingContent) {
    waitingContent.style.display = "flex";
  }
}

function showScreen(screenId) {
  document.querySelectorAll(".screen").forEach(screen => {
    screen.classList.remove("active");
  });

  const targetScreen = document.getElementById(screenId);

  if (!targetScreen) {
    console.warn("Screen not found:", screenId);
    return;
  }

  targetScreen.classList.add("active");
  currentScreenId = screenId;
  hideCharacterWidgetIfNeeded();
}

function renderCharacterWidgetSafely() {
  if (window.renderCharacterWidget) {
    window.renderCharacterWidget();
  }

  hideCharacterWidgetIfNeeded();
}

function hideCharacterWidgetIfNeeded() {
  const root = document.getElementById("characterWidgetRoot");

  if (!root) {
    return;
  }

  const hasCharacter = Boolean(sessionStorage.getItem("character"));
  const shouldShowWidget = hasCharacter && currentScreenId !== "joinScreen" && currentScreenId !== "connectedScreen";

  root.style.display = shouldShowWidget ? "block" : "none";
}

function getClientId() {
  let id = sessionStorage.getItem("clientId");

  if (!id) {
    id = crypto.randomUUID();
    sessionStorage.setItem("clientId", id);
  }

  return id;
}

function log(text, screenId) {
  const root = screenId
    ? document.getElementById(screenId)
    : document.querySelector(".screen.active");

  const logElement = root ? root.querySelector("[data-role='log']") : null;

  if (logElement) {
    logElement.innerHTML += "<p>" + text + "</p>";
  } else {
    console.log(text);
  }
}

function setStatus(text, screenId) {
  const root = screenId
    ? document.getElementById(screenId)
    : document.querySelector(".screen.active");

  const statusElement = root ? root.querySelector("[data-role='status']") : null;

  if (statusElement) {
    statusElement.innerText = text;
  } else {
    console.log(text);
  }
}

window.joinRoom = joinRoom;
