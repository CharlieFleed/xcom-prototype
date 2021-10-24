using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnPanel : MonoBehaviour
{
    CanvasGroup _cg;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0;
    }

    private void Update()
    {
        if (NetworkMatchManager.Instance.CurrentCharacter != null)
        {
            if (NetworkMatchManager.Instance.CurrentCharacter.Team.Owner.isLocalPlayer)
            {
                _cg.alpha = 0;
            }
            else
            {
                _cg.alpha = 1;
            }
        }
        else
        {
            _cg.alpha = 0;
        }
    }
}   
