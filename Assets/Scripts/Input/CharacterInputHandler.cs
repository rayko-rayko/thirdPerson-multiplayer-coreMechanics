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
    public Vector2 moveInputVector = Vector2.zero;
    private Vector2 viewInputVector = Vector2.zero;
    public bool _isJumpPressed = false;
    public bool _isRunPressed = false;
    public bool _isChangeCameraPressed = false;

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
        moveInputVector = characterInputActions.Controller.Movement.ReadValue<Vector2>();
        
        // View input
        viewInputVector.x = characterInputActions.Controller.Look.ReadValue<Vector2>().x;
        viewInputVector.y = characterInputActions.Controller.Look.ReadValue<Vector2>().y * -1;
        
        // Jump input
        _isJumpPressed = characterInputActions.Controller.Jump.triggered;
        
        // Run input
        _isRunPressed = characterInputActions.Controller.Run.IsPressed();
        
        // Change Camera input
        _isChangeCameraPressed = characterInputActions.Controller.Camera.triggered;
        
        // Set view input
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
        networkInputData.Buttons.Set(NetworkInputData.BUTTON_RUN, characterInputActions.Controller.Run.IsPressed());
        
        // Change Camera data
        networkInputData.Buttons.Set(NetworkInputData.BUTTON_CHANGE_CAMERA, characterInputActions.Controller.Camera.triggered);
        
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
