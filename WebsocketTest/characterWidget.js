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
    const characterName = document.getElementById("characterName");

    if (background && character.backgroundColor) {
      background.style.backgroundColor = character.backgroundColor;
    }

    if (profileImage && character.faceImage) {
      profileImage.src = character.faceImage;
    }

    if (characterName && character.name) {
      characterName.textContent = character.name;
    }

    if (characterCircle && character.backgroundColor) {
      characterCircle.style.backgroundColor = character.backgroundColor;
    }

    if (fullImage && character.fullImage) {
      fullImage.src = character.fullImage;
    }

    if (box1Text) {
      box1Text.textContent = character.box1Text || "";
      box1Text.style.backgroundColor = character.keywordColor1 || "#43FF32";
    }

    if (box2Text) {
      box2Text.textContent = character.box2Text || "";
      box2Text.style.backgroundColor = character.keywordColor2 || "#FFFFFF";
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