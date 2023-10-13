using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Fusion;
using UnityEngine.Serialization;

public class CharacterAnimation : NetworkBehaviour
{
    private Vector2 _inputMovement;
    
    #region Animation
    
    private Animator _playerAnimator;
    private NetworkMecanimAnimator _mecanimAnimator;
    [SerializeField] private float animationSmoothTime = 0.2f;

    private int moveXAnimationParameterId, moveZAnimationParameterId, speedYAnimationParameterId;
    Vector2 currentAnimationBlendVector, animationVelocity;

    #endregion

    // Other components
    private CharacterInputHandler _characterInputHandler;
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private NetworkInputData _networkInputData;
    private CharacterMovementHandler _characterMovementHandler;
    
    private void Awake()
    {
        _playerAnimator = GetComponentInChildren<Animator>();
        _mecanimAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
        _characterInputHandler = GetComponent<CharacterInputHandler>();
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }
    private void Start()
    {
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");
        speedYAnimationParameterId = Animator.StringToHash("SpeedY");
    }
    private void Update()
    {
       
    }
    public override void FixedUpdateNetwork()
    {
        SetAnimation();
    }
    
    void SetAnimation()
    {
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.movementInput != Vector2.zero && networkInputData.IsDown(NetworkInputData.BUTTON_RUN))
            {
                float targetY = 5f;
                currentAnimationBlendVector.y = Mathf.SmoothDamp(currentAnimationBlendVector.y, targetY, ref animationVelocity.y, animationSmoothTime);
            }
            
            currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, networkInputData.movementInput, ref animationVelocity, animationSmoothTime);
            
            _mecanimAnimator.Animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x);
            _mecanimAnimator.Animator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVector.y);
            
            _playerAnimator.SetFloat(speedYAnimationParameterId, _networkCharacterControllerPrototypeCustom.moveVelocityY.y );
            
            if (_networkCharacterControllerPrototypeCustom.IsGrounded)
            {
                _mecanimAnimator.SetTrigger("Land");
            }
        }
        
            
    }
}
