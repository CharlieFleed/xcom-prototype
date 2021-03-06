using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnPanel : MonoBehaviour
{
    [SerializeField] NetworkMatchManager _networkMatchManager;
    CanvasGroup _cg;
    bool _display = false;
    float _fadeSpeed = 15f;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0;
    }

    private void Update()
    {
        Check();
        Fade();
    }

    private void Check()
    {
        Unit currentUnit = _networkMatchManager.CurrentUnit;
        if (currentUnit != null)
        {
            if (currentUnit.Team.Owner.isLocalPlayer && !currentUnit.Team.IsAI)
            {
                _display = false;
            }
            else
            {
                _display = true;
            }
        }
        else
        {
            _display = false;
        }
    }

    private void Fade()
    {
        if (_display)
        {
            _cg.alpha = Mathf.Lerp(_cg.alpha, 1, _fadeSpeed * Time.deltaTime);
        }
        else
        {
            _cg.alpha = Mathf.Lerp(_cg.alpha, 0, _fadeSpeed * Time.deltaTime);
        }
    }
}   
