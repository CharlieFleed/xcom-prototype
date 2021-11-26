using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverHighlights : MonoBehaviour
{
    // Highlights
    [SerializeField] GameObject[] _covers;
    [SerializeField] GameObject[] _hCovers;

    public bool[] _flags;
    public bool[] _hFlags;
    MeshRenderer[] _coverRenderers;
    MeshRenderer[] _hCoverRenderers;

    private bool _main;
    public bool Main
    {
        set
        {
            _main = value;
            for (int i = 0; i < 4; i++)
            {
                _coverRenderers[i].material.color = _main ? _color1 : _color2;
                _hCoverRenderers[i].material.color = _main ? _color1 : _color2;
            }
        }
    }


    Color _color1 = new Color(1, 1, 1, 0.5f);
    Color _color2 = new Color(1, 1, 1, 0.25f);

    private void Awake()
    {
        _flags = new bool[4];
        _hFlags = new bool[4];
        _coverRenderers = new MeshRenderer[4];
        _hCoverRenderers = new MeshRenderer[4];

        _covers[0].transform.localPosition = new Vector3(0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale, 0.5f, 0);
        _covers[1].transform.localPosition = new Vector3(0, 0.5f, 0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale);
        _covers[2].transform.localPosition = new Vector3(-0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale, 0.5f, 0);
        _covers[3].transform.localPosition = new Vector3(0, 0.5f, -0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale);
        _hCovers[0].transform.localPosition = new Vector3(0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale, 0.5f, 0);
        _hCovers[1].transform.localPosition = new Vector3(0, 0.5f, 0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale);
        _hCovers[2].transform.localPosition = new Vector3(-0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale, 0.5f, 0);
        _hCovers[3].transform.localPosition = new Vector3(0, 0.5f, -0.5f * GridManager.Instance.TileExtents.z / GridManager.Instance.XZScale);
        for (int i = 0; i < 4; i++)
        {
            _coverRenderers[i] = _covers[i].GetComponent<MeshRenderer>();
            _hCoverRenderers[i] = _hCovers[i].GetComponent<MeshRenderer>();
        }
        Main = false;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (_covers[i].gameObject.activeSelf != _flags[i])
            {
                _covers[i].gameObject.SetActive(_flags[i]);
            }
            if (_hCovers[i].gameObject.activeSelf != _hFlags[i])
            {
                _hCovers[i].gameObject.SetActive(_hFlags[i]);
            }
        }
    }
}
