using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;

namespace SynicSugar.Samples {
    public class ChatMatchMake : MatchMakeBase {
        [SerializeField] Button startGame, backtoMenu;
        [SerializeField] GameObject OfflineModeButton;
        [SerializeField] GameModeSelect modeSelect;
        public bool isOfflineMode { get; private set; } = false;
        #region For GUI events.
        public override void SetGUIEvents(){
            base.SetGUIEvents();
        }
        #endregion

    #region StartMatchMake
        /// <summary>
        /// Basic API. Search lobby, and if can't join, create lobby.
        /// </summary>
        public override void StartMatchMake(){
            isOfflineMode = false;
            base.StartMatchMake();
        }
        internal override async UniTask StartMatchMakeEntity(){
            SwitchGUIState(SceneState.inMatchMake);
            Result result = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(2));
                
            if(result != Result.Success){
                EOSDebug.Instance.Log("MatchMaking Failed.");
                SwitchGUIState(SceneState.Standby);
                return;
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");

            SwitchCancelButtonActive(true);
            startGame.gameObject.SetActive(true);
        }
    #endregion
    #region Offline Mode
        public async void StartOfflineMode(){
            EOSDebug.Instance.Log("OFFLINE Mode.");
            isOfflineMode = true;
            //To　simulate matchmaking
            // OfflineMatchmakingDelay delay = new OfflineMatchmakingDelay(2000, 1000, 1000, 1000);
            OfflineMatchmakingDelay delay = OfflineMatchmakingDelay.NoDelay;
            //This is always true.
            bool isSuccess = await MatchMakeManager.Instance.CreateOfflineLobby(matchConditions.GetLobbyCondition(2), delay);

            startGame.gameObject.SetActive(true);
        }
    #endregion
        public override async void CancelMatchMaking(){
            if(isOfflineMode){
                await MatchMakeManager.Instance.DestoryOfflineLobby(false);
            }else{
                base.CancelMatchMaking();
            }
        }
        /// <summary>
        /// Register to ReturnMenu button.<br />
        /// Host delete and Guest leave the current lobby. Then, destroy MatchMakeManager and back to MainMenu.
        /// </summary>
        /// <returns></returns>
        public override async void CanelMatchMakingAndReturnToLobby(){
            if(isOfflineMode){
                await MatchMakeManager.Instance.DestoryOfflineLobby();
            }else{
                base.CanelMatchMakingAndReturnToLobby();
            }

            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString());
        }

        internal override void SwitchCancelButtonActive(bool isActivate){
            base.SwitchCancelButtonActive(isActivate);
            backtoMenu.gameObject.gameObject.SetActive(isActivate);
        }
        /// <summary>
        /// Manage UIs on after and before matchmaking.
        /// </summary>
        /// <param name="state"></param>
        protected override void SwitchGUIState(SceneState state){
            base.SwitchGUIState(state);
            OfflineModeButton.SetActive(state == SceneState.Standby);
            startGame.gameObject.SetActive(state == SceneState.ToGame);
            backtoMenu.gameObject.SetActive(false);
        }
    }
}