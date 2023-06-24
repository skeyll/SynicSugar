using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;

namespace  SynicSugar.Samples {
    public class MatchMake : MonoBehaviour{
        [SerializeField] GameObject matchmakePrefab;
        GameObject matchmakeContainer;
        [SerializeField] Button startMatchMake, closeLobby, startGame;
        CancellationTokenSource matchCancellToken;
        [SerializeField] MatchGUIState discriptions;
        [SerializeField] MatchMakeConditions matchConditions;
        [SerializeField] Text buttonText;
        //For Tank
        public InputField nameField;
        [SerializeField] Text playerName;
        
        int level;
        void Awake(){
            if(MatchMakeManager.Instance == null){
                matchmakeContainer = Instantiate(matchmakePrefab);
            }

            InitMatchState();
            MatchMakeManager.Instance.SetGUIState(discriptions);
        }

        void OnDestroy(){
            if(matchCancellToken != null){
                matchCancellToken.Cancel();
            }
        }
        async UniTaskVoid Start(){
            //Try recconect
            string LobbyID = GetParticipatingLobbyID();
            matchCancellToken = new CancellationTokenSource();

            // bool canReconnect = await MatchMakeManager.Instance.ReconnectParticipatingLobby(LobbyID, matchCancellToken);

            // if(canReconnect){
            //     EOSDebug.Instance.Log("Success Re-Connect Lobby.");
            //     return;
            // }
            startMatchMake.gameObject.SetActive(true);
        }
        void InitMatchState(){
            discriptions.searchLobby = "Searching for an opponent...";
            discriptions.waitothers = "Waiting for an opponent...";
            discriptions.tryconnect = "Try to connect...";
            discriptions.success = "Success MatchMaking";
            discriptions.fail = "Fail to match make";
            discriptions.trycancel = "Try to Disconnect...";
        }
        public void StartMatchMake(){
            StartMatching().Forget();

            if(nameField != null){
                nameField.gameObject.SetActive(false);
                TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
                playerName.text = $"PlayerName: {nameField.text}";
            }
        }
        async UniTask StartMatching(){
            //Prep
            EOSDebug.Instance.Log("Start MatchMake.");
            matchCancellToken = new CancellationTokenSource();

            //Try MatchMaking
            bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), matchCancellToken);
            
            if(!isSuccess){
                EOSDebug.Instance.Log("Backend may have something problem.");
                return;
            }
            EOSDebug.Instance.Log("Success Matching!");

            closeLobby.gameObject.SetActive(true);

            if(startGame != null){ //For ReadHeart and Chat
                startGame.gameObject.SetActive(true);
            }else{ //For Tank
                GameModeSelect modeSelect = new GameModeSelect();
                modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
            }
        }
        /// <summary>
        /// Call this to stop matchmake, and delete lobby after can matchmake.
        /// </summary>
        public void CloseLobby(){
            LeaveLobby().Forget();

            if(nameField != null){
                nameField.gameObject.SetActive(true);
            }
        }
        async UniTask LeaveLobby(){
            closeLobby.gameObject.SetActive(false);

            matchCancellToken = new CancellationTokenSource();
            // bool isSuccess = await MatchMakeManager.Instance.DestroyHostingLobby(matchCancellToken);
            
            startGame.gameObject.SetActive(false);
            startMatchMake.gameObject.SetActive(true);
        }
        //State event
        public void ChangeButtonContent(bool afterMatching){
            closeLobby.gameObject.SetActive(true);
            buttonText.text = afterMatching ? "Close Lobby" : "Stop MatchMake";
        }
        public void StopAdditionalInput(){
            startMatchMake.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Is there a Lobby that should be reconnected?<br />
        /// XXX Dependent on the location to save, change the way to check ID.
        /// </summary>
        /// <returns></returns>
        string GetParticipatingLobbyID(){
            return PlayerPrefs.GetString ("eos_lobbyid", System.String.Empty);
        }
    }
}
