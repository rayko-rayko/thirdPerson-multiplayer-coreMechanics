using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkInGameMessages : NetworkBehaviour
{
    InGameMessageUIHandler _inGameMessageUIHandler;

    public void SendInGameRPCMessage(string userNickName, string message)
    {
        RPC_InGameMessage($"<b>{userNickName}<b> {message}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InGameMessage(string message, RpcInfo rpcInfo = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");

        if (_inGameMessageUIHandler == null)
        {
            _inGameMessageUIHandler = NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessageUIHandler>();
        }
        
        if (_inGameMessageUIHandler != null)
        {
            _inGameMessageUIHandler.ONGameMessageReceived(message);
        }
    }
}
