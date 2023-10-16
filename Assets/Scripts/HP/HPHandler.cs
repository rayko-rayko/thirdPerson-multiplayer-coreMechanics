using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))] private byte _HP { get; set; }
    [Networked(OnChanged = nameof(OnStateChanged))] public bool isDead { get; set; }
    
    private bool _isInitialized = false;
    private const byte _startingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    public GameObject playerModel;
    public GameObject deathGameObjectPrefab;
    
    public bool skipSettingStartValues = false;
    
    // Other components
    private HitboxRoot _hitboxRoot;
    private CharacterMovementHandler _characterMovementHandler;
    private NetworkInGameMessages _networkInGameMessages;
    private NetworkPlayer _networkPlayer;

    private void Awake()
    {
        _hitboxRoot = GetComponent<HitboxRoot>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
        _networkInGameMessages = GetComponent<NetworkInGameMessages>();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }

    private void Start()
    {
        if (!skipSettingStartValues) 
        { _HP = _startingHP; isDead = false; }
        
        _isInitialized = true;
    }

    IEnumerator OnHitCO()
    {
        if (Object.HasInputAuthority) { uiOnHitImage.color = uiOnHitColor;}

        yield return new WaitForSeconds(.2f);
        
        if (Object.HasInputAuthority && !isDead) { uiOnHitImage.color = new Color(0, 0, 0, 0); }
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2f);
        _characterMovementHandler.RequestRespawn();
    }

    // This function only called on the server
    public void OnTakeDamage(string damageCausedByPlayerNickname, byte damage)
    {
        // Only take damage while alive
        if (isDead)
        {
            return;
        }

        if (damage > _HP)
        {
            damage = _HP;
        }
        
        _HP -= damage;
        
        Debug.Log($"{Time.time} {transform.name} took damage got {_HP} left");

        // Player killed
        if (_HP <= 0)
        {
            _networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayerNickname, $"Killed <b>{_networkPlayer.playerNickName.ToString()}<b>");
            
            Debug.Log($"{Time.time} {transform.name} died");

            StartCoroutine(ServerReviveCO());
            
            isDead = true;
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour._HP}");
        
        byte newHP = changed.Behaviour._HP;
        changed.LoadOld();
        byte oldHP = changed.Behaviour._HP;
        
        if (newHP < oldHP) { changed.Behaviour.OnHPReduced(); }
    }
    
    private void OnHPReduced()
    {
        if (!_isInitialized) { return; }
        StartCoroutine(OnHitCO());
    }

    static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged isDead {changed.Behaviour.isDead}");
        
        bool isDeadCurrent = changed.Behaviour.isDead;
        changed.LoadOld();
        bool isDeadOld = changed.Behaviour.isDead;
        
        // Handle on death for the player. Also heck if the player was dead but is now alive in that case revive the player.
        if(isDeadCurrent) { changed.Behaviour.OnDeath();}
        else if (!isDeadCurrent && isDeadOld) { changed.Behaviour.OnRevive();}
    }

    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");
        playerModel.gameObject.SetActive(false);
        _hitboxRoot.HitboxRootActive = false;
        _characterMovementHandler.SetCharacterControllerEnabled(false);
        
        Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");
        
        if (Object.HasInputAuthority) { uiOnHitImage.color =new Color(0,0,0,0);}
        playerModel.gameObject.SetActive(true);
        _hitboxRoot.HitboxRootActive = true;
        _characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        _HP = _startingHP;
        isDead = false;
    }
}
