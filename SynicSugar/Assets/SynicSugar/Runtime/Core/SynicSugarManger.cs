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
            CoreFactory = new EOSCoreFactory();
            DontDestroyOnLoad(this);
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
        public readonly SynicSugarState State = new SynicSugarState();
        internal SynicSugarCoreFactory CoreFactory { get; private set; }
        /// <summary>
        /// Get core name.
        /// </summary>
        /// <returns>Default factory is EOSDefault to use EOS.</returns>
        public string GetFactoryName(){
            return CoreFactory.CoreName;
        }
        void SetCoreFactory(){
            CoreFactory = new EOSCoreFactory();
        }
    }
}