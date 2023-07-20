using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.Connect;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.Login {
    public static class EOSConnect {
        public static bool HasLoggedinEOS(){
            return EOSManager.Instance.HasLoggedInWithConnect();
        }
        /// <summary>
        /// Login with DeviceID. If success, return true.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<(bool result, Result detail)> LoginWithDeviceID(CancellationTokenSource token = default(CancellationTokenSource)){
            bool isSuccess = false;
            bool waitingAuth = true;
            Result resultS = Result.Success;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new Epic.OnlineServices.Connect.CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, 
                (ref CreateDeviceIdCallbackInfo data) => {
                    if (data.ResultCode == ResultE.Success) {
                        isSuccess = true;
#if SYNICSUGAR_LOG
                        Debug.Log("EOS AUTH: Create new DeviceId");
#endif
                    }else if (data.ResultCode == ResultE.DuplicateNotAllowed){
                        isSuccess = true;                  
#if SYNICSUGAR_LOG
                        Debug.Log("EOS AUTH: Already create DeviceID");
#endif    
                    }
                    resultS = (Result)data.ResultCode;
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);

            if(!isSuccess){
                Debug.Log("EOS AUTH: can't get device id");
                return (false, resultS);
            }
            //Login
            waitingAuth = true;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken("UnityEditorLocalUser", info => {
#if SYNICSUGAR_LOG
                    Debug.Log(info.ResultCode);
#endif
                    isSuccess = (info.ResultCode == ResultE.Success);
                    resultS = (Result)info.ResultCode;
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);
            return (isSuccess, resultS);
        }
    }
}