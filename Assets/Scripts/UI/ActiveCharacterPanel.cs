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
        _cg.alpha = 0;
        _text = GetComponentInChildren<Text>(true);
        Character.OnActiveChanged += HandleCharacter_OnActiveChanged;
    }

    private void HandleCharacter_OnActiveChanged(Character character, bool active)
    {
        if (active)
        {
            _text.text = character.name;
            _cg.alpha = 1;
        }
        else
        {
            _text.text = ""; ;
            _cg.alpha = 0;
        }
    }

    private void OnDestroy()
    {
        Character.OnActiveChanged -= HandleCharacter_OnActiveChanged;
    }
}
