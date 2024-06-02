using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
#nullable enable

[Serializable]
public class ResponseData
{
    public PlayerState opposite_state;
    public GameChoice newChoice;
    public bool dirty;
    public string opposite_name = "-";
    public string opposite_skin = "";
}

public class RemotePlayer : MonoBehaviour
{

    PlayerSideController playerSideController;

    void Start()
    {
        playerSideController = GetComponent<PlayerSideController>();
        StartCoroutine(UpdateLoop());
    }

    public void UpdateRemotePlayerState(ResponseData remoteData)
    {

        if (remoteData.dirty){
            playerSideController.SetChoice(remoteData.newChoice);
        }

        if (playerSideController.characterSelector.selectedCharacter != remoteData.opposite_skin && remoteData.opposite_skin != ""){
            playerSideController.SetPlayerCharacter(remoteData.opposite_skin);
        }

        playerSideController.SetPlayerName(remoteData.opposite_name);

    }

    private IEnumerator GetOppositeState(string lobbyCode, string myUsername)
    {
        UriBuilder baseUri = new UriBuilder($"https://rps.ph3.ru/api/v1/game/lobby/code/{lobbyCode}/game-state/");
        var query = HttpUtility.ParseQueryString(baseUri.Query);
        query["player_name"] = myUsername;
        query["player_skin"] = GlobalState.LeftPlayerInfo.SelectedCharSkin;
        baseUri.Query = query.ToString();
        Uri finalUri = baseUri.Uri;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(finalUri))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);
                ResponseData data = JsonUtility.FromJson<ResponseData>(webRequest.downloadHandler.text);
                UpdateRemotePlayerState(data);
            }
        }
    }

    private IEnumerator UpdateLoop()
    {
        while( true )
        {
            yield return new WaitForSeconds( 1 );
            yield return GetOppositeState( GlobalState.RightPlayerInfo.SelectedPlayerName, GlobalState.LeftPlayerInfo.SelectedPlayerName );
        }
    }
}
