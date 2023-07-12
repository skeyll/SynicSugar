using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
using UnityEngine;
using System;

namespace  SynicSugar.Samples {
    public class AuthLogin : MonoBehaviour {
        [SerializeField] GameObject modeSelectCanvas;
        [SerializeField] bool needResultDetail;
        void Start(){
            bool hasLogin = EOSAuthentication.HasLoggedinEOSWithConnect();
            
            this.gameObject.SetActive(!hasLogin);
            modeSelectCanvas.SetActive(hasLogin);
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDeviceID(){
            LoginWithDeviceIDRequest().Forget();
        }
        public async UniTask LoginWithDeviceIDRequest(){
            this.gameObject.SetActive(false);
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            EOSDebug.Instance.Log("Trt to connect EOS with deviceID.");

            if(needResultDetail){
                string result = await EOSAuthentication.LoginWithDeviceID(cancellationToken, true);
      
                EOSDebug.Instance.Log($"Login Result is {result}");
                    
                if(result == "Success"){
                    modeSelectCanvas.SetActive(true);
                    EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                    return;
                }
            }else{
                //Just result true or false
                bool isSuccess = await EOSAuthentication.LoginWithDeviceID(cancellationToken);
                EOSDebug.Instance.Log($"DeviceLogin: {isSuccess}");

                if(isSuccess){
                    modeSelectCanvas.SetActive(true);
                    EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                    return;
                }
            }
            //False
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
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
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
