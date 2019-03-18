using System;
using System.Collections.Generic;
using Brisk.Entities;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class FieldOfView : MonoBehaviour
{
    [SerializeField] private NetEntity netEntity = null;
    [SerializeField] private float viewRange = 5;

    [Range(0, 360)]
    [Tooltip("The field of view width")]
    [SerializeField] private float viewConeWidth = 360;
    [Tooltip("Which layers this can't see through")]
    [SerializeField] private LayerMask obstacleMask = new LayerMask();
    [Tooltip("Raycasts per degree")]
    [SerializeField] private float meshResolution = .1f;
    [Tooltip("How \"deep\" the field of view penetrates the wall")]
    [SerializeField] private float maskCutawayDistance = 0.1f;


    [Header("Edge Detection")]
    [Range(0, 10)]
    [Tooltip("How many checks are done to make edges appear close to corners")]
    [SerializeField] private int edgeResolveIterations = 0;
    [Range(0, 10)]
    [Tooltip("How far a corner has to be to be checked behind another corner")]
    [SerializeField] private float edgeDistanceThreshold = 0;
    [Tooltip("The center of the field of view's actual wall detection")]
    [SerializeField] private Vector3 detectionOffset = Vector3.zero;

    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    
    private void Start()
    {
        if (!netEntity) Debug.LogError("Net Entity not set");

        // Destroy Field of View if it's not on the player's character
        if (!netEntity.Owner) gameObject.SetActive(false);

        viewMeshFilter = GetComponent<MeshFilter>();

        viewMesh = new Mesh { name = "View Mesh" };
        viewMeshFilter.mesh = viewMesh;
    }

    private void Update()
    {
        // Keep FOV mesh from rotating, reducing 'flickering'
        transform.rotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        DrawFieldOfView();
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void DrawFieldOfView()
    {
        var viewPoints = CalculateViewPoints();

        var vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (var i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i] - detectionOffset);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
    }


    // TODO: Implement RaycastCommand https://docs.unity3d.com/ScriptReference/RaycastCommand.html
    public List<Vector3> CalculateViewPoints()
    {
        var stepCount = Mathf.RoundToInt(viewConeWidth * meshResolution);
        var stepAngleSize = viewConeWidth / stepCount;

        var viewPoints = new List<Vector3>();
        var oldViewCast = new ViewCastInfo();
        for (var i = 0; i <= stepCount; i++)
        {
            var angle = transform.eulerAngles.y - viewConeWidth / 2 + stepAngleSize * i;
            var newViewCast = ViewCast(angle);

            if (i > 0)
            {
                var edgeDistanceThresholdExceeded =
                    Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit ||
                    (oldViewCast.hit && newViewCast.hit && oldViewCast.normal != newViewCast.normal &&
                     edgeDistanceThresholdExceeded))
                {
                    var edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        return viewPoints;
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        var minAngle = minViewCast.angle;
        var maxAngle = maxViewCast.angle;
        var minPoint = Vector3.zero;
        var maxPoint = Vector3.zero;

        for (var i = 0; i < edgeResolveIterations; i++)
        {
            var angle = (minAngle + maxAngle) / 2;
            var newViewCast = ViewCast(angle);

            var edgeDistanceThresholdExceeded =
                Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        var dir = DirectionFromAngle(globalAngle, true);

        if (Physics.Raycast((transform.position + detectionOffset), dir, out var hit, viewRange, obstacleMask))
        {
            // Applying maskCutawayDistance to make sure the walls remain visible.
            return new ViewCastInfo(true, hit.point + (-hit.normal * maskCutawayDistance), hit.distance, globalAngle,
                hit.normal);
        }

        return new ViewCastInfo(false, (transform.position + detectionOffset) + dir * viewRange, viewRange,
            globalAngle, hit.normal);
    }

    public struct EdgeInfo : IEquatable<EdgeInfo>
    {
        public readonly Vector3 pointA;
        public readonly Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }

        public bool Equals(EdgeInfo other)
        {
            return pointA.Equals(other.pointA) && pointB.Equals(other.pointB);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EdgeInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (pointA.GetHashCode() * 397) ^ pointB.GetHashCode();
        }

        public static bool operator ==(EdgeInfo left, EdgeInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EdgeInfo left, EdgeInfo right)
        {
            return !left.Equals(right);
        }
    }

    public struct ViewCastInfo : IEquatable<ViewCastInfo>
    {
        public readonly bool hit;
        public readonly Vector3 point;
        public readonly float distance;
        public readonly float angle;
        public readonly Vector3 normal;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle, Vector3 normal)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
            this.normal = normal;
        }

        public bool Equals(ViewCastInfo other)
        {
            return hit == other.hit && point.Equals(other.point) && distance.Equals(other.distance) && angle.Equals(other.angle) && normal.Equals(other.normal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ViewCastInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = hit.GetHashCode();
            hashCode = (hashCode * 397) ^ point.GetHashCode();
            hashCode = (hashCode * 397) ^ distance.GetHashCode();
            hashCode = (hashCode * 397) ^ angle.GetHashCode();
            hashCode = (hashCode * 397) ^ normal.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ViewCastInfo left, ViewCastInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ViewCastInfo left, ViewCastInfo right)
        {
            return !left.Equals(right);
        }
    }
}