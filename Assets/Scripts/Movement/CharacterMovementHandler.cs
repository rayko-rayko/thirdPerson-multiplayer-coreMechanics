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
    private Vector2 viewInput;
    
    // Rotation

    private float _cameraRotationX = 0;
    
    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private Camera _localCamera;

    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _localCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        _cameraRotationX += viewInput.y * Time.deltaTime * _networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);

        _localCamera.transform.localRotation = Quaternion.Euler(_cameraRotationX, 0, 0);
    }

    public override void FixedUpdateNetwork()
    {
        // Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            // ----------------------------Rotate the view---------------------------------
            // Rotate the view
            _networkCharacterControllerPrototypeCustom.Rotate(networkInputData.rotationInput);

            
            // -------------------------------Move-----------------------------------------
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();
            _networkCharacterControllerPrototypeCustom.Move(moveDirection);
            
            
            // -------------------------------Jump-----------------------------------------
            if (networkInputData.isJumpPressed)
            {
                _networkCharacterControllerPrototypeCustom.Jump();
            }
            
            // -------------------------------Run-----------------------------------------
            if (networkInputData.isRunPressed)
            {
                _networkCharacterControllerPrototypeCustom.maxSpeed = 10;
            }
            else if (!networkInputData.isRunPressed)
            {
                _networkCharacterControllerPrototypeCustom.maxSpeed = 2;
            }

            
        }
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }
}