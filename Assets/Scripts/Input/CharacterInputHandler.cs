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
    private bool _isChangeCameraPressed;

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
        ChangeCamera();
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
        Debug.Log("inputhandler "+ _isRunPressed);
        
        // Set view input
        _localCameraHandler.SetViewInputVector(viewInputVector);
    }
    void ChangeCamera()
    {
        // Change Camera input
        _isChangeCameraPressed = characterInputActions.Controller.Camera.triggered;
        if (_isChangeCameraPressed)
        {
            NetworkPlayer.Local.is3rdPersonCamera = !NetworkPlayer.Local.is3rdPersonCamera;
        }
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
