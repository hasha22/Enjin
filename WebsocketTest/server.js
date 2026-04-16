const WebSocket = require('ws')
const wss = new WebSocket.Server({port: 5040}, () =>{
    console.log('server running on ws://localhost:5040')
})

let host = null;
let players = [];

wss.on("connection", (ws) => {
  console.log("New connection");

  ws.on("message", (message) => {
    const msg = JSON.parse(message);

    // UNITY HOST CONNECTS
    if (msg.type === "host") {
      host = ws;
      console.log("Host connected");
    }

    // PLAYER CONNECTS
    else if (msg.type === "join") {
      players.push(ws);
      console.log("Player joined");

      // notify host
      if (host) {
        host.send(JSON.stringify({
          type: "player_joined"
        }));
      }
    }

    // PLAYER PRESSES BUTTON
    else if (msg.type === "button") {
      console.log("Button pressed");

      // send to Unity
      if (host) {
        host.send(JSON.stringify({
          type: "button_pressed"
        }));
      }
    }

    // UNITY SENDS MESSAGE TO PLAYERS
    else if (msg.type === "message") {
      players.forEach(p => {
        p.send(JSON.stringify({
          type: "message",
          text: msg.text
        }));
      });
    }

    else if (msg.type === "slider_submit") {
  console.log("Slider submitted:", msg.value, msg.label);

  // send to Unity (host)
  if (host) {
    host.send(JSON.stringify({
      type: "slider_submit",
      value: msg.value,
      label: msg.label
    }));
  }

  // optionally send to all players too
  players.forEach(p => {
    p.send(JSON.stringify({
      type: "slider_submit",
      value: msg.value,
      label: msg.label
    }));
  });
}
  });

  

  ws.on("close", () => {
    console.log("Disconnected");

    if (ws === host) {
      host = null;
      players = [];
    } else {
      players = players.filter(p => p !== ws);
    }
  });
});



