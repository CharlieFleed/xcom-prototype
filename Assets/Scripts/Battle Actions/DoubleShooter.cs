using UnityEngine;
using System.Collections;

public class DoubleShooter : Shooter
{
    public override string ActionName { get { return _weapon.Name + " x2"; } }
    public override string ConfirmText { get { return "Fire " + _weapon.Name + " x2"; } }

    protected override void Shoot()
    {
        if (_targets.Count > 0 && _targets.Peek().Available)
        {
            InvokeActionConfirmed(this);
            ShotStats _shotStats = _targets.Peek();
            HideTargets();
            InvokeTargetingEnd();
            BattleEventShot shot1 = new BattleEventShot(this, _shotStats);
            MatchManager.Instance.AddBattleEvent(shot1, true);
            BattleEventShot shot2 = new BattleEventConditionalShot(this, _shotStats);
            MatchManager.Instance.AddBattleEvent(shot2, true);
            Deactivate();
            InvokeActionComplete(this);
        }
    }
}
