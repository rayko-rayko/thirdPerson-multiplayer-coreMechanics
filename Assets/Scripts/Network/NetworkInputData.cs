using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector3 aimForwardVector;
    public NetworkBool isMovementPressed;
    
    public NetworkButtons Buttons;
    public const int BUTTON_RUN = 0;
    
    public NetworkBool isJumpPressed;
    
    public bool IsUp(int button) { return Buttons.IsSet(button) == false; }
    
    public bool IsDown(int button) { return Buttons.IsSet(button); }
}
