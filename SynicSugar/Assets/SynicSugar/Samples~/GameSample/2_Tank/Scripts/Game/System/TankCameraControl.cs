using SynicSugar.P2P;
using UnityEngine;

namespace SynicSugar.Samples.Tank {
    public class TankCameraControl : MonoBehaviour {
        private static readonly Vector3 RELATIVE_POSITION = new Vector3(0f, 6f, -2f);
        private static readonly Vector3 RELATIVE_ROTATION = new Vector3(25f, 0f, 0f);
        Camera mainCamera;
        int _freeCameraIndex;
        public int freeCameraIndex {
            get { return _freeCameraIndex; }
            private set { 
                if(value >= p2pInfo.Instance.AllUserIds.Count)
                    value = 0;

                _freeCameraIndex = value;
            }
        }
        void Awake(){
            mainCamera = Camera.main;
        }
        /// <summary>
        /// Follow local user as child.
        /// </summary>
        internal void SetFollowLocalTarget(){
            freeCameraIndex = p2pInfo.Instance.GetUserIndex();
            mainCamera.transform.SetParent(ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.LocalUserId).transform, false);

            mainCamera.transform.localPosition = RELATIVE_POSITION;
            mainCamera.transform.localRotation = Quaternion.Euler(RELATIVE_ROTATION);
        }
        /// <summary>
        /// Follow target as child.
        /// </summary>
        /// <param name="newTarget"></param>
        internal void SetFollowTarget(Transform newTarget, int newTargetIndex){
            freeCameraIndex = newTargetIndex;
            mainCamera.transform.SetParent(newTarget, false);

            mainCamera.transform.localPosition = RELATIVE_POSITION;
            mainCamera.transform.localRotation = Quaternion.Euler(RELATIVE_ROTATION);
        }
        /// <summary>
        /// Change camera target on Death.
        /// </summary>
        /// <param name="userIndex">To check to need to change target.</param>
        internal void SwitchTargetToNextSurvivor(int userIndex){
            //No need to change the target if new dead player is not current target.
            if(userIndex != freeCameraIndex){
                return;
            }
            int currentindex = freeCameraIndex;
            //1
            //index to back
            for(int i = currentindex; i < p2pInfo.Instance.AllUserIds.Count; i++){
                TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.AllUserIds[i]);
                //If the target is dead, skip the user
                if(player == null || !player.gameObject.activeSelf){
                    continue;
                }

                SetFollowTarget(player.transform, i);
                return;
            }
            //0 to index
            for(int i = 0; i < currentindex; i++){
                TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.AllUserIds[i]);
                if(player == null || !player.gameObject.activeSelf){
                    continue;
                }

                SetFollowTarget(player.transform, i);
                return;
            }
            //If cannot change target, change it to local player.
            SetFollowLocalTarget();
        }
    }
}