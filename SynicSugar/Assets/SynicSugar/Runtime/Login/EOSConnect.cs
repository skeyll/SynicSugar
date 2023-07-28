using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.Connect;
using System;
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
        public static async UniTask<(bool isSuccess, Result detail)> LoginWithDeviceID(CancellationToken token = default(CancellationToken)){
            bool needTryCatch = token == default;
            token = needTryCatch ? new CancellationTokenSource().Token : token;

            bool isSuccess = false;
            bool waitingAuth = true;
            Result resultS = Result.Canceled;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new Epic.OnlineServices.Connect.CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, 
                (ref CreateDeviceIdCallbackInfo data) => {
                    if (data.ResultCode == ResultE.Success) {
                        isSuccess = true;
#if SYNICSUGAR_LOG
                        Debug.Log("EOSConnect: Create new DeviceId");
#endif
                    }else if (data.ResultCode == ResultE.DuplicateNotAllowed){
                        isSuccess = true;                  
#if SYNICSUGAR_LOG
                        Debug.Log("EOSConnect: Already have DeviceID in local");
#endif    
                    }
                    resultS = (Result)data.ResultCode;
                    waitingAuth = false;
                });
            if(needTryCatch){
                try{
                    await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token);
                }catch(OperationCanceledException){  
                    Debug.Log("LoginWithDeviceID: Cancel CreateDeviceId.");
                    return (false, resultS);
                }
            }else{
                await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token);
            }

            if(!isSuccess){
                Debug.Log("LoginWithDeviceID: can't get device id");
                return (false, resultS);
            }
            //Login
            waitingAuth = true;
            resultS = Result.Canceled;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken("UnityEditorLocalUser", info => {
#if SYNICSUGAR_LOG
                    Debug.Log(info.ResultCode);
#endif
                    isSuccess = (info.ResultCode == ResultE.Success);
                    resultS = (Result)info.ResultCode;
                    waitingAuth = false;
                });

            if(needTryCatch){     
                try{
                    await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token);
                }catch(OperationCanceledException){
                    Debug.Log("LoginWithDeviceID: Cancel StartConnectLoginWithDeviceToken.");
                    return (false, resultS);
                }
            }else{
                await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token);
            }
            return (isSuccess, resultS);
        }
    }
}