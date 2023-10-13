using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.Diagnostics;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    public TextMeshProUGUI playerNickNameTM;
    
    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;
    public Transform cameraTargetFollowPoint;
    
    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> playerNickName { get; set; }
    
    // Remote Client Token Hash
    [Networked] public int token { get; set; }

    private bool _isPublicJoinMessageSent = false;

    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;
    
    // Other components
    private NetworkInGameMessages _networkInGameMessages;
    
    // Camera mode
    public bool is3rdPersonCamera { get; set; } public bool isFPSCamera { get; set; }

    private void Awake()
    {
        _networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }
    private void Start()
    {
        isFPSCamera = true;
        is3rdPersonCamera = false;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            // Sets the layer of the local plyers model
            Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            
            // Disable main camera
            if (Camera.main != null)
            {
                Camera.main.gameObject.SetActive(false);
            }
            
            // Enable 1 audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>(true);
            audioListener.enabled = true;
            
            // Enable the local camera
            localCameraHandler._localCamera.enabled = true;
            
            // Detach camera if enable
            localCameraHandler.transform.parent = null;
            
            // Enable UI for local player 
            localUI.SetActive(true);
            
            RPC_SetNickName(GameManager.instance.playerNickname);

            Debug.Log("Spawned local player");
        }
        else
        {
            // // Disable the camera if we are not the local player
            // Camera localCamera = GetComponentInChildren<Camera>();
            // localCamera.enabled = false;
            
            // Disable the local camera for remote players
            localCameraHandler._localCamera.enabled = false;
            
            // Disable UI for remote player
            localUI.SetActive(false);
            
            // Only 1 audio listener is allowed in the scene so disable remote players audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;
            
            Debug.Log("Spawned remote player");
            
        }
        // Set the player as a player object
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        
        //Make it easier to tell which player is which.
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (Object.HasStateAuthority)
        {
            if (Runner.TryGetPlayerObject(playerRef, out NetworkObject playerLeftNetworkObject))
            {
                if (playerLeftNetworkObject == Object)
                {
                    //_networkInGameMessages.SendInGameRPCMessage(playerNickName.ToString(), "left");
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().playerNickName.ToString(), "left");
                }
            }
        }
        
        if (playerRef == Object.InputAuthority)
        { 
            Runner.Despawn(Object);
        }
    }
    
    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.playerNickName}");
        
        changed.Behaviour.OnNickNameChanged();

    }
    private void OnNickNameChanged()
    {
        Debug.Log($"Nick name changed for player to {playerNickName} for player {gameObject.name}");

        playerNickNameTM.text = playerNickName.ToString();
    }

    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo rpcInfo = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.playerNickName = nickName;

        if (!_isPublicJoinMessageSent)
        {
            _networkInGameMessages.SendInGameRPCMessage(nickName, "joined");

            _isPublicJoinMessageSent = true;
        }
    }

    void OnDestroy()
    {
        // Get rid of the local camera if we get destroyed as a new one will be spawned with the new Network palyer
        if (localCameraHandler != null)
        {
            Destroy(localCameraHandler.gameObject);
        }
    }
}
