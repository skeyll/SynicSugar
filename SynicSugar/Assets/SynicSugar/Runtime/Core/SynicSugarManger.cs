using UnityEngine;
using MemoryPack;
#if UNITY_EDITOR
using System; //For Cleanup event.
#endif

namespace SynicSugar {
    [DefaultExecutionOrder(-75)]
    public sealed class SynicSugarManger : MonoBehaviour {
#region Singleton
        public static SynicSugarManger Instance { get; private set; }
        void Awake() {
            Logger.Log("SynicSugarManger", "Start initialization of SynicSugarManger.");
            if( Instance != null ) {
                Destroy( gameObject );
                Logger.Log("SynicSugarManger", "Discard this instance since SynicSugarManger already exists.");
                return;
            }

            Instance = this;
            MemoryPackFormatterProvider.Register(new UserIdFormatter());
            CoreFactory = new EOSCoreFactory();
            State = new SynicSugarState();
            DontDestroyOnLoad(this);
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            #if UNITY_EDITOR
                CleanupForEditor?.Invoke();
                CleanupForEditor = null;
            #endif
            }
        }
        void Start(){
            LocalUserId = UserId.GenerateOfflineUserId();
            Logger.Log("SynicSugarManger", $"Set OllineUserId. UserId: {LocalUserId}");
        }
    #if UNITY_EDITOR
        /// <summary>
        /// To clean up notify or event in Editor.
        /// </summary>
        public event Action CleanupForEditor;
    #endif
#endregion
        public SynicSugarState State { get; private set; }
        internal SynicSugarCoreFactory CoreFactory { get; private set; }
        public UserId LocalUserId { get; private set; }
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
                Logger.LogError("SetCoreFactory", "This call is invalid. This function must be called before the local user logs in a server.");
                return Result.InvalidAPICall;
            }

            CoreFactory = coreFactory.CreateInstance();
            Logger.Log("SetCoreFactory", $"Set new core factory. FactoryName: {CoreFactory.CoreName}");

            return Result.Success;
        }
        /// <summary>
        /// Set formal ID　after log in.
        /// </summary>
        /// <param name="userId"></param>
        internal void SetLocalUserId(UserId userId){
            Logger.Log("SetLocalUserId", $"The LocalUserId has changed. Previous UserId: {LocalUserId}, New UserId: {userId}");
            LocalUserId = userId;
        }
    }
}