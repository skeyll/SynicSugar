using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.Auth;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;

namespace  SynicSugar.Samples 
{
    public class DevLogin : MonoBehaviour 
    {
    #region Singleton Instance
        public static DevLogin Instance { get; private set; }
        private void Awake() 
        {
            if( Instance != null ) 
            {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            portI.text = "7777";
        }
        private void OnDestroy() 
        {
            if( Instance == this ) 
            {
                Instance = null;
            }
        }
    #endregion
        [SerializeField] private InputField portI, nameI;
        public async UniTask<Result> LoginWithDevelopperLogin(CancellationTokenSource token)
        {
            if(string.IsNullOrEmpty(portI.text) || string.IsNullOrEmpty(nameI.text))
            {
                Debug.Log("LoginWithDevelopperLogin: PortID or Name is empty.");
                return Result.InvalidParameters;
            }

            Result resultS = Result.Canceled;
            bool waitingAuth = true;
            EOSManager.Instance.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer, $"localhost:{portI.text}", nameI.text, info =>
            {
                Debug.Log(info.ResultCode);
                Debug.Log(info.LocalUserId);
                resultS = (Result)info.ResultCode;
                waitingAuth = false;
            });
            await UniTask.WaitUntil(() => !waitingAuth, cancellationToken: token.Token);
            return resultS;
        }
    }
}