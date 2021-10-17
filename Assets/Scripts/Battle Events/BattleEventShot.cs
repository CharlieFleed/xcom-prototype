using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BattleEventShot : BattleEvent
{
    #region Fields

    protected Shooter _shooter;
    protected ShotStats _shotStats;
    float _waitTimeout = 2;

    enum Phase { Camera, Wait, Shoot, Shooting, Shot }
    Phase _phase;

    public static event Action<Shooter, GridEntity> OnShooting = delegate { };
    public static event Action OnShootingEnd = delegate { };

    #endregion

    public BattleEventShot(Shooter shooter, ShotStats shotStats) : base()
    {
        _shooter = shooter;
        _shotStats = shotStats;
        _shooter.OnShot += HandleShot;
        _phase = Phase.Camera;
        // NOTE: change this
        Walker walker = _shotStats.Target.GetComponent<Walker>();
        if (walker)
        {
            walker.Pause();
        }
    }

    void HandleShot()
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
                bool hit = UnityEngine.Random.Range(0, 100) < _shotStats.HitChance ? true : false;
                bool crit = UnityEngine.Random.Range(0, 100) < _shotStats.CritChance ? true : false;
                int damage = 0;
                if (hit)
                {
                    if (crit)
                    {
                        damage = UnityEngine.Random.Range(_shooter.Weapon.MaxDamage + 1, _shooter.Weapon.MaxDamage + 1 + _shooter.Weapon.BaseDamage);
                    }
                    else
                    {
                        damage = UnityEngine.Random.Range(_shooter.Weapon.BaseDamage, _shooter.Weapon.MaxDamage + 1);
                    }
                }
                _shotStats.Target.GetComponent<Health>().TakeDamage(damage, hit, crit);
                // NOTE: change this
                Walker walker = _shotStats.Target.GetComponent<Walker>();
                if (walker)
                {
                    walker.Resume();
                }
                MatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false);
                OnShootingEnd();
                End();
                break;
        }
    }

    public override void End()
    {
        base.End();
        _shooter.OnShot -= HandleShot;
    }
}
