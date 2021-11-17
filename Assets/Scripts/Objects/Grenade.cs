using UnityEngine;
using System.Collections;
using System;

public class Grenade : MonoBehaviour
{
    [SerializeField] string _name;
    [SerializeField] string _description;
    [SerializeField] int _damage = 4;
    [SerializeField] int _radius = 10;
    [SerializeField] int _uses = 2;
    [SerializeField] GameObject _detonationFXPrefab;
    [SerializeField] AudioClip _detonationAudioClip;

    public string Name { get { return _name; } }
    public string Description { get { return _description; } }
    public int Damage { get { return _damage; } set { _damage = value; } }
    public int Radius { get { return _radius; } set { _radius = value; } }
    public int Uses { get { return _uses; } }
    public GameObject DetonationFXPrefab { get { return _detonationFXPrefab; } set { _detonationFXPrefab = value; } }
    public AudioClip DetonationAudioClip { get { return _detonationAudioClip; } set { _detonationAudioClip = value; } }

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
        Instantiate(_detonationFXPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_detonationAudioClip, transform.position);
        OnDetonate();
        Destroy(this.gameObject);
    }

    public void Throw()
    {        
        _uses--;
    }
}
