using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace SynicSugar.Editor {
    public class Menu : MonoBehaviour {
        private const string DevPath = "Assets/SynicSugar/Editor/Extensions/SynicSugarPrefabs.asset";
        private const string LibraryPath = "Packages/net.skeyll.synicsugar/Editor/Extensions/SynicSugarPrefabs.asset";

        [MenuItem("GameObject/SynicSugar/EOSManager")]
        static void GenerateEOSManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(LibraryPath);
            if(menu == null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(DevPath);
            }
            GameObject gameObject = GameObject.Instantiate(menu.EOSManager);
            gameObject.name = "EOSManager";
        }
        [MenuItem("GameObject/SynicSugar/NetworkManager")]
        static void GenerateNetworkManager(){
            SynicSugarMenuScripatable menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(LibraryPath);
            if(menu == null){
                menu = AssetDatabase.LoadAssetAtPath<SynicSugarMenuScripatable>(DevPath);
            }
            GameObject gameObject = GameObject.Instantiate(menu.NetWorkManager);
            gameObject.name = "NetworkManager";
        }
    }
}
#endif
