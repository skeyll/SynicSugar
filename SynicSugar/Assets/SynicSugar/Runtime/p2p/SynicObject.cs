using System.Collections.Generic;
using UnityEngine;

namespace SynicSugar.P2P {
    public static class SynicObject {
        #region AllSpawn
        /// <summary>
        /// Spawn all connected users as Gameobject.<br />
        /// If original has many components, this will becomes very heavy.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static List<GameObject> AllSpawn(GameObject original){
            List<GameObject> objcs = new List<GameObject>();
            //Local User
            objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original));

            //Remote Users
            foreach(UserId id in p2pInfo.Instance.userIds.RemoteUserIds){
                objcs.Add(Instantiate(id, original));
            }
            return objcs; 
        }
        public static List<GameObject> AllSpawn(GameObject original, Transform parent){     
            List<GameObject> objcs = new List<GameObject>();
            //Local User
            objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, parent));

            //Remote Users
            foreach(UserId id in p2pInfo.Instance.userIds.RemoteUserIds){
                objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, parent));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawn(GameObject original, Transform parent, bool instantiateInWorldSpace){
            List<GameObject> objcs = new List<GameObject>();
            //Local User
            objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, parent, instantiateInWorldSpace));

            //Remote Users
            foreach(UserId id in p2pInfo.Instance.userIds.RemoteUserIds){
                objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, parent, instantiateInWorldSpace));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation){
            List<GameObject> objcs = new List<GameObject>();
            //Local User
            objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, position, rotation));

            //Remote Users
            foreach(UserId id in p2pInfo.Instance.userIds.RemoteUserIds){
                objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, position, rotation));
            }
            return objcs;
        }
        public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent){
            List<GameObject> objcs = new List<GameObject>();
            //Local User
            objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, position, rotation, parent));
            
            //Remote Users
            foreach(UserId id in p2pInfo.Instance.userIds.RemoteUserIds){
                objcs.Add(Instantiate(p2pInfo.Instance.userIds.LocalUserId, original, position, rotation, parent));
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
            GameObject obj = UnityEngine.Object.Instantiate (original);
            var ids = obj.GetComponents<INetworkOwner>();
            foreach(var i in ids){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Transform parent){
            GameObject obj = UnityEngine.Object.Instantiate (original, parent);
            var ids = obj.GetComponents<INetworkOwner>();
            foreach(var i in ids){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Transform parent, bool instantiateInWorldSpace){
            GameObject obj = UnityEngine.Object.Instantiate (original, parent, instantiateInWorldSpace);
            var ids = obj.GetComponents<INetworkOwner>();
            foreach(var i in ids){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Vector3 position, Quaternion rotation){
            GameObject obj = UnityEngine.Object.Instantiate (original, position, rotation);
            var ids = obj.GetComponents<INetworkOwner>();
            foreach(var i in ids){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        public static GameObject Instantiate(UserId id, GameObject original, Vector3 position, Quaternion rotation, Transform parent){
            GameObject obj = UnityEngine.Object.Instantiate (original, position, rotation, parent);
            var ids = obj.GetComponents<INetworkOwner>();
            foreach(var i in ids){
                i.SetOwnerID(id);
            }
            return obj; 
        }
        #endregion
    }
}