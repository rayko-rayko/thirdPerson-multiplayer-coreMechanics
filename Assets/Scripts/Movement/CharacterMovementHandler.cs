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
    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private void Awake() { _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>(); }
    
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
            
            // -------------------------------Jump-----------------------------------------
            if (networkInputData.isJumpPressed) { _networkCharacterControllerPrototypeCustom.Jump(); }
            
            // -------------------------------Run------------------------------------------
            if (networkInputData.IsDown(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 10; }
            else if (networkInputData.IsUp(NetworkInputData.BUTTON_RUN)) { _networkCharacterControllerPrototypeCustom.maxSpeed = 2; }

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