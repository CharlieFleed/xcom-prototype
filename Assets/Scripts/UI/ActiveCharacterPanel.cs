using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveCharacterPanel : MonoBehaviour
{
    Text _text;
    CanvasGroup _cg;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _text = GetComponentInChildren<Text>(true);
    }

    // Update is called once per frame
    void Update()
    {
        _cg.alpha = (MatchManager.Instance.ActiveCharacter != null) ? 1 : 0;
        if (MatchManager.Instance.ActiveCharacter != null)
        {
            _text.text = MatchManager.Instance.ActiveCharacter.name;
        }     
    }
}
