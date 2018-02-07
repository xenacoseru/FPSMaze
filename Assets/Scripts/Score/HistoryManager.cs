using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{

    public GameObject historyPrefab;
    private SocketIOComponent SocketIO;
    private Dictionary<string, GameObject> matchList = new Dictionary<string, GameObject>();
    private JSONParser myJsonParser;
    public GameObject contentParent;


    void Awake()
    {
       SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();
       JSONObject temp = new JSONObject();
       temp.AddField("username", UserData.userName);
       SocketIO.Emit("getMyHistory",temp);
       SocketIO.On("receiveHistoryList", OnReceiveHistoryList);
        myJsonParser = new JSONParser();
        
    }

    private void OnReceiveHistoryList(SocketIOEvent obj)
    {
        
        DestroyAllMatchGameObjects();
        matchList = new Dictionary<string, GameObject>();

        JSONObject matches = obj.data.GetField("history");


        for (int i = 0; i < matches.Count; i++)
        {
            string username = myJsonParser.ElementFromJsonToString(matches[i].GetField("myname").ToString())[1];
            string kills = matches[i].GetField("kills").ToString().Replace("\"", "");
            string deaths = matches[i].GetField("deaths").ToString().Replace("\"", "");

            GameObject newMatchHistory = Instantiate(historyPrefab);
            newMatchHistory.transform.Find("Text").GetComponent<Text>().text = "			"+username+"	 K:"+kills+"    D:"+deaths;
            newMatchHistory.transform.SetParent(contentParent.transform, false);
            matchList.Add(username, newMatchHistory);

        }

    }

    private void DestroyAllMatchGameObjects()
    {
        foreach (string matchname in matchList.Keys)
        {
           Destroy(matchList[matchname]);
        }
    }

    private RectTransform rect;

    public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData)
    {
        var pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
        if (pointerData == null) { return; }

        var currentPosition = rect.position;
        currentPosition.x += pointerData.delta.x;
        currentPosition.y += pointerData.delta.y;
        rect.position = currentPosition;
    }
}
