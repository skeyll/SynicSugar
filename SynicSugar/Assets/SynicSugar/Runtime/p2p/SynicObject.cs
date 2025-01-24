using System.Collections.Generic;
using UnityEngine;

namespace SynicSugar.P2P {
    public static class SynicObject {
        #region AllSpawn
        /// <summary>
        /// Spawn Gameobjects for all users include disconnected users.<br />
        /// If original has many components, this will becomes very heavy.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static List<GameObject> AllSpawn(GameObject original){
            Logger.Log("AllSpawn", $"Start AllSpawn for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.AllUserIds){
                objcs.Add(Instantiate(id, original));
            }
            return objcs; 
        }
        public static List<GameObject> AllSpawn(GameObject original, Transform parent){    
            Logger.Log("AllSpawn", $"Start AllSpawn for {original.name}"); 
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.AllUserIds){
                objcs.Add(Instantiate(id, original, parent));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawn(GameObject original, Transform parent, bool instantiateInWorldSpace){
            Logger.Log("AllSpawn", $"Start AllSpawn for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.AllUserIds){
                objcs.Add(Instantiate(id, original, parent, instantiateInWorldSpace));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation){
            Logger.Log("AllSpawn", $"Start AllSpawn for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.AllUserIds){
                objcs.Add(Instantiate(id, original, position, rotation));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent){
            Logger.Log("AllSpawn", $"Start AllSpawn for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.AllUserIds){
                objcs.Add(Instantiate(id, original, position, rotation, parent));
            }
            return objcs;
        }
        #endregion
        #region AllSpawnForCurrent
        /// <summary>
        /// Spawn Gameobjects for all CURRENT connected users.<br />
        /// If original has many components, this will becomes very heavy.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static List<GameObject> AllSpawnForCurrent(GameObject original){
            Logger.Log("AllSpawnForCurrent", $"Start AllSpawnForCurrent for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.CurrentAllUserIds){
                objcs.Add(Instantiate(id, original));
            }
            return objcs; 
        }
        public static List<GameObject> AllSpawnForCurrent(GameObject original, Transform parent){
            Logger.Log("AllSpawnForCurrent", $"Start AllSpawnForCurrent for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.CurrentAllUserIds){
                objcs.Add(Instantiate(id, original, parent));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawnForCurrent(GameObject original, Transform parent, bool instantiateInWorldSpace){
            Logger.Log("AllSpawnForCurrent", $"Start AllSpawnForCurrent for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.CurrentAllUserIds){
                objcs.Add(Instantiate(id, original, parent, instantiateInWorldSpace));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawnForCurrent(GameObject original, Vector3 position, Quaternion rotation){
            Logger.Log("AllSpawnForCurrent", $"Start AllSpawnForCurrent for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.CurrentAllUserIds){
                objcs.Add(Instantiate(id, original, position, rotation));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawnForCurrent(GameObject original, Vector3 position, Quaternion rotation, Transform parent){
            Logger.Log("AllSpawnForCurrent", $"Start AllSpawnForCurrent for {original.name}");
            List<GameObject> objcs = new List<GameObject>();
            foreach(UserId id in p2pInfo.Instance.userIds.CurrentAllUserIds){
                objcs.Add(Instantiate(id, original, position, rotation, parent));
            }
            return objcs;
        }
        #endregion
        #region Instantiate
        /// <summary>
        /// Call GameObject.Instantiate, then set user id to each compornent for Network. <br />
        /// If original has many components, this will becomes very heavy.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public static GameObject Instantiate(UserId id, GameObject original){
            Logger.Log("SynicObject.Instantiate", $"Instantiate object for {id}");
            GameObject obj = UnityEngine.Object.Instantiate (original);
            var nos = obj.GetComponents<INetworkOwner>();

            foreach(var i in nos){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Transform parent){
            Logger.Log("SynicObject.Instantiate", $"Instantiate object for {id}");
            GameObject obj = UnityEngine.Object.Instantiate (original, parent);
            var nos = obj.GetComponents<INetworkOwner>();
            foreach(var i in nos){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Transform parent, bool instantiateInWorldSpace){
            Logger.Log("SynicObject.Instantiate", $"Instantiate object for {id}");
            GameObject obj = UnityEngine.Object.Instantiate (original, parent, instantiateInWorldSpace);
            var nos = obj.GetComponents<INetworkOwner>();
            foreach(var i in nos){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Vector3 position, Quaternion rotation){
            Logger.Log("SynicObject.Instantiate", $"Instantiate object for {id}");
            GameObject obj = UnityEngine.Object.Instantiate (original, position, rotation);
            var nos = obj.GetComponents<INetworkOwner>();
            foreach(var i in nos){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Vector3 position, Quaternion rotation, Transform parent){
            Logger.Log("SynicObject.Instantiate", $"Instantiate object for {id}");
            GameObject obj = UnityEngine.Object.Instantiate (original, position, rotation, parent);
            var nos = obj.GetComponents<INetworkOwner>();
            foreach(var i in nos){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        #endregion
    }
}