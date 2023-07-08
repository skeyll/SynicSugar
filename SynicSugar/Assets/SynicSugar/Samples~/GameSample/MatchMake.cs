using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace  SynicSugar.Samples {
    public class MatchMake : MonoBehaviour{
        [SerializeField] GameObject matchmakePrefab;
        GameObject matchmakeContainer;
        [SerializeField] Button startMatchMake, closeLobby, startGame;
        CancellationTokenSource matchCancellToken;
        [SerializeField] MatchGUIState descriptions;
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
            MatchMakeManager.Instance.SetGUIState(descriptions);
        }

        void OnDestroy(){
            if(matchCancellToken != null){
                matchCancellToken.Cancel();
            }
        }
        async void Start(){
            //Allow this only on Chat
            if(MatchMakeManager.Instance.lobbyIdSaveType == MatchMakeManager.RecconectLobbyIdSaveType.NoReconnection){
                return;
            }
            //Try recconect
            string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
            EOSDebug.Instance.Log($"SavedLobbyID is {LobbyID}.");
            
            if(string.IsNullOrEmpty(LobbyID)){
                return;
            }
            
            startMatchMake.gameObject.SetActive(false);
            CancellationTokenSource token = new CancellationTokenSource();

            bool canReconnect = await MatchMakeManager.Instance.ReconnecLobby(LobbyID, token);

            if(canReconnect){
                EOSDebug.Instance.Log($"Success Recconect! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
                
                closeLobby.gameObject.SetActive(true);
                startGame.gameObject.SetActive(true);
                return;
            }
            EOSDebug.Instance.Log("Failer Re-Connect Lobby.");
            startMatchMake.gameObject.SetActive(true);
        }
        void InitMatchState(){
            descriptions.searchLobby = "Searching for an opponent...";
            descriptions.waitothers = "Waiting for an opponent...";
            descriptions.tryconnect = "Try to connect...";
            descriptions.success = "Success MatchMaking";
            descriptions.fail = "Fail to match make";
            descriptions.trycancel = "Try to Disconnect...";
            descriptions.stopAdditionalInput.AddListener(StopAdditionalInput);
            descriptions.acceptCancel.AddListener(() => ChangeButtonContent(false));
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
            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

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
            bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake(token: matchCancellToken);
            
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
