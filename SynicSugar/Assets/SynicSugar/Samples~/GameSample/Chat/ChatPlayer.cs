using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
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
            if(string.IsNullOrEmpty(Name)){
                int userNumberInAllUserIds = p2pInfo.Instance.AllUserIds.FindIndex(id =>id == OwnerUserID);
                Name = $"Player{userNumberInAllUserIds}";
            }
            systemManager.GenerateVCStateObject(OwnerUserID);

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
        }
        void RegisterButtonEvent(){
            uiSets.transform.Find("Submit").GetComponent<Button>().onClick.AddListener(DecideChat);
            uiSets.transform.Find("Resend").GetComponent<Button>().onClick.AddListener(ResendPreContent);
            uiSets.transform.Find("Test").GetComponent<Button>().onClick.AddListener(DoStressTest);
            uiSets.transform.Find("Synic").GetComponent<Button>().onClick.AddListener(SendLargePacketViaSynic);
            uiSets.transform.Find("Large").GetComponent<Button>().onClick.AddListener(SetTextureWithNewColor);
            uiSets.transform.Find("Name").GetComponent<Button>().onClick.AddListener(DecideUserName);
            uiSets.transform.Find("Clear").GetComponent<Button>().onClick.AddListener(ClearChat);
            uiSets.transform.Find("Leave").GetComponent<Button>().onClick.AddListener(LeaveSession);
            uiSets.transform.Find("Close").GetComponent<Button>().onClick.AddListener(CloseSession);

            if(p2pInfo.Instance.AllUserIds.Count > 1){ //Just for online mode
                uiSets.transform.Find("StopReceiver").GetComponent<Button>().onClick.AddListener(() => StopReceiver());
                uiSets.transform.Find("StartReceiver").GetComponent<Button>().onClick.AddListener(() => RestartReceiver());
                uiSets.transform.Find("Pause").GetComponent<Button>().onClick.AddListener(() => PauseSession(false));
                uiSets.transform.Find("PauseF").GetComponent<Button>().onClick.AddListener(() => PauseSession(true));
                uiSets.transform.Find("Restart").GetComponent<Button>().onClick.AddListener(RestartSession);
                uiSets.transform.Find("StartVC").GetComponent<Button>().onClick.AddListener(StartVC);
                uiSets.transform.Find("StopVC").GetComponent<Button>().onClick.AddListener(StopVC);
            }else{
                Destroy(uiSets.transform.Find("StopReceiver").gameObject);
                Destroy(uiSets.transform.Find("StartReceiver").gameObject);
                Destroy(uiSets.transform.Find("Pause").gameObject);
                Destroy(uiSets.transform.Find("PauseF").gameObject);
                Destroy(uiSets.transform.Find("Restart").gameObject);
                Destroy(uiSets.transform.Find("StartVC").gameObject);
                Destroy(uiSets.transform.Find("StopVC").gameObject);
            }
        }
        //On call, send this ch byte with args.
        //This args means Record packet info and send it as normal way (No split packet).
        //
        //Normally, SynicSugar sends packets multiple times to ensure　transmission. However, the Packets doesn't reach the user who was completely disconnected with game crashed.
        //For processes that must be reached, they can be recorded and manually resent when reconnecting.
        [Rpc(false, true)] 
        public void UpdateChatText(string message){
            //SynicSugar inserts "SendProcess" into IL.
            //We can't put "if" or another on the top of this method.(At the top is always the sending process).
            //So, if we need the condition to send or not, call this after that condition from the other method.
            //
            // --- On IL(Like Binary), Inserted Senging Process --- 
            //
        #if SYNICSUGAR_FPSTEST
            Debug.Log("Call UpdateChatText");
        #endif
            string chat = $"{Name}: {message}{System.Environment.NewLine}";
            systemManager.chatText.text += chat;

            submitCount++;
            systemManager.UpdateChatCount();
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
            if(p2pInfo.Instance.AllUserIds.Count > 1){ 
                systemManager.inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.CurrentRemoteUserIds[0]).submitCount}";
            }else{
                systemManager.inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / --";
            }
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
        /// <summary>
        /// Too heavy process.
        /// </summary>
        public void SetTextureWithNewColor(){
            Sprite logo = Resources.Load<Sprite>("SynicSugarLogo");
            Color[] pixels = logo.texture.GetPixels();
            string colorName = p2pInfo.Instance.IsHost() ? "Red" : "Gray";
            Color newColor = p2pInfo.Instance.IsHost() ? Color.red : Color.gray;

            for(int i = 0; i < pixels.Length; i++){
                //Change black pixel to new color.
                if(pixels[i].a >= 0.1f && pixels[i].r <= 0.1f){
                    pixels[i] = newColor;
                }
            }

            EOSDebug.Instance.Log($"New logo color: {colorName}");
            
            Texture2D texture = new Texture2D(256, 256, TextureFormat.RGBA4444, false);
            texture.SetPixels(pixels);
            texture.Apply();
            //Normal RPC can send only 1000 bytes;
            //RPC for Large can send 300KB, but for saving bandwidth, pixels is converted to png before sending.
            //This is abount 15KB
            byte[] png = texture.EncodeToPNG();

            SendLargePacketViaRpc(png);
        }
        [Rpc(true, false)]
        public void SendLargePacketViaRpc(byte[] png){
            Texture2D texture = new Texture2D(256, 256, TextureFormat.RGBA4444, false);
            texture.LoadImage(png);
            EOSDebug.Instance.Log($"Change texture color");

            systemManager.ForLargePacket.texture = texture;
        }
        public void SendLargePacketViaSynic(){
            var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var array = new char[4000];
            var random = new System.Random();

            for (int i = 0; i < array.Length; i++){
                array[i] = sample[random.Next(sample.Length)];
            }
            LargePacket = new string(array);
            systemManager.chatText.text += LargePacket;
            //Pass OnlySelf to 2nd arg, to send Synic data as simple RPC. The opponent will drop the packets for self data except for the moment re-cconect.
            //When the 3rd arg is true, this becomea the rpc to send large packet.
            
            foreach (var id in p2pInfo.Instance.CurrentRemoteUserIds){
                ConnectHub.Instance.SyncSynic(id, SynicType.OnlySelf, 1, true);
            }
        }
        public void DecideUserName(){
            UpdateName(systemManager.nameField.text);
        }
        public void ClearChat(){
            systemManager.chatText.text = System.String.Empty;
        }
        public void StopReceiver(){
            EOSDebug.Instance.Log("Chat Mode: StopReceiver");
            ConnectHub.Instance.PausePacketReceiver();
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
            if(p2pInfo.Instance.AllUserIds.Count > 1){
                await ConnectHub.Instance.ExitSession();
            }else{
                await MatchMakeManager.Instance.DestoryOfflineLobby();
            }
            systemManager.modeSelect.ChangeGameScene("MainMenu");

        }
        public async void CloseSession(){
            isStressTesting = false;
            EOSDebug.Instance.Log("Chat Mode: Close");

            if(p2pInfo.Instance.AllUserIds.Count > 1){
                await ConnectHub.Instance.CloseSession();
            }else{
                await MatchMakeManager.Instance.DestoryOfflineLobby();
            }
            systemManager.modeSelect.ChangeGameScene("MainMenu");
        }
    }
}