using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    private Camera _localCamera;
    public Transform cameraAnchorPoint;

    // Input
    private Vector2 viewInput;
    
    // Rotation
    private float _cameraRotationX = 0;
    private float _cameraRotationY = 0;
    
    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    
    private void Awake()
    {
        _localCamera = GetComponent<Camera>();
        _networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }

    private void Start()
    {
        // Detach camera if enabled
        if (_localCamera.enabled)
        {
            _localCamera.transform.parent = null;
        }
    }

    private void LateUpdate()
    {
        if (cameraAnchorPoint == null) return;

        if (!_localCamera.enabled) return;
        
        // Move the camera to the position of the player
        _localCamera.transform.position = cameraAnchorPoint.position;
        
        // Calculate rotation
        _cameraRotationX += viewInput.y * Time.deltaTime * _networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);

        _cameraRotationY += viewInput.x * Time.deltaTime * _networkCharacterControllerPrototypeCustom.rotationSpeed;
        
        // Apply Rotation
        _localCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0);
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }
}
