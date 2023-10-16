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
    #region Animation
    
    private NetworkMecanimAnimator _mecanimAnimator;
    [SerializeField] private float animationSmoothTime = 0.2f;

    private int moveXAnimationParameterId, moveZAnimationParameterId, speedYAnimationParameterId;
    Vector2 currentAnimationBlendVector, animationVelocity;

    #endregion

    // Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private NetworkInputData _networkInputData;
    
    private void Awake()
    {
        _mecanimAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
    }
    private void Start()
    {
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");
        speedYAnimationParameterId = Animator.StringToHash("SpeedY");
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
            
            _mecanimAnimator.Animator.SetFloat(speedYAnimationParameterId, _networkCharacterControllerPrototypeCustom.moveVelocityY.y );
            
            if (_networkCharacterControllerPrototypeCustom.IsGrounded)
            {
                _mecanimAnimator.SetTrigger("Land");
            }
        }
        
            
    }
}
