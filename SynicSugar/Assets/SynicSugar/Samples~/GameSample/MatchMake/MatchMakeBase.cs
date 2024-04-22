using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
#if SYNICSUGAR_TMP
using TMPro;
#endif

namespace  SynicSugar.Samples {
    public class MatchMakeBase : MonoBehaviour{
        [SerializeField] GameObject matchmakePrefab;
        [SerializeField] Button startMatchMake, closeLobby;
        [SerializeField] protected MatchMakeConditions matchConditions;
        [SerializeField] Text buttonText;
    #if SYNICSUGAR_TMP
        [SerializeField] TMP_Text matchmakeState;

    #else
        [SerializeField] Text matchmakeState;

    #endif
        //For Tank
        protected enum SceneState{
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
        public virtual void SetGUIEvents(){
            MatchMakeManager.Instance.MatchMakingGUIEvents = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
        #if SYNICSUGAR_TMP
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;
        #else
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;
        #endif

            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableStart += OnDisableStart;
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += OnEnableCancel;
        }
    #endregion
    #region For Recconect
        void Start(){
            //Dose this game allow user to re-join thedisconnected match? 
            if(MatchMakeManager.Instance.lobbyIdSaveType == MatchMakeManager.RecconectLobbyIdSaveType.NoReconnection){
                return;
            }

            //Try recconect
            //Sample projects use the playerprefs to save LobbyIDs for recconection.
            string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
            TryToreconnect(LobbyID).Forget();
        }
        async UniTask TryToreconnect(string LobbyID){
            //On the default way, return Empty when there is no lobby data in local.
            if(string.IsNullOrEmpty(LobbyID)){
                EOSDebug.Instance.Log($"This user doesn't have LobbyId key.");
                return;
            }
            EOSDebug.Instance.Log($"Try reconnection with LobbyID.");
            
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
        public virtual void StartMatchMake(){
            EOSDebug.Instance.Log("Start MatchMake.");
            StartMatchMakeEntity().Forget();
        }
        //We can't set NOT void process to Unity Event.
        //So, register StartMatchMake() to Button instead of this.
        //Or, change this to async void StartMatchMakeEntity() at the expense of performance. We can pass async void to UnityEvents.
        internal virtual async UniTask StartMatchMakeEntity(){}

        /// <summary>
        /// Register to Cancel button. <br />
        /// Host delete and Guest leave the current lobby.
        /// </summary>
        public virtual void CancelMatchMaking(){
            MatchMakeManager.Instance.ExitCurrentMatchMake(false).Forget();
        }
        /// <summary>
        /// Register to ReturnMenu button.<br />
        /// Host delete and Guest leave the current lobby. Then, destroy MatchMakeManager and back to MainMenu.
        /// </summary>
        /// <returns></returns>
        public virtual async void CanelMatchMakingAndReturnToLobby(){
            await MatchMakeManager.Instance.ExitCurrentMatchMake(true);
        }
        /// <summary>
        /// Manage UIs on after and before matchmaking.
        /// </summary>
        /// <param name="state"></param>
        protected virtual void SwitchGUIState(SceneState state){
            if(startMatchMake != null){
                startMatchMake.gameObject.SetActive(state == SceneState.Standby);
            }
            if(closeLobby != null){
                closeLobby.gameObject.SetActive(state == SceneState.ToGame);
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
        internal virtual void SwitchCancelButtonActive(bool isActivate){
            closeLobby.gameObject.SetActive(isActivate);
        }
    #endregion
    }
}
