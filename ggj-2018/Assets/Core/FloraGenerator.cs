using UnityEngine;
using System.Collections.Generic;

public class FloraGenerator : MonoBehaviour
{
  public Collider RaycastColliderOverride
  {
    get { return _raycastColliderOverride; }
    set { _raycastColliderOverride = value; }
  }

  [SerializeField]
  private FloraDefinition[] _floraDefinitions = null;

  [SerializeField]
  private MeshFilter _targetMeshFilter = null;

  [SerializeField]
  private Collider _raycastColliderOverride = null;

  [SerializeField]
  private float _radius = 50.0f;

  [SerializeField]
  private float _avoidInterlapRadius = 0.0f;

  [SerializeField]
  private int _floraCount = 50;

  [SerializeField]
  private bool _generateCollider = false;

  private Transform _helperTransform;

  private void Start()
  {
    Generate();
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.blue;

    Vector3 circlePos = Vector3.forward * _radius;
    for (int i = 0; i < 32; ++i)
    {
      Vector3 nextPos = Quaternion.Euler(0, 360 / 32.0f, 0) * circlePos;
      Gizmos.DrawLine(transform.position + circlePos, transform.position + nextPos);
      circlePos = nextPos;
    }
  }

  private void OnDestroy()
  {
    DestroyMesh();
  }

  public void DestroyMesh()
  {
#if UNITY_EDITOR
    if (Application.isPlaying)
    {
      Destroy(_targetMeshFilter.sharedMesh);
    }
    else
    {
      DestroyImmediate(_targetMeshFilter.sharedMesh);
    }
#else
      Destroy(_targetMeshFilter.sharedMesh);
#endif
  }

  public void Generate()
  {
    if (_helperTransform == null)
    {
      _helperTransform = new GameObject("helper-transform").transform;
      _helperTransform.parent = transform;
      _helperTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    if (_targetMeshFilter.sharedMesh != null)
    {
      DestroyMesh();
    }

    List<CombineInstance> combineList = new List<CombineInstance>();
    for (int i = 0; i < _floraCount; ++i)
    {
      // Choose a flora definition 
      int chosenFloraIndex = Random.Range(0, _floraDefinitions.Length);
      FloraDefinition chosenFlora = _floraDefinitions[chosenFloraIndex];

      // Choose a random position in a circle
      Vector3 pos = Vector3.one;
      pos += Quaternion.Euler(0, Random.value * 360, 0) * Vector3.forward * _radius * Random.value;

      // Adjust to not overlap other positions
      if (_avoidInterlapRadius > 0)
      {
        bool overlap = false;
        do
        {
          for (int j = 0; j < combineList.Count; ++j)
          {
            Vector3 otherPos = combineList[j].transform * new Vector4(0, 0, 0, 1);
            Vector3 delta = pos - otherPos;
            delta.y = 0;
            float dist = delta.magnitude;
            overlap |= dist < _avoidInterlapRadius;
          }

          if (overlap)
          {
            Vector3 dir = pos.normalized;
            if (dir.magnitude < 0.1f)
            {
              dir = Vector3.forward;
            }

            pos += dir * _avoidInterlapRadius;
          }
        } while (overlap && pos.magnitude < _radius);
      }

      // Raycast down to find the ground
      Vector3 worldPos = transform.TransformPoint(pos);
      Vector3 normal = Vector3.up;
      RaycastHit hitInfo;
      bool hit = false;
      if (RaycastColliderOverride != null)
      {
        Ray ray = new Ray(worldPos + Vector3.up, Vector3.down);
        if (RaycastColliderOverride.Raycast(ray, out hitInfo, 100.0f))
        {
          pos = transform.InverseTransformPoint(hitInfo.point);
          pos.y += chosenFlora.RaycastYOffset;

          normal = transform.InverseTransformDirection(hitInfo.normal);
          hit = true;
        }
      }
      else
      {
        if (Physics.Raycast(worldPos + Vector3.up, Vector3.down, out hitInfo, 100.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
          pos = transform.InverseTransformPoint(hitInfo.point);
          pos.y += chosenFlora.RaycastYOffset;

          normal = transform.InverseTransformDirection(hitInfo.normal);
          hit = true;
        }
      }

      // Position a helper transform at the point to get the transform matrix
      _helperTransform.position = pos;
      Quaternion xRot = Quaternion.Euler(Random.Range(-chosenFlora.RotationVariance, chosenFlora.RotationVariance), 0, 0);
      Quaternion yRot = Quaternion.Euler(0, Random.value * 360, 0);
      Quaternion zRot = Quaternion.Euler(0, 0, Random.Range(-chosenFlora.RotationVariance, chosenFlora.RotationVariance));
      Vector3 weightedNormal = Vector3.Lerp(Vector3.up, normal, chosenFlora.SurfaceNormalWeight);
      Quaternion surfaceRot = Quaternion.FromToRotation(Vector3.up, weightedNormal);
      _helperTransform.rotation = surfaceRot * yRot * xRot * zRot;
      _helperTransform.localScale = Vector3.one * Random.Range(chosenFlora.ScaleMin, chosenFlora.ScaleMax);

      // Set the mesh combine info for this instance
      if (hit && (chosenFlora.CanSpawnInWater || hitInfo.point.y > chosenFlora.MinDistToWater))
      {
        combineList.Add(new CombineInstance()
        {
          transform = _helperTransform.localToWorldMatrix,
          mesh = chosenFlora.GeneratedMesh
        });
      }
    }

    Mesh floraMesh = new Mesh();
    floraMesh.name = "generated-flora";
    floraMesh.CombineMeshes(combineList.ToArray(), true, true, false);
    _targetMeshFilter.sharedMesh = floraMesh;

    if (_generateCollider)
    {
      MeshCollider floraCollider = GetComponent<MeshCollider>();
      if (floraCollider == null)
      {
        floraCollider = gameObject.AddComponent<MeshCollider>();
        floraCollider.convex = false;
      }

      floraCollider.sharedMesh = floraMesh;
    }
  }
}