using UnityEngine;

namespace SynicSugar.Samples.Tank {
    public class CameraControl : MonoBehaviour {
        private static readonly Vector3 RELATIVE_POSITION = new Vector3(0f, 5f, -10f);
        private static readonly Vector3 RELATIVE_ROTATION = new Vector3(45f, 0f, 0f);
        /// <summary>
        /// Follow target as child.
        /// </summary>
        /// <param name="newTarget"></param>
        internal void SetFollowTarget(Transform newTarget){
            transform.SetParent(newTarget, false);

            transform.localPosition = RELATIVE_POSITION;
            transform.localRotation = Quaternion.Euler(RELATIVE_ROTATION);
        }
    }
}