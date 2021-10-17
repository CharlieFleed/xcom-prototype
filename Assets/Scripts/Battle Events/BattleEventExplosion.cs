using UnityEngine;
using System.Collections;
using System;

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
                //Debug.Log($"{colliders.Length}");
                if (colliders.Length > 0)
                {
                    foreach (var collider in colliders)
                    {
                        Health health = collider.transform.root.GetComponentInChildren<Health>();
                        if (health != null && !health.IsDead) // only consider objects with Health
                        {
                            bool crit = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
                            bool hit = true;
                            int damage = UnityEngine.Random.Range(crit ? _explosive.Damage + 1 : 1, (crit ? 2 * _explosive.Damage : _explosive.Damage) + 1);
                            health.TakeDamage(damage, hit, crit);
                            MatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false);
                        }
                    }
                }
                _explosive.Detonate();
                _waitTimeout = 1.0f;
                _phase = Phase.Wait2;
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
