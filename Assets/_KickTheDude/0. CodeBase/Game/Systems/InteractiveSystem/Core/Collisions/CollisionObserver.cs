using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class CollisionObserver : MonoBehaviour
{
    public event Action<Collision> CollisionEnter;
    public event Action<Collision> CollisionExit;

    [SerializeField, BoxGroup("PARAMETERS")] private LayerMask collidingLayerMask;
    [SerializeField, BoxGroup("PARAMETERS")] private bool _useCooldown = true;
    [SerializeField, BoxGroup("PARAMETERS")] private float _cooldown = 0.1f;
    [SerializeField, BoxGroup("PARAMETERS")] private bool _storeCatchedColliders = true;
    [SerializeField, BoxGroup("PARAMETERS")] private bool _sendEvents = true;

    [SerializeField, ReadOnly, BoxGroup("DEBUG")] private List<Collider> catchedColliders = new List<Collider>();

    public bool IsCollidingNow { get
        {
            catchedColliders.RemoveAll(item => item == null);
            catchedColliders.RemoveAll(item => item.enabled == false);
            catchedColliders.RemoveAll(item => !item.gameObject.activeInHierarchy);

            if (catchedColliders.Count > 0) return true; else return false;
        }
        private set { }
    }

    private Coroutine cooldown;
    private bool _readyToCatch = true;

    private void OnDisable()
    {
        catchedColliders.Clear();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsReadyToCatchCollision(collision)) return;
        if (IsUsingCooldown) { EnableCooldown(); }

        if (_storeCatchedColliders) catchedColliders.Add(collision.collider);
        if (_sendEvents) NotifyAboutCollisionEnter(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_storeCatchedColliders) catchedColliders.Remove(collision.collider);
        if (_sendEvents) NotifyAboutCollisionExit(collision);
    }

    private bool IsReadyToCatchCollision(Collision collision)
        => !IsOnCooldown && IsCollisionOnAllowedLayer(collision);

    private bool IsUsingCooldown
        => _useCooldown;

    private bool IsOnCooldown
        => !_readyToCatch;

    private bool IsCollisionOnAllowedLayer(Collision collision)
        => ((1 << collision.collider.gameObject.layer) & collidingLayerMask) != 0;

    public void RemoveColliderFromCatchedColliders(Collider collider)
    {
        catchedColliders.Remove(collider);
    }

    private void EnableCooldown()
    {
        if (cooldown != null) StopCoroutine(cooldown);

        cooldown = StartCoroutine(CooldownCatching());
    }

    private IEnumerator CooldownCatching()
    {
        _readyToCatch = false;
        yield return new WaitForSeconds(_cooldown);
        _readyToCatch = true;
    }

    private void NotifyAboutCollisionEnter(Collision collision)
        => CollisionEnter?.Invoke(collision);

    private void NotifyAboutCollisionExit(Collision collision)
        => CollisionExit?.Invoke(collision);
}