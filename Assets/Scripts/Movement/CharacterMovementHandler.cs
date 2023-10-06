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
    [Header("Animation")] 
    public Animator characterAnimator;
    
    public bool isJumping { get; private set; }
    [Networked] public bool isMoving { get; private set; }
    [Networked] public bool isRuninng { get; private set; }
    
    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private CharacterInputHandler _characterInputHandler;

    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _characterInputHandler = GetComponent<CharacterInputHandler>();
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
            
            // // -------------------------------Animation-------------------------------------
            // Vector2 velocityVector = new Vector3(_characterInputHandler.moveInputVector.x * _networkCharacterControllerPrototypeCustom.maxSpeed, _characterInputHandler.moveInputVector.y * _networkCharacterControllerPrototypeCustom.maxSpeed);
            // velocityVector.Normalize();
            //
            // Debug.Log(_networkCharacterControllerPrototypeCustom.maxSpeed + " velocity vector movement handler ");
            //
            // characterAnimator.SetFloat("MoveX", velocityVector.x);
            // characterAnimator.SetFloat("MoveZ", velocityVector.y);
            // characterAnimator.SetFloat("SpeedY", _networkCharacterControllerPrototypeCustom.moveVelocityY);
            
            
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