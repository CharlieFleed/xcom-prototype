using UnityEngine;
using UnityEngine.EventSystems;

public class StandAloneInputModuleV2 : StandaloneInputModule
{
    public GameObject GetCurrentFocusedGameObjectPublic()
    {
        return GetCurrentFocusedGameObject();
    }
}