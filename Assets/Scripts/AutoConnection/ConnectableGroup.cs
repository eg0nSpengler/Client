using System;
using System.Linq;
using UnityEngine;

public class ConnectableGroup : MonoBehaviour
{
    [SerializeField]
    private Vector2Int gridSize;

    public Connectable[,] Grid { get; private set; } = new Connectable[0, 0];

    public Vector2Int Size { get; private set; }
    
    public void UpdateGrid()
    {
        // update grid size
        if (gridSize.x == Grid.GetLength(0) && gridSize.y == Grid.GetLength(1)) return;

        var original = Grid;
        int minRows = Math.Min(original.GetLength(0), gridSize.x);
        int minCols = Math.Min(original.GetLength(1), gridSize.y);

        Grid = new Connectable[gridSize.x, gridSize.y];
        for (int x = 0; x < minRows; x++)
        {
            for (int z = 0; z < minCols; z++)
            {
                Grid[x, z] = original[x, z];
            }
        }

        Size = new Vector2Int(Grid.GetLength(0), Grid.GetLength(1));

        // update grid content
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int z = 0; z < Grid.GetLength(1); z++)
            {
                Physics.Raycast(new Vector3(x, -.5f, z), Vector3.up, out var hit, 1f);
                if (hit.collider != null && hit.collider.attachedRigidbody)
                {
                    Grid[x, z] = hit.collider.attachedRigidbody.GetComponent<Connectable>();

                    continue;
                }

                if (hit.collider != null)
                {
                    Grid[x, z] = hit.collider.GetComponent<Connectable>();
                }
            }
        }
    }

    public Connectable GetConnectableAtPosition(Vector2Int position, ConnectableSettings connectableSettings)
    {
        // Position is out of range
        if (position.x < 0 || position.x >= Grid.GetLength(0) ||
            position.y < 0 || position.y >= Grid.GetLength(1)) return null;

        Connectable connectable = Grid[position.x, position.y];
        if (connectable && connectableSettings.ConnectsWith.Any(x => x.name == connectable.ConnectableSettings.name))
        {
            return connectable;
        }

        // There is no tile at position
        return null;
    }

    public Connectable GetConnectableAtPosition(Vector2Int position)
    {
        // Position is out of range
        if (position.x < 0 || position.x >= Grid.GetLength(0) ||
            position.y < 0 || position.y >= Grid.GetLength(1)) return null;

        return Grid[position.x, position.y];
    }

    private void OnDrawGizmos()
    {
        var bottomLeft = transform.position + new Vector3(-.5f, 0, -.5f);
        var topLeft = transform.position + new Vector3(-.5f, 0, Size.y - .5f);
        var bottomRight = transform.position + new Vector3(Size.x - .5f, 0, -.5f);
        var topRight = transform.position + new Vector3(Size.x - .5f, 0, Size.y - .5f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topRight, bottomRight);
    }
}