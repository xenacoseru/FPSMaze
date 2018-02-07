//GLOBAL VARIABLES
var server = require('http').createServer();
var io = require('socket.io')(server);
var fs = require('fs');
var DbManager = require("./Server/DbManager.js");
var dbM = new DbManager();


var allPlayersLogged = {};
var mapNameInGameIdDatabase = {};



io.sockets.on('connection', function(socket){
    console.log('Client connected is '+socket.id);  

	socket.on('register', onRegister);
	socket.on('login', onLogin);
	socket.on('disconnect', onClientDisconnect);
	socket.on('avatarImg',onNewPhoto);
	socket.on('getPhoto', onGetPhoto);
	socket.on('addFriend',onAddFriend);
	socket.on('GetMyFriends',onGetMyFriends);
	socket.on('getPhotoFriend',onGetFriendPhoto);
	socket.on('removeFriend', onRemoveFriend);
	socket.on('newPassword', onNewPassword);
	socket.on('messageGlobalChat', onMessageGlobalChat);
	socket.on('endroomgame', onEndRoomGame);
	socket.on('getMyHistory', onGetHistory);
});

server.listen(process.env.PORT||3000);
console.log("Server started.");

var onClientDisconnect = function(){
	if(allPlayersLogged[this.id]){
        dbM.MakeLoginOnOff(allPlayersLogged[this.id],false);
        delete allPlayersLogged[allPlayersLogged[this.id]];
        delete allPlayersLogged[this.id];
        io.sockets.emit('askfriends');
    }
}
var onRegister = function(data){
	console.log(data);

	var socketRef = this;
    dbM.InserIntoUsers(data["username"],data["password"],data["email"], function(err) {
        if(err)
            console.log(err);
        if(err==="duplicate")
            socketRef.emit("usernameExist");
        else
            socketRef.emit("registerSuccesfull");
    });
}

var checkAlreadyLog = function(name){
    for(var socketId in allPlayersLogged){
        if(allPlayersLogged[socketId] === name)
            return true;
    }
    return false;
}

var onLogin = function(data){
    var socketRef = this;
    if(!checkAlreadyLog(data["username"]))
    {
        dbM.CheckLogin(data["username"],data["password"], function(err,resultrow) {
            if(err)
                console.log(err);

            if(err==="fail")
                socketRef.emit("wrongData");
            else if(err==="succes")
            {
                socketRef.emit("loginSuccesfull",{
                    username : resultrow["username"],
                    email: resultrow["email"],
                    nomatches : resultrow["nomatches"],
                    nomatcheswon: resultrow["nomatcheswon"],
                    photourl : resultrow["photourl"]
                });


                allPlayersLogged[socketRef.id] = data["username"];
                //to do update databse for log
                //send array of connected frinds
                //console.log("Dupa logare idu meu este "+id);
                mapNameInGameIdDatabase[data["username"]] = resultrow["idUser"];
                dbM.MakeLoginOnOff(data["username"],true);

                io.sockets.emit('askfriends');

            }
        });
    }
    else
    {
        socketRef.emit("alreadyLoged");
    }
}

var onNewPhoto = function(data){
	var dataphoto = data.photo.replace(/^data:image\/\w+;base64,/, "");
	var buf = new Buffer(dataphoto, 'base64');
	var path = './Server/Photos/'+data["username"]+".png";
	fs.exists(path, function(exists) {
				  if(exists) {
				    //DELETE AND RECREATE
				    console.log('File exists. Deleting now ...');
				    fs.unlinkSync(path);
				    fs.writeFileSync(path, buf);
				  } 
				  else {
				    //NEW FILE
				    console.log('New phot');
					fs.writeFileSync(path, buf);
				  }
		});
	dbM.SetPathOfPhoto(data["username"],path);
	io.sockets.emit('askfriends');

}

var onGetPhoto = function(data){
	var path = data["photourl"];
	var base64_data_photo = new Buffer(fs.readFileSync(path)).toString('base64');
	this.emit('photobase64',{
		photoBase64:base64_data_photo
	});
}
var onGetFriendPhoto = function(data){
	var path = data["photourl"];
	var base64_data_photo = new Buffer(fs.readFileSync(path)).toString('base64');
	this.emit('friendPhoto',{
		friendName :data["username"],
		photoBase64:base64_data_photo
	});
}
var getSocketIdOfUser = function(name,mySocketId){
    for(var sockId in allPlayersLogged){
        if(allPlayersLogged[sockId] === name && sockId!==mySocketId)
            return sockId;
    }
    return -1;
}

var onAddFriend = function(data){
    var socketId = getSocketIdOfUser(data["myfriend"],this.id);
    if(socketId!==-1){
        console.log("Jucatorul "+data["myfriend"]+" cu "+socketId+" este online");      

        dbM.InsertFriend(mapNameInGameIdDatabase[allPlayersLogged[this.id]], data["myfriend"]);
        this.emit("newFriend",{
            name: allPlayersLogged[socketId]
        });

        io.to(socketId).emit("newFriend",{
            name: allPlayersLogged[this.id]
        });
    }
    else
    {
        console.log("Jucatorul "+data["myfriend"]+ " nu este online ");
        this.emit("playerNotOnline");
    }
}

var onGetMyFriends = function(data){
	var idRow = mapNameInGameIdDatabase[data["username"]];
    var socketREF = this;

    console.log("CE "+idRow);
    dbM.GetListOfFriendById(idRow, function(status,listOfFriends){
        if(status=="noFriends"){
            console.log("No friends for "+idRow);
        }

        if(status=="Friends"){                      
            console.log("Lista prietenilor este :"+ listOfFriends);
        }

        socketREF.emit("listFriends",{            
            friends : listOfFriends
        });

    });
}

var onRemoveFriend = function(data){
	 var socketId = getSocketIdOfUser(data["friendName"],this.id);

    if(socketId!==-1){
        console.log("Jucatorul "+data["friendName"]+" cu "+socketId+" este online");        
        dbM.RemoveFriend(mapNameInGameIdDatabase[allPlayersLogged[this.id]], data["friendName"]);
     
        io.to(socketId).emit("removeFriend",{
            name: allPlayersLogged[this.id]
        });        
    }
    else
    {
        console.log("Jucatorul "+data["friendName"]+ " nu este online ");
        dbM.RemoveFriend(mapNameInGameIdDatabase[allPlayersLogged[this.id]], data["friendName"]);
        io.sockets.emit('askfriends');
    }
}

var onNewPassword = function(data){
	var username = data["username"];
	var currentPassword = data["currentPassword"];
	var newPassword = data["newPassword"];
	console.log(username+" "+currentPassword+" "+newPassword);
	var socketRef = this;
	dbM.UpdateNewPassword(username,currentPassword,newPassword,function(succes){
		if(succes=="noMatch"){
			socketRef.emit("currentPasswordWrong");
		}
		if(succes=="Match"){
			socketRef.emit("passwordChanged");
		}
	})
}

var onMessageGlobalChat = function(data){
    console.log("Jucatorul "+this.id+" a scris "+data["message"] + "lui "+data["destination"]);
    var socketId = getSocketIdOfUser(data["destination"],this.id);
    
    console.log(data["message"]+" "+data["destination"]);
     io.to(socketId).emit("newMessageGlobalChat",{
            socket_id : this.id,
            name :allPlayersLogged[this.id],
            message : data["message"]
        }); 
    
    this.emit("newMessageGlobalChat",{
            socket_id : this.id,
            name : data["destination"],
            message : data["message"]
        });
}


var onEndRoomGame = function(data){
	   console.log(data);
	   dbM.InsertDataIntoHistory(data["MyName"], data["KILLS"], data["DEATHS"],function(err){
	   	 if(err)
            console.log(err);
        else
        	console.log("succes insert on history");
	   });

       io.sockets.emit('askfriends');
}

var onGetHistory = function(data){
	var username = data.username;
	var socketREF = this;
	dbM.GetHistoryListByName(username,function(stats,rows){
		if(stats==="noMatch"){
			console.log("Nothing to send");
		}
		if(stats==="Matches"){
			console.log("Trying to send");
			 socketREF.emit("receiveHistoryList",{            
		            history : rows
		      });
		}
	});
}