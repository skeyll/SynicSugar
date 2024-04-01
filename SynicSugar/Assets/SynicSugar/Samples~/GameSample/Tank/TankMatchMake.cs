using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
using SynicSugar.P2P;

namespace SynicSugar.Samples {
    public class TankMatchMake : MatchMakeBase {
        [SerializeField] GameObject hostConclude;
        [SerializeField] Transform memberContentParent;
        [SerializeField] GameObject memberStatePrefab;
        [SerializeField] GameObject CreateButton, SearchButton;
        [SerializeField] GameModeSelect modeSelect; //For tankmatchmaking scene, and to cancel matchmake then return to menu.
        [SerializeField] Text lobbyMemberCount;
        public InputField nameField;
        [SerializeField] Text playerName;

        //Key is UserId
        Dictionary<string, LobbyMemberState> LobbyMemberStatus = new();
        #region For GUI events.
        public override void SetGUIEvents(){
            base.SetGUIEvents();

            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableHostConclude += OnEnableHostConclude;
            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableHostConclude += OnDisableHostConclude;
            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableCancelKickConclude += OnDisableCancelKickFinish;
            MatchMakeManager.Instance.MatchMakingGUIEvents.OnLobbyMemberCountChanged += OnLobbyMemberCountChanged;
            //For user attribute to use just in Lobby.
            MatchMakeManager.Instance.MemberUpdatedNotifier.Register(t => OnUpdatedMemberAttribute(t));
        }
        /// <summary>
        /// Attach to the button to close matchmaking automatically.
        /// </summary>
        public void FinishMatchMakingAfterMeetRequiredCondition(){
            MatchMakeManager.Instance.ConcludeMatchMake();
            OnDisableHostConclude();
        }
        /// <summary>
        /// Set active Conclude(manualy close) button.
        /// </summary>
        void OnEnableHostConclude(){
            hostConclude.gameObject.SetActive(true);
        }
        /// <summary>
        /// Set dis-active Conclude(manualy close) button.
        /// </summary>
        void OnDisableHostConclude(){
            hostConclude.gameObject.SetActive(false);
        }
        /// <summary>
        /// For GUI. Just display current lobbu count.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isParticipated"></param>
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
        /// <summary>
        /// For GUI. Display lobby member status about Attributes.
        /// </summary>
        /// <param name="target"></param>
        void OnUpdatedMemberAttribute(UserId target){
            List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
            LobbyMemberState state = LobbyMemberStatus[target.ToString()];
            
            state.SetData(target, AttributeData.GetValueAsString(data, "NAME"), AttributeData.GetValueAsString(data, "LEVEL"));
        }
        void OnDisableCancelKickFinish(){
            SwitchCancelButtonActive(false);
        }
        #endregion

    #region StartMatchMake
        // BASIC
        /// <summary>
        /// Basic API. Search lobby, and if can't join, create lobby.
        /// </summary>
        public override void StartMatchMake(){
            //To clear old objects.
            ClearLobbyMemberState();

            base.StartMatchMake();

            //For some samples to need user name before MatchMaking.
            //Just set user name for game.
            nameField.gameObject.SetActive(false);
            TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
            playerName.text = $"PlayerName: {nameField.text}";
        }
        internal override async UniTask StartMatchMakeEntity(){
            SwitchGUIState(SceneState.inMatchMake);
            bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                
            if(!isSuccess){
                EOSDebug.Instance.Log("MatchMaking Failed.");
                SwitchGUIState(SceneState.Standby);
                return;
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

            SwitchCancelButtonActive(true);
            
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
        }
        // CREATE 
        /// <summary>
        /// Just create lobby.
        /// </summary>
        public void CreateMatchMake(){
            SwitchGUIState(SceneState.inMatchMake);
            ClearLobbyMemberState();

            CreateMatchMakeEntity().Forget();

            nameField.gameObject.SetActive(false);
            TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
            playerName.text = $"PlayerName: {nameField.text}";
        }
        async UniTask CreateMatchMakeEntity(){
            bool isSuccess = await MatchMakeManager.Instance.CreateLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                
            if(!isSuccess){
                EOSDebug.Instance.Log("MatchMaking Failed.");
                SwitchGUIState(SceneState.Standby);
                return;
            }
            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

            SwitchCancelButtonActive(true);
            
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
        }

        // SEARCH
        /// <summary>
        /// Just Search lobby.
        /// </summary>
        public void SearchMatchMake(){
            SwitchGUIState(SceneState.inMatchMake);
            ClearLobbyMemberState();

            SearchMatchMakeEntity().Forget();
            
            nameField.gameObject.SetActive(false);
            TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
            playerName.text = $"PlayerName: {nameField.text}";
        }
        async UniTask SearchMatchMakeEntity(){
            bool isSuccess = await MatchMakeManager.Instance.SearchLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                
            if(!isSuccess){
                EOSDebug.Instance.Log("MatchMaking Failed.");
                SwitchGUIState(SceneState.Standby);
                return;
            }
            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

            SwitchCancelButtonActive(true);
            
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.Tank.ToString());
        }

        void ClearLobbyMemberState(){
            foreach(var s in LobbyMemberStatus){
                Destroy(s.Value.gameObject);
            }
            LobbyMemberStatus.Clear();
        }
    #endregion
        /// <summary>
        /// Register to ReturnMenu button.<br />
        /// Host delete and Guest leave the current lobby. Then, destroy MatchMakeManager and back to MainMenu.
        /// </summary>
        /// <returns></returns>
        public override async void CanelMatchMakingAndReturnToLobby(){
            base.CanelMatchMakingAndReturnToLobby();

            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString());
        }
        public override void CancelMatchMaking(){
            base.CancelMatchMaking();

            nameField.gameObject.SetActive(true);
        }

        /// <summary>
        /// Manage UIs on after and before matchmaking.
        /// </summary>
        /// <param name="state"></param>
        protected override void SwitchGUIState(SceneState state){
            base.SwitchGUIState(state);
            CreateButton.SetActive(state == SceneState.Standby);
            SearchButton.SetActive(state == SceneState.Standby);
        }
    }
}