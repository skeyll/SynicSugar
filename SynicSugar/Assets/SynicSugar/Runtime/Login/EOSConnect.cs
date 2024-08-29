using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.Connect;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.Login {
    public static class EOSConnect {
        /// <summary>
        /// Checks if the user has logged in to EOS　and whether a UserId has been set in EOSManager.
        /// </summary>
        /// <returns>Returns true if the user has successfully logged in with EOS Connect,
        /// otherwise returns false.</returns>
        [Obsolete("This is Obsolete. We can use SynicSugarManger.Instance.State.IsLoggedIn instead of this.")]
        public static bool HasLoggedinEOS(){
            return EOSManager.Instance.HasLoggedInWithConnect();
        }
        /// <summary>
        /// Login with DeviceID. If success, return true. <br />
        /// We can't use DeviceId directly for security. This id is saved secure pos like as Keystore.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [Obsolete("This is Obsolete. We can use SynicSugarAuthentication.Login in SynicSugar.Auth instead of this.")]
        public static async UniTask<Result> LoginWithDeviceID(CancellationToken token = default(CancellationToken)){
            bool finishCallback = false;
            Result result = Result.Canceled;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, OnCreateDeviceIdCallback);
                
            try{
                await UniTask.WaitUntil(() => finishCallback, cancellationToken: token);
            }catch(OperationCanceledException){  
                Debug.Log("LoginWithDeviceID: CreateDeviceId is canceled.");
                return Result.Canceled;
            }

            if(result != Result.Success){
                Debug.Log("LoginWithDeviceID: can't get device id");
                return result;
            }
            //Login
            finishCallback = false;
            result = Result.Canceled;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken("Guest", info => {
                    result = (Result)info.ResultCode;
                    finishCallback = true;
                });

            try{
                await UniTask.WaitUntil(() => finishCallback, cancellationToken: token);
            }catch(OperationCanceledException){
                Debug.Log("LoginWithDeviceID: Cancel StartConnectLoginWithDeviceToken.");
                return result;
            }
            
            if(result == Result.Success){   
                SynicSugarManger.Instance.SetLocalUserId(UserId.GetUserId(EOSManager.Instance.GetProductUserId()));
                SynicSugarManger.Instance.State.IsLoggedIn = true;
            }else{
                SynicSugarManger.Instance.State.IsLoggedIn = false;
            }
            return result;

            void OnCreateDeviceIdCallback(ref CreateDeviceIdCallbackInfo data){
                result = (Result)data.ResultCode;
                if (result is Result.Success or Result.DuplicateNotAllowed) {
                #if SYNICSUGAR_LOG
                    Debug.Log(result is Result.Success  ? "EOSConnect: Create new DeviceId" : "EOSConnect: Already have DeviceID in local");
                #endif
                    result = Result.Success;
                }
                finishCallback = true;
            }
        }

        /// <summary>
        /// Login with DeviceID. If success, return true. <br />
        /// We can't use DeviceId directly for security. This id is saved secure pos like as Keystore.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [Obsolete("This is Obsolete. We can use SynicSugarAuthentication.Login in SynicSugar.Auth instead of this.")]
        public static async UniTask<Result> LoginWithDeviceID(string displayName, CancellationToken token = default(CancellationToken)){
            bool finishCallback = false;
            Result result = Result.Canceled;
            //DeviceID
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            var createDeviceIdOptions = new CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };

            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, OnCreateDeviceIdCallback);
                
            try{
                await UniTask.WaitUntil(() => finishCallback, cancellationToken: token);
            }catch(OperationCanceledException){  
                Debug.Log("LoginWithDeviceID: CreateDeviceId is canceled.");
                return Result.Canceled;
            }

            if(result != Result.Success){
                Debug.Log("LoginWithDeviceID: can't get device id");
                return result;
            }
            //Login
            finishCallback = false;
            result = Result.Canceled;
            //Pass UserID on each Game.
            EOSManager.Instance.StartConnectLoginWithDeviceToken(displayName, info => {
                    result = (Result)info.ResultCode;
                    finishCallback = true;
                });

            try{
                await UniTask.WaitUntil(() => finishCallback, cancellationToken: token);
            }catch(OperationCanceledException){
                Debug.Log("LoginWithDeviceID: Cancel StartConnectLoginWithDeviceToken.");
                return result;
            }

            if(result == Result.Success){   
                SynicSugarManger.Instance.SetLocalUserId(UserId.GetUserId(EOSManager.Instance.GetProductUserId()));
                SynicSugarManger.Instance.State.IsLoggedIn = true;
            }else{
                SynicSugarManger.Instance.State.IsLoggedIn = false;
            }
            return result;

            void OnCreateDeviceIdCallback(ref CreateDeviceIdCallbackInfo data){
                result = (Result)data.ResultCode;
                if (result is Result.Success or Result.DuplicateNotAllowed) {
                #if SYNICSUGAR_LOG
                    Debug.Log(result is Result.Success  ? "EOSConnect: Create new DeviceId" : "EOSConnect: Already have DeviceID in local");
                #endif
                    result = Result.Success;
                }
                finishCallback = true;
            }
        }
        /// <summary>
        /// Delete any existing Device ID access credentials for the current user profile on the local device. <br />
        /// On Android and iOS devices, uninstalling the application will automatically delete any local Device ID credentials.<br />
        /// This doesn't means delete User on EOS. We can't delete users from EOS.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [Obsolete("This is Obsolete. We can use SynicSugarAccount.DeleteAccount in SynicSugar.Auth instead of this.")]
        public static async UniTask<Result> DeleteDeviceID(CancellationToken token = default(CancellationToken)){
            var connectInterface = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
            DeleteDeviceIdOptions options = new DeleteDeviceIdOptions();
            Result result = Result.Canceled;
            bool finishCallback = false;

            connectInterface.DeleteDeviceId(ref options, null, OnDeleteDeviceId);

            try{
                await UniTask.WaitUntil(() => finishCallback, cancellationToken: token);
            }catch(OperationCanceledException){
                Debug.Log("DeleteDeviceID: Canceled.");
                return Result.Canceled;
            }
            
            return result;

            void OnDeleteDeviceId(ref DeleteDeviceIdCallbackInfo data){
                result = (Result)data.ResultCode;
                if(result != Result.Success){
                    Debug.Log("DeleteDeviceID: Failure: " + data.ResultCode);
                }
                finishCallback = true;
            }
        }
    }
}