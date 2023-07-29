using UnityEngine;

namespace SynicSugar {
    public class EOSManagerInstanceManger : MonoBehaviour {
#region Singleton
        private EOSManagerInstanceManger(){}
        private static EOSManagerInstanceManger Instance { get; set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
    }
}