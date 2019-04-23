using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Connectable Data", menuName = "SS3D/Connectable Data")]
public class ConnectableSettings : ScriptableObject
{
    public ConnectableType connectableType;

    public Vector3 GlobalRotationOffset;
    
    public Mesh EndMesh;
    public Vector3 EndRotationOffset;

    public Mesh StraightMesh;
    public Vector3 StraightRotationOffset;

    public Mesh CornerMesh;
    public Vector3 CornerRotationOffset;

    public Mesh SplitMesh;
    public Vector3 SplitRotationOffset;

    public Mesh CrossMesh;
    public Vector3 CrossRotationOffset;
 
    public Material QuadColorMaterial;
    public Material TriColorMaterial;
    public Material DoubleColorMaterial;
    public Material SingleColorMaterial;
    public Material NoColorMaterial;

    public List<ConnectableSettings> ConnectsWith;
}