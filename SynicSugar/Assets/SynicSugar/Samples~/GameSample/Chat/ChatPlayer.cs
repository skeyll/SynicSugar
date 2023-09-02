using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples {
    //To use "ConnectHub.Instance.GetUserInstance<ChatPlayer>(UserID), make this [NetworkPlayer(true)].
    [NetworkPlayer(true)]
    public partial class ChatPlayer : MonoBehaviour {
        ChatSystemManager systemManager;
        bool isStressTesting;
        GameObject uiSets; //Holds buttons.
        [Synic(1)]
        public string LargePacket;
        [Synic(0)]
        public string Name;
        [Synic(0)]
        public int submitCount;
        void Start(){
            GameObject chatCanvas = GameObject.Find("Chat");
            systemManager = chatCanvas.GetComponent<ChatSystemManager>();
            Name = GenerateBasicName();

            // For example, suppose this local Player's ID is A. 
            // Player A can only control the character Instance that has OwnerUserID A.
            // If this instance holds A, this is isLocal for A.
            // On the other hand, when this has B, this is isRemote(=!isLocal) for A.
            if(isLocal){
                //Instantiate GUI 
                uiSets = Instantiate(systemManager.uiSetsPrefabs, chatCanvas.transform);
                RegisterButtonEvent();
                EOSDebug.Instance.Log("Chat Mode: Start");
            }

            string GenerateBasicName(){
                return $"User{OwnerUserID.ToString().Substring(4, 8)}";
            }
        }
        void RegisterButtonEvent(){
            uiSets.transform.Find("Submit").GetComponent<Button>().onClick.AddListener(DecideChat);
            uiSets.transform.Find("Resend").GetComponent<Button>().onClick.AddListener(ResendPreContent);
            uiSets.transform.Find("Test").GetComponent<Button>().onClick.AddListener(DoStressTest);
            uiSets.transform.Find("TestL").GetComponent<Button>().onClick.AddListener(DoLargePacketTest);
            uiSets.transform.Find("Name").GetComponent<Button>().onClick.AddListener(DecideUserName);
            uiSets.transform.Find("Clear").GetComponent<Button>().onClick.AddListener(ClearChat);
            uiSets.transform.Find("StopReceiver").GetComponent<Button>().onClick.AddListener(() => StopReceiver());
            uiSets.transform.Find("StartReceiver").GetComponent<Button>().onClick.AddListener(() => RestartReceiver());
            uiSets.transform.Find("Pause").GetComponent<Button>().onClick.AddListener(() => PauseSession(false));
            uiSets.transform.Find("PauseF").GetComponent<Button>().onClick.AddListener(() => PauseSession(true));
            uiSets.transform.Find("Restart").GetComponent<Button>().onClick.AddListener(RestartSession);
            uiSets.transform.Find("Leave").GetComponent<Button>().onClick.AddListener(LeaveSession);
            uiSets.transform.Find("Close").GetComponent<Button>().onClick.AddListener(CloseSession);
            uiSets.transform.Find("StartVC").GetComponent<Button>().onClick.AddListener(StartVC);
            uiSets.transform.Find("StopVC").GetComponent<Button>().onClick.AddListener(StopVC);
        }

        [Rpc(true)] //On call, send this ch byte with args.
        public void UpdateChatText(string message){
            //SynicSugar inserts "SendProcess" into IL.
            //We can't put "if" or another on the top of this method.(At the top is always the sending process).
            //So, if we need the condition to send or not, call this after that condition from the other method.
            //
            // --- On IL(Like Binary), Inserted Senging Process --- 
            //


            string chat = $"{Name}: {message}{System.Environment.NewLine}";
            systemManager.chatText.text += chat;

            submitCount++;
            systemManager.inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.RemoteUserIds[0]).submitCount}";
        }
        [Rpc]
        public void UpdateName(string newName){
            Name = newName;
        }
        //This main use is to send data when the disconnected user returns to session.
        //So, we don't use LastTargetRPCPayload in local again, but if want use it, deserialize byte[] with MemoryPack.
        public void ResendPreContent(){
            ConnectHub.Instance.ResendLastRPC();
            Debug.Log("SendLast Content!");

            string chat = $"{Name}: {MemoryPack.MemoryPackSerializer.Deserialize<string>(p2pInfo.Instance.LastRPCPayload)}{System.Environment.NewLine}";
            systemManager.chatText.text += chat;

            submitCount++;
            systemManager.inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.RemoteUserIds[0]).submitCount}";
        }
        //---For button
        public void DecideChat(){
            if(string.IsNullOrEmpty(systemManager.contentField.text)){
                return;
            }
            UpdateChatText(systemManager.contentField.text);
        }
        public void DoStressTest(){
            if(isStressTesting){
                return;
            }
            isStressTesting = true;
            for(int i = 0; i < 100; i++){
                string message = i % 2 == 0 ? "Even" : "Odd";
                UpdateChatText(message);
                
                if(!isStressTesting){
                    break;
                }
            }
            isStressTesting = false;
        }
        public void DoLargePacketTest(){
            var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var array = new char[4000];
            var random = new System.Random();

            for (int i = 0; i < array.Length; i++){
                array[i] = sample[random.Next(sample.Length)];
            }
            LargePacket = new string(array);
            systemManager.chatText.text += LargePacket;
            //When the 3rd arg is true, this becomea the rpc to send large packet.
            //Pass false to 4th arg. The opponent will drop the packets for self data except for the moment re-cconect.
            ConnectHub.Instance.SyncSynic(p2pInfo.Instance.RemoteUserIds[0], 1, true, false);
        }
        public void DecideUserName(){
            UpdateName(systemManager.nameField.text);
        }
        public void ClearChat(){
            systemManager.chatText.text = System.String.Empty;
        }
        public void StopReceiver(){
            EOSDebug.Instance.Log("Chat Mode: StopReceiver");
            ConnectHub.Instance.PausetPacketReceiver();
        }

        public void RestartReceiver(){
            EOSDebug.Instance.Log("Chat Mode: Restart");
            ConnectHub.Instance.StartPacketReceiver();
        }
        public void PauseSession(bool isForced){
            isStressTesting = false;
            EOSDebug.Instance.Log("Chat Mode: Pause");
            ConnectHub.Instance.PauseConnections(isForced).Forget();
        }
        public void RestartSession(){
            EOSDebug.Instance.Log("Chat Mode: Restart");
            systemManager.chatText.text = System.String.Empty;
            ConnectHub.Instance.RestartConnections();
        }

        
        public void StartVC(){
            RTCManager.Instance.StartVoiceSending();
        }
        public void StopVC(){
            RTCManager.Instance.StopVoiceSending();
        }
        public async void LeaveSession(){
            isStressTesting = false;
            EOSDebug.Instance.Log("Chat Mode: Leave");
            CancellationTokenSource token = new CancellationTokenSource();
            await ConnectHub.Instance.ExitSession(token);
            systemManager.modeSelect.ChangeGameScene("MainMenu");

        }
        public async void CloseSession(){
            isStressTesting = false;
            EOSDebug.Instance.Log("Chat Mode: Close");
            CancellationTokenSource token = new CancellationTokenSource();
            await ConnectHub.Instance.CloseSession(token);
            systemManager.modeSelect.ChangeGameScene("MainMenu");
        }
    }
}