using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{
    public void Thrown()
    {
        transform.parent.GetComponent<AnimationController>().Thrown();
    }

    public void Shot()
    {
        transform.parent.GetComponent<AnimationController>().Shot();
    }
}
