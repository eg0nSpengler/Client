using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private const float WallHeight = 3f;
    private const float FloorWidth = 1f;
    
    public Vector3Int position;

	private void Awake ()
    {
        Snap();
        if (Tiles.ContainsKey(position))
        {
            //We cannot have two tiles at the same area, so let's get rid of the last one.
            Destroy(Tiles[position].gameObject);
        }
        Tiles.Add(position, this);
    }

    [ContextMenu("Snap")]
    private void Snap()
    {
        //We don't simply convert the transform.position, because we have to take care of wall height and floor width
        position = new Vector3Int(Mathf.RoundToInt(transform.position.x / FloorWidth),
            Mathf.RoundToInt(transform.position.y / WallHeight),
            Mathf.RoundToInt(transform.position.z / FloorWidth));
    }

    [ContextMenu("Snap Transform")]
    private void SnapTransform()
    {
        Snap();
        transform.position = new Vector3(position.x * FloorWidth,
            position.y * WallHeight,
            position.z * FloorWidth);
    }
    private static readonly Dictionary<Vector3Int, Tile> Tiles = new Dictionary<Vector3Int, Tile>();
    
    //---METHODS FOR GETTING TILES OF THE WORLD ONLY BELOW THIS COMMENT---

    public static Tile GetAt(int x, int y, int z)
    {
        return GetAt(new Vector3Int(x, y, z));
    }
    
    public static Tile GetAt(Vector3Int position)
    {
        return Tiles.TryGetValue(position, out var tile) ? tile : null;
    }

    public static List<Tile> GetCube(int x, int y, int z, int x2, int y2, int z2)
    {
        //Let's flip values so the algorithm works even when the smaller coordinate is given second
        if (x > x2)
        {
            var helper = x2;
            x2 = x;
            x = helper;
        }
        if (y > y2)
        {
            var helper = y2;
            y2 = y;
            y = helper;
        }
        if (z > z2)
        {
            var helper = z2;
            z2 = z;
            z = helper;
        }

        var tileList = new List<Tile>();
        //Do not remove +1s so the end is inclusive
        for (var i = x; i < x2 + 1; i++)
        {
            for (var j = y; j < y2 + 1; j++)
            {
                for (var k = z; k < z2 + 1; k++)
                {
                    tileList.Add(GetAt(i, j, k));
                }
            }
        }
        return tileList;
    }

    public static List<Tile> GetCube(Vector3Int start, Vector3Int end)
    {
        return GetCube(start.x, start.y, start.z, end.x, end.y, end.z);
    }

    public static List<Tile> GetSphere(int x, int y, int z, int radius)
    {
        var tileList = new List<Tile>();
        if (radius < 0)
        {
            Debug.LogError("GetSphere() radius cannot be negative!");
            return null;
        }

        //Do not remove +1s so the end is inclusive
        for (var i = x - radius; i < x + radius + 1; i++)
        {
            for (var j = y - radius; j < y + radius + 1; j++)
            {
                for (var k = z - radius; k < z + radius + 1; k++)
                {
                    //Is this tile inside the sphere?
                    if ((i - x) * (i - x) + (j - y) * (j - y) + (k - z) * (k - z)
                        <= radius * radius)
                    {
                        tileList.Add(GetAt(i, j, k));
                    }
                }
            }
        }
        return tileList;
    }

    public static List<Tile> GetSphere(Vector3Int center, int radius)
    {
        return GetSphere(center.x, center.y, center.z, radius);
    }
}