const WebSocket = require("ws");

const wss = new WebSocket.Server({ port: 5040 });

let host = null;

console.log("Server running on ws://localhost:5040");

wss.on("connection", (ws) => {
  console.log("New connection");

  ws.on("message", (data) => {
    let msg;

    try {
      msg = JSON.parse(data);
    } catch (e) {
      return;
    }

    // Unity connects
    if (msg.type === "host") {
      host = ws;
      console.log("Unity connected");
    }

    // Browser slider → Unity
    if (msg.type === "slider_submit") {
      console.log("Slider:", msg.value);

      if (host) {
        host.send(JSON.stringify({
          type: "slider_submit",
          value: msg.value,
          label: msg.label
        }));
      }
    }

    // UNITY button → Browser
    if (msg.type === "button_pressed") {
      console.log("Button pressed in Unity");

      wss.clients.forEach(client => {
        if (client !== host && client.readyState === WebSocket.OPEN) {
          client.send(JSON.stringify({
            type: "button_pressed"
          }));
        }
      });
    }
  });

  ws.on("close", () => {
    if (ws === host) host = null;
  });
});