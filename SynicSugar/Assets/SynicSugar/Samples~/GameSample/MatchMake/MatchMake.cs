using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace  SynicSugar.Samples {
    public class MatchMake : MonoBehaviour{
        [SerializeField] GameObject matchmakePrefab;
        [SerializeField] Button startMatchMake, closeLobby, startGame, backtoMenu, hostConclude;
        [SerializeField] MatchMakeConditions matchConditions;
        [SerializeField] Text buttonText, matchmakeState;
        [SerializeField] GameModeSelect modeSelect; //For tankmatchmaking scene, and to cancel matchmake then return to menu.
        //For Tank
        public InputField nameField;
        [SerializeField] Text playerName, lobbyMemberCount;
        [SerializeField] Transform memberContentParent;
        [SerializeField] GameObject memberStatePrefab;
        //Key is UserId
        Dictionary<string, LobbyMemberState> LobbyMemberStatus = new();
        enum SceneState{
            Standby, inMatchMake, ToGame
        }
    #region Init
        void Awake(){
            if(MatchMakeManager.Instance == null){
                Instantiate(matchmakePrefab);
            }
            SetGUIEvents();
        }
        /// <summary>
        /// Change button and text state in matchmaking.
        /// </summary>
        void SetGUIEvents(){
            MatchMakeManager.Instance.MatchMakingGUIEvents = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;

            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableStart += OnDisableStart;
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += OnEnableCancel;

            if(SceneManager.GetActiveScene().name == "TankMatchMake"){
                MatchMakeManager.Instance.MatchMakingGUIEvents.EnableHostConclude += OnEnableHostConclude;
                MatchMakeManager.Instance.MatchMakingGUIEvents.DisableHostConclude += OnDisableHostConclude;
                MatchMakeManager.Instance.MatchMakingGUIEvents.DisableCancelKickConclude += OnDisableCancelKickFinish;
                MatchMakeManager.Instance.MatchMakingGUIEvents.OnLobbyMemberCountChanged += OnLobbyMemberCountChanged;
                //For user attribute to use just in Lobby.
                MatchMakeManager.Instance.MemberUpdatedNotifier.Register(t => OnUpdatedMemberAttribute(t));
            }
        }
    #endregion
    #region For Recconect
        async UniTaskVoid Start(){
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
                SwitchGUIState(SceneState.ToGame);
                return;
            }
            EOSDebug.Instance.Log("Fail to Re-Connect Lobby.");
            SwitchGUIState(SceneState.Standby);
        }
    #endregion
        //Register MatchMaking button
        public void StartMatchMake(){
            //For Tank
            ClearLobbyMemberState();

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
        //Or, change this to async void StartMatchMakeEntity() at the expense of performance. We can pass async void to UnityEvents.
        async UniTask StartMatchMakeEntity(){
            //We have two ways to call SearchAndCreateLobby.
            //If pass self caneltoken, we should use Try-catch.
            //If not, the API returns just bool result.
            //Basically, we don't have to give token to SynicSugar APIs.
            bool selfTryCatch = false;

            if(!selfTryCatch){ //Recommend
                bool isSuccess = false;

                if(SceneManager.GetActiveScene().name == "TankMatchMake"){ //To set max members and min members
                    isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                }else{ //MaxLobbyMember is just 2.
                    isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(2));
                }
                    
                if(!isSuccess){
                    EOSDebug.Instance.Log("MatchMaking Failed.");
                    SwitchGUIState(SceneState.Standby);
                    return;
                }
            }else{ //Sample for another way. To cancel via token in manual.
                try{
                    CancellationTokenSource matchCTS = new CancellationTokenSource();
                    bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), token: matchCTS);

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

            if(startGame != null){ //For ReadHeart and Chat
                startGame.gameObject.SetActive(true);
            }else{ //For Tank
                modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
            }
        }
        /// <summary>
        /// Register to Cancel button. <br />
        /// Host delete and Guest leave the current lobby.
        /// </summary>
        public void CancelMatchMaking(){
            MatchMakeManager.Instance.CancelCurrentMatchMake().Forget();

            if(nameField != null){
                nameField.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// Register to ReturnMenu button.<br />
        /// Host delete and Guest leave the current lobby. Then, destroy MatchMakeManager and back to MainMenu.
        /// </summary>
        /// <returns></returns>
        public async void CanelMatchMakingAndReturnToLobby(){
            await MatchMakeManager.Instance.CancelCurrentMatchMake(true);
            
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString());
        }
        //For Tank
        public void FinishMatchMakingAfterMeetRequiredCondition(){
            MatchMakeManager.Instance.ConcludeMatchMake();
            OnDisableHostConclude();
        }
        /// <summary>
        /// Manage UIs on after and before matchmaking.
        /// </summary>
        /// <param name="state"></param>
        void SwitchGUIState(SceneState state){
            if(startMatchMake != null){
                startMatchMake.gameObject.SetActive(state == SceneState.Standby);
            }
            if(closeLobby != null){
                closeLobby.gameObject.SetActive(state == SceneState.ToGame);
            }
            if(startGame != null){
                startGame.gameObject.SetActive(state == SceneState.ToGame);
            }
            if(backtoMenu != null){
                backtoMenu.gameObject.SetActive(false);
            }
        }
        #region For GUI events
        void OnDisableStart(){
            startMatchMake.gameObject.SetActive(false);
        }
        void OnEnableCancel(){
            SwitchCancelButtonActive(true);
            buttonText.text = "Cancel Matchmaking";
        }
        void OnDisableCancelKickFinish(){
            SwitchCancelButtonActive(false);
        }
        //For Tank
        void OnEnableHostConclude(){
            hostConclude.gameObject.SetActive(true);
        }
        //For Tank
        void OnDisableHostConclude(){
            hostConclude.gameObject.SetActive(false);
        }
        //For Tank
        void OnLobbyMemberCountChanged(UserId target, bool isParticipated){
            if(isParticipated){
                GameObject stateObj = Instantiate(memberStatePrefab, memberContentParent);
                LobbyMemberState state = stateObj.GetComponent<LobbyMemberState>();
                LobbyMemberStatus.Add(target.ToString(), state);
            }else{
                Destroy(LobbyMemberStatus[target.ToString()].gameObject);
                LobbyMemberStatus.Remove(target.ToString());
            }
            lobbyMemberCount.text = $"Current: {MatchMakeManager.Instance.GetCurrentLobbyMemberCount()} / Max: {MatchMakeManager.Instance.GetMaxLobbyMemberCount()}";
        }
        void SwitchCancelButtonActive(bool isActivate){
            //To return main menu
            if(backtoMenu != null){
                backtoMenu.gameObject.SetActive(isActivate);
            }
            closeLobby.gameObject.SetActive(isActivate);
        }
        #endregion
        
        void OnUpdatedMemberAttribute(UserId target){
            List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
            LobbyMemberState state = LobbyMemberStatus[target.ToString()];
            
            state.SetData(target, AttributeData.GetValueAsString(data, "NAME"), AttributeData.GetValueAsString(data, "LEVEL"));
        }
        void ClearLobbyMemberState(){
            if(SceneManager.GetActiveScene().name != "TankMatchMake"){ 
                return;
            }
            foreach(var s in LobbyMemberStatus){
                Destroy(s.Value.gameObject);
            }
            LobbyMemberStatus.Clear();
        }
    }
}
