using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
/// <summary>
/// Awake: Prep for matchmaking (To register events for basis, detail and so on.)
/// UserInputs: Matchmaking (Srandard, Create, Search)
/// </summary>
namespace SynicSugar.Samples.Tank 
{
    public class TankMatchMake : MonoBehaviour 
    {
        private enum MatchmakingState 
        { 
            NoneAndAfterStart, Standby, InMatchmakingStandard, InMatchmakingAfterMeetingMinCondition
        }
        private enum MatchmakingType 
        { 
            Standard, CreateAsHost, JoinAsGuest, Offline
        }
        /// <summary>
        /// Display matchmaking state on GUI.
        /// </summary>
        [SerializeField] private Text matchmakeState;
        [SerializeField] private Button startMatchMake, createLobby, searchLobby, offlineMode,
                        cancelMatchMake, manualStart;
        [SerializeField] private TankMatchMakeConditions matchConditions;
        private MatchmakingLobbyMaker lobbyMaker;

        [SerializeField] private GameObject MembersPanel;
        [SerializeField] private Transform memberContentParent;
        [SerializeField] private GameObject memberStatePrefab;
        [SerializeField] private Text lobbyMemberCount;

        //Key is UserId
        private Dictionary<string, TankLobbyMemberState> LobbyMemberStatus = new();

    #region Prep for matchmaking
        //At first, prep GUI events for matchmaking　and try reconnection.
        private void Start()
        {
            SwitchButtonsActive(MatchmakingState.NoneAndAfterStart);
            lobbyMaker = new MatchmakingLobbyMaker(matchConditions);
            SetGUIEvents();
            
            //Sample projects use save API of SynicSugar to save into Playerprefs for recconection.
            //If you saved LobbyId by the your save code, you need get Id from there yourself.
            string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();

            if(!string.IsNullOrEmpty(LobbyID)) //If user has lobbyid in local, try to join lobby.
            { 
                TryToReconnect(LobbyID).Forget();
                return;
            }
            SwitchButtonsActive(MatchmakingState.Standby);
        }
        private async UniTask TryToReconnect(string LobbyID)
        {
            Result result = await MatchMakeManager.Instance.ReconnectLobby(LobbyID);

            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("TryToReconnect: Failure: ", result);
                SwitchButtonsActive(MatchmakingState.Standby);
                return;
            }
            SynicSugarDebug.Instance.Log($"TryToReconnect: Success! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SceneChanger.ChangeGameScene(Scene.Tank);
        }
        /// <summary>
        /// Register tests and button events for in-matchmaking.
        /// </summary>
        public void SetGUIEvents()
        {
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
        private void OnDisableStart()
        {
            //Cancel action need be disable when creating or joining lobby.
            SwitchButtonsActive(MatchmakingState.NoneAndAfterStart);
        }
        /// <summary>
        /// Finish to create or join a lobby, and accept canceling process.
        /// </summary>
        private void OnEnableCancelAndKick()
        {
            SwitchButtonsActive(MatchmakingState.InMatchmakingStandard);

            if(MatchMakeManager.Instance.isHost)
            {
                SwitchKickButtonsActive(true);
            }
        }
        /// <summary>
        /// Can close Lobby and start game after meets requested members. 
        /// </summary>
        private void OnEnableHostConclude()
        {
            SwitchButtonsActive(MatchmakingState.InMatchmakingAfterMeetingMinCondition);
        }
        /// <summary>
        /// Disactivate close button when doesn't meet requested members. 
        /// </summary>
        private void OnDisableHostConclude()
        {
            SwitchButtonsActive(MatchmakingState.InMatchmakingStandard);
        }
        /// <summary>
        /// Unctivate all buttons when close or complete matchmaking.
        /// </summary>
        private void DisableCancelKickConclude()
        {
            SwitchButtonsActive(MatchmakingState.NoneAndAfterStart);

            if(MatchMakeManager.Instance.isHost)
            {
                SwitchKickButtonsActive(false);
            }
        }
        /// <summary>
        /// Switch button active
        /// </summary>
        /// <param name="state"></param>
        private void SwitchButtonsActive(MatchmakingState state)
        {
            //Before matchmake
            startMatchMake.gameObject.SetActive(state == MatchmakingState.Standby);
            createLobby.gameObject.SetActive(state == MatchmakingState.Standby);
            searchLobby.gameObject.SetActive(state == MatchmakingState.Standby);
            offlineMode.gameObject.SetActive(state == MatchmakingState.Standby);
            matchConditions.SwitchInputfieldActive(state == MatchmakingState.Standby);
            //In Matchmaking.
            MembersPanel.SetActive(state != MatchmakingState.Standby);
            //To cancel matchmake
            cancelMatchMake.gameObject.SetActive(state == MatchmakingState.InMatchmakingStandard || state == MatchmakingState.InMatchmakingAfterMeetingMinCondition);
            //To start game by hand
            manualStart.gameObject.SetActive(state == MatchmakingState.InMatchmakingAfterMeetingMinCondition);
        }
        /// <summary>
        /// For GUI. Just display current lobbu count.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isParticipated"></param>
        private void OnLobbyMemberCountChanged(UserId target, bool isParticipated)
        {
            if(isParticipated)
            {
                GameObject stateObj = Instantiate(memberStatePrefab, memberContentParent);
                TankLobbyMemberState state = stateObj.GetComponent<TankLobbyMemberState>();
                LobbyMemberStatus.Add(target.ToString(), state);
            }
            else
            {
                Destroy(LobbyMemberStatus[target.ToString()].gameObject);
                LobbyMemberStatus.Remove(target.ToString());
            }
            lobbyMemberCount.text = $"Current: {MatchMakeManager.Instance.GetCurrentLobbyMemberCount()} / Max: {MatchMakeManager.Instance.GetMaxLobbyMemberCount()}";
        }
        /// <summary>
        /// For GUI. Display lobby member status about Attributes.
        /// </summary>
        /// <param name="target"></param>
        private void OnUpdatedMemberAttribute(UserId target)
        {
            List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
            TankLobbyMemberState state = LobbyMemberStatus[target.ToString()];
            
            state.SetData(target, data.GetValue<string>("NAME"), AttributeData.GetValueAsString(data, "LEVEL"));
        }
        /// <summary>
        /// Disactivate kick buttons.
        /// </summary>
        private void SwitchKickButtonsActive(bool isActivate)
        {
            foreach(var s in LobbyMemberStatus)
            {
                if(MatchMakeManager.Instance.isLocalUserId(s.Key)) continue; 

                s.Value.SwitchKickButtonActive(isActivate);
            }
        }
    #endregion
    #region StartMatchMake
        /// <summary>
        /// For buttons.
        /// </summary>
        /// <param name="type">MatchmakingType</param>
        [EnumAction(typeof(MatchmakingType))]
        public void StartMatchMake(int type)
        {
            //To clear old objects.
            ClearLobbyMemberState();

            MatchMakeEntity((MatchmakingType)type).Forget();
        }
        private async UniTask MatchMakeEntity(MatchmakingType type)
        {
            SwitchButtonsActive(MatchmakingState.NoneAndAfterStart);
            Result result = Result.None;
            Lobby conditionObject = lobbyMaker.GenerateConditionLobbyObject(8, false);
            //User attributes
            List<AttributeData> userAttributes = matchConditions.GenerateUserAttributes();
            //To pass name to game scene
            TankPassedData.PlayerName = AttributeData.GetValueAsString(userAttributes, "NAME");

            switch (type)
            {
                case MatchmakingType.Standard:
                    result = await MatchMakeManager.Instance.SearchAndCreateLobby(conditionObject, minLobbyMember: 2, userAttributes: userAttributes);
                break;
                case MatchmakingType.CreateAsHost:
                    result = await MatchMakeManager.Instance.CreateLobby(conditionObject, minLobbyMember: 2, userAttributes: userAttributes);
                break;
                case MatchmakingType.JoinAsGuest:
                    result = await MatchMakeManager.Instance.SearchLobby(conditionObject, minLobbyMember: 2, userAttributes: userAttributes);
                break;
                case MatchmakingType.Offline:
                    result = await MatchMakeManager.Instance.CreateOfflineLobby(conditionObject, OfflineMatchmakingDelay.NoDelay, userAttributes: userAttributes);
                break;
            }
                
            if(result != Result.Success)
            {
                SwitchButtonsActive(MatchmakingState.Standby);
                return;
            }

            SceneChanger.ChangeGameScene(Scene.Tank);
        }
        /// <summary>
        /// Reset pre data.
        /// </summary>
        private void ClearLobbyMemberState()
        {
            foreach(var s in LobbyMemberStatus)
            {
                Destroy(s.Value.gameObject);
            }
            LobbyMemberStatus.Clear();
        }
    #endregion
        /// <summary>
        /// For button. Host start game by hand before fill a lobby with max members.
        /// </summary>
        public void StartGameByHand()
        {
            MatchMakeManager.Instance.ConcludeMatchMake();
        }

        /// <summary>
        /// For button. Just cancel matchmaking.
        /// </summary>
        public void CancelMatchMaking()
        {
            AsyncCancelMatchMaking().Forget();

        }
        private async UniTask AsyncCancelMatchMaking()
        {
            await MatchMakeManager.Instance.ExitCurrentMatchMake(false);
        }
    }
}