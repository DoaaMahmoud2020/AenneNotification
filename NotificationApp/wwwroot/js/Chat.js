﻿//const connection = new signalR.HubConnectionBuilder()
//   // .withUrl("/chatHub")
//    .withUrl("http://doaaberam2020-001-site1.htempurl.com/chatHub")
//    .build();

////This method receive the message and Append to our list
//connection.on("ReceiveMessage", (user, message) => {
//    alert("dodosuccess");
//    const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
//    const encodedMsg = user + " :: " + msg;
//    const li = document.createElement("li");
//    li.textContent = encodedMsg;
//    document.getElementById("messagesList").appendChild(li);
//});

//connection.start().catch(err => console.error(err.toString()));

//Send the message

//document.getElementById("sendMessage").addEventListener("click", event => {
//    const user = document.getElementById("userName").value;
//    const message = document.getElementById("userMessage").value;
//    //connection.invoke("GetMatchedDriver", 5,2,2 ,0,0).catch(err => console.error(err.toString()));
//    connection.invoke("SendMessage", user, message).catch(err => console.error(err.toString()));

//    event.preventDefault();
//});


//const connection = new signalR.HubConnectionBuilder()
//    .withUrl("http://doaaberam2020-001-site1.htempurl.com/tripNotification")
//    .build();
let connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:6715/TripNotification",
        { skipNegotiation: true, transport: signalR.HttpTransportType.WebSockets })
    //.configureLogging(LogLevel.Information)
    .build();
connection.start()
    .then(res => {
        var userID = 78;
        var UserType = 3;
        console.log('connection started assssssss');

        connection.invoke('OnConnectedAsync', userID, UserType);
    })
    .catch(err => {
        console.error(err);
    });
//This method receive the message and Append to our list
connection.on("notifiedcurrentlongandlattfordriver", (Client_Id, Client_Pickup_Long, Client_Pickup_Latt) => { //Driver 
    alert(" Listen Lang:" + Client_Pickup_Long + "Lat:" + Client_Pickup_Latt);
    console.log("lat:" + Client_Pickup_Latt + "Long:" + Client_Pickup_Long);
    //Doaa Mahmoud: driver location , 
    connection.invoke("GetCurrentLongAndLattForDriver", 78, Client_Pickup_Long, Client_Pickup_Latt,27, 31.4035379, 31.0740967).then(res => {
        console.log('connection Resul');
    })
        .catch(err => console.error(err.toString()));
    //const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    //const encodedMsg = user + " :: " + msg;
    //const li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);
});
                                                          
connection.on("NotifiedNearestDriverLongAndLattForDriver", (Client_Id1, Client_Pickup_Long1, Client_Pickup_Latt1) => { //Driver 
   // alert("");
    alert("Nearst  ClientID:"+Client_Id1+" lat:" + Client_Pickup_Latt1 + "Long:" + Client_Pickup_Long1);
   
    //const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    //const encodedMsg = user + " :: " + msg;
    //const li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);
});
//
//connection.start(r=>alert("started")).catch(err => console.error(err.toString()));

//connection.start().then(function () {
//    debugger;
//    alert("started");
//});
//Send the message

document.getElementById("sendMessage").addEventListener("click", event => {
    const user = document.getElementById("userName").value;
    const message = document.getElementById("userMessage").value;
  
  //  [7: 39 PM, 3 / 22 / 2020]
    connection.invoke("getmatcheddriver", 5, 2, 2, 31.393918333333332, 31.059498333333334).catch(err => console.error(err.toString()));
    // connection.invoke("SendMessage", user, message).catch(err => console.error(err.toString()));
    alert("send");
    event.preventDefault();
});