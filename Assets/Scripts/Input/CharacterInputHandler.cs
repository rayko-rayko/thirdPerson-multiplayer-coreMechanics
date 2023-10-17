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
        _characterMovementHandler = GetComponentInChildren<CharacterMovementHandler>();
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
    }
    
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData
        {
            // Move data
            movementInput = moveInputVector,
            // Aim data
            aimForwardVector = _localCameraHandler.transform.forward,
            // Jump data
            isJumpPressed = isJumpPressed,
            // Fire data
            isFirePressed = characterInputActions.Controller.Shoot.triggered
        };

        // Run data
        networkInputData.Buttons.Set(NetworkInputData.BUTTON_RUN, characterInputActions.Controller.Run.IsPressed());
        
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
    // private void OnDisable()
    // {
    //     characterInputActions.Disable();
    // }
}


/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    private Vector2 _moveInputVector = Vector2.zero;
    private Vector2 _viewInputVector = Vector2.zero;

    private bool _isJumpButtonPressed = false;
    private bool _isFireButtonPressed = false;
    private bool _isGrenadeFireButtonPressed = false;
    private bool _isFireRocketButtonPressed = false;
    
    // Other Components
    // private CharacterMovementHandler _characterMovementHandler;
    private LocalCameraHandler _localCameraHandler;
    private CharacterMovementHandler _characterMovementHandler;

    private void Awake()
    {
        // _characterMovementHandler = GetComponent<CharacterMovementHandler>();
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        _characterMovementHandler = GetComponentInChildren<CharacterMovementHandler>();

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_characterMovementHandler.Object.HasInputAuthority)
        {
            Debug.Log($"characterinputhandler : hasinputauthority  false {transform.name}");
            return;
        }
        
        //View Input
        _viewInputVector.x = Input.GetAxis("Mouse X");
        _viewInputVector.y = Input.GetAxis("Mouse Y") * -1; // Invert the mouse look

        // _characterMovementHandler.SetViewInputVector(_viewInputVector);
        
        // Move Input
        _moveInputVector.x = Input.GetAxis("Horizontal");
        _moveInputVector.y = Input.GetAxis("Vertical");
        
        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            _isJumpButtonPressed = true;
        }
        
        // Fire
        if (Input.GetButtonDown("Fire1"))
        {
            _isFireButtonPressed = true;
        }
        
        // Grenade Throw
        if (Input.GetKeyDown(KeyCode.G))
        {
            _isGrenadeFireButtonPressed = true;
        }
        
        // Rocket Fire
        if (Input.GetKeyDown(KeyCode.F))
        {
            _isFireRocketButtonPressed = true;
        }
        
        // Set view
        _localCameraHandler.SetViewInputVector(_viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();
        
        // Aim Data
        networkInputData.aimForwardVector = _localCameraHandler.transform.forward;
        
        // Move Data
        networkInputData.movementInput = _moveInputVector;
        
        // Jump Data
        networkInputData.isJumpPressed = _isJumpButtonPressed;
        
        // Fire Data
        networkInputData.isFirePressed = _isFireButtonPressed;
        //
        // // Grenade Fire Date
        // networkInputData.isGrenadeFireButtonPressed = _isGrenadeFireButtonPressed;
        //
        // // Rocket Fire Data
        // networkInputData.isRocketFireButtonPressed = _isFireRocketButtonPressed;

        // Reset variables now that we have read their states
        _isJumpButtonPressed = false;
        _isFireButtonPressed = false;
        _isGrenadeFireButtonPressed = false;
        _isFireRocketButtonPressed = false;
        
        return networkInputData;
    }
    
}
*/