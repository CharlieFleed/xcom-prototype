using UnityEngine;
using System.Collections;
using DecisionTree;
using System;

public class BattleEventEngageAction : BattleEvent
{
    ActionsController _actionsController;
    GridEntity _gridEntity;
    ActionEngage _engageAction;
    float _waitTimeout = 1;

    enum Phase { Camera, Wait, Run, Wait2, Wait3 }
    Phase _phase;

    public static event Action<GridEntity> OnEngaging = delegate { };
    public static event Action<GridEntity> OnEngagingEnd = delegate { };

    public BattleEventEngageAction(ActionsController actionsController, ActionEngage engageAction, GridEntity gridEntity)
    {
        _actionsController = actionsController;
        _engageAction = engageAction;
        _gridEntity = gridEntity;
    }    

    public override void Run()
    {
        base.Run();
        switch (_phase)
        {
            case Phase.Camera:
                OnEngaging(_gridEntity);
                _phase = Phase.Wait;
                break;
            case Phase.Wait:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    _phase = Phase.Run;
                }
                break;
            case Phase.Run:
                _actionsController.OnActionComplete += _actionsController_OnActionComplete;
                _engageAction.RunTree();
                _phase = Phase.Wait2;
                break;
            case Phase.Wait2:
                break;

            case Phase.Wait3:
                _waitTimeout -= Time.deltaTime;
                if (_waitTimeout <= 0)
                {
                    OnEngagingEnd(_gridEntity);
                    End();
                }
                break;
            default:
                break;
        }
    }

    private void _actionsController_OnActionComplete(Unit obj)
    {
        _phase = Phase.Wait3;
        _waitTimeout = 2;
        _actionsController.OnActionComplete -= _actionsController_OnActionComplete;
        
    }
}
