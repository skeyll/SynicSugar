using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This sample is Minimum sample.
/// Awake: Prep for matchmaking (For basis of Events. This sample dosen't have condition for matchmaking)
/// Start: Try to reconnect
/// UserInputs: Matchmaking or OfflineMode
namespace  SynicSugar.Samples.ReadHearts 
{
    public class ReadHeartsMatchMake : MonoBehaviour 
    {
        private enum MatchmakingState
        {
            NoneAndAfterStart, Standby, InMatchmaking, ReadyToStartGame
        }
        [SerializeField] private GameObject matchmakePrefab;
        [SerializeField] private Button startMatchMake, closeLobby, startGame;
        [SerializeField] private MatchMakeConditions matchConditions;
        [SerializeField] private Text buttonText;
        [SerializeField] private Text matchmakeState;

    #region Prep for matchmaking
        //At first, prep GUI events for matchmaking.
        private void Awake()
        {
            if(MatchMakeManager.Instance == null)
            {
                Instantiate(matchmakePrefab);
            }
            //Prep matchmaking
            SetGUIEvents();
        }
        /// <summary>
        /// Register tests and button events for in-matchmaking.
        /// </summary>
        private void SetGUIEvents()
        {
            MatchMakeManager.Instance.MatchMakingGUIEvents = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;

            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableStart += OnDisableStart;
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += OnEnableCancel;
        }
        private void OnDisableStart()
        {
            SwitchButtonsActive(MatchmakingState.NoneAndAfterStart);
        }
        private void OnEnableCancel()
        {
            SwitchButtonsActive(MatchmakingState.InMatchmaking);
        }
    #endregion
        //Register MatchMaking button
        public void StartMatchMake()
        {
            SynicSugarDebug.Instance.Log("Start MatchMake.");
            StartMatchMakeEntity().Forget();
        }
        //We can't set NOT void process to Unity Event.
        //So, register StartMatchMake() to Button instead of this.
        //Or, change this to async void StartMatchMakeEntity() at the expense of performance. We can pass async void to UnityEvents.
        private async UniTask StartMatchMakeEntity()
        {
            Result result = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(2));
                
            if(result != Result.Success)
            {
                SynicSugarDebug.Instance.Log("MatchMaking Failed.", result);
                SwitchButtonsActive(MatchmakingState.Standby);
                return;
            }

            SynicSugarDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SwitchButtonsActive(MatchmakingState.ReadyToStartGame);
        }
        /// <summary>
        /// For button. Just cancel matchmaking.
        /// </summary>
        public void CancelMatchMaking()
        {
            AsyncCancelMatchMaking().Forget();
        }
        public async UniTaskVoid AsyncCancelMatchMaking()
        {
            await MatchMakeManager.Instance.ExitCurrentMatchMake(false);
        }
        /// <summary>
        /// Switch button active
        /// </summary>
        /// <param name="state"></param>
        private void SwitchButtonsActive(MatchmakingState state)
        {
            //To start matchmake
            startMatchMake.gameObject.SetActive(state == MatchmakingState.Standby);
            //To cancel matchmake
            closeLobby.gameObject.SetActive(state == MatchmakingState.InMatchmaking);
            //To start game
            startGame.gameObject.SetActive(state == MatchmakingState.ReadyToStartGame);
        }
    }
}