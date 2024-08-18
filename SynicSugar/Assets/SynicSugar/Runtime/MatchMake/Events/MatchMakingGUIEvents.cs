#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using SynicSugar.P2P;
#if SYNICSUGAR_TMP
using TMPro;
#else
using UnityEngine.UI;
#endif

namespace SynicSugar.MatchMake {
    /// <summary>
    /// To register and manage events that occur during matchmaking.
    /// </summary>
    [Serializable]
    public class MatchMakingGUIEvents {
        public MatchMakingGUIEvents(){}
    #if SYNICSUGAR_TMP
        public MatchMakingGUIEvents(TMP_Text displayedStateText){
            stateText = displayedStateText;
        }
    #else
        public MatchMakingGUIEvents(Text displayedStateText){
            stateText = displayedStateText;
        }
    #endif

        /// <summary>
        /// To display text state.
        /// </summary>
    #if SYNICSUGAR_TMP
        public TMP_Text stateText = null;
    #else
        public Text stateText = null;
    #endif
        bool enabledManualConclude = false;
        // public TextP
        public bool canKick { get; internal set; }
    #region Event
        /// <summary>
        /// After push start button, prevent to be re-pressed matchmaking button.
        /// </summary>
        public event Action DisableStart;
        /// <summary>
        /// After join and create lobby, accept cancel matchmake and host's kick member.
        /// </summary>
        public event Action EnableCancelKick;
        /// <summary>
        /// This is enabled when the required members are passed to the Matchmaking API.<br />
        /// After filled in the minimum number of members, can manually close Lobby to start p2p.
        /// </summary>
        public event Action EnableHostConclude;
        /// <summary>
        /// This is enabled when the required members are passed to the Matchmaking API.<br />
        /// Invoked when members leave and condition is no longer met.
        /// </summary>
        public event Action DisableHostConclude;
        /// <summary>
        /// After complete or cancel matchmaking, prevent to change lobby state.
        /// </summary>
        public event Action DisableCancelKickConclude;
        /// <summary>
        /// Invoked when the number of participants in the lobby changes.
        /// </summary>
        public event Action<UserId, bool> OnLobbyMemberCountChanged;
    #endregion
    #region Text
        /// <summary>
        /// Wait for starting(pushing) the matchmaking process (button).<br />
        /// </summary>
        public string Standby;
        /// <summary>
        /// Until join or create lobby.<br />
        /// Invoke DisableStart().
        /// </summary>
        public string StartMatchmaking;
        /// <summary>
        /// Wait for opponents in Lobby until finish Matchmake.<br />
        /// Invoke EnableCancelKick(). canKick becomes true.
        /// </summary>
        public string WaitForOpponents;
        /// <summary>
        /// After Filled lobby or close the matchmaking in manually, start to exchange data for p2p.<br />
        /// Invoke DisableCancelKickFinish().
        /// </summary>
        public string FinishMatchmaking;
        /// <summary>
        /// Can start p2p and game.<br />
        /// (Move to GameScene or that UI mode.)
        /// </summary>
        public string ReadyForConnection;
        /// <summary>
        /// Leave or destroy lobby.<br />
        /// Invoke DisableCancelKickFinish().
        /// </summary>
        public string TryToCancel;
        /// <summary>
        /// Until join lobby. Same StartMatchmaking<br />
        /// Invoke DisableStart().
        /// </summary>
        public string StartReconnection;
    #endregion
        internal enum State {
            Standby, Start, Wait, Conclude, Ready, Cancel, Recconect
        }
        internal void Clear(){
            DisableStart = null;
            EnableCancelKick = null;
            EnableHostConclude = null;
            DisableHostConclude = null;
            DisableCancelKickConclude = null;
            OnLobbyMemberCountChanged = null;
        }
        
    #if SYNICSUGAR_TMP
        internal void ChangeState(State state){
            switch(state){
                case State.Standby:
                    canKick = false;
                    enabledManualConclude = false;
                    if(stateText != null){
                        stateText.SetText(Standby);
                    }
                break;
                case State.Start:
                    DisableStart?.Invoke();
                    canKick = false;
                    enabledManualConclude = false;
                    if(stateText != null){
                        stateText.SetText(StartMatchmaking);
                    }
                break;
                case State.Wait:
                    EnableCancelKick?.Invoke();
                    canKick = true;
                    if(stateText != null){
                        stateText.SetText(WaitForOpponents);
                    }
                break;
                case State.Conclude:
                    DisableCancelKickConclude?.Invoke();
                    canKick = false;
                    if(stateText != null){
                        stateText.SetText(FinishMatchmaking);
                    }
                break;
                case State.Ready:
                    if(stateText != null){
                        stateText.SetText(ReadyForConnection);
                    }
                break;
                case State.Cancel:
                    DisableCancelKickConclude?.Invoke();
                    canKick = false;
                    if(stateText != null){
                        stateText.SetText(TryToCancel);
                    }
                break;
            }
        }
    #else
        internal void ChangeState(State state){
            switch(state){
                case State.Standby:
                    if(stateText != null){
                        stateText.text = Standby;
                    }
                break;
                case State.Start:
                    DisableStart?.Invoke();
                    canKick = false;
                    if(stateText != null){
                        stateText.text = StartMatchmaking;
                    }
                break;
                case State.Wait:
                    EnableCancelKick?.Invoke();
                    canKick = true;
                    if(stateText != null){
                        stateText.text = WaitForOpponents;
                    }
                break;
                case State.Conclude:
                    DisableCancelKickConclude?.Invoke();
                    canKick = false;
                    if(stateText != null){
                        stateText.text = FinishMatchmaking;
                    }
                break;
                case State.Ready:
                    if(stateText != null){
                        stateText.text = ReadyForConnection;
                    }
                break;
                case State.Cancel:
                    DisableCancelKickConclude?.Invoke();
                    canKick = false;
                    if(stateText != null){
                        stateText.text = TryToCancel;
                    }
                break;
            }
        }
    #endif
        /// <summary>
        /// To display Member count. <br />
        /// After meet lobby min member counts.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isParticipated"></param>
        /// <param name="meetMinCondition"></param>
        internal void LobbyMemberCountChanged(UserId target, bool isParticipated, bool meetMinCondition){
            OnLobbyMemberCountChanged?.Invoke(target, isParticipated);
            UnityEngine.Debug.Log("A" + meetMinCondition);
            if(!MatchMakeManager.Instance.isHost){
                return;
            }
            UnityEngine.Debug.Log("B" + meetMinCondition);
            if(enabledManualConclude != meetMinCondition){
                enabledManualConclude = meetMinCondition;
                
                if(meetMinCondition){
                #if SYNICSUGAR_LOG
                    UnityEngine.Debug.Log("LobbyMemberCountChanged: Matchmaking meets min member conditions. Host can close Lobby from now.");
                #endif
                    EnableHostConclude?.Invoke();
                }else{
                    DisableHostConclude?.Invoke();
                }
            }
        }
        internal void LocalUserIsPromoted(bool meetMinCondition){
            if(!MatchMakeManager.Instance.isHost){
                return;
            }
            if(enabledManualConclude != meetMinCondition){
                enabledManualConclude = meetMinCondition;
                
                if(meetMinCondition){
                #if SYNICSUGAR_LOG
                    UnityEngine.Debug.Log("LobbyMemberCountChanged: Matchmaking meets min member conditions. Host can close Lobby from now.");
                #endif
                    EnableHostConclude?.Invoke();
                }else{
                    DisableHostConclude?.Invoke();
                }
            }
        }
        /// <summary>
        /// To display Member count.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isParticipated"></param>
        internal void LobbyMemberCountChanged(UserId target, bool isParticipated){
            OnLobbyMemberCountChanged?.Invoke(target, isParticipated);
        }
    }
}