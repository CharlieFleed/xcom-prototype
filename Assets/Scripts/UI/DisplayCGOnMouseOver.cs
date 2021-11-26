using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCGOnMouseOver : MonoBehaviour
{
    [SerializeField] CanvasGroup _cg;

    float _fadeSpeed = 15f;
    bool _display;

    private void Start()
    {
        _cg.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        FadeDisplay();
    }

    private void FadeDisplay()
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

    private void OnMouseOver()
    {
        _display = true;
    }
    private void OnMouseExit()
    {
        _display = false;
    }
}
