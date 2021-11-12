using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePlayerMarker : MonoBehaviour
{
    Unit _unit;
    Walker _walker;
    [SerializeField] GameObject _marker;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _walker = GetComponentInParent<Walker>();
    }

    // Update is called once per frame
    void Update()
    {
        _marker.SetActive(_unit.IsActive && !_walker.IsWalking);
        transform.rotation = Quaternion.identity;
    }
}
