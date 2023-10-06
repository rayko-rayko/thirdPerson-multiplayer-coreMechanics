using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.Diagnostics;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;
    public Transform cameraTargetFollowPoint;
    
    
    // Camera mode
    public bool is3rdPersonCamera { get; set; }
    
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            // Sets the layer of the local palyers model
            Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            
            // Disable main camera
            Camera.main.gameObject.SetActive(false);

            Debug.Log("Spawned local player");
        }
        else
        {
            // Disable the camera if we are not the local player
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;
            
            // Only 1 audio listener is allowed in the scene so disable remote players audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;
            
            Debug.Log("Spawned remote player");
            
        }
        //Make it easier to tell which player is which.
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (playerRef == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
