using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
using UnityEngine;
namespace  SynicSugar.Samples 
{
    public class Login : MonoBehaviour 
    {
        [SerializeField] GameObject modeSelectCanvas;
        [SerializeField] bool needResultDetail;
        private void Start()
        {
        #if SYNICSUGAR_FPSTEST
            if(!SynicSugarManger.Instance.State.IsLoggedIn)
            {
                //Set game FPS
                Application.targetFrameRate = 60;
            }
        #endif
            
            this.gameObject.SetActive(!SynicSugarManger.Instance.State.IsLoggedIn);
            modeSelectCanvas.SetActive(SynicSugarManger.Instance.State.IsLoggedIn);
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDeviceID()
        {
            LoginWithDeviceIDRequest().Forget();
        }
        public async UniTask LoginWithDeviceIDRequest()
        {
            this.gameObject.SetActive(false);
            SynicSugarDebug.Instance.Log("Trt to connect EOS with deviceID.");
            //(bool, Result)
            var result = await SynicSugarAuthentication.Login("TestPlayer");
    
            if(result == Result.Success)
            {
                modeSelectCanvas.SetActive(true);
                SynicSugarDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!. id: " + SynicSugarManger.Instance.LocalUserId.ToString());
                return;
            }

            //False
            this.gameObject.SetActive(true);
            SynicSugarDebug.Instance.Log($"Fault EOS authentication. {result}");
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void DeleteDeviceID()
        {
            DeleteDeviceIDRequest().Forget();
        }
        public async UniTask DeleteDeviceIDRequest()
        {
            var result = await SynicSugarAccount.DeleteAccount();
            if(result == Result.Success)
            {
                SynicSugarDebug.Instance.Log("Delete DeviceID: Success.");
                return;
            }
            SynicSugarDebug.Instance.Log("Delete DeviceID: Failare.");
        }
        /// <summary>
        /// For button event
        /// </summary>
        public void LoginWithDevelopperTool()
        {
            LoginWithDevToolRequest().Forget();
        }
        public async UniTask LoginWithDevToolRequest()
        {
            this.gameObject.SetActive(false);
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            SynicSugarDebug.Instance.Log("Trt to connect EOS with DevTool");
            Result result = await DevLogin.Instance.LoginWithDevelopperLogin(cancellationToken);

            if(result == Result.Success)
            {
                modeSelectCanvas.SetActive(true);
                SynicSugarDebug.Instance.Log("SUCCESS EOS AUTHENTHICATION!.");
                return;
            }
            this.gameObject.SetActive(true);
            SynicSugarDebug.Instance.Log("Fault EOS authentication.");
        }
    }
}