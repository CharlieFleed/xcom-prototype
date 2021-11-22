using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorBarController : MonoBehaviour, IUIChildController
{
    [SerializeField] ArmorBar _armorBarPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<Armor, ArmorBar> _armorBars = new Dictionary<Armor, ArmorBar>();    

    public void Init()
    {
        Armor.OnArmorAdded += Armor_OnArmorAdded;
        Armor.OnArmorRemoved += Armor_OnArmorRemoved;
    }

    private void Armor_OnArmorRemoved(Armor armor)
    {
        if (_armorBars.ContainsKey(armor) == true)
        {
            if (_armorBars[armor] != null)
            {
                Destroy(_armorBars[armor].gameObject);
            }
            _armorBars.Remove(armor);
        }
    }

    private void Armor_OnArmorAdded(Armor armor)
    {
        //Debug.Log($"Armor added for {armor.name}.");
        if (_armorBars.ContainsKey(armor) == false)
        {
            GridEntity gridEntity = armor.GetComponent<GridEntity>();
            var armorBar = Instantiate(_armorBarPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _armorBars.Add(armor, armorBar);
            armorBar.SetArmor(armor);
        }
    }

    private void OnDestroy()
    {
        Armor.OnArmorAdded -= Armor_OnArmorAdded;
        Armor.OnArmorRemoved -= Armor_OnArmorRemoved;
    }
}
