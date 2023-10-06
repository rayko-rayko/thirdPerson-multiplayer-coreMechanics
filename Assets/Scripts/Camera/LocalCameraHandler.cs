using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine;

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
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    
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
        
        // Find the cinemachine virtual camera if we haven't already
        if (_cinemachineVirtualCamera == null)
        {
            _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        else
        {
            if (NetworkPlayer.Local.is3rdPersonCamera)
            {
                if (!_cinemachineVirtualCamera.enabled)
                {
                    _cinemachineVirtualCamera.Follow = NetworkPlayer.Local.cameraTargetFollowPoint;
                    _cinemachineVirtualCamera.LookAt = NetworkPlayer.Local.cameraTargetFollowPoint;
                    _cinemachineVirtualCamera.enabled = true;
                    
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("Default"));
                }
                // Let the camerae be handled by cinemachine
                return;
            }
            else
            {
                if (_cinemachineVirtualCamera.enabled)
                {
                    _cinemachineVirtualCamera.enabled = false;
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
                }
            }
        }
        
        // Move the camera to the position of the player
        _localCamera.transform.position = cameraAnchorPoint.position;
        
        // Calculate rotation
        _cameraRotationX += viewInput.y * Time.deltaTime * _networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -75, 75);
        
        _cameraRotationY += viewInput.x * Time.deltaTime * _networkCharacterControllerPrototypeCustom.rotationSpeed;
        
        // Apply Rotation
        _localCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0);
    }
    
    

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }
}
