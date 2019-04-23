using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Connectable : MonoBehaviour
{
    [Tooltip("Add new Connectable Settings through Create>SS3D>Connectable Data")]
    public ConnectableSettings ConnectableSettings;

    [SerializeField]
    private AdjacentData adjacentData;

    [SerializeField]
    private ConnectableGroup connectableGroup;

    [Space]
    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshRenderer meshRenderer;

    private Dictionary<Vector2Int, Directions> adjacentDictionary = new Dictionary<Vector2Int, Directions>
    {
        [new Vector2Int(-1, 1)] = Directions.NorthWest,
        [new Vector2Int(0, 1)] = Directions.North,
        [new Vector2Int(1, 1)] = Directions.NorthEast,
        [new Vector2Int(1, 0)] = Directions.East,
        [new Vector2Int(1, -1)] = Directions.SouthEast,
        [new Vector2Int(0, -1)] = Directions.South,
        [new Vector2Int(-1, -1)] = Directions.SouthWest,
        [new Vector2Int(-1, 0)] = Directions.West,
    };

    private Vector2Int position
    {
        get => new Vector2Int(Convert.ToInt16(transform.position.x), Convert.ToInt16(transform.position.z));
    }

    private void Start()
    {
        if (!connectableGroup) connectableGroup = GetComponentInParent<ConnectableGroup>();
    }

    public void Construct(bool updateGrid)
    {
        if (!connectableGroup) connectableGroup = GetComponentInParent<ConnectableGroup>();
        if (connectableGroup)
        {
            if (updateGrid) connectableGroup.UpdateGrid();

            Connect();
            foreach (Connectable connectable in adjacentData.adjacentConnectables)
            {
                if (connectable) connectable.Connect();
            }
        }
    }

    private void Reset()
    {
        meshFilter = GetComponentInChildren<MeshFilter>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    // method of connecting, works, but UGLY and long.
    private void Connect()
    {
        GetAdjacentsOnGrid();

        switch (ConnectableSettings.connectableType)
        {
            // Only rotating (Doors for example)
            case ConnectableType.OnlyConnect:
                switch (adjacentData.directions & ~(Directions.NorthEast | Directions.SouthEast |
                                                    Directions.SouthWest | Directions.NorthWest))
                {
                    case Directions.North | Directions.South:
                        transform.eulerAngles = new Vector3(0, 90, 0);
                        break;
                    case Directions.East | Directions.West:
                        transform.eulerAngles = new Vector3(0, 0, 0);
                        break;
                }

                break;

            // Material Connections
            case ConnectableType.MaterialBased:
                switch (adjacentData.directions)
                {
                    case Directions.None:
                        UpdateMaterialAndRotation(ConnectableSettings.NoColorMaterial, ConnectableSettings.GlobalRotationOffset, 0);
                        break;

                    // North
                    case Directions.North:
                    case Directions.North | Directions.NorthEast:
                    case Directions.North | Directions.NorthWest:
                    case Directions.North | Directions.NorthEast | Directions.NorthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 270);
                        break;

                    // East
                    case Directions.East:
                    case Directions.East | Directions.NorthEast:
                    case Directions.East | Directions.SouthEast:
                    case Directions.East | Directions.NorthEast | Directions.SouthEast:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 0);
                        break;

                    // South
                    case Directions.South:
                    case Directions.South | Directions.SouthEast:
                    case Directions.South | Directions.SouthWest:
                    case Directions.South | Directions.SouthEast | Directions.SouthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 90);
                        break;

                    // West
                    case Directions.West:
                    case Directions.West | Directions.NorthWest:
                    case Directions.West | Directions.SouthWest:
                    case Directions.West | Directions.NorthWest | Directions.SouthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 180);
                        break;

                    // North East Outer
                    case Directions.NorthEast | Directions.East | Directions.NorthWest:
                    case Directions.North | Directions.NorthEast | Directions.East:
                    case Directions.North | Directions.NorthEast | Directions.East | Directions.NorthWest:
                    case Directions.North | Directions.NorthEast | Directions.East | Directions.SouthEast:
                    case Directions.North | Directions.NorthEast | Directions.East | Directions.SouthEast |
                         Directions.NorthWest:
                    case Directions.NorthEast | Directions.East | Directions.SouthEast | Directions.NorthWest:
                    case Directions.North | Directions.NorthEast | Directions.SouthEast | Directions.NorthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.TriColorMaterial, ConnectableSettings.GlobalRotationOffset, 90);
                        break;

                    // South East Outer
                    case Directions.East | Directions.SouthEast | Directions.SouthWest:
                    case Directions.South | Directions.SouthEast | Directions.East:
                    case Directions.South | Directions.SouthEast | Directions.East | Directions.NorthEast:
                    case Directions.South | Directions.SouthEast | Directions.East | Directions.SouthWest:
                    case Directions.South | Directions.SouthEast | Directions.East | Directions.SouthWest |
                         Directions.NorthEast:
                    case Directions.SouthEast | Directions.East | Directions.SouthWest | Directions.NorthEast:
                    case Directions.South | Directions.SouthEast | Directions.SouthWest | Directions.NorthEast:

                        UpdateMaterialAndRotation(ConnectableSettings.TriColorMaterial, ConnectableSettings.GlobalRotationOffset, 180);
                        break;

                    // North West Outer
                    case Directions.NorthEast | Directions.West | Directions.NorthWest:
                    case Directions.North | Directions.NorthWest | Directions.West:
                    case Directions.North | Directions.NorthWest | Directions.West | Directions.SouthWest:
                    case Directions.North | Directions.NorthWest | Directions.West | Directions.NorthEast:
                    case Directions.North | Directions.NorthWest | Directions.West | Directions.NorthEast |
                         Directions.SouthWest:
                    case Directions.NorthWest | Directions.West | Directions.NorthEast | Directions.SouthWest:
                    case Directions.North | Directions.NorthWest | Directions.NorthEast | Directions.SouthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.TriColorMaterial, ConnectableSettings.GlobalRotationOffset, 0);
                        break;

                    // South West Outer
                    case Directions.South | Directions.SouthWest | Directions.West:
                    case Directions.South | Directions.SouthWest | Directions.West | Directions.SouthEast:
                    case Directions.South | Directions.SouthWest | Directions.West | Directions.NorthWest:
                    case Directions.South | Directions.SouthWest | Directions.West | Directions.NorthWest |
                         Directions.SouthEast:
                    case Directions.SouthWest | Directions.West | Directions.NorthWest | Directions.SouthEast:
                    case Directions.South | Directions.SouthWest | Directions.NorthWest | Directions.SouthEast:
                    case Directions.SouthEast | Directions.SouthWest | Directions.West:
                        UpdateMaterialAndRotation(ConnectableSettings.TriColorMaterial, ConnectableSettings.GlobalRotationOffset, 270);
                        break;

                    // North East Inner
                    case Directions.NorthEast:
                        UpdateMaterialAndRotation(ConnectableSettings.SingleColorMaterial, ConnectableSettings.GlobalRotationOffset, 180);
                        break;

                    // South East Inner
                    case Directions.SouthEast:
                        UpdateMaterialAndRotation(ConnectableSettings.SingleColorMaterial, ConnectableSettings.GlobalRotationOffset, 270);
                        break;

                    // South West Inner
                    case Directions.SouthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.SingleColorMaterial, ConnectableSettings.GlobalRotationOffset, 0);
                        break;
                    // North West Inner
                    case Directions.NorthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.SingleColorMaterial, ConnectableSettings.GlobalRotationOffset, 90);
                        break;

                    // Split NorthEast SouthEast
                    case Directions.NorthEast | Directions.SouthEast:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 0);
                        break;

                    // Split NorthEast SouthEast
                    case Directions.SouthEast | Directions.SouthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 90);
                        break;

                    // Split NorthEast SouthEast
                    case Directions.SouthWest | Directions.NorthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 180);
                        break;

                    // Split NorthEast SouthEast
                    case Directions.NorthWest | Directions.NorthEast:
                        UpdateMaterialAndRotation(ConnectableSettings.DoubleColorMaterial, ConnectableSettings.GlobalRotationOffset, 270);
                        break;

                    // Quad Cases
                    case Directions.North | Directions.South:
                    case Directions.North | Directions.South | Directions.NorthEast:
                    case Directions.North | Directions.South | Directions.SouthEast:
                    case Directions.North | Directions.South | Directions.SouthWest:
                    case Directions.North | Directions.South | Directions.NorthWest:
                    case Directions.North | Directions.South | Directions.NorthEast | Directions.SouthEast:
                    case Directions.North | Directions.South | Directions.NorthEast | Directions.SouthEast |
                         Directions.NorthWest:
                    case Directions.North | Directions.South | Directions.NorthEast | Directions.SouthEast |
                         Directions.SouthWest:
                    case Directions.North | Directions.South | Directions.NorthEast | Directions.SouthEast |
                         Directions.East:
                    case Directions.North | Directions.South | Directions.NorthWest | Directions.SouthWest:
                    case Directions.North | Directions.South | Directions.NorthWest | Directions.SouthWest |
                         Directions.NorthEast:
                    case Directions.North | Directions.South | Directions.NorthWest | Directions.SouthWest |
                         Directions.SouthEast:
                    case Directions.North | Directions.South | Directions.NorthWest | Directions.SouthWest |
                         Directions.West:
                    case Directions.North | Directions.East | Directions.West | Directions.NorthEast |
                         Directions.SouthWest | Directions.NorthWest:
                    case Directions.East | Directions.South | Directions.West | Directions.NorthEast |
                         Directions.SouthEast | Directions.SouthWest:
                    case Directions.North | Directions.East | Directions.South | Directions.NorthEast |
                         Directions.SouthEast | Directions.NorthWest:
                    case Directions.North | Directions.South | Directions.West | Directions.SouthEast |
                         Directions.SouthWest | Directions.NorthWest:
                    case Directions.East | Directions.West:
                    case Directions.East | Directions.West | Directions.NorthEast:
                    case Directions.North | Directions.NorthEast | Directions.SouthEast:
                    case Directions.East | Directions.SouthEast | Directions.West:
                    case Directions.East | Directions.West | Directions.SouthWest:
                    case Directions.East | Directions.West | Directions.NorthWest:
                    case Directions.East | Directions.West | Directions.NorthEast | Directions.NorthWest:
                    case Directions.East | Directions.West | Directions.NorthEast | Directions.NorthWest |
                         Directions.SouthEast:
                    case Directions.East | Directions.West | Directions.NorthEast | Directions.NorthWest |
                         Directions.SouthWest:
                    case Directions.East | Directions.West | Directions.NorthEast | Directions.NorthWest |
                         Directions.North:
                    case Directions.East | Directions.West | Directions.SouthEast | Directions.SouthWest:
                    case Directions.East | Directions.West | Directions.SouthEast | Directions.SouthWest |
                         Directions.NorthEast:
                    case Directions.East | Directions.West | Directions.SouthEast | Directions.SouthWest |
                         Directions.NorthWest:
                    case Directions.East | Directions.West | Directions.SouthEast | Directions.SouthWest |
                         Directions.South:
                    case Directions.North | Directions.South | Directions.West | Directions.NorthEast |
                         Directions.SouthEast | Directions.SouthWest | Directions.NorthWest:
                    case Directions.East | Directions.South | Directions.West | Directions.NorthEast |
                         Directions.SouthEast | Directions.SouthWest | Directions.NorthWest:
                    case Directions.North | Directions.East | Directions.West | Directions.NorthEast |
                         Directions.SouthEast | Directions.SouthWest | Directions.NorthWest:
                    case Directions.East | Directions.West | Directions.NorthEast | Directions.SouthEast |
                         Directions.SouthWest | Directions.NorthWest:
                    case Directions.North | Directions.East | Directions.South | Directions.NorthEast |
                         Directions.SouthEast | Directions.SouthWest | Directions.NorthWest:
                    case Directions.North | Directions.South | Directions.NorthEast | Directions.SouthEast |
                         Directions.SouthWest | Directions.NorthWest:
                    case Directions.NorthWest | Directions.NorthEast | Directions.South | Directions.SouthWest |
                         Directions.West:
                    case Directions.North | Directions.NorthEast | Directions.East | Directions.SouthEast |
                         Directions.SouthWest | Directions.NorthWest:
                    case Directions.NorthEast | Directions.East | Directions.SouthEast | Directions.West:
                    case Directions.NorthWest | Directions.North | Directions.NorthEast | Directions.SouthEast |
                         Directions.SouthWest:
                    case Directions.NorthEast | Directions.East | Directions.SouthWest | Directions.West:
                    case Directions.NorthWest | Directions.North | Directions.NorthEast | Directions.South:
                    case Directions.North | Directions.NorthEast | Directions.East | Directions.SouthEast |
                         Directions.SouthWest:
                        UpdateMaterialAndRotation(ConnectableSettings.QuadColorMaterial, ConnectableSettings.GlobalRotationOffset, 0);
                        break;

                    default:

                        // Woops! the surrounding tile combination you encountered hasn't been added to the switch yet, do so in the cases above.
                        Debug.Log(
                            $"{gameObject.name}: No fitting connection setup yet. Please paste \"case Directions.{adjacentData.directions.ToString().Replace(", ", " | Directions.")}:\" where it should be in Connectable.cs");
                        break;
                }

                break;

            // Connecting meshes
            case ConnectableType.MeshBased:
                switch (adjacentData.directions & ~(Directions.NorthEast | Directions.SouthEast |
                                                    Directions.SouthWest | Directions.NorthWest))
                {
                    // North Orientation
                    case Directions.North:
                        UpdateMeshAndRotation(ConnectableSettings.EndMesh,
                            ConnectableSettings.EndRotationOffset + new Vector3(0, 0, 90));
                        break;
                    // East Orientation
                    case Directions.East:
                        UpdateMeshAndRotation(ConnectableSettings.EndMesh,
                            ConnectableSettings.EndRotationOffset + new Vector3(0, 0, 180));
                        break;
                    // South Orientation
                    case Directions.South:
                        UpdateMeshAndRotation(ConnectableSettings.EndMesh,
                            ConnectableSettings.EndRotationOffset + new Vector3(0, 0, 270));
                        break;
                    // West Orientation
                    case Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.EndMesh,
                            ConnectableSettings.EndRotationOffset + new Vector3(0, 0, 0));
                        break;

                    // North South Orientation
                    case Directions.North | Directions.South:
                        UpdateMeshAndRotation(ConnectableSettings.StraightMesh,
                            ConnectableSettings.StraightRotationOffset + new Vector3(0, 0, 90));
                        break;
                    case Directions.East | Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.StraightMesh,
                            ConnectableSettings.StraightRotationOffset + new Vector3(0, 0, 0));
                        break;


                    // North East Orientation
                    case Directions.North | Directions.East:
                        UpdateMeshAndRotation(ConnectableSettings.CornerMesh,
                            ConnectableSettings.CornerRotationOffset + new Vector3(0, 0, 0));
                        break;
                    // East South Orientation
                    case Directions.East | Directions.South:
                        UpdateMeshAndRotation(ConnectableSettings.CornerMesh,
                            ConnectableSettings.CornerRotationOffset + new Vector3(0, 0, 90));
                        break;
                    // South West Orientation
                    case Directions.South | Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.CornerMesh,
                            ConnectableSettings.CornerRotationOffset + new Vector3(0, 0, 180));
                        break;
                    // North West Orientation
                    case Directions.North | Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.CornerMesh,
                            ConnectableSettings.CornerRotationOffset + new Vector3(0, 0, 270));
                        break;

                    // North East South Orientation
                    case Directions.North | Directions.East | Directions.South:
                        UpdateMeshAndRotation(ConnectableSettings.SplitMesh,
                            ConnectableSettings.SplitRotationOffset + new Vector3(0, 0, 180));
                        break;
                    // East South West Orientation
                    case Directions.East | Directions.South | Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.SplitMesh,
                            ConnectableSettings.SplitRotationOffset + new Vector3(0, 0, 270));
                        break;
                    // North South West Orientation
                    case Directions.North | Directions.South | Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.SplitMesh,
                            ConnectableSettings.SplitRotationOffset + new Vector3(0, 0, 0));
                        break;
                    // North East West Orientation
                    case Directions.North | Directions.East | Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.SplitMesh,
                            ConnectableSettings.SplitRotationOffset + new Vector3(0, 0, 90));
                        break;

                    // North East South West Orientation
                    case Directions.North | Directions.East | Directions.South |
                         Directions.West:
                        UpdateMeshAndRotation(ConnectableSettings.CrossMesh, ConnectableSettings.SplitRotationOffset);
                        break;

                    default:
                        Debug.LogWarning($"{gameObject.name}: No fitting connections for: {adjacentData.directions}");
                        break;
                }

                break;
        }
    }

    private void UpdateMeshAndRotation(Mesh mesh, Vector3 rotation)
    {
        meshFilter.sharedMesh = mesh;
        transform.eulerAngles = rotation;
    }

    private void UpdateMaterialAndRotation(Material material, Vector3 rotationOffset, float rotation)
    {
        meshRenderer.sharedMaterial = material;
        transform.eulerAngles = rotationOffset + new Vector3(0, 0, -rotation);
    }

    private void UpdateAdjacent(Vector2Int offset, int adjacentConnectablesIndex)
    {
        Connectable adjacent = connectableGroup.GetConnectableAtPosition(position + offset, ConnectableSettings);
        adjacentData.adjacentConnectables[adjacentConnectablesIndex] =
            connectableGroup.GetConnectableAtPosition(position + offset);
        if (adjacent)
        {
            adjacentData.directions |= adjacentDictionary[offset];
            adjacentData.adjacentConnectables[adjacentConnectablesIndex] = adjacent;
        }
    }

    private void GetAdjacentsOnGrid()
    {
        if (!connectableGroup) connectableGroup = GetComponentInParent<ConnectableGroup>();
        if (connectableGroup.Grid.Length < 1) connectableGroup.UpdateGrid();

        if (!connectableGroup)
        {
            Debug.LogWarning("No ConnectableGroup parent found");
        }

        adjacentData.directions = Directions.None;
        Connectable adjacent;

        adjacentData.adjacentConnectables = new Connectable[8];

        for (int i = 0; i < adjacentDictionary.Count; i++)
        {
            UpdateAdjacent(adjacentDictionary.ElementAt(i).Key, i);
        }
    }
}