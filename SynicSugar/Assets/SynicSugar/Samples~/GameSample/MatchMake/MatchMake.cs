using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace  SynicSugar.Samples {
    public class MatchMake : MonoBehaviour{
        [SerializeField] GameObject matchmakePrefab;
        GameObject matchmakeContainer;
        [SerializeField] Button startMatchMake, closeLobby, startGame, backtoMenu;
        [SerializeField] MatchGUIState descriptions;
        [SerializeField] MatchMakeConditions matchConditions;
        [SerializeField] Text buttonText;
        [SerializeField] GameModeSelect modeSelect; //For tankmatchmaking scene, and to cancel matchmake then return to menu.
        //For Tank
        public InputField nameField;
        [SerializeField] Text playerName;
        int level;
        enum GameMode{
            Tank, ReadHearts, Chat
        }
        GameMode gameMode;
        enum SceneState{
            Standby, inMatchMake, ToGame
        }
    #region Init
        void Awake(){
            if(MatchMakeManager.Instance == null){
                matchmakeContainer = Instantiate(matchmakePrefab);
            }
            SetGUIState();
            SetGameMode();
        }
        void SetGUIState(){
            descriptions = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            descriptions.stopAdditionalInput.AddListener(StopAdditionalInput);
            descriptions.acceptCancel.AddListener(() => ActivateCancelButton(false));

            MatchMakeManager.Instance.SetGUIState(descriptions);
        }
        void SetGameMode(){
            string scene = SceneManager.GetActiveScene().ToString();
            switch(scene){
                case "TankMatchMake":
                gameMode = GameMode.Tank;
                break;
                case "ReadHearts":
                gameMode = GameMode.ReadHearts;
                break;
                case "Chat":
                gameMode = GameMode.Chat;
                break;
            }
        }
    #endregion
    #region For Recconect
        async void Start(){
            //Dose this game allow user to re-join thedisconnected match? 
            if(MatchMakeManager.Instance.lobbyIdSaveType == MatchMakeManager.RecconectLobbyIdSaveType.NoReconnection){
                return;
            }

            //Try recconect
            //Sample projects use the playerprefs to save LobbyIDs for recconection.
            string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
            //On the default way, return Empty when there is no lobby data in local.
            if(string.IsNullOrEmpty(LobbyID)){
                return;
            }

            EOSDebug.Instance.Log($"SavedLobbyID is {LobbyID}.");
            
            startMatchMake.gameObject.SetActive(false);
            CancellationTokenSource token = new CancellationTokenSource();

            bool canReconnect = await MatchMakeManager.Instance.ReconnectLobby(LobbyID, token);

            if(canReconnect){
                EOSDebug.Instance.Log($"Success Recconect! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
                SwitchGUIState(SceneState.inMatchMake);
                return;
            }
            EOSDebug.Instance.Log("Failer Re-Connect Lobby.");
            startMatchMake.gameObject.SetActive(true);
        }
    #endregion
    // #region MatchMaking
        public void StartMatchMake(){
            EOSDebug.Instance.Log("Start MatchMake.");
            StartMatchMakeEntity().Forget();

            //For some samples to need user name before MatchMaking.
            //Just set user name for game.
            if(nameField != null){
                nameField.gameObject.SetActive(false);
                TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
                playerName.text = $"PlayerName: {nameField.text}";
            }
        }
        //We can't set NOT void process to Unity Event.
        //So, register StartMatchMake() to Button instead of this.
        async UniTask StartMatchMakeEntity(){
            //We have two ways to call SearchAndCreateLobby.
            //If pass self caneltoken, we should use Try-catch.
            //If not, the API returns just bool result.
            //Basically, we don't have to give token to SynicSugar APIs.
            bool selfTryCatch = false;

            if(!selfTryCatch){ //Recommend
                bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition());
                
                if(!isSuccess){
                    EOSDebug.Instance.Log("MatchMaking Failed.");
                    SwitchGUIState(SceneState.Standby);
                    return;
                }
            }else{ //Sample for another way
                try{
                    CancellationTokenSource matchCTS = new CancellationTokenSource();
                    bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), matchCTS);

                    if(!isSuccess){
                        EOSDebug.Instance.Log("Backend may have something problem.");
                        SwitchGUIState(SceneState.Standby);
                        return;
                    }
                }catch(OperationCanceledException){
                    EOSDebug.Instance.Log("Cancel MatchMaking");
                    SwitchGUIState(SceneState.Standby);
                    return;
                }
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

            SwitchCancelButtonActive(true);

            GoGameScene();
        }
        /// <summary>
        /// Cancel matchmaking (Host delete and Guest leave the current lobby)
        /// </summary>
        public void CancelMatchMaking(){
            CancelMatchMakingEntity().Forget();

            if(nameField != null){
                nameField.gameObject.SetActive(true);
            }
        }
        async UniTask CancelMatchMakingEntity(){
            SwitchCancelButtonActive(false);

            bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake();
            
            SwitchGUIState(SceneState.Standby);
        }
        /// <summary>
        /// Cancel matchmaking (Host delete and Guest leave the current lobby)
        /// Then destroy MatchMakeManager and back to MainMenu.
        /// </summary>
        /// <returns></returns>
        public async void CanelMatchMakingAndReturnToLobby(){
            SwitchCancelButtonActive(false);

            bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake(true);
            
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString());
        }
        //State event
        public void ActivateCancelButton(bool afterMatching){
            SwitchCancelButtonActive(true);
            buttonText.text = afterMatching ? "Close Lobby" : "Stop MatchMake";
        }
        public void StopAdditionalInput(){
            startMatchMake.gameObject.SetActive(false);
        }
        
        void SwitchCancelButtonActive(bool isActivate){
            //To return main menu
            if(backtoMenu != null){
                backtoMenu.gameObject.SetActive(isActivate);
            }
            closeLobby.gameObject.SetActive(isActivate);
        }
        
        void GoGameScene(){
            if(startGame != null){ //For ReadHeart and Chat
                startGame.gameObject.SetActive(true);
            }else{ //For Tank
                modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
            }
        }
        void SwitchGUIState(SceneState state){
            startMatchMake.gameObject.SetActive(state == SceneState.Standby);
            closeLobby.gameObject.SetActive(state == SceneState.ToGame);
            startGame.gameObject.SetActive(state == SceneState.ToGame);
            backtoMenu.gameObject.SetActive(false);
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
