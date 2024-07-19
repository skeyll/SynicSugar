using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
/// <summary>
/// Awake: Prep for matchmaking (To register events for basis, detail and so on.)
/// UserInputs: Matchmaking (Srandard, Create, Search)
/// </summary>
namespace SynicSugar.Samples.Tank {
    public class TankMatchMake : MonoBehaviour {
        enum MATCHMAKEING_STATE { 
            NoneAndAfterStart, Standby, InMatchmakingStandard, InMatchmakingAfterMeetingMinCondition
        }
        enum MATCHMAKEING_TYPE { 
            Standard, CreateAsHost, JoinAsGuest
        }
        [SerializeField] GameObject matchmakePrefab;
        /// <summary>
        /// Display matchmaking state on GUI.
        /// </summary>
        [SerializeField] Text matchmakeState;
        [SerializeField] Button startMatchMake, createLobby, searchLobby, 
                        cancelMatchMake, manualStart;
        [SerializeField] MatchMakeConditions matchConditions;



        [SerializeField] Transform memberContentParent;
        [SerializeField] GameObject memberStatePrefab;
        [SerializeField] Text lobbyMemberCount;
        public InputField nameField;
        [SerializeField] Text playerName;

        //Key is UserId
        Dictionary<string, LobbyMemberState> LobbyMemberStatus = new();

    #region Prep for matchmaking
        //At first, prep GUI events for matchmaking.
        void Awake(){
            if(MatchMakeManager.Instance == null){
                Instantiate(matchmakePrefab);
            }
            //Prep matchmaking
            SetGUIEvents();
        }
        /// <summary>
        /// Register tests and button events for in-matchmaking.
        /// </summary>
        public void SetGUIEvents(){
            //Basis
            MatchMakeManager.Instance.MatchMakingGUIEvents = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;
            //Start, cancel and finsh
            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableStart += OnDisableStart;
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += OnEnableCancelAndKick;
            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableCancelKickConclude += DisableCancelKickConclude;
            //For Host
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableHostConclude += OnEnableHostConclude;
            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableHostConclude += OnDisableHostConclude;
            //For matching stats
            MatchMakeManager.Instance.MatchMakingGUIEvents.OnLobbyMemberCountChanged += OnLobbyMemberCountChanged;
            MatchMakeManager.Instance.MemberUpdatedNotifier.Register(t => OnUpdatedMemberAttribute(t));
        }
        /// <summary>
        /// Just after starting matchmaking
        /// </summary>
        void OnDisableStart(){
            //Cancel action need be disable when creating or joining lobby.
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
        }
        /// <summary>
        /// Finish to create or join a lobby, and accept canceling process.
        /// </summary>
        void OnEnableCancelAndKick(){
            SwitchButtonsActive(MATCHMAKEING_STATE.InMatchmakingStandard);
            if(MatchMakeManager.Instance.isHost){
                SwitchKickButtonsActive(true);
            }
        }
        /// <summary>
        /// Can close Lobby and start game after meets requested members. 
        /// </summary>
        void OnEnableHostConclude(){
            SwitchButtonsActive(MATCHMAKEING_STATE.InMatchmakingAfterMeetingMinCondition);
        }
        /// <summary>
        /// Disactivate close button when doesn't meet requested members. 
        /// </summary>
        void OnDisableHostConclude(){
            SwitchButtonsActive(MATCHMAKEING_STATE.InMatchmakingStandard);
        }
        /// <summary>
        /// Unctivate all buttons when close or complete matchmaking.
        /// </summary>
        void DisableCancelKickConclude(){
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
            if(MatchMakeManager.Instance.isHost){
                SwitchKickButtonsActive(false);
            }
        }
        /// <summary>
        /// Switch button active
        /// </summary>
        /// <param name="state"></param>
        void SwitchButtonsActive(MATCHMAKEING_STATE state){
            //To start matchmake
            startMatchMake.gameObject.SetActive(state == MATCHMAKEING_STATE.Standby);
            createLobby.gameObject.SetActive(state == MATCHMAKEING_STATE.Standby);
            searchLobby.gameObject.SetActive(state == MATCHMAKEING_STATE.Standby);
            //To cancel matchmake
            cancelMatchMake.gameObject.SetActive(state == MATCHMAKEING_STATE.InMatchmakingStandard || state == MATCHMAKEING_STATE.InMatchmakingAfterMeetingMinCondition);
            //To start game by hand
            manualStart.gameObject.SetActive(state == MATCHMAKEING_STATE.InMatchmakingAfterMeetingMinCondition);
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
            
            state.SetData(target, data.GetValue<string>("NAME"), AttributeData.GetValueAsString(data, "LEVEL"));
        }
        /// <summary>
        /// Disactivate kick buttons.
        /// </summary>
        void SwitchKickButtonsActive(bool isActivate){
            foreach(var s in LobbyMemberStatus){
                if(MatchMakeManager.Instance.isLocalUserId(s.Key)){
                    continue;
                }
                s.Value.SwitchKickButtonActive(isActivate);
            }
        }
    #endregion
    #region StartMatchMake
        /// <summary>
        /// For buttons.
        /// </summary>
        /// <param name="type">MATCHMAKEING_TYPE</param>
        public void StartMatchMake(int type){
            //To clear old objects.
            ClearLobbyMemberState();
            SetName();
            EOSDebug.Instance.Log($"Start MatchMake. TYPE: {(MATCHMAKEING_TYPE)type}");

            MatchMakeEntity((MATCHMAKEING_TYPE)type).Forget();
        }
        async UniTask MatchMakeEntity(MATCHMAKEING_TYPE type){
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
            Result result = Result.None;
            switch (type){
                case MATCHMAKEING_TYPE.Standard:
                    await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                break;
                case MATCHMAKEING_TYPE.CreateAsHost:
                    await MatchMakeManager.Instance.CreateLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                break;
                case MATCHMAKEING_TYPE.JoinAsGuest:
                    await MatchMakeManager.Instance.SearchLobby(matchConditions.GetLobbyCondition(16), minLobbyMember: 2, userAttributes: MatchMakeConfig.GenerateUserAttribute());
                break;
            }
                
            if(result != Result.Success){
                EOSDebug.Instance.Log("MatchMaking Failed.", result);
                SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                return;
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SceneChanger.ChangeGameScene(SCENELIST.Tank);
        }
        /// <summary>
        /// If input field is empty, set Random value.
        /// </summary>
        void SetName(){
            nameField.gameObject.SetActive(false);
            TankPassedData.PlayerName = string.IsNullOrEmpty(nameField.text) ? $"Player{UnityEngine.Random.Range(0, 100)}" : nameField.text;
            playerName.text = $"PlayerName: {nameField.text}";
        }
        /// <summary>
        /// Reset pre data.
        /// </summary>
        void ClearLobbyMemberState(){
            foreach(var s in LobbyMemberStatus){
                Destroy(s.Value.gameObject);
            }
            LobbyMemberStatus.Clear();
        }
    #endregion
        /// <summary>
        /// For button. Host start game by hand before fill a lobby with max members.
        /// </summary>
        public void StartGameByHand(){
            MatchMakeManager.Instance.ConcludeMatchMake();
        }

        /// <summary>
        /// For button. Just cancel matchmaking.
        /// </summary>
        public void CancelMatchMaking(){
            AsyncCancelMatchMaking().Forget();

        }
        async UniTask AsyncCancelMatchMaking(){
            await MatchMakeManager.Instance.ExitCurrentMatchMake(true);
            nameField.gameObject.SetActive(true);
        }
    }
}