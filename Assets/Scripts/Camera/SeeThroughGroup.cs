using UnityEngine;
using System.Collections;

public class SeeThroughGroup : MonoBehaviour
{
    [SerializeField] SeeThroughGroup[] _linkedGroups;

    public void Hide()
    {
        foreach (Transform child in transform)
        {
            SeeThrough seeThrough = child.GetComponent<SeeThrough>();
            if (seeThrough != null)
            {
                seeThrough.HideOnlyYou();
            }
            SeeThroughGroup seeThroughGroup = child.GetComponent<SeeThroughGroup>();
            if (seeThroughGroup != null)
            {
                seeThroughGroup.Hide();
            }
        }
        foreach (SeeThroughGroup group in _linkedGroups)
        {
            group.Hide();
        }
    }
}
