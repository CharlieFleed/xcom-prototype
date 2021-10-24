using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Mirror;

public class BattleAction : NetworkBehaviour
{
    [SerializeField] string _actionName;
    public virtual string ActionName { get { return _actionName; } }

    [SerializeField] string _confirmText;
    public virtual string ConfirmText { get { return _confirmText; } }

    public int Cost = 1;
    public bool Available = true;
    public bool EndsTurn = false;
    public Sprite Icon;
    public bool IsConfirmable = true;

    protected bool _isActive;
    public bool IsActive { get { return _isActive; } protected set { _isActive = value; OnActiveChanged(_isActive); } }
    public event Action<bool> OnActiveChanged = delegate { }; // NOTE: not used?

    public event Action<BattleAction> OnActionConfirmed = delegate { };
    public event Action<BattleAction> OnActionComplete = delegate { };
    public event Action<BattleAction> OnActionCancelled = delegate { };

    protected void InvokeActionConfirmed(BattleAction battleAction)
    {
        OnActionConfirmed.Invoke(battleAction);
    }
    protected void InvokeActionComplete(BattleAction battleAction)
    {
        OnActionComplete.Invoke(battleAction);
    }
    protected void InvokeActionCancelled(BattleAction battleAction)
    {
        OnActionCancelled.Invoke(battleAction);
    }

    virtual public void HandleConfirmClick()
    { }

    virtual public void Init(int numActions)
    {
        Available = Cost <= numActions;
    }

    virtual public void Activate()
    {
        IsActive = true;
    }

    virtual protected void Deactivate()
    {
        IsActive = false;
    }

    virtual public void Cancel()
    {
        //Debug.Log("BattleAction - OnActionCancelled");
        OnActionCancelled(this);
        Deactivate();
    }
}
