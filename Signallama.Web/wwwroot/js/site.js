
// create hub connection
var connection = new signalR.HubConnectionBuilder().withUrl("/hubs/chat").build();

// perform stuff here before getting connected (e.g.: deactivate stuff...)
updateConnectionStatus(false);

document.getElementById('chatInput').addEventListener('keydown', function (event) {
    const textValue = document.getElementById('chatInput').value;
    if (textValue && event.key === 'Enter') {
        sendMessage();
    }
});

// create event handlers
connection.on("ShowReply", function (message) {
    console.log(message);
    appendMessage(false, `${message}`);
});

connection.onclose(() => {
    updateConnectionStatus(false);
});

// start the connection
connection.start().then(function () {
    //perform stuff here on connected
    onConnected(connection);
}).catch(function (err) {
    //on error
    updateConnectionStatus(false);
    return console.error(err.toString());
});

function onConnected(connection) {
    console.log(`connection started: ${connection.state}`);
    updateConnectionStatus(true);
}

function updateConnectionStatus(isConnected) {
    const statusElement = document.getElementById('connectionState');
    if (isConnected) {
        statusElement.innerHTML = 'Connected';
        statusElement.classList.remove('text-bg-danger');
        statusElement.classList.add('text-bg-success');
    } else {
        statusElement.innerHTML = 'Disconnected';
        statusElement.classList.remove('text-bg-success');
        statusElement.classList.add('text-bg-danger');
    }
}

function sendMessage() {
    const message = document.getElementById('chatInput').value;
    if (message) {
        appendMessage(true, message);
        document.getElementById('chatInput').value = '';
        connection.send("Send", "", message);
    }
}

function appendMessage(isSender, message) {
    const chatMessages = document.getElementById('chatMessages');
    const messageElement = createMessageElement(message, isSender, null)
    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function createMessageElement(message, isSender, id) {
    const messageElement = document.createElement('div');
    messageElement.classList.add('message', isSender ? 'sent' : 'received');
    messageElement.innerText = message;
    if (id) {
        messageElement.id = id;
    }
    return messageElement;
}