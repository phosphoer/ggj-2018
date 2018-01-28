using UnityEngine;

[CreateAssetMenu(fileName = "new-flora", menuName = "BoatGame/Flora Definition")]
public class FloraDefinition : ScriptableObject
{
  public Mesh Mesh;
  public float ScaleMin = 1.0f;

  public float ScaleMax = 1.0f;

  public float RotationVariance = 1.0f;

  public float RaycastYOffset = -0.1f;

  [Range(0.0f, 1.0f)]
  public float SurfaceNormalWeight = 1.0f;

  public bool CanSpawnInWater = true;

  public float MinDistToWater = 1.0f;

  public Gradient WindWeightGradient;

  public Mesh GeneratedMesh
  {
    get
    {
      if (_generatedMesh == null)
      {
        _generatedMesh = Instantiate(Mesh);
        Vector3[] verts = _generatedMesh.vertices;
        Color[] colors = new Color[verts.Length];
        Bounds meshBounds = _generatedMesh.bounds;
        for (int i = 0; i < verts.Length; ++i)
        {
          colors[i] = WindWeightGradient.Evaluate(verts[i].y / meshBounds.size.y);
        }

        _generatedMesh.colors = colors;
      }

      return _generatedMesh;
    }
  }

  private Mesh _generatedMesh;
}