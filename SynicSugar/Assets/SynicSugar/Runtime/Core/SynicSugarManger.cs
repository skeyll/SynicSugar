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
        /// <summary>
        /// Change the process used as the core. Valid only if the local user is not logged in. <br />
        /// Default core is for EOS.
        /// </summary>
        /// <param name="coreFactory"></param>
        /// <returns></returns>
        public Result SetCoreFactory(SynicSugarCoreFactory coreFactory){
            if(State.IsLoggedIn){
            #if SYNICSUGAR_LOG
                Debug.Log("SetCoreFactory: Cannot change because this user are already logged in.");
            #endif
                return Result.InvalidAPICall;
            }

            CoreFactory = coreFactory.CreateInstance();
            return Result.Success;
        }
    }
}