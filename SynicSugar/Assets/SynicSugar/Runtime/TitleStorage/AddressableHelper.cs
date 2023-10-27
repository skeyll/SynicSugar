#if SYNICSUGAR_ADDRESSABLE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace SynicSugar {
    internal static class AddressableHelper {
        public static bool Exist<T>(string target){
            foreach (var local in Addressables.ResourceLocators){
                local.Locate(target, typeof(T), out IList<IResourceLocation> resourceLocations);
                if (resourceLocations != null){
                    if (resourceLocations.Count > 1) {
                        Debug.Log($"{target} is duplicated: {resourceLocations.Count}.");
                    }
                    return true;
                }
            }
            return false;
        }
        static List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
        /// <summary>
        /// Load object as AAS and Release all on SceneLoad.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="address">target</param>
        /// <returns></returns>
        internal static async UniTask<T> LoadAddressableAsync<T>(string address){
            var loadedObject = Addressables.LoadAssetAsync<T>(address);

            //For error handling
            if(loadedObject.Status == AsyncOperationStatus.Failed){
                return loadedObject.Result;
            }

            await loadedObject.ToUniTask();
            
            handles.Add(loadedObject);

            return loadedObject.Result;
        }
        internal static void ReleaseUsedResources() {
            for (int i = 0; i < handles.Count;  i++) {
                Addressables.Release(handles[i]);
            }
            handles.Clear();
        }
    }
}
#endif