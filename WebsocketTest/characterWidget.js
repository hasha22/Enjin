(function () {
  const ROOT_ID = "characterWidgetRoot";
  const STYLE_ID = "characterWidgetStyles";

  function injectCharacterWidgetStyles() {
    if (document.getElementById(STYLE_ID)) return;

    const style = document.createElement("style");
    style.id = STYLE_ID;
    style.textContent = `
      .character-widget-root {
        position: fixed;
        top: 6px;
        left: 6px;
        z-index: 2000;
        font-family: 'Dobra', serif;
      }

      .character-widget-root .circle,
      .character-widget-root .character-widget-circle {
        width: 100px;
        height: 100px;
        border-radius: 50%;
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 4px;
      }

      .character-widget-root .circle-image,
      .character-widget-root .character-widget-face-image {
        width: 92px;
        height: 92px;
        border-radius: 50%;
        object-fit: cover;
        cursor: pointer;
      }

      .character-widget-root .character-widget-initials {
        display: none;
        width: 92px;
        height: 92px;
        border-radius: 50%;
        align-items: center;
        justify-content: center;
        color: #333333;
        cursor: pointer;
        font-family: 'Dobra', serif;
        font-size: 18px;
        font-weight: bold;
        text-align: center;
        padding: 8px;
        box-sizing: border-box;
      }

      .character-widget-root .modal,
      .character-widget-root .character-widget-modal {
        display: none;
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.5);
        justify-content: center;
        align-items: center;
        z-index: 3000;
      }

      .character-widget-root .modal.active,
      .character-widget-root .character-widget-modal.active {
        display: flex;
      }

      .character-widget-root .modal-content,
      .character-widget-root .character-widget-modal-content {
        width: min(560px, calc(100vw - 48px));
        max-height: calc(100vh - 64px);
        min-height: 360px;
        background-color: #D9D9D9;
        border: 5px solid #99C998;
        border-radius: 20px;
        display: flex;
        justify-content: flex-start;
        align-items: center;
        padding: 10px;
        overflow: auto;
        box-sizing: border-box;
      }

      .character-widget-root .modal-content img,
      .character-widget-root .character-widget-modal-content img {
        max-width: 50%;
        max-height: 100%;
        object-fit: contain;
        border-radius: 15px;
      }

      .character-widget-root .modal-right-section,
      .character-widget-root .character-widget-right-section {
        width: 70%;
        min-height: 100%;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        gap: 12px;
        padding: 16px;
        box-sizing: border-box;
      }

      .character-widget-root .text-box,
      .character-widget-root .character-widget-text-box {
        width: 80%;
        background-color: #FFFFFF;
        border-radius: 15px;
        padding: 14px;
        text-align: center;
        font-family: 'Dobra', serif;
        font-size: 16px;
        color: #333333;
        min-height: 48px;
        display: flex;
        align-items: center;
        justify-content: center;
        box-sizing: border-box;
      }

      .character-widget-root .modal-text,
      .character-widget-root .character-widget-modal-text {
        width: 85%;
        text-align: center;
        font-family: 'Dobra', serif;
        font-size: 14px;
        line-height: 1.35;
        color: #333333;
        margin: 4px 0 0;
      }

      @media (max-width: 560px) {
        .character-widget-root .modal-content,
        .character-widget-root .character-widget-modal-content {
          flex-direction: column;
          width: calc(100vw - 32px);
          min-height: 0;
          padding: 14px;
        }

        .character-widget-root .modal-content img,
        .character-widget-root .character-widget-modal-content img {
          max-width: 70%;
          max-height: 220px;
        }

        .character-widget-root .modal-right-section,
        .character-widget-root .character-widget-right-section {
          width: 100%;
          min-height: 0;
          padding: 8px;
        }
      }
    `;

    document.head.appendChild(style);
  }

  function getSavedCharacter() {
    const savedCharacter = sessionStorage.getItem("character");

    if (!savedCharacter) {
      return null;
    }

    try {
      return JSON.parse(savedCharacter);
    } catch (error) {
      console.error("Could not parse character from sessionStorage:", error);
      return null;
    }
  }

  function createCharacterWidgetShell() {
    if (document.getElementById(ROOT_ID)) {
      return;
    }

    const root = document.createElement("div");
    root.id = ROOT_ID;
    root.className = "character-widget-root";

    const circle = document.createElement("div");
    circle.id = "characterCircle";
    circle.className = "circle character-widget-circle";

    const profileImage = document.createElement("img");
    profileImage.id = "profileImage";
    profileImage.className = "circle-image character-widget-face-image";
    profileImage.alt = "profile";
    profileImage.src = "";

    circle.appendChild(profileImage);

    const initials = document.createElement("button");
    initials.id = "characterInitials";
    initials.className = "character-widget-initials";
    initials.type = "button";
    initials.textContent = "";

    circle.appendChild(initials);

    const modal = document.createElement("div");
    modal.id = "imageModal";
    modal.className = "modal character-widget-modal";

    const modalContent = document.createElement("div");
    modalContent.className = "modal-content character-widget-modal-content";

    const fullImage = document.createElement("img");
    fullImage.id = "fullImage";
    fullImage.className = "full-character-image";
    fullImage.alt = "full character";
    fullImage.src = "";

    const rightSection = document.createElement("div");
    rightSection.className = "modal-right-section character-widget-right-section";

    const box1Text = document.createElement("div");
    box1Text.id = "box1Text";
    box1Text.className = "text-box character-widget-text-box";
    box1Text.style.backgroundColor = "#FFFFFF";

    const box2Text = document.createElement("div");
    box2Text.id = "box2Text";
    box2Text.className = "text-box character-widget-text-box";

    const modalDescription = document.createElement("p");
    modalDescription.id = "modalDescription";
    modalDescription.className = "modal-text character-widget-modal-text";

    rightSection.appendChild(box1Text);
    rightSection.appendChild(box2Text);
    rightSection.appendChild(modalDescription);

    modalContent.appendChild(fullImage);
    modalContent.appendChild(rightSection);
    modal.appendChild(modalContent);

    root.appendChild(circle);
    root.appendChild(modal);
    document.body.appendChild(root);

    attachCharacterWidgetEvents();
  }

  function attachCharacterWidgetEvents() {
    const profileImage = document.getElementById("profileImage");
    const characterInitials = document.getElementById("characterInitials");
    const imageModal = document.getElementById("imageModal");

    if (!profileImage || !imageModal) return;
    if (profileImage.dataset.characterWidgetEventsAttached === "true") return;

    const openModal = function () {
      imageModal.classList.add("active");
    };

    profileImage.addEventListener("click", openModal);

    if (characterInitials) {
      characterInitials.addEventListener("click", openModal);
    }

    imageModal.addEventListener("click", function (event) {
      if (event.target === imageModal) {
        imageModal.classList.remove("active");
      }
    });

    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        imageModal.classList.remove("active");
      }
    });

    profileImage.dataset.characterWidgetEventsAttached = "true";
  }

  function renderCharacterWidget() {
    injectCharacterWidgetStyles();
    createCharacterWidgetShell();
    attachCharacterWidgetEvents();

    const character = getSavedCharacter();
    console.log("RENDERING CHARACTER:", character);
    const root = document.getElementById(ROOT_ID);

    if (!character) {
      if (root) root.style.display = "none";
      console.log("No character found in sessionStorage");
      return;
    }

    if (root) root.style.display = "block";

    const profileImage = document.getElementById("profileImage");
    const characterInitials = document.getElementById("characterInitials");
    const characterCircle = document.getElementById("characterCircle");
    const fullImage = document.getElementById("fullImage");
    const box1Text = document.getElementById("box1Text");
    const box2Text = document.getElementById("box2Text");
    const modalDescription = document.getElementById("modalDescription");
    const background = document.getElementById("Background");

    if (background && character.backgroundColor) {
      background.style.backgroundColor = character.backgroundColor;
    }

    if (profileImage && character.faceImage) {
      profileImage.src = character.faceImage;
      profileImage.style.display = "block";
      profileImage.alt = character.characterName || "profile";
    } else if (profileImage) {
      profileImage.removeAttribute("src");
      profileImage.style.display = "none";
    }

    if (characterInitials) {
      characterInitials.textContent = getCharacterInitials(character.characterName);
      characterInitials.style.display = character.faceImage ? "none" : "flex";
    }

    if (characterCircle && character.backgroundColor) {
      characterCircle.style.backgroundColor = character.backgroundColor;
    }

    if (fullImage && character.fullImage) {
      fullImage.src = character.fullImage;
      fullImage.style.display = "block";
    } else if (fullImage) {
      fullImage.removeAttribute("src");
      fullImage.style.display = "none";
    }

    if (box1Text) {
      box1Text.textContent = character.box1Text || "";
    }

    if (box2Text) {
      box2Text.textContent = character.box2Text || "";

      if (character.backgroundColor) {
        box2Text.style.backgroundColor = character.backgroundColor;
      }
    }

    if (modalDescription) {
      modalDescription.textContent = character.modalDescription || "";
    }
  }

  function getCharacterInitials(characterName) {
    if (!characterName) return "Character";

    return String(characterName)
      .replace(/([a-z])([A-Z])/g, "$1 $2")
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0].toUpperCase())
      .join("");
  }

  window.renderCharacterWidget = renderCharacterWidget;

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", renderCharacterWidget);
  } else {
    renderCharacterWidget();
  }
})();
