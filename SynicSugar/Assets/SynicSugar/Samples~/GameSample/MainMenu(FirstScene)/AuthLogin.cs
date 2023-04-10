using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
using UnityEngine.UI;
using UnityEngine;

namespace  SynicSugar.Samples {
    public class AuthLogin : MonoBehaviour {
        [SerializeField] GameObject modeSelectCanvas;
        CancellationTokenSource cancellationToken;
        [SerializeField] GameObject authPrefab;
        GameObject authContainer;
        void Start() {
            if(EOSAuthentication.Instance == null){
                authContainer = Instantiate(authPrefab);
            }
        }
        void OnDestroy(){
            if(cancellationToken != null){
                cancellationToken.Cancel();
            }
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDeviceID(){
            LoginWithDeviceIDRequest().Forget();
        }
        public async UniTask LoginWithDeviceIDRequest(){
            this.gameObject.SetActive(false);
            cancellationToken = new CancellationTokenSource();
            EOSDebug.Instance.Log("Trt to connect EOS with deviceID.");
            bool isSuccess = await EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken);

            if(isSuccess){
                modeSelectCanvas.SetActive(true);
                EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }
            this.gameObject.SetActive(true);
            EOSDebug.Instance.Log("Fault EOS authentication.");
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDevelopperTool(){
            LoginWithDevToolRequest().Forget();
        }
        public async UniTask LoginWithDevToolRequest(){
            this.gameObject.SetActive(false);
            cancellationToken = new CancellationTokenSource();
            EOSDebug.Instance.Log("Trt to connect EOS with DevTool");
            bool isSuccess = await DevLogin.Instance.LoginWithDevelopperLogin(cancellationToken);

            if(isSuccess){
                modeSelectCanvas.SetActive(true);
                EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }
            this.gameObject.SetActive(true);
            EOSDebug.Instance.Log("Fault EOS authentication.");
        }
    }
}
