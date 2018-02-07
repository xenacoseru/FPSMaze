using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class JSONParser{

    public string RegisterDataToJson(string username, string password, string email)
    {
        return string.Format(@"{{""username"":""{0}"",""password"":""{1}"",""email"":""{2}""}}", username, password, email);
    }

    public string LoginDataToJson(string username, string password)
    {
        return string.Format(@"{{""username"":""{0}"",""password"":""{1}""}}", username, password);
    }

    public string LoginUserAndUrlPhotoToJSON(string username,string url)
    {
        return string.Format(@"{{""username"":""{0}"",""photourl"":""{1}""}}", username, url);
    }

    public string[] ElementFromJsonToString(string target)
    {
        string[] newString = Regex.Split(target, "\"");
        return newString;
    }

    public string NewFriendPackage(string myusername,string posibleFriend)
    {
        return string.Format(@"{{""username"":""{0}"",""myfriend"":""{1}""}}", myusername, posibleFriend);
    }

  

    public string ChangePassword(string username, string currentPassword, string newPassword)
    {
        return string.Format(@"{{""username"":""{0}"",""currentPassword"":""{1}"",""newPassword"":""{2}""}}", username, currentPassword, newPassword);
    }


    public string MessageToPersonToJson(string message, string destination)
    {
        return string.Format(@"{{""message"":""{0}"",""destination"":""{1}""}}", message, destination);
    }
}
