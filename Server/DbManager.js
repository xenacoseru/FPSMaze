var DbManager = function(){

    var self = this;
    this.mysql      = require('mysql');
    this.connection = this.mysql.createConnection({
        host     : 'localhost',
        user     : 'root',
        password : '',
        database : 'TheMaze'
    });

    this.connection.connect();

    this.InserIntoUsers = function(myUsername, myPassword, myEmail, cb){
        this.connection.query('SELECT * from users where username=?',myUsername, function(err, rows, fields) {
            if (err)
                cb(err);

            console.log('The solution is: ', rows);
            if(rows.length==0)
            {
                var myUser = { username: myUsername, password: myPassword, email : myEmail, isOnline : false, nomatches: 0, nomatcheswon: 0, photourl: 'empty' };
                self.connection.query('INSERT INTO users SET ?', myUser, function(err,res){
                    if(err) throw err;
                    succes = true;
                    console.log('Last insert ID:', res.insertId);
                    cb();
                });          
            }
            else
            {
                cb("duplicate");
            }
        });
    }

    this.CheckLogin = function(myUsername, myPassword, cb){
        var myUser = {username: myUsername, password: myPassword};
        this.connection.query('SELECT * from users where username=? and password=?',[myUsername,myPassword], function(err, rows, fields) {
            if (err)
                cb(err);

            console.log('The solution is: ', rows);
            if(rows.length==0)
            {
                console.log("Datele introduse nu exista");
                cb("fail")
            }
            else
            {
                console.log("Good login "+rows[0]["idUser"]);
                cb("succes",rows[0]);
            }
        });
    }

    this.MakeLoginOnOff = function(myUsername, status){
        this.connection.query('UPDATE users SET isOnline = ? WHERE username = ?', [status, myUsername]);
    }
    
    this.InsertFriend = function(myId, nameFriend, cb){
        var _idUser, _idFriend;
        console.log(" Idu meu este "+myId);

        this.GetIdFromUserByName(nameFriend, function(param){
            if(param!=-1){
                _idFriend = param;
                console.log(myId +" "+_idFriend);

                var newRow = { idUser: myId, idFriend : _idFriend };
                var newRow1 = { idUser: _idFriend, idFriend : myId };

                self.connection.query('INSERT INTO friends SET ?', newRow, function(err,res){
                    if(err) throw err;
                    succes = true;
                    console.log('Last insert ID:', res.insertId);
                });

                self.connection.query('INSERT INTO friends SET ?', newRow1, function(err,res){
                    if(err) throw err;
                    succes = true;
                    console.log('Last insert ID:', res.insertId);
                });     
            }
        });        
    } 

    this.RemoveFriend = function(myId, nameFriend, cb){
        var _idUser, _idFriend;
        console.log(" Idu meu este "+myId);
        this.GetIdFromUserByName(nameFriend, function(param){
            if(param!=-1){
                _idFriend = param;
                console.log(myId +" "+_idFriend);

                var newRow = { idUser: myId, idFriend : _idFriend };
                var newRow1 = { idUser: _idFriend, idFriend : myId };

                self.connection.query('DELETE from friends where idUser=? and idFriend=?', [myId,_idFriend], function(err,res){
                    if(err) throw err;
                    succes = true;
                });

                self.connection.query('DELETE from friends where idUser=? and idFriend=?', [_idFriend,myId], function(err,res){
                    if(err) throw err;
                    succes = true;                    
                });     
            }
        });        
    } 
    this.GetListOfFriendById = function(myId, cb){
            //Select distinct idFriend,username,isOnline from users join friends on users.idUser = friends.idFriend where users.idUser In (SELECT idFriend FROM users u natural join friends f where u.idUser = 5) and users.idUser = friends.idFriend ;
            console.log("IDUL este "+myId);
            this.connection.query('Select distinct idFriend,username,isOnline,photourl from themaze.users join themaze.friends on themaze.users.idUser = themaze.friends.idFriend where users.idUser In (SELECT idFriend FROM themaze.users u natural join themaze.friends f where u.idUser = ?) and themaze.users.idUser = themaze.friends.idFriend',myId, function(err, rows, fields) {
                if (err)
                    cb(err);

                console.log('The solution is: ', rows);
                if(rows!==undefined){
                if(rows.length==0)
                {
                    cb("noFriends")
                }
                else
                {                
                    cb("Friends",rows);
                };
            };
        });
    }
    
    this.GetIdFromUserByName = function(nameUser, callback){
        var complete = false;
        this.connection.query('SELECT * from users where username=?',nameUser, function(err, rows, fields) {
            if (err)
            {
                callback(err);
                throw err;
            }
            if(rows.length!=0)
            {
                idUser = rows[0]["idUser"];
                complete = true;
                console.log(idUser);
                callback(idUser);
            }
            else
            {
                idUser = -1;
                callback(-1);
            }           
        });
    }

    this.InsertDataIntoHistory = function(_name, _kills, _deaths, cb){
        var newRowHistory = { myname: _name, kills:_kills, deaths: _deaths };

        self.connection.query('INSERT INTO history SET ?', newRowHistory, function(err){
                if(err) {
                     cb(err);
                throw err;
            }
        });

    }


     this.GetHistoryListByName = function(_name, cb){
        this.connection.query('SELECT * FROM history where MyName = ?',_name, function(err, rows, fields) {
            if (err)
                cb(err);

            console.log('The solution is: ', rows);
            if(rows.length==0)
            {
                cb("noMatches")
            }
            else
            {                
                cb("Matches",rows);
            }
        });
    }

    this.SetPathOfPhoto = function(_username, _photourl){
        this.connection.query('UPDATE users SET photourl = ? WHERE username = ?', [_photourl, _username]);
    }

    this.CheckIfCurrentPasswordIsOk = function(_username,_currentPassword,callback){
        this.connection.query('SELECT * FROM users where username = ? and password = ?', [_username, _currentPassword], function(err,rows,fields){
             if (err){
                callback(err);
                throw err;
            }
            console.log('The solution is: ', rows);
            if(rows.length==0)
            {
                callback("noMatch")
            }
            else
            {                
                callback("Match",rows);
            }
        });
    }

    this.UpdateNewPassword = function(_username,_currentPassword,_newPassword,cb){

        this.CheckIfCurrentPasswordIsOk(_username,_currentPassword,function(checkMatch){
            if(checkMatch=="noMatch"){
                console.log("Password doesnt match");
                cb("noMatch");
            }
            else
            {
                console.log("aici "+_username+" "+_newPassword);
                self.connection.query('UPDATE users SET password = ? WHERE username = ?', [_newPassword, _username] , function(err,res){
                    if(err) throw err;
                    cb("Match");
                });
                
            }
        });
    }
}

module.exports = DbManager;