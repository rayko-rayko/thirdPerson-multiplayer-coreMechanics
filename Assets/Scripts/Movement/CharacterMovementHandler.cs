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
    [SerializeField] private Camera _camera;
    public bool isJumping { get; private set; }
    [Networked] public bool isMoving { get; private set; }
    [Networked] public bool isRuninng { get; private set; }
    
    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;

    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
    }

    public override void FixedUpdateNetwork()
    {
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
            
            // -------------------------------Jump-----------------------------------------
            if (networkInputData.isJumpPressed) { _networkCharacterControllerPrototypeCustom.Jump(); }
            
            // -------------------------------Run------------------------------------------
            if (networkInputData.IsDown(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 10; isRuninng = true; }
            else if (networkInputData.IsUp(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 2; isRuninng = false; }
            
            //change camera
            if (networkInputData.IsDown(NetworkInputData.BUTTON_CHANGE_CAMERA)) { NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera; }
            // else if (networkInputData.IsUp(NetworkInputData.BUTTON_CHANGE_CAMERA)) { NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera; }
            
            
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
            transform.position = Utils.GetRandomSpawnPoint();
        }
    }
}