using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples {
    public class ChatSystemManager : MonoBehaviour {
        [SerializeField] GameObject matchmakeCanvas, chatCanvas;
        public GameModeSelect modeSelect;
        public Text chatText, currentState;
        public InputField contentField;
        public GameObject chatPlayerPrefab, uiSetsPrefabs;
        
        void Start() {
            //At first, instantiate network objects.
            //It are registered to ConnectHub automatically.
            SynicObject.AllSpawn(chatPlayerPrefab);

            ConnectHub.Instance.StartPacketReceiver();
        }
        public void SwitchPanelContent(){
            matchmakeCanvas.SetActive(false);
            chatCanvas.SetActive(true);
        }
    }
}