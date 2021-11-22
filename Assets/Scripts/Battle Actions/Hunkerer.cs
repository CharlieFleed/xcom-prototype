using UnityEngine;
using System.Collections;
using Mirror;
using System;

public class Hunkerer : BattleAction
{
    public static event Action<Hunkerer> OnHunkererAdded = delegate { };
    public static event Action<Hunkerer> OnHunkererRemoved = delegate { };
    public event Action<bool> OnIsHunkeringChanged = delegate { };

    GridEntity _gridEntity;
    GridAgent _gridAgent;

    bool _isHunkering;
    public bool IsHunkering { get { return _isHunkering; } set { _isHunkering = value; OnIsHunkeringChanged(_isHunkering); } }


    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
    }

    private void Start()
    {
        OnHunkererAdded(this);
    }

    private void OnDestroy()
    {
        OnHunkererRemoved(this);
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        bool canHunker = false;
        for (int i = 0; i < 4; i++)
        {
            canHunker |= _gridEntity.CurrentNode.HalfWalls[i];
            canHunker |= _gridEntity.CurrentNode.Walls[i];
        }
        Available = canHunker;
        IsHunkering = false;
    }

    private void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
    }

    override public void HandleConfirmClick()
    {
        Hunker();
    }

    void Hunker()
    {
        CmdHunker();
    }

    [Command]
    void CmdHunker()
    {
        RpcHunker();
    }

    [ClientRpc]
    void RpcHunker()
    {
        Debug.Log($"{name} Hunkerer RpcHunker");
        IsHunkering = true;
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }
}
