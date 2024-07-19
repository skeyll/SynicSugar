using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This sample is　Minimum sample.
/// Awake: Prep for matchmaking (For basis of Events. This sample dosen't have condition for matchmaking)
/// Start: Try to reconnect
/// UserInputs: Matchmaking or OfflineMode
namespace  SynicSugar.Samples.ReadHearts {
    public class ReadHeartsMatchMake : MonoBehaviour {
        enum MATCHMAKEING_STATE {
            NoneAndAfterStart, Standby, InMatchmaking, ReadyToStartGame
        }
        [SerializeField] GameObject matchmakePrefab;
        [SerializeField] Button startMatchMake, closeLobby, startGame;
        [SerializeField] MatchMakeConditions matchConditions;
        [SerializeField] Text buttonText;
        [SerializeField] Text matchmakeState;

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
        void SetGUIEvents(){
            MatchMakeManager.Instance.MatchMakingGUIEvents = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;

            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableStart += OnDisableStart;
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += OnEnableCancel;
        }
        void OnDisableStart(){
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
        }
        void OnEnableCancel(){
            SwitchButtonsActive(MATCHMAKEING_STATE.InMatchmaking);
        }
    #endregion
        //Register MatchMaking button
        public void StartMatchMake(){
            EOSDebug.Instance.Log("Start MatchMake.");
            StartMatchMakeEntity().Forget();
        }
        //We can't set NOT void process to Unity Event.
        //So, register StartMatchMake() to Button instead of this.
        //Or, change this to async void StartMatchMakeEntity() at the expense of performance. We can pass async void to UnityEvents.
        async UniTask StartMatchMakeEntity(){
            //We have two ways to call SearchAndCreateLobby.
            //If pass self caneltoken, we should use Try-catch.
            //If not, the API returns just result.
            //Basically, we don't have to give token to SynicSugar APIs and should use SynicSugar APIs to cancel matchmaking.
            bool selfTryCatch = false;

            if(!selfTryCatch){ //Recommend
                Result result = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(2));
                    
                if(result != Result.Success){
                    EOSDebug.Instance.Log("MatchMaking Failed.", result);
                    SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                    return;
                }
            }else{ //Sample for another way. To cancel via token in manual.
                try{
                    CancellationTokenSource matchCTS = new CancellationTokenSource();
                    Result result = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), token: matchCTS);

                    if(result != Result.Success){
                        EOSDebug.Instance.Log("MatchMaking Failed.", result);
                        SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                        return;
                    }
                }catch(OperationCanceledException){
                    EOSDebug.Instance.Log("Cancel MatchMaking by CancelToken");
                    SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                    return;
                }
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SwitchButtonsActive(MATCHMAKEING_STATE.ReadyToStartGame);
        }
        /// <summary>
        /// For button. Just cancel matchmaking.
        /// </summary>
        public void CancelMatchMaking(){
            AsyncCancelMatchMaking().Forget();
        }
        public async UniTaskVoid AsyncCancelMatchMaking(){
            await MatchMakeManager.Instance.ExitCurrentMatchMake(false);
        }
        /// <summary>
        /// Switch button active
        /// </summary>
        /// <param name="state"></param>
        void SwitchButtonsActive(MATCHMAKEING_STATE state){
            //To start matchmake
            startMatchMake.gameObject.SetActive(state == MATCHMAKEING_STATE.Standby);
            //To cancel matchmake
            closeLobby.gameObject.SetActive(state == MATCHMAKEING_STATE.InMatchmaking);
            //To start game
            startGame.gameObject.SetActive(state == MATCHMAKEING_STATE.ReadyToStartGame);
        }
    }
}
