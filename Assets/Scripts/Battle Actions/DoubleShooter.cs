using UnityEngine;
using System.Collections;
using Mirror;

public class DoubleShooter : Shooter
{
    public override string ActionName { get { return _weapon.Name + " x2"; } }
    public override string ConfirmText { get { return "Fire " + _weapon.Name + " x2"; } }

    protected override void Shoot()
    {
        if (_shots.Count > 0 && _shots.Peek().Available)
        {
            HideTargets();
            InvokeTargetingEnd();
            CmdShoot(_shots.Peek().Target.gameObject);
        }
    }

    [ClientRpc]
    protected override void RpcShoot(GameObject target)
    {
        Debug.Log("DoubleShooter RpcShoot");
        UpdateShots();
        ShotStats shotStats = null;
        foreach (var shot in _shots)
        {
            if (shot.Target == target.GetComponent<GridEntity>())
            {
                shotStats = shot;
            }
        }
        Debug.Log($"Double Shooter Shoot {shotStats.Target.name}");
        InvokeOnTargetSelected(this, shotStats.Target);
        InvokeOnTargetingEnd();
        BattleEventShot shotEvent1 = new BattleEventShot(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shotEvent1, true, 2);
        BattleEventConditionalShot shotEvent2 = new BattleEventConditionalShot(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shotEvent2, true, 2);
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        Available &= _weapon.Bullets > 1;
    }
}
