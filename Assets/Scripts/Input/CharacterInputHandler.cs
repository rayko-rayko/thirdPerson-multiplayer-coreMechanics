using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CharacterInputHandler : MonoBehaviour
{
    public Vector2 moveInputVector { get; set; }
    private Vector2 viewInputVector = Vector2.zero;
    public bool isJumpPressed = false;
    public bool isRunPressed = false;
    public bool isChangeCameraPressed = false;
    public bool isFirePressed = false; 

    // Other components
    public static CharacterInputActions characterInputActions;
    private LocalCameraHandler _localCameraHandler;
    private CharacterMovementHandler _characterMovementHandler;

    private void Awake()
    {
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
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
        if (!_characterMovementHandler.Object.HasInputAuthority)
        {
            Debug.Log($"characterinputhandler : hasinputauthority  false {transform.name}");
            return;
        }
        
        // Move input
        moveInputVector = characterInputActions.Controller.Movement.ReadValue<Vector2>();
        
        // View input
        viewInputVector.x = characterInputActions.Controller.Look.ReadValue<Vector2>().x;
        viewInputVector.y = characterInputActions.Controller.Look.ReadValue<Vector2>().y * -1;
        
        // Jump input
        isJumpPressed = characterInputActions.Controller.Jump.triggered;
        
        // Run input
        isRunPressed = characterInputActions.Controller.Run.IsPressed();
        
        // Change Camera input
        isChangeCameraPressed = characterInputActions.Controller.Camera.triggered;
        
        // Fire input
        isFirePressed = characterInputActions.Controller.Shoot.triggered;
        
        // Set view input
        _localCameraHandler.SetViewInputVector(viewInputVector);
        
        Debug.Log($"{moveInputVector} --> moveinputvector, {isJumpPressed} --> isjump from {gameObject.name}");
    }
    
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        // Move data
        networkInputData.movementInput = moveInputVector;
        
        // Aim data
        networkInputData.aimForwardVector = _localCameraHandler.transform.forward;
        
        // Jump data
        networkInputData.isJumpPressed = isJumpPressed;
        
        // Run data
        networkInputData.Buttons.Set(NetworkInputData.BUTTON_RUN, characterInputActions.Controller.Run.IsPressed());
        
        // Fire data
        networkInputData.isFirePressed = characterInputActions.Controller.Shoot.triggered;
        
        // Reset variable
        isJumpPressed = false;
        isRunPressed = false;
        isFirePressed = false;
        
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
