using UnityEngine;
using System.Collections;
using System;

public class BattleEventItemUse : BattleEvent
{
    #region Fields

    enum Phase { Camera, Wait, UseItem, UsingItem, ItemUsed }
    Phase _phase;

    ItemUser _itemUser;
    Item _item;
    ShotStats _target;

    float _waitTimeout = .5f;

    public static event Action<ItemUser, GridEntity> OnUsingItem = delegate { };
    public static event Action OnUsingItemEnd = delegate { };

    #endregion

    public BattleEventItemUse(ItemUser itemUser, ShotStats target) : base()
    {
        _itemUser = itemUser;
        _target = target;
        _itemUser.OnUsed += HandleItemUsed;
        _phase = Phase.Camera;
    }

    void HandleItemUsed(Item item)
    {
        _item = item;
        _phase = Phase.ItemUsed;
    }

    public override void Run()
    {
        base.Run();
        switch (_phase)
        {
            case Phase.Camera:
                OnUsingItem(_itemUser, _target.Target);
                _phase = Phase.Wait;
                break;
            case Phase.Wait:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    _phase = Phase.UseItem;
                }
                break;
            case Phase.UseItem:
                _phase = Phase.UsingItem;
                _itemUser.DoUse();
                break;
            case Phase.UsingItem:
                // wait for OnItemUsed event
                break;
            case Phase.ItemUsed:
                _item.UseOn(_target.Target);
                
                OnUsingItemEnd();
                End();
                break;
        }
    }
}
