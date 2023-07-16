using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples {
    public class ChatSystemManager : MonoBehaviour {
        [SerializeField] GameObject matchmakeCanvas, chatCanvas;
        public GameModeSelect modeSelect;
        public Text chatText;
        public InputField contentField;
        public GameObject chatPlayerPrefab, uiSetsPrefabs;
        
        void Start() {
            p2pConfig.Instance.ConnectionNotifier.Disconnected += OnDisconect;
            p2pConfig.Instance.ConnectionNotifier.Connected += OnConnected;
            //At first, instantiate network objects.
            //It are registered to ConnectHub automatically.
            SynicObject.AllSpawn(chatPlayerPrefab);

            ConnectHub.Instance.StartPacketReceiver();
        }
        public void SwitchPanelContent(){
            matchmakeCanvas.SetActive(false);
            chatCanvas.SetActive(true);
        }
        void OnDisconect(){
            chatText.text += $"{p2pConfig.Instance.ConnectionNotifier.TargetUserId} is Disconnected / {p2pConfig.Instance.ConnectionNotifier.ClosedReason}";
        }
        void OnConnected(){
            chatText.text += $"{p2pConfig.Instance.ConnectionNotifier.TargetUserId} Join";
        }
    }
}