using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Static instance of GameManager so other scripts can access it
    public static GameManager instance = null;

    private byte[] _connectionToken;

    public Vector2 cameraViewRotation = Vector2.zero;
    public string playerNickname = "";

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Check if token is valid, if no get a new one
        if (_connectionToken == null)
        {
            _connectionToken = ConnectionTokenUtils.NewToken();
            Debug.Log($"Player connection token {ConnectionTokenUtils.HashToken(_connectionToken)}");
        }
    }

    public void SetConnectionToken(byte[] connectionToken)
    {
        this._connectionToken = connectionToken;
    }

    public byte[] GetConnectionToken()
    {
        return _connectionToken;
    }
}
