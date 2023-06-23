using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples {
    //To use "ConnectHub.Instance.GetUserInstance<ChatPlayer>(UserID), make this [NetworkPlayer(true)].
    [NetworkPlayer]
    public partial class ChatPlayer : MonoBehaviour {
        ChatSystemManager systemManager;
        bool isStressTesting;
        int stressCount;
        GameObject uiSets; //Holds buttons.
        void Start(){
            GameObject chatCanvas = GameObject.Find("Chat");
            systemManager = chatCanvas.GetComponent<ChatSystemManager>();

            // For example, suppose this local Player's ID is A. 
            // Player A can only control the character Instance that has OwnerUserID A.
            // If this instance holds A, this is isLocal for A.
            // On the other hand, when this has B, this is isRemote(=!isLocal) for A.
            if(isLocal){
                uiSets = Instantiate(systemManager.uiSetsPrefabs, chatCanvas.transform);
                RegisterButtonEvent();
                systemManager.currentState.text = "InGame";
            }
        }
        void RegisterButtonEvent(){
            uiSets.transform.Find("Submit").GetComponent<Button>().onClick.AddListener(DecideChat);
            uiSets.transform.Find("Test").GetComponent<Button>().onClick.AddListener(DoStressTest);
            uiSets.transform.Find("Clear").GetComponent<Button>().onClick.AddListener(ClearChat);
            uiSets.transform.Find("Pause").GetComponent<Button>().onClick.AddListener(PauseSession);
            uiSets.transform.Find("Restart").GetComponent<Button>().onClick.AddListener(RestartSession);
            uiSets.transform.Find("Leave").GetComponent<Button>().onClick.AddListener(LeaveSession);
            uiSets.transform.Find("Close").GetComponent<Button>().onClick.AddListener(CloseSession);
        }
        //For Button
        public void DecideChat(){
            UpdateChatText(systemManager.contentField.text);
        }
        [Rpc]
        public void UpdateChatText(string message){
            string selfSign = isLocal ? "★" : "";
            string chat = $"{selfSign}{OwnerUserID}: {message}{System.Environment.NewLine}";
            systemManager.chatText.text = systemManager.chatText.text + chat;
        }
        //For Button
        public void DoStressTest(){
            if(isStressTesting){
                return;
            }
            isStressTesting = true;
            for(int i = 0; i < 100; i++){
                string message = i % 2 == 0 ? "Even" : "Odd";
                SendMassMessages(message);
                
                if(!isStressTesting){
                    break;
                }
            }
            isStressTesting = false;
        }
        [Rpc]
        public void SendMassMessages(string message){
            //SynicSugar inserte "SendProcess" here.
            //We can't put "if" on the top of this method.
            //So, we need to call this from the other code for IF Statement.

            systemManager.chatText.text = $"{OwnerUserID}'s EnduranceTest: {stressCount.ToString()}";
            stressCount++;
        }
        //---For button
        public void ClearChat(){
            stressCount = 0;
            systemManager.chatText.text = System.String.Empty;
        }
        public void PauseSession(){
            isStressTesting = false;
            systemManager.currentState.text = "Pause";
            ConnectHub.Instance.PauseSession(true).Forget();
        }
        public void RestartSession(){
            systemManager.currentState.text = "InGame";
            stressCount = 0;
            systemManager.chatText.text = System.String.Empty;
            ConnectHub.Instance.ReStartSession();
        }
        public void LeaveSession(){
            isStressTesting = false;
            systemManager.currentState.text = "Leave";
            ConnectHub.Instance.LeaveSession();
        }
        public void CloseSession(){
            isStressTesting = false;
            systemManager.currentState.text = "Close";
            ConnectHub.Instance.CloseSession();
        }
    }
}