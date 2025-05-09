
// create hub connection
var connection = new signalR.HubConnectionBuilder().withUrl(signalRBaseAddress+"/hubs/clock").build();

// perform stuff here before getting connected (e.g.: deactivate stuff...)
//...

// create event handlers
connection.on("ShowTime", function (message) {
    document.getElementById("time").innerHTML = new Date(message).toLocaleString();
});

// start the connection
connection.start().then(function () {
    //perform stuff here on connected
    console.log("connection started!");
}).catch(function (err) {
    //on error
    return console.error(err.toString());
});