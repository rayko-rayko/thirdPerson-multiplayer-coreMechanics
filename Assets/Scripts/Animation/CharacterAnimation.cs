using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CharacterAnimation : NetworkBehaviour
{
    private bool isWalking;
    private bool isJumping;

    private bool initialized;
    
    // Other components
    [Header("Animation")] 
    public Animator characterAnimator;
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private CharacterMovementHandler _characterMovementHandler;

    #region Animation
    
    [SerializeField] private float animationSmoothTime = 0.2f;

    private int moveXAnimationParameterId, moveZAnimationParameterId, speedYAnimationParameterId;

    Vector3 currentAnimationBlendVector, animationVelocity;

    #endregion

    private void Awake()
    {
        characterAnimator = GetComponentInChildren<Animator>();
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();

        isWalking = false;
        initialized = false;
    }

    public void Initialized(Animator animator)
    {
        this.characterAnimator = animator;
        this.characterAnimator.applyRootMotion = false;
        initialized = true;
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
    
    
    // HATIRLA
    // void PlayerRun()
    // {
    //     _isStartRun = _playerInput.Input.Run.triggered;
    //     if (_isMovementPressed && _isRunPressed)
    //     {
    //         runSpeed = Mathf.Lerp(accelerationMax, accelerationMin, 1f*Time.deltaTime);
    //         float targetY = 2f;
    //         currentAnimationBlendVector.y = Mathf.SmoothDamp(currentAnimationBlendVector.y, targetY, ref animationVelocity.y, animationSmoothTime);
    //     }
    //     else
    //     {
    //         runSpeed = accelerationMin;
    //         float targetY = 0f;
    //         currentAnimationBlendVector.y = Mathf.SmoothDamp(currentAnimationBlendVector.y, targetY, ref animationVelocity.y, animationSmoothTime);
    //     }
    // }
    // void PlayerAnimation()
    // {
    //     currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, _inputMovement, ref animationVelocity, animationSmoothTime);
    //     
    //     _playerAnimator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x);
    //     _playerAnimator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVector.y);
    //     _playerAnimator.SetFloat(speedYAnimationParameterId, speedY);
    // }

    void Animation()
    {
        if (!initialized) return;

        if (isWalking != _characterMovementHandler.isMoving)
        {
            isWalking = _characterMovementHandler.isMoving;
        }
        
        Vector3 velocityVector = new Vector3(_networkCharacterControllerPrototypeCustom.Velocity.x, _networkCharacterControllerPrototypeCustom.Velocity.y, _networkCharacterControllerPrototypeCustom.Velocity.z);
        velocityVector.Normalize();
        
        currentAnimationBlendVector = Vector3.SmoothDamp(currentAnimationBlendVector, velocityVector, ref animationVelocity, animationSmoothTime);

        if (_characterMovementHandler.isMoving) { SetPlayerMovement(currentAnimationBlendVector.x, currentAnimationBlendVector.z); }

        if (_characterMovementHandler.isJumping) { SetPlayerJump(currentAnimationBlendVector.y); }
    }

    void SetPlayerMovement(float moveX, float moveZ)
    {
        characterAnimator.SetFloat(moveXAnimationParameterId, moveX);
        characterAnimator.SetFloat(moveZAnimationParameterId, moveZ);
    }
    
    void SetPlayerJump(float speedY)
    {
        characterAnimator.SetFloat(speedYAnimationParameterId, _networkCharacterControllerPrototypeCustom.moveVelocityY);
    }
}
