using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.Auth {
    public static class EOSAuthentication {
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
                        Debug.Log("EOS AUTH: Create new DeviceId");
                    }else if (data.ResultCode == Result.DuplicateNotAllowed){
                        isSuccess = true;
                        Debug.Log("EOS AUTH: Already create DeviceID");
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
                    Debug.Log(info.ResultCode);
                    isSuccess = (info.ResultCode == Result.Success);
                    waitingAuth = false;
                });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);
            return isSuccess;
        }
    }
}