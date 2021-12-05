using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
    public Team Team { set; get; }
    public bool Started { private set; get; }

    private void Start()
    {
        Started = true;
    }

    private void OnDestroy()
    {
        if (Team != null)
            Team.RemoveMember(this);
    }
}
