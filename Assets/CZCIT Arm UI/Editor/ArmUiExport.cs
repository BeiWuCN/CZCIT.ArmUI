using System;
using System.IO;
using UnityEditor;

namespace CZCIT.ArmUI.Editor
{
    public static class ArmUiExport
    {
        [MenuItem("Tools/CZCIT Arm UI/Export Package")]
        public static void Export()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string output = Path.Combine(desktop, "CZCIT-ArmUI-26.11.17.unitypackage");
            AssetDatabase.ExportPackage("Assets/CZCIT Arm UI", output, ExportPackageOptions.Recurse);
        }
    }
}
