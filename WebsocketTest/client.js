// PLAYTEST VERSION
// One HTML page, one WebSocket, no reconnect between screens.

const DEPLOYED_SERVER_URL = "wss://enjin--enjin--qpbmsj2bcc7n.code.run/";
const SERVER_URL = getServerUrl();

let ws = null;
let joinedRoomCode = null;
let clientId = getClientId();

let currentRound = 1;
let votingDuration = 60;
let votingStartedAt = Date.now();

let currentVoteType = "first_vote";
let selectedSecondVote = null;

let hasSubmittedVote = false;
let countdownInterval = null;
let discussionCountdownInterval = null;
let currentScreenId = "joinScreen";

connectWebSocket();
setupVotingControls();
hideCharacterWidgetIfNeeded();

function getServerUrl() {
  return DEPLOYED_SERVER_URL;
}

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
      case "player_screen_changed":
        handlePlayerScreenChanged(data);
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
function handlePlayerScreenChanged(data) {
  savePlayerData(data);

  if (data.roundNumber) {
    currentRound = Number(data.roundNumber);
  }

  if (data.voteType) {
    currentVoteType = data.voteType;
  }

  if (data.votingDuration) {
    votingDuration = Number(data.votingDuration);
  }

  if (data.screenId === "characterScreen") {
    renderCharacterWidgetSafely();
    showScreen("characterScreen");
    return;
  }

  if (data.screenId === "situationScreen") {
    renderCharacterWidgetSafely();
    showScreen("situationScreen");
    return;
  }

  if (data.screenId === "votingScreen") {
    clearDiscussionTimer();
    resetVotingScreen();
    renderCharacterWidgetSafely();
    showScreen("votingScreen");
    startVotingTimer();
    return;
  }

  if (data.screenId === "discussionScreen") {
    clearInterval(countdownInterval);
    renderCharacterWidgetSafely();
    showDiscussionTurnScreen(data);
    return;
  }

  if (data.screenId === "waitingScreen") {
    clearDiscussionTimer();
    renderCharacterWidgetSafely();
    showWaitingStateScreen(data);
    return;
  }

  if (data.screenId === "gameOverScreen") {
    clearInterval(countdownInterval);
    clearDiscussionTimer();
    showScreen("gameOverScreen");
    return;
  }

  console.warn("Unknown screenId from Unity:", data.screenId);
}

function showWaitingStateScreen(data) {
  updateRoundPlaceholders(data.roundNumber || currentRound);

  const screenByState = {
    waiting_for_situation: "waitingSituationScreen",
    waiting_for_discussion: "waitingDiscussionScreen",
    waiting_after_discussion: "waitingAfterDiscussionScreen",
    waiting_for_enjin_update: "waitingEnjinUpdateScreen",
    waiting_for_results: "waitingResultsScreen"
  };

  showScreen(screenByState[data.playerState] || "waitingDiscussionScreen");
}

function updateRoundPlaceholders(roundNumber) {
  const round = Number(roundNumber || currentRound || 1);

  document.querySelectorAll("[data-round-number]").forEach(element => {
    element.innerText = round;
  });
}

function showDiscussionTurnScreen(data) {
  const timerElement = document.getElementById("discussionTimer");
  const myTurnBlock = document.getElementById("myTurnBlock");
  const otherSpeakerBlock = document.getElementById("otherSpeakerBlock");
  const speakerNameElement = document.getElementById("speakerName");
  const currentClientId = normalizeId(clientId || sessionStorage.getItem("clientId"));
  const currentPlayerName = normalizeId(sessionStorage.getItem("playerName"));
  const speakerId = normalizeId(data.currentSpeakerPlayerID);
  const speakerNameId = normalizeId(data.currentSpeakerName);
  const isMyTurn = data.isCurrentSpeaker === true
    || (speakerId && speakerId === currentClientId)
    || (speakerNameId && speakerNameId === currentPlayerName);
  const duration = Number(data.votingDuration || 0);

  if (myTurnBlock) {
    myTurnBlock.style.display = isMyTurn ? "block" : "none";
  }

  if (otherSpeakerBlock) {
    otherSpeakerBlock.style.display = isMyTurn ? "none" : "block";
  }

  if (speakerNameElement && data.currentSpeakerName) {
    speakerNameElement.innerText = data.currentSpeakerName;
  }

  showScreen("discussionScreen");
  startDiscussionTimer(duration, timerElement);
}

function normalizeId(value) {
  return typeof value === "string" ? value.trim().toLowerCase() : "";
}

function startDiscussionTimer(duration, timerElement) {
  clearDiscussionTimer();

  let remainingSeconds = Math.max(Number(duration || 0), 0);

  function renderDiscussionTimer() {
    if (!timerElement) {
      return;
    }

    const minutes = Math.floor(remainingSeconds / 60);
    const seconds = remainingSeconds % 60;
    timerElement.innerText = "Timer: " + minutes + "m " + seconds + "s";
  }

  renderDiscussionTimer();

  discussionCountdownInterval = setInterval(() => {
    remainingSeconds = Math.max(remainingSeconds - 1, 0);
    renderDiscussionTimer();

    if (remainingSeconds <= 0) {
      clearDiscussionTimer();
    }
  }, 1000);
}

function clearDiscussionTimer() {
  clearInterval(discussionCountdownInterval);
  discussionCountdownInterval = null;
}
function handleCharacterInfo(data)
{
   console.log("CHARACTER_INFO RECEIVED:", data);
  const character = buildCharacterObject(data);
  console.log("Built character:", character);

  sessionStorage.setItem(
        "character",
        JSON.stringify(character)
    );
    console.log("STORED CHARACTER:", sessionStorage.getItem("character"));

    renderCharacterWidgetSafely();
}
function buildCharacterObject(data)
{
    let faceImage = "";
    let fullImage = "";
    let backgroundColor = "";
    let keywordColor1 = "";
    let keywordColor2 = "";

    switch(data.characterName)
    {
        case "AIArtist":
            faceImage = "AIArtist_portrait.png";
            fullImage = "AIArtist.png";
            backgroundColor = "#99C998";
            keywordColor1 = "#00FFB5";
            keywordColor2 = "#FFF100";
            break;
        case "CulturalOrganizer":
            faceImage = "CulturalOrganizer_portrait.png";
            fullImage = "CulturalOrganizer.png";
            backgroundColor = "#7EA5D8";
            keywordColor1 = "#FF006F";
            keywordColor2 = "#9B00F3";
            break;
        case "EthicalAdvisor":
            faceImage = "EthicalAdvisor_portrait.png";
            fullImage = "EthicalAdvisor.png";
            backgroundColor = "#f735ea";
            keywordColor1 = "#9B00F3";
            keywordColor2 = "#FF006F";
            break;
        case "FinanceEmployee":
            faceImage = "FinanceEmployee_portrait.png";
            fullImage = "FinanceEmployee.png";
            backgroundColor = "#FFD700";
            keywordColor1 = "#FFF100";
            keywordColor2 = "#00FFB5";
            break;
        case "UIDesigner":
            faceImage = "UIDesigner_portrait.png";
            fullImage = "UIDesigner.png";
            backgroundColor = "#088F8F";
            keywordColor1 = "#FF006F";
            keywordColor2 = "#00FFB5";
            break;
    }

    return {
        faceImage,
        fullImage,
        box1Text: data.keyword1,
        box2Text: data.keyword2,
        modalDescription: data.characterDescription,
        keywordColor1,
        keywordColor2
    };
}

function parseServerMessage(event) {
  const msg = JSON.parse(event.data);

  let data = {};

  if (typeof msg.data === "string" && msg.data.length > 0) {
    try {
      data = JSON.parse(msg.data);
    } catch (error) {
      console.warn("Could not parse msg.data as JSON:", msg.data);
      data = {};
    }
  } else if (msg.data && typeof msg.data === "object") {
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
  sessionStorage.removeItem("character");

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
  savePlayerData(data);

  renderCharacterWidgetSafely();
  showScreen("characterScreen");
}

function handleShowScenario(data) {
  savePlayerData(data);

  renderCharacterWidgetSafely();
  showScreen("situationScreen");
}

function handleVotingStarted(data) {
  savePlayerData(data);

  resetVotingScreen();
  renderCharacterWidgetSafely();
  showScreen("votingScreen");
  startVotingTimer();
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
  const enactVoteBtn = document.getElementById("enactVoteBtn");
  const rejectVoteBtn = document.getElementById("rejectVoteBtn");

  if (submitVoteBtn) {
    submitVoteBtn.disabled = false;
    submitVoteBtn.innerText = "Submit Vote";
  }

  if (enactVoteBtn) {
    enactVoteBtn.disabled = false;
  }

  if (rejectVoteBtn) {
    rejectVoteBtn.disabled = false;
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

  if (data.playerState) {
    sessionStorage.setItem("playerState", data.playerState);
  }

  if (data.character) {
    const character = buildCharacterObject(data.character);

    if (character) {
      sessionStorage.setItem("character", JSON.stringify(character));
    }
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

  if (enactVoteBtn) {
    enactVoteBtn.addEventListener("click", () => {
      selectedSecondVote = "yes";
      submitVote("manual");
    });
  }

  if (rejectVoteBtn) {
    rejectVoteBtn.addEventListener("click", () => {
      selectedSecondVote = "no";
      submitVote("manual");
    });
  }
}

function resetVotingScreen() {
  hasSubmittedVote = false;
  selectedSecondVote = null;

  const voteSlider = document.getElementById("voteSlider");
  const voteValue = document.getElementById("voteValue");
  const submitVoteBtn = document.getElementById("submitVoteBtn");
  const votingContent = document.getElementById("votingContent");
  const firstVoteContent = document.getElementById("firstVoteContent");
  const secondVoteContent = document.getElementById("secondVoteContent");
  const roundNumber = document.getElementById("roundNumber");
  const votingTitle = document.getElementById("votingTitle");
  const timerElement = document.getElementById("timer");
  const enactVoteBtn = document.getElementById("enactVoteBtn");
  const rejectVoteBtn = document.getElementById("rejectVoteBtn");
  const isSecondVote = currentVoteType === "second_vote";

  votingStartedAt = Date.now();

  if (roundNumber) {
    roundNumber.innerText = currentRound;
  }

  if (voteSlider && voteValue) {
    voteSlider.value = 3;
    voteValue.innerText = voteSlider.value;
  }

  if (submitVoteBtn) {
    submitVoteBtn.disabled = false;
    submitVoteBtn.innerText = "Submit Vote";
  }

  if (enactVoteBtn) {
    enactVoteBtn.disabled = false;
  }

  if (rejectVoteBtn) {
    rejectVoteBtn.disabled = false;
  }

  if (votingContent) {
    votingContent.style.display = "flex";
  }

  if (votingTitle) {
    votingTitle.style.display = isSecondVote ? "none" : "block";
  }

  if (timerElement) {
    timerElement.style.display = "block";
  }

  if (firstVoteContent) {
    firstVoteContent.style.display = isSecondVote ? "none" : "flex";
  }

  if (secondVoteContent) {
    secondVoteContent.style.display = isSecondVote ? "flex" : "none";
  }

  const statusText = isSecondVote
    ? "Choose whether to enact or reject the policy"
    : "Choose your position on the scale";

  setStatus(statusText, "votingScreen");
}

function startVotingTimer() {
  clearInterval(countdownInterval);

  updateTimerText();

  countdownInterval = setInterval(() => {
    updateTimerText();

    const remainingSeconds = getRemainingSeconds();

    if (remainingSeconds <= 0) {
      clearInterval(countdownInterval);

      if (!hasSubmittedVote) {
        if (currentVoteType === "second_vote") {
          handleSecondVoteTimeout();
        } else {
          submitVote("timeout");
        }
      }
    }
  }, 250);
}

function handleSecondVoteTimeout() {
  const enactVoteBtn = document.getElementById("enactVoteBtn");
  const rejectVoteBtn = document.getElementById("rejectVoteBtn");

  hasSubmittedVote = true;

  if (enactVoteBtn) {
    enactVoteBtn.disabled = true;
  }

  if (rejectVoteBtn) {
    rejectVoteBtn.disabled = true;
  }

  setStatus("Time is up. Waiting for the main screen.", "votingScreen");
}

function getRemainingSeconds() {
  const now = Date.now();
  const elapsedSeconds = Math.floor((now - votingStartedAt) / 1000);
  return Math.max(votingDuration - elapsedSeconds, 0);
}

function updateTimerText() {
  const remainingSeconds = getRemainingSeconds();
  const timerElement = document.getElementById("timer");

  if (timerElement) {
    timerElement.innerText = "Time left: " + remainingSeconds + "s";
  }
}

function mapSliderValueToFirstVote(sliderValue) {
  const value = Number(sliderValue);

  if (value === 1) return "disagree";
  if (value === 2) return "mostly_disagree";
  if (value === 3) return "neutral";
  if (value === 4) return "mostly_agree";
  if (value === 5) return "agree";

  return "neutral";
}

function submitVote(submitReason) {
  if (hasSubmittedVote) {
    return;
  }

  const voteSlider = document.getElementById("voteSlider");
  const submitVoteBtn = document.getElementById("submitVoteBtn");
  const enactVoteBtn = document.getElementById("enactVoteBtn");
  const rejectVoteBtn = document.getElementById("rejectVoteBtn");

  if (currentVoteType !== "second_vote" && !voteSlider) {
    return;
  }

  if (!ws || ws.readyState !== WebSocket.OPEN) {
    setStatus("WebSocket is not open. Vote was not sent.", "votingScreen");
    return;
  }

  hasSubmittedVote = true;

  const vote = currentVoteType === "second_vote"
    ? selectedSecondVote
    : mapSliderValueToFirstVote(voteSlider.value);

  if (!vote) {
    hasSubmittedVote = false;
    setStatus("Choose an option before submitting.", "votingScreen");
    return;
  }

  const roomCode = joinedRoomCode || sessionStorage.getItem("roomCode");

  if (submitVoteBtn) {
    submitVoteBtn.disabled = true;
    submitVoteBtn.innerText = "Saving...";
  }

  if (enactVoteBtn) {
    enactVoteBtn.disabled = true;
  }

  if (rejectVoteBtn) {
    rejectVoteBtn.disabled = true;
  }

  setStatus("Saving your vote...", "votingScreen");

  ws.send(JSON.stringify({
    type: "submit_vote",
    room: roomCode,
    clientId: clientId,
    roundNumber: currentRound,
    voteType: currentVoteType,
    voteValue: vote,
    submitReason: submitReason
  }));
}

function showWaitingForOthersScreen() {
  showScreen("voteSavedScreen");
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
