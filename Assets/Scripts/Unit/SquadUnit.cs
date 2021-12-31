using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadUnit : MonoBehaviour
{
    public Squad Squad { set; get; }

    private void OnDestroy()
    {
        if (Squad != null)
            Squad.Units.Remove(this);
    }
}
