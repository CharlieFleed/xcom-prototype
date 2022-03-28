using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour, IDescription
{
    [SerializeField] ExplosiveData _data;

    public Damage Damage { get { return _data.Damage; } }
    public int Radius { get { return _data.Radius; } }
    public string Description { get { return _data.Description; } }

    private void Awake()
    {
        GetComponent<Health>().OnDied += HandleExplosive_OnDied;
    }

    private void HandleExplosive_OnDied()
    {
        BattleEventExplosion explosion = new BattleEventExplosion(this);
        NetworkMatchManager.Instance.AddBattleEvent(explosion, true, 2);
    }

    public void Detonate()
    {
        // show detonation FX
        Instantiate(_data.DetonationFXPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_data.DetonationAudioClip, transform.position);
        StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }
}
