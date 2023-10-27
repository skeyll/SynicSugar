using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;
using UnityEngine.UI;
namespace SynicSugar.Samples {
    public class ChatSystemManager : MonoBehaviour {
        [SerializeField] GameObject matchmakeCanvas, chatCanvas;
        public GameModeSelect modeSelect;
        public Text chatText, inputCount;
        public InputField contentField, nameField;
        public GameObject chatPlayerPrefab, uiSetsPrefabs;
        Dictionary<string, Text> vcStates; //Key is UserId.ToString().
        [SerializeField] Transform vcStatesContentParent;
        [SerializeField] Text vcStatePrefab;
        public RawImage ForLargePacket;

        void Start() {
            vcStates = new();
            p2pInfo.Instance.ConnectionNotifier.OnTargetDisconnected += OnDisconect;
            p2pInfo.Instance.ConnectionNotifier.OnTargetConnected += OnConnected;
            p2pInfo.Instance.SyncSnyicNotifier.OnSyncedSynic += OnSyncedSynic;
            //At first, instantiate network objects.
            //It are registered to ConnectHub automatically.
            SynicObject.AllSpawn(chatPlayerPrefab);
            
            ConnectHub.Instance.StartPacketReceiver();
            RTCManager.Instance.StartVoiceSending();
            // VC actions with No args
            // RTCManager.Instance.ParticipantUpdatedNotifier.Register(() => OnStartSpeaking(), t => OnStopSpeaking());
            RTCManager.Instance.ParticipantUpdatedNotifier.Register(t => OnStartSpeaking(t), t => OnStopSpeaking(t));
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
        void OnDisconect(UserId id){
            chatText.text += $"{id} is Disconnected / {p2pInfo.Instance.LastDisconnectedUsersReason}{System.Environment.NewLine}";
        }
        void OnConnected(UserId id){
            chatText.text += $"{id} Join {System.Environment.NewLine}";
            //Send local data
            ConnectHub.Instance.SyncSynic(p2pInfo.Instance.LastConnectedUsersId, 0, false, true);
        }
        //Called each time a SyncSynic packet is received
        async void OnSyncedSynic(){
            //As for the reconnect process, it is done in the local user event.
            if(p2pInfo.Instance.IsLoaclUser(p2pInfo.Instance.LastSyncedUserId) && p2pInfo.Instance.AcceptHostSynic){
                await UniTask.WaitUntil(() => p2pInfo.Instance.HasReceivedAllSyncSynic);
                //Update counter
                inputCount.text = $"ChatCount: {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.LocalUserId).submitCount} / {ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.RemoteUserIds[0]).submitCount}";
                return;
            }
            if(p2pInfo.Instance.SyncedSynicPhase == 1){  
                EOSDebug.Instance.Log("GetLargePacket");
                chatText.text = ConnectHub.Instance.GetUserInstance<ChatPlayer>(p2pInfo.Instance.RemoteUserIds[0]).LargePacket;
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