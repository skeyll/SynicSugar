using UnityEditor;
using UnityEngine;
namespace SynicSugar.Editor {
    public class Reimporter : EditorWindow {
        private const string DevPath = "Assets/SynicSugar/Editor/Extensions/SynicSugarPrefabs.asset";
        private const string LibraryPath = "Packages/net.skeyll.synicsugar/Editor/Extensions/SynicSugarPrefabs.asset";

        [MenuItem("Tools/SynicSugar/ReimportEOSManager")]
        static void ReimportEOSManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(LibraryPath);
            if(menu is null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(DevPath);
            }
            if(menu is null){
                Debug.Log("EOSManager's path is not found.");
                return;
            }

            Component[] components = menu.EOSManager.GetComponentsInChildren<Component>();

            foreach (Component component in components){
                if (component != null && component.GetType() != typeof(Transform)){
                    MonoScript script = MonoScript.FromMonoBehaviour(component as MonoBehaviour);
                    if (script != null){
                        string path = AssetDatabase.GetAssetPath(script);
                        if (!string.IsNullOrEmpty(path)){
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                            Debug.Log($"Reimported: {path}");
                        }else{
                            Debug.LogWarning($"Could not find asset path for script: {script.name}");
                        }
                    }
                }
            }
            Debug.Log("EOSManager is reimported!");
        }
        [MenuItem("Tools/SynicSugar/ReimportNetworkManager")]
        static void ReimportNetworkManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(LibraryPath);
            if(menu is null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(DevPath);
            }
            if(menu is null){
                Debug.Log("NetworkManager's path is not found.");
                return;
            }

            Component[] components = menu.NetWorkManager.GetComponentsInChildren<Component>();

            foreach (Component component in components){
                if (component != null && component.GetType() != typeof(Transform)){
                    MonoScript script = MonoScript.FromMonoBehaviour(component as MonoBehaviour);
                    if (script != null){
                        string path = AssetDatabase.GetAssetPath(script);
                        if (!string.IsNullOrEmpty(path)){
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                            Debug.Log($"Reimported: {path}");
                        }else{
                            Debug.LogWarning($"Could not find asset path for script: {script.name}");
                        }
                    }
                }
            }
            Debug.Log("NetworkManager is reimported!");
        }
    }
}