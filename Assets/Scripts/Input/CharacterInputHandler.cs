using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class CharacterInputHandler : MonoBehaviour
{
    private Vector2 moveInputVector = Vector2.zero;
    private Vector2 viewInputVector = Vector2.zero;
    private bool _isJumpPressed = false;
    private bool _isRunPressed = false;

    // Other components
    public static CharacterInputActions characterInputActions;
    private LocalCameraHandler _localCameraHandler;

    private void Awake()
    {
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CharacterMovementInput();
    }

    void CharacterMovementInput()
    {
        // Move input
        moveInputVector.x = characterInputActions.Controller.Movement.ReadValue<Vector2>().x;
        moveInputVector.y = characterInputActions.Controller.Movement.ReadValue<Vector2>().y;
        
        // View input
        viewInputVector.x = characterInputActions.Controller.Look.ReadValue<Vector2>().x;
        viewInputVector.y = characterInputActions.Controller.Look.ReadValue<Vector2>().y * -1;
        
        // Jump data
        _isJumpPressed = characterInputActions.Controller.Jump.IsPressed();
        
        // Run data
        _isRunPressed = characterInputActions.Controller.Run.IsPressed();
        
        // Set view
        _localCameraHandler.SetViewInputVector(viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        // Move data
        networkInputData.movementInput = moveInputVector;
        
        // Aim data
        networkInputData.aimForwardVector = _localCameraHandler.transform.forward;
        
        // Jump data
        networkInputData.isJumpPressed = _isJumpPressed;
        
        // Run data
        networkInputData.isRunPressed = _isRunPressed;
        
        // Reset variable
        _isJumpPressed = false;
        _isRunPressed = false;
        
        return networkInputData;
    }

    
    private void OnEnable()
    {
        characterInputActions = new CharacterInputActions();
        characterInputActions.Enable();
    }
    private void OnDisable()
    {
        characterInputActions.Disable();
    }
}
