using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.UI;

public class SessionInfoListUIItem : MonoBehaviour
{
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinButton;
    
    private SessionInfo _sessionInfo;
    
    //Events
    public event Action<SessionInfo> OnJoinSession;

    public void SetInformation(SessionInfo sessionInfo)
    {
        this._sessionInfo = sessionInfo;
        sessionNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";

        bool isJoinButtonActive = !(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers);
        
        joinButton.gameObject.SetActive(isJoinButtonActive);
    }

    public void OnClick()
    {
        Debug.Log("onclick");
        // Invoke the join session event
        OnJoinSession?.Invoke(_sessionInfo);
    }
}
