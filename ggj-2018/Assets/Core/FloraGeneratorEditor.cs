#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloraGenerator))]
[CanEditMultipleObjects]
public class FloraGeneratorEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    if (GUILayout.Button("Generate"))
    {
      foreach (Object obj in targets)
      {
        FloraGenerator floraGen = obj as FloraGenerator;
        if (floraGen != null)
        {
          floraGen.Generate();
        }
      }
    }

    if (GUILayout.Button("Destroy"))
    {
      foreach (Object obj in targets)
      {
        FloraGenerator floraGen = obj as FloraGenerator;
        if (floraGen != null)
        {
          floraGen.DestroyMesh();
        }
      }
    }
  }
}

#endif