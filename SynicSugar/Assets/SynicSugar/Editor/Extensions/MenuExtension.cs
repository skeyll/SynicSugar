using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace SynicSugar.Editor {
    public class Menu : MonoBehaviour {
        [MenuItem("GameObject/SynicSugar/EOSManager")]
        static void GenerateEOSManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.LibraryPath);
            if(menu is null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.DevPath);
            }
            GameObject gameObject = GameObject.Instantiate(menu.EOSManager);
            gameObject.name = "EOSManager";
        }
        [MenuItem("GameObject/SynicSugar/NetworkManager")]
        static void GenerateNetworkManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.LibraryPath);
            if(menu is null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(AssetPath.DevPath);
            }
            GameObject gameObject = GameObject.Instantiate(menu.NetWorkManager);
            gameObject.name = "NetworkManager";
        }
    }
}
#endif
