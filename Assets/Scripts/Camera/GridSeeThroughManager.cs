using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridSeeThroughManager : MonoBehaviour
{
    CameraController _cameraController;

    private void Start()
    {
        _cameraController = CameraController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] raycastHit = Physics.RaycastAll(ray);
        if (raycastHit.Length > 0)
        {
            for (int i = 0; i < raycastHit.Length; i++)
            {
                GridObject obj = raycastHit[i].collider.GetComponent<GridObject>();
                if (obj != null)
                {
                    if (obj.Y > _cameraController.Level)
                    {
                        obj.GetComponent<SeeThrough>().Hidden = true;
                    }
                }
            }
        }
    }
}
