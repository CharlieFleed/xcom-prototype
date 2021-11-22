using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BattleEventThrow : BattleEvent
{
    #region Fields

    enum Phase { Camera, Wait, Throw, Throwing, Thrown, Detonate }
    Phase _phase;

    Thrower _thrower;
    Grenade _grenade;
    GridNode _target;

    float _waitTimeout = .5f;

    public static event Action<Thrower, GridNode> OnThrowing = delegate { };
    public static event Action OnThrowingEnd = delegate { };

    #endregion

    public BattleEventThrow(Thrower thrower, GridNode target) : base()
    {
        _thrower = thrower;
        _target = target;
        _thrower.OnThrown += HandleThrown;
        _phase = Phase.Camera;
    }

    void HandleThrown(Grenade grenade)
    {
        _grenade = grenade;
        _grenade.OnDetonate += HandleDetonate;
        //if (_phase == Phase.Throwing)
            _phase = Phase.Thrown;
    }

    void HandleDetonate()
    {
        //if (_phase == Phase.Thrown)
            _phase = Phase.Detonate;
    }

    public override void Run()
    {
        base.Run();
        switch (_phase)
        {
            case Phase.Camera:
                OnThrowing(_thrower, _target);
                _phase = Phase.Wait;
                break;
            case Phase.Wait:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    _phase = Phase.Throw;
                }
                break;
            case Phase.Throw:
                _phase = Phase.Throwing;
                _thrower.DoThrow();
                break;
            case Phase.Throwing:
                // wait for OnThrown event
                break;
            case Phase.Thrown:
                // wait for OnDetonate event
                break;
            case Phase.Detonate:
                Collider[] colliders = Physics.OverlapSphere(_target.FloorPosition, _grenade.Radius * GridManager.Instance.XZScale);
                if (colliders.Length > 0)
                {
                    List<Health> targets = new List<Health>();
                    foreach (var collider in colliders)
                    {
                        Health health = collider.transform.root.GetComponentInChildren<Health>();
                        if (health != null && !health.IsDead) // only consider objects with Health
                        {
                            targets.Add(health);
                        }
                    }
                    if (targets.Count > 0)
                    {
                        if (NetworkRandomGenerator.Instance.Ready())
                        {
                            // sort
                            targets.Sort((a, b) => a.Id.CompareTo(b.Id));
                            foreach (var health in targets)
                            {
                                bool hit = false;
                                bool crit = false;
                                DamageDealer.DealDamage(
                                    health,
                                    health.GetComponent<Armor>(),
                                    _grenade.Damage,
                                    100,
                                    50,
                                    out hit,
                                    out crit);
                                //int damage = NetworkRandomGenerator.Instance.RandomRange(crit ? _grenade.Damage.BaseExplosiveDamage + 1 : 1, (crit ? 2 * _grenade.Damage.BaseExplosiveDamage : _grenade.Damage.BaseExplosiveDamage) + 1);
                                //health.TakeDamage(damage, hit, crit);
                                NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false);
                            }
                            OnThrowingEnd();
                            End();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        OnThrowingEnd();
                        End();
                    }
                }
                else
                {
                    OnThrowingEnd();
                    End();
                }
                break;
        }
    }

    public override void End()
    {
        base.End();
        _thrower.OnThrown -= HandleThrown;
        _grenade.OnDetonate -= HandleDetonate;
    }
}
