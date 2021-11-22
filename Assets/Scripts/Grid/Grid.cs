using UnityEngine;
using System.Collections;

public class Grid
{
    int _gridSizeX;
    int _gridSizeY;
    int _gridSizeZ;
    GridNode[] _grid;

    public Grid(int gridSizeX, int gridSizeY, int gridSizeZ)
    {
        _gridSizeX = gridSizeX;
        _gridSizeY = gridSizeY;
        _gridSizeZ = gridSizeZ;
        _grid = new GridNode[_gridSizeX * _gridSizeY * _gridSizeZ];
    }

    public void SetNode(int x, int y, int z, GridNode node)
    {
        _grid[x * _gridSizeY * _gridSizeZ + y * _gridSizeZ + z] = node;
    }

    public GridNode Node(int x, int y, int z)
    {
        if (x >= 0 && x < _gridSizeX && y >= 0 && y < _gridSizeY && z >= 0 && z < _gridSizeZ)
            return _grid[x * _gridSizeY * _gridSizeZ + y * _gridSizeZ + z];
        else
            return null;
    }

    public GridNode[] Nodes()
    {
        return _grid;
    }

    public int GetLength(int index)
    {
        switch (index)
        {
            case 0:
                return _gridSizeX;
            case 1:
                return _gridSizeY;
            case 2:
                return _gridSizeZ;
            default:
                return 0;
        }
    }
}
