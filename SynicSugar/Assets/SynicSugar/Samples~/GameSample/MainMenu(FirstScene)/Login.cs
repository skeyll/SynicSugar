using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Login;
using UnityEngine;
namespace  SynicSugar.Samples {
    public class Login : MonoBehaviour {
        [SerializeField] GameObject modeSelectCanvas;
        [SerializeField] bool needResultDetail;
        void Start(){
            bool hasLogin = EOSConnect.HasLoggedinEOS();
        #if SYNICSUGAR_FPSTEST
            if(!hasLogin){
                //Set game FPS
                Application.targetFrameRate = 60;
            }
        #endif
            
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
            EOSDebug.Instance.Log("Trt to connect EOS with deviceID.");
            //(bool, Result)
            var result = await EOSConnect.LoginWithDeviceID("TestPlayer");
    
            if(result == Result.Success){
                modeSelectCanvas.SetActive(true);
                EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }

            //False
            this.gameObject.SetActive(true);
            EOSDebug.Instance.Log($"Fault EOS authentication. {result}");
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void DeleteDeviceID(){
            DeleteDeviceIDRequest().Forget();
        }
        public async UniTask DeleteDeviceIDRequest(){
            var result = await EOSConnect.DeleteDeviceID();
            if(result == Result.Success){
                EOSDebug.Instance.Log("Delete DeviceID: Success.");
                return;
            }
            EOSDebug.Instance.Log("Delete DeviceID: Failare.");
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
            Result result = await DevLogin.Instance.LoginWithDevelopperLogin(cancellationToken);

            if(result == Result.Success){
                modeSelectCanvas.SetActive(true);
                EOSDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }
            this.gameObject.SetActive(true);
            EOSDebug.Instance.Log("Fault EOS authentication.");
        }
    }
}