
using System.IO;
using UnityEditor;

namespace SynicSugar.Build {
    public static class UnityBuildActions {
        const string k_PackageName = "SynicSugar";
        const string k_PackagePath = "SynicSugar";
        const string k_ExportPath = "Build";

        public static void BuildUnityPackage () {
            ExportPackage($"{k_ExportPath}/{k_PackageName}.unitypackage");
        }

        public static string ExportPackage (string exportPath) {
            // Ensure export path.
            var dir = new FileInfo(exportPath).Directory;
            if (dir != null && !dir.Exists) {
                dir.Create();
            }
 
            AssetDatabase.ExportPackage(
                $"Assets/{k_PackagePath}",
                exportPath,
                ExportPackageOptions.Recurse
            );

            return Path.GetFullPath(exportPath);
        }
   } 
}
