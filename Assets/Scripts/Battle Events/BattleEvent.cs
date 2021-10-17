using UnityEngine;
using System.Collections;
using System;

public abstract class BattleEvent
{
    public bool ended;

    public virtual void Run()
    { }

    public virtual void End()
    {
        ended = true;
    }
}
