using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Screenshot
{
#if UNITY_EDITOR
  [MenuItem("Utilities/Take Screenshot")]
  public static void Save()
  {
    string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
    string[] projectPath = Application.dataPath.Split('/');
    string projectName = projectPath[projectPath.Length - 2];
    path += string.Format("/{0}-shot-{1}.png", projectName, System.DateTime.Now.ToString("MM.dd.HH.mm.ss"));
    ScreenCapture.CaptureScreenshot(path, 2);
  }
#endif
}