using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace SynicSugar.Editor {
    public class ReImporter : MonoBehaviour {
        [MenuItem("Tools/SynicSugar/ReimportEOSManager")]
        static void ReimportEOSManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.LibraryPath);
            if(menu is null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.DevPath);
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
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.LibraryPath);
            if(menu is null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.DevPath);
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
#endif