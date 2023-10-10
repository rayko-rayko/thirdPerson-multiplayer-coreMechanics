using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class CharacterMovementHandler : NetworkBehaviour
{
    private bool _isRespawnRequested = false;
    
    [SerializeField] private Camera _camera;
    public bool isJumping { get; private set; }
    [Networked] public bool isMoving { get; private set; }
    [Networked] public bool isRuninng { get; private set; }
    
    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private HPHandler _hpHandler;

    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _hpHandler = GetComponent<HPHandler>();
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
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            _networkCharacterControllerPrototypeCustom.Move(moveDirection);

            if (networkInputData.movementInput != Vector2.zero) isMoving = true;
            else isMoving = false;
            networkInputData.isMovementPressed = isMoving;
            
            // -------------------------------Jump-----------------------------------------
            if (networkInputData.isJumpPressed) { _networkCharacterControllerPrototypeCustom.Jump(); }
            
            // --------------------------------Run-----------------------------------------
            if (networkInputData.IsDown(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 8; isRuninng = true; }
            else if (networkInputData.IsUp(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 3; isRuninng = false; }
            
            // ----------------------------Change-Camera------------------------------------
            if (networkInputData.IsDown(NetworkInputData.BUTTON_CHANGE_CAMERA)) { NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera; }
            
            /* // -------------------------------Animation-------------------------------------
            characterAnimator.SetFloat("MoveX", _characterInputHandler.moveInputVector.x);
            characterAnimator.SetFloat("MoveZ", _characterInputHandler.moveInputVector.y);
            characterAnimator.SetFloat("SpeedY", _networkCharacterControllerPrototypeCustom.moveVelocityY.y);
            */
            
            // Check if we have fallen off the world
            CheckFallRespawn();
        }
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