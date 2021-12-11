using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BattleEventExplosion : BattleEvent
{
    #region Fields

    enum Phase { Camera, Wait, Explosion, Wait2 }
    Phase _phase;

    Explosive _explosive;

    float _waitTimeout = .5f;

    public static event Action<GridEntity> OnExploding = delegate { };
    public static event Action OnExplodingEnd = delegate { };

    #endregion

    public BattleEventExplosion(Explosive explosive) : base()
    {
        _explosive = explosive;
        _phase = Phase.Camera;
    }

    public override void Run()
    {
        base.Run();
        switch (_phase)
        {
            case Phase.Camera:
                OnExploding(_explosive.GetComponent<GridEntity>());
                _phase = Phase.Wait;
                break;
            case Phase.Wait:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    _phase = Phase.Explosion;
                }
                break;
            case Phase.Explosion:
                Collider[] colliders = Physics.OverlapSphere(_explosive.GetComponent<GridEntity>().CurrentNode.FloorPosition, _explosive.Radius * GridManager.Instance.XZScale);
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
                                    _explosive.Damage,
                                    100,
                                    50,
                                    out hit,
                                    out crit);
                                NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false, 0);
                            }
                            _explosive.Detonate();
                            _waitTimeout = 1.0f;
                            _phase = Phase.Wait2;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        _explosive.Detonate();
                        _waitTimeout = 1.0f;
                        _phase = Phase.Wait2;
                    }
                }
                else
                {
                    _explosive.Detonate();
                    _waitTimeout = 1.0f;
                    _phase = Phase.Wait2;
                }
                break;
            case Phase.Wait2:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    OnExplodingEnd();
                    End();
                }
                break;
        }
    }
}
