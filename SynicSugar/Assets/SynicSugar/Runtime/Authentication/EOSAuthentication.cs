using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.Auth {
    public static class EOSAuthentication {
        /// <summary>
        /// Login with DeviceID. If success, return true.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<bool> LoginWithDeviceID(CancellationTokenSource token){
            bool isSuccess = false;
            bool waitingAuth = true;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new Epic.OnlineServices.Connect.CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, 
                (ref CreateDeviceIdCallbackInfo data) => {
                    if (data.ResultCode == Result.Success) {
                        isSuccess = true;
#if SYNICSUGAR_LOG
                        Debug.Log("EOS AUTH: Create new DeviceId");
#endif
                    }else if (data.ResultCode == Result.DuplicateNotAllowed){
                        isSuccess = true;                  
#if SYNICSUGAR_LOG
                        Debug.Log("EOS AUTH: Already create DeviceID");
#endif    
                    }
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);

            if(!isSuccess){
                Debug.Log("EOS AUTH: can't get device id");
                return false;
            }
            //Login
            waitingAuth = true;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken("UnityEditorLocalUser", info => {
#if SYNICSUGAR_LOG
                    Debug.Log(info.ResultCode);
#endif
                    isSuccess = (info.ResultCode == Result.Success);
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);
            return isSuccess;
        }
        /// <summary>
        /// Login with DeviceID. If success, return <i>Success</i>. If failure, return the error code's string.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="needResult"></param>
        /// <returns></returns>
        public static async UniTask<string> LoginWithDeviceID(CancellationTokenSource token, bool needResult){
            string result = Result.UserKicked.ToString();
            bool isSuccess = false;
            bool waitingAuth = true;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new Epic.OnlineServices.Connect.CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, 
                (ref CreateDeviceIdCallbackInfo data) => {
                    result = data.ResultCode.ToString();
                    if (data.ResultCode == Result.Success){
                        isSuccess = true;
#if SYNICSUGAR_LOG
                        Debug.Log("EOS AUTH: Create new DeviceId");
#endif
                    }else if (data.ResultCode == Result.DuplicateNotAllowed){
                        isSuccess = true;
#if SYNICSUGAR_LOG
                        Debug.Log("EOS AUTH: Already create DeviceID");
#endif
                    }
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);

            if(!isSuccess){
                Debug.Log("EOS AUTH: can't get device id");
                return result;
            }
            //Login
            waitingAuth = true;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken("UnityEditorLocalUser", info => {
#if SYNICSUGAR_LOG
                    Debug.Log(info.ResultCode);
#endif
                    result = info.ResultCode.ToString();
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);
            return result;
        }
    }
}