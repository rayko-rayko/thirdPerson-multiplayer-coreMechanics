using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    private bool _isRespawnRequested = false;
    public bool isJumping { get; private set; }
    [Networked] public bool isMoving { get; private set; }
    [Networked] public Vector3 movementDirection { get; private set; }
    [Networked] public bool isRuninng { get; private set; }

    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private CharacterInputHandler _characterInputHandler;
    private HPHandler _hpHandler;
    
    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _characterInputHandler = GetComponent<CharacterInputHandler>();
        _hpHandler = GetComponent<HPHandler>();
    }

    private void Update()
    {
        ChangeCamera();
    }

    void ChangeCamera()
    {
        if (_characterInputHandler.isChangeCameraPressed)
        {
            NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera;
            if (NetworkPlayer.Local.is3rdPersonCamera)
            {
                NetworkPlayer.Local.isFPSCamera = false;
            }
            else if (!NetworkPlayer.Local.is3rdPersonCamera)
            {
                NetworkPlayer.Local.isFPSCamera = true;
            }
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (_isRespawnRequested)
            {
                Respawn();
                return;
            }
            if (_hpHandler.isDead) { return;}
        }

        Vector3 direction;
        // Get the input from the network
        
        if (GetInput(out NetworkInputData networkInputData))
        {
            // ---------Rotate the transform according to the client aim vector------------
            transform.forward = networkInputData.aimForwardVector;
            
            // Cancel out rotation on X axis as we don't want our character to tilt
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;
            
            // -------------------------------Move-----------------------------------------
            
            direction = default;
            direction = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            direction.Normalize();

            movementDirection = direction; 
            _networkCharacterControllerPrototypeCustom.Move(movementDirection);
            
            // -------------------------------Jump-----------------------------------------
            if (networkInputData.isJumpPressed) { _networkCharacterControllerPrototypeCustom.Jump(); isJumping = true; }
            
            // --------------------------------Run-----------------------------------------
            if (networkInputData.IsDown(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 8; isRuninng = true; }
            else if (networkInputData.IsUp(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 3; isRuninng = false; }
            
            // Check if we have fallen off the world
            CheckFallRespawn();
        }
        
        if (networkInputData.movementInput == Vector2.zero) { isMoving = false; }
        else { isMoving = true; }
        networkInputData.isMovementPressed = isMoving;
    }
    
    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");
                Respawn();
            }
        }
    }

    public void RequestRespawn() { _isRespawnRequested = true; }
    void Respawn()
    {
        _networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());
        _hpHandler.OnRespawned();
        _isRespawnRequested = false;
    }
    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        _networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
}