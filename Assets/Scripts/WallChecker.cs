using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallChecker : MonoBehaviour
{
    public Vector3 HalfExtents = new Vector3(1.2f, 0, 0.4f);
    private void OnDrawGizmos()
    {
        Physics.queriesHitBackfaces = true;
        RaycastHit[] hits = Physics.BoxCastAll(transform.position + (1 - 0.001f) * Vector3.up, HalfExtents, Vector3.down, Quaternion.identity, 2 - 0.002f);
        Physics.queriesHitBackfaces = false;
        if (hits.Length > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
            Debug.Log($"{hits[0].collider.name}");
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}
