using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] NetworkMatchManager _networkMatchManager;
    [SerializeField] GameObject _victoryPanel;
    [SerializeField] GameObject _defeatPanel;
    [SerializeField] GameObject _drawPanel;

    CanvasGroup _cg;
    bool _display = false;
    float _fadeSpeed = 15f;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0;
        _victoryPanel.SetActive(false);
        _defeatPanel.SetActive(false);
        _drawPanel.SetActive(false);
    }

    private void Update()
    {
        Check();
        Fade();
    }

    private void Check()
    {
        if (_networkMatchManager.State == MatchState.End)
        {
            _display = true;
            if (_networkMatchManager.Winner != null)
            {
                if (_networkMatchManager.Winner.Owner.isLocalPlayer)
                {
                    _victoryPanel.SetActive(true);
                }
                else
                {
                    _defeatPanel.SetActive(true);
                }
            }
            else
            {
                _drawPanel.SetActive(true);
            }
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
        _cg.blocksRaycasts = _display;
    }
}
