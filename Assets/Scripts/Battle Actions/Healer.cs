using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : BattleAction
{
    #region Fields

    [SerializeField] protected Medikit _medikit;
    public Medikit Medikit { get { return _medikit; } set { _medikit = value; } }

    public override string ActionName { get { return base.ActionName + " (" + _medikit.Name + ")"; } }

    GridEntity _target;

    public event Action<Healer> OnHeal = delegate { };
    public event Action<Medikit> OnHealed = delegate { };

    GridNeighborSelector _gridNeighborSelector;

    #endregion

    private void Awake()
    {
        _gridNeighborSelector = GetComponent<GridNeighborSelector>();
    }

    // Update is called once per frame
    void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
    }

    public void SetTarget(GridEntity target)
    {
        _target = target;
        Heal();
    }

    void Heal()
    {
        InvokeActionConfirmed(this);
        BattleEventHeal healEvent = new BattleEventHeal(this, _target);
        NetworkMatchManager.Instance.AddBattleEvent(healEvent, true);
        Deactivate();
        InvokeActionComplete(this);
    }

    void Healed()
    {
        OnHealed(_medikit);
    }

    public void DoHeal()
    {
        OnHeal(this);
        Invoke("Healed", 1f);
    }

    override public void Activate()
    {
        base.Activate();
        _gridNeighborSelector.Activate();
    }

    override public void Cancel()
    {
        base.Cancel();
        _gridNeighborSelector.Cancel();
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        Available &= _medikit.Uses > 0;
    }
}
