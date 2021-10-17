using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePlayerMarker : MonoBehaviour
{
    Character _character;
    Walker _walker;
    [SerializeField] GameObject _marker;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
        _walker = GetComponentInParent<Walker>();
    }

    // Update is called once per frame
    void Update()
    {
        _marker.SetActive(_character.IsActive && !_walker.IsWalking);
        transform.rotation = Quaternion.identity;
    }
}
