using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsBarController : MonoBehaviour, IUIChildController
{
    [SerializeField] ActionsBar _actionsBarPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<Character, ActionsBar> _actionsBars = new Dictionary<Character, ActionsBar>();

    public void Init()
    {
        Character.OnCharacterAdded += HandleCharacterAdded;
        Character.OnCharacterRemoved += HandleCharacterRemoved;
    }

    private void HandleCharacterRemoved(Character character)
    {
        if (_actionsBars.ContainsKey(character) == true)
        {
            if (_actionsBars[character] != null)
            {
                Destroy(_actionsBars[character].gameObject);
            }
            _actionsBars.Remove(character);
        }
    }

    private void HandleCharacterAdded(Character character)
    {
        if (_actionsBars.ContainsKey(character) == false)
        {
            GridEntity gridEntity = character.GetComponent<GridEntity>();
            var actionsBar = Instantiate(_actionsBarPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _actionsBars.Add(character, actionsBar);
            actionsBar.SetCharacter(character);
        }
    }

    private void OnDestroy()
    {
        Character.OnCharacterAdded -= HandleCharacterAdded;
        Character.OnCharacterRemoved -= HandleCharacterRemoved;
    }
}

