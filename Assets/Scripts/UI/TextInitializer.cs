using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextInitializer : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    [SerializeReference] GameObject _description;

    // Start is called before the first frame update
    void Start()
    {
        IDescription d = _description.GetComponent<IDescription>();
        if (d != null)
            _text.text =  d.Description;
    }
}

public interface IDescription
{
    string Description { get; }
}
