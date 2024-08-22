using UnityEngine;

namespace SynicSugar {
    public sealed class SynicSugarStateManger : MonoBehaviour {
#region Singleton
        public static SynicSugarStateManger Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
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
        SynicSugarCoreFactoryBase coreFactory;
        void SetCoreFactory(){
            coreFactory = new SynicSugarCoreFactory();
        }
        internal SynicSugarCoreFactoryBase GetCoreFactory(){
            return coreFactory;
        }
    }
}