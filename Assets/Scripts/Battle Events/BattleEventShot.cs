using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BattleEventShot : BattleEvent
{
    #region Fields

    protected Shooter _shooter;
    protected ShotStats _shotStats;
    float _waitTimeout = 1.5f;

    protected enum Phase { Camera, Wait, Shoot, Shooting, Shot, Wait2 }
    protected Phase _phase;

    public static event Action<Shooter, GridEntity> OnShooting = delegate { };
    public static event Action<Shooter, GridEntity> OnShootingEnd = delegate { };

    #endregion

    public BattleEventShot(Shooter shooter, ShotStats shotStats) : base()
    {
        _shooter = shooter;
        _shotStats = shotStats;
        _shooter.OnShot += HandleShooter_OnShot;
        _phase = Phase.Camera;
    }

    void HandleShooter_OnShot()
    {
        if (_phase == Phase.Shooting)
            _phase = Phase.Shot;
    }

    public override void Run()
    {
        base.Run();
        switch (_phase)
        {
            case Phase.Camera:
                OnShooting(_shooter, _shotStats.Target);
                _phase = Phase.Wait;
                break;
            case Phase.Wait:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    _phase = Phase.Shoot;
                }
                break;
            case Phase.Shoot:
                _shooter.DoShoot();
                _phase = Phase.Shooting;
                break;
            case Phase.Shooting:
                // wait for OnShot event
                break;
            case Phase.Shot:
                if (NetworkRandomGenerator.Instance.Ready())
                {
                    bool hit = false;
                    bool crit = false;
                    DamageDealer.DealDamage(
                        _shotStats.Target.GetComponent<Health>(),
                        _shotStats.Target.GetComponent<Armor>(),
                        _shooter.Weapon.Damage,
                        _shotStats.HitChance,
                        _shotStats.CritChance,
                        out hit,
                        out crit);
                    
                    if (hit)
                    {
                        // show shoot FX
                        if (_shooter.Weapon.HitFXPrefab)
                            GameObject.Instantiate(_shooter.Weapon.HitFXPrefab, _shotStats.Target.transform.position, Quaternion.identity);                        
                    }
                    NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false, 0);
                    _waitTimeout = 2;
                    _phase = Phase.Wait2;
                }
                break;
            case Phase.Wait2:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    OnShootingEnd(_shooter, _shotStats.Target);
                    End();
                }
                break;
        }
    }

    public override void End()
    {
        base.End();
        //Debug.Log($"End of battle event shot");
        _shooter.OnShot -= HandleShooter_OnShot;
    }
}
