using UnityEngine;
using System.Collections;
using HSM;

public class ConditionDoISeeAnEnemy : Condition
{
    Viewer _viewer;

    public ConditionDoISeeAnEnemy(Viewer viewer) : base()
    {
        _viewer = viewer;
    }

    public override bool Test()
    {
        return _viewer.SeeList.Count > 0;
    }
}
