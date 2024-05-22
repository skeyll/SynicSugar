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
            var createDeviceIdOptions = new CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

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
            EOSManager.Instance.StartConnectLoginWithDeviceToken("Guest", info => {
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
            await DeleteDeviceID();
            return (isSuccess, resultS);
        }
        /// <summary>
        /// Login with DeviceID. If success, return true.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<(bool isSuccess, Result detail)> LoginWithDeviceID(string displayName, CancellationToken token = default(CancellationToken)){
            bool needTryCatch = token == default;
            token = needTryCatch ? new CancellationTokenSource().Token : token;

            bool isSuccess = false;
            bool waitingAuth = true;
            Result resultS = Result.Canceled;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

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
            EOSManager.Instance.StartConnectLoginWithDeviceToken(displayName, info => {
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
            await DeleteDeviceID();
            return (isSuccess, resultS);
        }
        /// <summary>
        /// Delete any existing Device ID access credentials for the current user profile on the local device. <br />
        /// On Android and iOS devices, uninstalling the application will automatically delete any local Device ID credentials.<br />
        /// This doesn't means delete User on EOS. We can't delete users from EOS.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<(bool isSuccess, Result detail)> DeleteDeviceID(CancellationToken token = default(CancellationToken)){
            bool needTryCatch = token == default;
            token = needTryCatch ? new CancellationTokenSource().Token : token;

            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            DeleteDeviceIdOptions options = new DeleteDeviceIdOptions();
            Result resultS = Result.Canceled;
            bool finishDeleted = false;
            connectInterface.DeleteDeviceId(ref options, null, (ref DeleteDeviceIdCallbackInfo data) => {
                resultS = (Result)data.ResultCode;
                if(data.ResultCode != ResultE.Success){
                    Debug.Log("DeleteDeviceID: Failuer " + data.ResultCode);
                }
                finishDeleted = true;
            });

            if(needTryCatch){ 
                try{
                    await UniTask.WaitUntil(() => finishDeleted, cancellationToken: token);
                }catch(OperationCanceledException){
                    Debug.Log("DeleteDeviceID: Canceled.");
                    return (false, resultS);
                }
            }else{
                await UniTask.WaitUntil(() => finishDeleted, cancellationToken: token);
            }
            
            return (resultS == Result.Success, resultS);
        }
    }
}