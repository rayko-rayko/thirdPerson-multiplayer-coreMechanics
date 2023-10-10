using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get;  set; }

    public ParticleSystem fireParticileSystem;
    public Transform aimPoint;
    public LayerMask collisionLayers;
    
    private float _lastTimeFired = 0;
    
    // Other components
    private HPHandler _hpHandler;

    private void Awake()
    {
        _hpHandler = GetComponent<HPHandler>();
    }

    public override void FixedUpdateNetwork()
    {
        if (_hpHandler.isDead) { return; }
        
        // Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isFirePressed && Object.HasStateAuthority) { Fire(networkInputData.aimForwardVector);}
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        // Limit fire rate
        if (Time.time - _lastTimeFired < .15f) {return;}

        StartCoroutine(FireEffectCO());
        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitInfo, collisionLayers, HitOptions.IncludePhysX);

        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if (hitInfo.Distance > 0) { hitDistance = hitInfo.Distance; }

        if (hitInfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitInfo.Hitbox.transform.root.name}");
            
            if (Object.HasStateAuthority) { hitInfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(); }
            isHitOtherPlayer = true;
        }
        else if (hitInfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitInfo.Collider.transform.name}");
        }

        if ( isHitOtherPlayer) { Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1); }
        else { Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1); }
        
        _lastTimeFired = Time.time;
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;
        fireParticileSystem.Play();
        
        yield return new WaitForSeconds(0.1f);
        isFiring = false;
    }
    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        bool isFiringCurrent = changed.Behaviour.isFiring;
        changed.LoadOld(); // Load the old value

        bool isFiringOld = changed.Behaviour.isFiring;
        
        if(isFiringCurrent && !isFiringOld) { changed.Behaviour.OnFireRemote();}
    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority) { fireParticileSystem.Play(); }
    }
}
