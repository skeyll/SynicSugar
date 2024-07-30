using UnityEngine;

namespace SynicSugar.Samples.Tank {
    public class TankCameraControl : MonoBehaviour {
        private static readonly Vector3 RELATIVE_POSITION = new Vector3(0f, 5f, 1f);
        private static readonly Vector3 RELATIVE_ROTATION = new Vector3(45f, 0f, 0f);
        Camera mainCamera;
        void Awake(){
            mainCamera = Camera.main;
        }
        /// <summary>
        /// Follow target as child.
        /// </summary>
        /// <param name="newTarget"></param>
        internal void SetFollowTarget(Transform newTarget){
            mainCamera.transform.SetParent(newTarget, false);

            mainCamera.transform.localPosition = RELATIVE_POSITION;
            mainCamera.transform.localRotation = Quaternion.Euler(RELATIVE_ROTATION);
        }
    }
}