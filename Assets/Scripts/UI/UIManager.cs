using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _targetsPanel;
    [SerializeField] GameObject _confirmActionButton;

    private void Update()
    {
        if (_confirmActionButton.activeSelf)
        {
            _targetsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 68 + 160, 0);
        }
        else
        {
            _targetsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 68, 0);
        }
    }

}
