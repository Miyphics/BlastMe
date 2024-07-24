using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

public static class EditorContextMenu
{
    [MenuItem("Assets/Edit with Notepad++ ^e")]
    private static bool EditWithNotepad()
    {
        string path;
        var obj = Selection.activeObject;
        if (obj == null)
            path = "Assets";
        else
            path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

        if (path != null && path.Length > 0)
        {
            if (File.Exists(path))
            {
                Process.Start(@"C:\Program Files\Notepad++\notepad++.exe", path);
                return true;
            }
        }
        else
        {
            Debug.LogWarning("Path is uncorrect");
        }

        return false;
    }
}
