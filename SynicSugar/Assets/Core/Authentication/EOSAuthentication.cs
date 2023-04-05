using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.Auth {
    public class EOSAuthentication : MonoBehaviour {
    #region Singleton Instance
        private EOSAuthentication(){}
        public static EOSAuthentication Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
    #endregion
        public async UniTask<bool> LoginWithDeviceID(CancellationTokenSource token){
            bool isSuccess = false;
            bool waitingAuth = true;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new Epic.OnlineServices.Connect.CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };
            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, 
                (ref CreateDeviceIdCallbackInfo data) => {
                    if (data.ResultCode == Result.Success) {
                        isSuccess = true;
                        Debug.Log("Create new DeviceId");
                    }else if (data.ResultCode == Result.DuplicateNotAllowed){
                        isSuccess = true;
                        Debug.Log("Already create DeviceID");
                    }
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);

            if(!isSuccess){
                return false;
            }
            //Login
            waitingAuth = true;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken("UnityEditorLocalUser", info => {
                    Debug.Log(info.ResultCode);
                    Debug.Log(info.LocalUserId);
                    isSuccess = (info.ResultCode == Result.Success);
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);
            return isSuccess;
        }
    }
}