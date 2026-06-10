(function () {
  const ROOT_ID = "characterWidgetRoot";

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
    //box1Text.style.backgroundColor = keywordColor1;

    const box2Text = document.createElement("div");
    box2Text.id = "box2Text";
    box2Text.className = "text-box character-widget-text-box";
    //box2Text.style.backgroundColor = keywordColor2;

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
    const imageModal = document.getElementById("imageModal");

    if (!profileImage || !imageModal) return;
    if (profileImage.dataset.characterWidgetEventsAttached === "true") return;

    profileImage.addEventListener("click", function () {
      imageModal.classList.add("active");
    });

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

    const profileImage = document.getElementById("profileImage");
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
    }

    if (characterCircle && character.backgroundColor) {
      characterCircle.style.backgroundColor = character.backgroundColor;
    }

    if (fullImage && character.fullImage) {
      fullImage.src = character.fullImage;
    }

    if (box1Text) {
      box1Text.textContent = character.box1Text || "";
      box1Text.style.backgroundColor = character.keywordColor1 || "#000000";
    }

    if (box2Text) {
      box2Text.textContent = character.box2Text || "";
      box2Text.style.backgroundColor = character.keywordColor2 || "#000000";
    }

    if (modalDescription) {
      modalDescription.textContent = character.modalDescription || "";
    }
  }

  window.renderCharacterWidget = renderCharacterWidget;

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", renderCharacterWidget);
  } else {
    renderCharacterWidget();
  }
})();