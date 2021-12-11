using UnityEngine;
using System.Collections;
using System;

public abstract class BattleEvent
{
    public bool _ended;

    public virtual void Run()
    { }

    public virtual void End()
    {
        _ended = true;
    }
}
