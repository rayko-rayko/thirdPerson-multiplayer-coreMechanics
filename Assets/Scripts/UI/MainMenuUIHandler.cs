using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")] 
    public GameObject playerDetailsPanel;
    public GameObject sessionBrowserPanel;
    public GameObject createSessionPanel;
    public GameObject statusPanel;
    
    [Header("Player settings")]
    public TMP_InputField playerNameInputField;

    [Header("New Game Session")] 
    public TMP_InputField sessionNameInputField;

    private void Start()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
        {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerNickname");
        }
    }

    void HideAllPanels()
    {
        playerDetailsPanel.SetActive(false);
        sessionBrowserPanel.SetActive(false);
        createSessionPanel.SetActive(false);
        statusPanel.SetActive(false);
    }

    public void OnFindGameClicked()
    {
        PlayerPrefs.SetString("PlayerNickname", playerNameInputField.text);
        PlayerPrefs.Save();

        GameManager.instance.playerNickname = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        
        networkRunnerHandler.OnJoinLobby();
        
        HideAllPanels();
        sessionBrowserPanel.SetActive(true);
        
        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();
    }

    public void OnCreateNewGameClicked()
    {
        HideAllPanels();
        
        createSessionPanel.SetActive(true);
    }

    public void OnStartNewSessionClicked()
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.CreateGame(sessionNameInputField.text, "MainScene");
        
        HideAllPanels();
        statusPanel.gameObject.SetActive(true);
    }
    
    public void OnJoiningServer()
    {
        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }
}