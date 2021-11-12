using UnityEngine;
using System.Collections;
using Mirror;

public class DoubleShooter : Shooter
{
    public override string ActionName { get { return _weapon.Name + " x2"; } }
    public override string ConfirmText { get { return "Fire " + _weapon.Name + " x2"; } }

    protected override void Shoot()
    {
        if (_targets.Count > 0 && _targets.Peek().Available)
        {
            HideTargets();
            InvokeTargetingEnd();
            CmdShoot(_targets.Peek().Target.gameObject);
        }
    }

    [ClientRpc]
    protected override void RpcShoot(GameObject target)
    {
        Debug.Log("DoubleShooter RpcShoot");
        GetTargets();
        ShotStats shotStats = null;
        foreach (var shot in _targets)
        {
            if (shot.Target == target.GetComponent<GridEntity>())
            {
                shotStats = shot;
            }
        }
        Debug.Log($"Double Shooter Shoot {shotStats.Target.name}");
        BattleEventShot shotEvent1 = new BattleEventShot(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shotEvent1, true);
        BattleEventConditionalShot shotEvent2 = new BattleEventConditionalShot(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shotEvent2, true);
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        GetTargets();
        Available &= HasAvailableTargets();
        Available &= _weapon.Bullets > 1;
    }
}
