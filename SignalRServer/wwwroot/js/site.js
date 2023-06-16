const connection = new signalR.HubConnectionBuilder()
    .withUrl("/messageHub", {
        accessTokenFactory: null,
        withCredentials: true
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

const messageList = document.getElementById("messageList");
const streamList = document.getElementById("streamList");

const broadcastForm = document.getElementById("broadcastForm");
const broadcastMessageInput = document.getElementById("broadcastMessage");

const sendSelfForm = document.getElementById("sendSelfForm");
const sendSelfMessageInput = document.getElementById("sendSelfMessage");

const sendOthersForm = document.getElementById("sendOthersForm");
const sendOthersMessageInput = document.getElementById("sendOthersMessage");

const sendSpecificForm = document.getElementById("sendSpecificForm");
const sendSpecificMessageInput = document.getElementById("sendSpecificMessage");
const sendSpecificConnectionIdInput = document.getElementById("sendSpecificConnectionId");

const sendToGroupForm = document.getElementById("sendToGroupForm");
const groupToSendMessageInput = document.getElementById("groupToSendMessage");
const messageToSendToGroupInput = document.getElementById("messageToSendToGroup");

const joinGroupForm = document.getElementById("joinGroupForm");
const groupToJoinInput = document.getElementById("groupToJoin");

const leaveGroupForm = document.getElementById("leaveGroupForm");
const groupToLeaveInput = document.getElementById("groupToLeave");

const triggerStreamButton = document.getElementById("triggerStreamButton");

const handleReceivedMessage = (message) => {
    const newMessage = document.createElement("li");
    newMessage.innerText = message;
    messageList.appendChild(newMessage);
};

const handleFormSubmission = (form, inputElement, callback) => {
    form.addEventListener("submit", (event) => {
        event.preventDefault();
        const message = inputElement.value;
        callback(message);
        inputElement.value = "";
    });
};

// SignalR method invocations
const invokeBroadcastMessage = (message) => {
    if (message.includes(';')) {
        var messages = message.split(';');

        var subject = new signalR.Subject();
        connection.send("BroadcastStream", subject).catch(error => console.log(error.toString()));

        messages.forEach(msg => {
            subject.next(msg);
        })
        subject.complete();
    }
    else {
        connection.invoke("BroadcastMessage", message);
    }
};

const invokeSendToCaller = (message) => {
    connection.invoke("SendToCaller", message);
};

const invokeSendToOthers = (message) => {
    connection.invoke("SendToOthers", message);
};

const invokeSendToSpecificUser = (connectionId, message) => {
    connection.invoke("SendToSpecificUser", connectionId, message);
};

const invokeSendToGroup = (groupName, message) => {
    connection.invoke("SendToGroup", groupName, message);
};

const invokeAddUserToGroup = (groupName) => {
    connection.invoke("AddUserToGroup", groupName);
};

const invokeRemoveUserFromGroup = (groupName) => {
    connection.invoke("RemoveUserFromGroup", groupName);
};

// Initialize the SignalR connection
const initializeConnection = async () => {
    try {
        await connection.start();
        console.log("Connected to Hub");
    } catch (error) {
        console.error("Couldn't connect to Hub: ", error);
        setTimeout(initializeConnection, 5000);
    }
};

// Register event listeners
connection.on("RecieveMessage", handleReceivedMessage);

handleFormSubmission(broadcastForm, broadcastMessageInput, invokeBroadcastMessage);
handleFormSubmission(sendSelfForm, sendSelfMessageInput, invokeSendToCaller);
handleFormSubmission(sendOthersForm, sendOthersMessageInput, invokeSendToOthers);
handleFormSubmission(sendSpecificForm, sendSpecificMessageInput, (message) => {
    const connectionId = sendSpecificConnectionIdInput.value;
    invokeSendToSpecificUser(connectionId, message);
});

handleFormSubmission(sendToGroupForm, messageToSendToGroupInput, (message) => {
    const groupName = groupToSendMessageInput.value;
    console.log(message);
    invokeSendToGroup(groupName, message);
});

handleFormSubmission(joinGroupForm, groupToJoinInput, invokeAddUserToGroup);
handleFormSubmission(leaveGroupForm, groupToLeaveInput, invokeRemoveUserFromGroup);

triggerStreamButton.addEventListener("click", () => {
    connection.stream("TriggerStream", 10)
        .subscribe({
            next: (message) => {
                const newMessage = document.createElement("li");
                newMessage.innerText = message;
                streamList.appendChild(newMessage);
            }
        });
});

initializeConnection();
