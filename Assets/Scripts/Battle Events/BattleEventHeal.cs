using UnityEngine;
using System.Collections;
using System;

public class BattleEventHeal : BattleEvent
{
    #region Fields

    enum Phase { Camera, Wait, Heal, Healing, Healed }
    Phase _phase;

    Healer _healer;
    Medikit _medikit;
    GridEntity _target;

    float _waitTimeout = .5f;

    public static event Action<Healer, GridEntity> OnHealing = delegate { };
    public static event Action OnHealingEnd = delegate { };

    #endregion

    public BattleEventHeal(Healer healer, GridEntity target) : base()
    {
        _healer = healer;
        _target = target;
        _healer.OnHealed += HandleHealed;
        _phase = Phase.Camera;
    }

    void HandleHealed(Medikit medikit)
    {
        _medikit = medikit;
        _phase = Phase.Healed;
    }

    public override void Run()
    {
        base.Run();
        switch (_phase)
        {
            case Phase.Camera:
                OnHealing(_healer, _target);
                _phase = Phase.Wait;
                break;
            case Phase.Wait:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    _phase = Phase.Heal;
                }
                break;
            case Phase.Heal:
                _phase = Phase.Healing;
                _healer.DoHeal();
                break;
            case Phase.Healing:
                // wait for OnHealed event
                break;
            case Phase.Healed:
                Health health = _target.GetComponent<Health>();
                health.TakeDamage(-_medikit.Healment, true, false);
                NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false);
                OnHealingEnd();
                End();
                break;
        }
    }
}
