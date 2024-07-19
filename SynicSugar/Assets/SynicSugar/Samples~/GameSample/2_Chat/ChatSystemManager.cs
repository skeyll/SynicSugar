using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;
using UnityEngine.UI;
namespace SynicSugar.Samples {
    public class ChatSystemManager : MonoBehaviour {
        [SerializeField] GameObject matchmakeCanvas, chatCanvas;
        public Text chatText, inputCount;
        public InputField contentField, nameField;
        public GameObject chatPlayerPrefab, uiSetsPrefabs;
        Dictionary<string, Text> vcStates; //Key is UserId.ToString().
        [SerializeField] Transform vcStatesContentParent;
        [SerializeField] Text vcStatePrefab;
        public RawImage ForLargePacket;

        async UniTaskVoid Start() {
            vcStates = new();
            if(p2pInfo.Instance.AllUserIds.Count > 1){ //Regsiter notify for Online mode.
                p2pInfo.Instance.ConnectionNotifier.OnTargetDisconnected += OnDisconect;
                p2pInfo.Instance.ConnectionNotifier.OnTargetConnected += OnConnected;
                p2pInfo.Instance.SyncSnyicNotifier.OnSyncedSynic += OnSyncedSynic;
            }
            //At first, instantiate network objects.
            //It are registered to ConnectHub automatically.
            SynicObject.AllSpawn(chatPlayerPrefab);
            //For reconencter
            //IsReconnecter means that this local user is a reconnector and they has not received Synic data about themself.
            if(p2pInfo.Instance.IsReconnecter){
                //To get SynicPacket.
                ConnectHub.Instance.StartSynicReceiver();
                //This flag(HasReceivedAllSyncSynic) cannot be used at the same time. Once it returns True, it returns False again.
                await UniTask.WaitUntil(() => p2pInfo.Instance.HasReceivedAllSyncSynic);
                UpdateChatCount();
            }
            
            //To get AllPacket.
            ConnectHub.Instance.StartPacketReceiver(PacketReceiveTiming.FixedUpdate, 5);
            if(p2pInfo.Instance.AllUserIds.Count > 1){ //VC setting for Online mode.
                RTCManager.Instance.StartVoiceSending();
                // VC actions with No args
                // RTCManager.Instance.ParticipantUpdatedNotifier.Register(() => OnStartSpeaking(), t => OnStopSpeaking());
                RTCManager.Instance.ParticipantUpdatedNotifier.Register(t => OnStartSpeaking(t), t => OnStopSpeaking(t));
            }
        }
    #if SYNICSUGAR_FPSTEST
        void Update(){
            float fps = 1f / Time.deltaTime;
            Debug.Log("--UPDATE-- fps:" + fps);
        }
    #endif
        public void SwitchPanelContent(){
            matchmakeCanvas.SetActive(false);
            chatCanvas.SetActive(true);
        }
        public void GenerateVCStateObject(UserId userId){
            Text stateText = Instantiate(vcStatePrefab, vcStatesContentParent);
            vcStates.Add(userId.ToString(), stateText);
            string tmpName = p2pInfo.Instance.IsLoaclUser(userId) ? "LocalPlayer" : "RemotePlayer";
            stateText.text = $"{tmpName}: Not-Speaking";
        }
        
        public void UpdateChatCount(){
            if(p2pInfo.Instance.AllUserIds.Count > 1){
                inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.CurrentRemoteUserIds[0]).submitCount}";
            }else{
                inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / --";
            }
        }
        void OnDisconect(UserId id){
            chatText.text += $"{id} is Disconnected / {p2pInfo.Instance.LastDisconnectedUsersReason}{System.Environment.NewLine}";
        }
        void OnConnected(UserId id){
            chatText.text += $"{id} Join {System.Environment.NewLine}";
            //Send local data
            ConnectHub.Instance.SyncSynic(p2pInfo.Instance.LastConnectedUsersId, SynicType.WithOthers, 0, false);
        }
        //Called each time a SyncSynic packet is received.
        //Use when Synic is used as just a large packet.
        void OnSyncedSynic(){
            if(p2pInfo.Instance.SyncedSynicPhase == 1){  
                EOSDebug.Instance.Log("GetLargePacket");
                chatText.text = ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.CurrentRemoteUserIds[0]).LargePacket;
            }
        }
        //VC actions with No args
        // void OnStartSpeaking(){
        //     if(!vcStates.ContainsKey(RTCManager.Instance.LastStateUpdatedUserId.ToString())){
        //         return;
        //     }
        //     string name = ConnectHub.Instance.GetUserInstance<ChatPlayer>(RTCManager.Instance.LastStateUpdatedUserId).Name;
        //     vcStates[RTCManager.Instance.LastStateUpdatedUserId.ToString()].text = $"{name}: Speaking";
        // }
        // void OnStopSpeaking(){
        //     if(!vcStates.ContainsKey(RTCManager.Instance.LastStateUpdatedUserId.ToString())){
        //         return;
        //     }
        //     string name = ConnectHub.Instance.GetUserInstance<ChatPlayer>(RTCManager.Instance.LastStateUpdatedUserId).Name;
        //     vcStates[RTCManager.Instance.LastStateUpdatedUserId.ToString()].text = $"{name}: Not-Speaking";
        // }

        void OnStartSpeaking(UserId target){
            if(!vcStates.ContainsKey(target.ToString())){
                return;
            }
            string name = ConnectHub.Instance.GetUserInstance<ChatPlayer>(target).Name;
            vcStates[target.ToString()].text = $"{name}: Speaking";
        }
        void OnStopSpeaking(UserId target){
            if(!vcStates.ContainsKey(target.ToString())){
                return;
            }
            string name = ConnectHub.Instance.GetUserInstance<ChatPlayer>(target).Name;
            vcStates[target.ToString()].text = $"{name}: Not-Speaking";
        }
    }
}