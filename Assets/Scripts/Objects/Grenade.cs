using UnityEngine;
using System.Collections;
using System;

public class Grenade : MonoBehaviour
{
    [SerializeField] GrenadeData _grenadeData;
    [SerializeField] int _uses = 2;

    public string Name { get { return _grenadeData.Name; } }
    public string Description { get { return _grenadeData.Description; } }
    public Damage Damage { get { return _grenadeData.Damage; } }
    public int Radius { get { return _grenadeData.Radius; } }
    public int Uses { get { return _uses; } }
    public GameObject DetonationFXPrefab { get { return _grenadeData.DetonationFXPrefab; } }
    public AudioClip DetonationAudioClip { get { return _grenadeData.DetonationAudioClip; } }

    public GrenadeData GrenadeData { get { return _grenadeData; } set { _grenadeData = value; } }

    Vector3 _target;
    public event Action OnDetonate = delegate { };

    public void SetTarget(Vector3 target)
    {
        _target = target;
    }

    private void Update()
    {
        if ((transform.position - _target).magnitude < 0.5f)
        {
            Detonate();
        }
    }

    void Detonate()
    {
        // show detonation FX
        Instantiate(_grenadeData.DetonationFXPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_grenadeData.DetonationAudioClip, transform.position);
        OnDetonate();
        Destroy(this.gameObject);
    }

    public void Throw()
    {        
        _uses--;
    }
}
