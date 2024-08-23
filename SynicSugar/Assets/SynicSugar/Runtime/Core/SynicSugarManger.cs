using UnityEngine;

namespace SynicSugar {
    public sealed class SynicSugarManger : MonoBehaviour {
#region Singleton
        public static SynicSugarManger Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( gameObject );
                return;
            }
            Instance = this;
            SetCoreFactory();
            DontDestroyOnLoad(this);
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
        public readonly SynicSugarState State = new SynicSugarState();
        SynicSugarCoreFactory coreFactory;
        void SetCoreFactory(){
            coreFactory = new EOSCoreFactory();
        }
        internal SynicSugarCoreFactory GetCoreFactory(){
            return coreFactory;
        }
    }
}