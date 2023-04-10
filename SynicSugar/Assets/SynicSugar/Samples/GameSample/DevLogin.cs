using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.Auth;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;

namespace  SynicSugar.Samples {
    public class DevLogin : MonoBehaviour {
    #region Singleton Instance
        private DevLogin(){}
        public static DevLogin Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            portI.text = "7777";
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
    #endregion
        [SerializeField] InputField portI, nameI;
        public async UniTask<bool> LoginWithDevelopperLogin(CancellationTokenSource token){
            if(string.IsNullOrEmpty(portI.text) || string.IsNullOrEmpty(nameI.text)){
                return false;
            }

            bool isSuccess = false;
            bool waitingAuth = true;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer, $"localhost:{portI.text}", nameI.text, info =>{
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
