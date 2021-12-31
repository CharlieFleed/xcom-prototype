using UnityEngine;
using System.Collections;
using HSM;

public class DoISeeAnEnemyCondition : Condition
{
    Viewer _viewer;

    public DoISeeAnEnemyCondition(Viewer viewer) : base()
    {
        _viewer = viewer;
    }

    public override bool Test()
    {
        return _viewer.SeeList.Count > 0;
    }
}
