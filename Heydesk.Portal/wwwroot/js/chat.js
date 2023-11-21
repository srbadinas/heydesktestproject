var conversationMessages = document.querySelector('#conversation-messages');

conversationMessages.scrollTop = conversationMessages.scrollHeight;

var sendMessageForm = document.querySelector('#send-message-form');

var sendMessageConversationIdField = document.querySelector('#send-message-conversation-id-field');
var sendMessageUserIdField = document.querySelector('#send-message-user-id-field');

var sendMessageField = document.querySelector('#send-message-field');
var sendMessageBtn = document.querySelector('#send-message-btn');

sendMessageField.addEventListener('keydown', function (e) {
    if (e.keyCode === 13) {
        e.preventDefault();
        sendMessageBtn.click();
    }
});

var connection = new signalR.HubConnectionBuilder().withUrl('https://localhost:7062/ChatHub')
    .withAutomaticReconnect()
    .build();

connection.start().then(async function () {
    sendMessageField.disabled = false;
    sendMessageBtn.disabled = false;
    await connection.invoke("ConnectChat", parseInt(sendMessageConversationIdField.value));
}).catch(err => {
    console.error(err);
});

connection.onclose(async () => {
    await connection.start();
})

window.onunload = async function (e) {
    await connection.invoke("DisconnectChat", parseInt(sendMessageConversationIdField.value))
}

connection.on('ReceiveMessage', function (message, isMe) {
    var alertDiv = document.createElement('div');

    if (isMe == true) {
        alertDiv.className = 'alert alert-secondary';
    } else {
        alertDiv.className = 'alert alert-success';
    }

    alertDiv.innerHTML = message;

    conversationMessages.appendChild(alertDiv);
    conversationMessages.scrollTop = conversationMessages.scrollHeight;
});

sendMessageForm.addEventListener('submit', async function(e) {
    e.preventDefault();

    try {
        var msg = sendMessageField.value;
        sendMessageField.value = '';
        sendMessageField.disabled = true;
        sendMessageBtn.disabled = true;

        await fetch('https://localhost:7062/api/Conversations/SendMessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                ConversationId: parseInt(sendMessageConversationIdField.value),
                UserId: parseInt(sendMessageUserIdField.value),
                Message: msg,
            })
        });

        conversationMessages.scrollTop = conversationMessages.scrollHeight;
        sendMessageField.disabled = false;
        sendMessageBtn.disabled = false;

    } catch (err) {
        console.error(err);
    }

    //connection.invoke("SendMessage", parseInt(sendMessageConversationIdField.value), parseInt(sendMessageUserIdField.value), sendMessageField.value).catch(function (err) {
    //    console.error(err.toString());
    //});
});
