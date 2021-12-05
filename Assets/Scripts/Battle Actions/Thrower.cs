using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;

public class Thrower : BattleAction
{
    #region Fields

    [SerializeField] protected Grenade _grenade;
    [SerializeField] int _throwRange = 15;
    [SerializeField] GameObject _grenadePrefab;

    public static event Action<Grenade> OnThrowing = delegate { };

    public Grenade Grenade { get { return _grenade; } set { _grenade = value; } }
    public int ThrowRange { get { return _throwRange; } }

    public override string ActionName { get { return _grenade.Name; } }

    public event Action<Thrower, GridNode> OnTargetSelected = delegate { };
    public event Action<Grenade> OnThrown = delegate { };
    public event Action<Thrower> OnThrow = delegate { };

    GridNodeSelector _gridNodeSelector;
    GridNode _target;
    GameObject _grenadeProjectile;

    #endregion

    private void Awake()
    {
        _gridNodeSelector = GetComponent<GridNodeSelector>();
    }

    private void Update()
    {
        if (IsActive)
        {
            _input.Update();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (IsActive)
        {
            if (_input.GetKeyDown(KeyCode.Escape) || _input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
        _input.Clear();
    }

    public void SetTarget(GridNode target)
    {
        _target = target;
        OnTargetSelected(this, target);
        Throw();
    }

    void Throw()
    {
        InvokeActionConfirmed(this);
        BattleEventThrow throwEvent = new BattleEventThrow(this, _target);
        NetworkMatchManager.Instance.AddBattleEvent(throwEvent, true);
        Deactivate();
        InvokeActionComplete(this);
    }

    [Command]
    void CmdThrow(Vector3Int target)
    {
        RpcThrow(target);
    }

    [ClientRpc]
    void RpcThrow(Vector3Int target)
    {
        Debug.Log($"{name} Thrower RpcThrow");
        InvokeActionConfirmed(this);
        BattleEventThrow _throw = new BattleEventThrow(this, _target);
        NetworkMatchManager.Instance.AddBattleEvent(_throw, true);
        Deactivate();
        InvokeActionComplete(this);
    }

    public void Thrown()
    {
        Vector3 impulse;
        Vector3 origin;
        List<Vector3> origins = new List<Vector3>() { GetComponent<GridEntity>().CurrentNode.FloorPosition + 2 * Vector3.up };
        foreach (var sidestep in GridCoverManager.Instance.SideSteps(GetComponent<GridEntity>(), _target))
        {
            origins.Add(sidestep.FloorPosition + 1.5f * Vector3.up);
        }
        TrajectoryPredictor.Instance.TrajectoryToTarget(origins, _target.FloorPosition, out impulse, out Vector3[] trajectory, out origin);
        // clone the grenade
        _grenadeProjectile = Instantiate(_grenadePrefab, Vector3.zero, Quaternion.identity);
        _grenadeProjectile.AddComponent<Grenade>();
        _grenadeProjectile.GetComponent<Grenade>().GrenadeData = _grenade.GrenadeData;
        _grenadeProjectile.transform.position = origin;
        _grenadeProjectile.GetComponent<Grenade>().SetTarget(_target.FloorPosition);
        _grenadeProjectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
        _grenade.Throw();
        OnThrown(_grenadeProjectile.GetComponent<Grenade>());
    }

    public void DoThrow()
    {
        OnThrow(this);        
    }

    override public void Activate()
    {
        base.Activate();
        _gridNodeSelector.Activate();
        OnThrowing(_grenade);
    }

    override public void Cancel()
    {
        base.Cancel();
        _gridNodeSelector.Cancel();
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        Available &= _grenade.Uses > 0;
    }
}
