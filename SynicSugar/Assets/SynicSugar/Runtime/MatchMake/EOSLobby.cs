using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;
using SynicSugar.RTC;

namespace SynicSugar.MatchMake {
    internal class EOSLobby {
        internal Lobby CurrentLobby { get; private set; } = new Lobby();

        bool waitingMatch, waitLeave, canLeave;
        LobbySearch CurrentSearch;
        Dictionary<Lobby, LobbyDetails> SearchResults = new Dictionary<Lobby, LobbyDetails>();
        //User config
        uint MAX_SEARCH_RESULT;
        int timeoutMS, initconnectTimeoutMS;
        List<AttributeData> userAttributes;
        bool useManualFinishMatchMake;
        uint requiredMembers;
        //Notification
        /// <summary>
        /// Join, Leave, Kicked, Promote or so on...
        /// </summary>
        ulong LobbyMemberStatusNotifyId;
        /// <summary>
        /// Lobby attributes
        /// </summary>
        ulong LobbyUpdateNotififyId;
        /// <summary>
        /// Member attributes.
        /// </summary>
        ulong LobbyMemberUpdateNotifyId;

        bool isMatchSuccess;
        string socketName = string.Empty;
        CancellationTokenSource timerCTS;
        /// <summary>
        /// For callback
        /// </summary>
        Result matchmakingResultCode, cancelResultCode;

        internal EOSLobby(uint maxSearch, int MatchmakingTimeout, int InitialConnectionTimeout){
            MAX_SEARCH_RESULT = maxSearch;
            //For Unitask
            timeoutMS = MatchmakingTimeout * 1000;
            initconnectTimeoutMS = InitialConnectionTimeout * 1000;
            timerCTS = new CancellationTokenSource();
        }
        /// <summary>
        /// If call this in matchmaking, it could cause a bug.
        /// </summary>
        /// <param name="MatchmakingTimeout"></param>
        /// <param name="InitialConnectionTimeout"></param>
        internal void SetTimeoutSec(int MatchmakingTimeout, int InitialConnectionTimeout){
            //For Unitask
            timeoutMS = MatchmakingTimeout * 1000;
            initconnectTimeoutMS = InitialConnectionTimeout * 1000;
        }

        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns></returns>
        internal async UniTask<Result> StartMatching(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes, uint minLobbyMember){
            matchmakingResultCode = Result.None;
            this.userAttributes = userAttributes;
            useManualFinishMatchMake = minLobbyMember > 0;
            requiredMembers = minLobbyMember;

            //Start timer 
            var timer = TimeoutTimer();
            //Serach
            MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Start);
            Result canJoin = await JoinExistingLobby(lobbyCondition, token);

            if(canJoin == Result.Success){
                // Wait for SocketName to use p2p connection
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Wait);

                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);

                if(timerCTS.IsCancellationRequested){
                    return Result.TimedOut;
                }
                if(token.IsCancellationRequested){
                    return Result.Canceled;
                }

                if(isMatchSuccess){
                    MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Conclude);
                    Result canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(canInit != Result.Success){
                        Debug.LogErrorFormat("Fail InitConnectConfig :{0}", canInit);
                        return canInit;
                    }

                    Result canConnect = await OpenConnection(token);
                    if(canConnect != Result.Success){
                        Debug.LogErrorFormat("Fail OpenConnection :{0}", canConnect);
                        return canConnect;
                    }

                    await MatchMakeManager.Instance.OnSaveLobbyID();
                }
                return isMatchSuccess ? Result.Success : matchmakingResultCode;
            }
            //If player cannot join lobby as a guest, creates a lobby as a host and waits for other player.
            //Create
            Result canCreate = await CreateLobby(lobbyCondition, token);
            if(canCreate == Result.Success){
                // Wait for other player to join
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Wait);

                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);
                
                if(timerCTS.IsCancellationRequested){
                    return Result.TimedOut;
                }
                if(token.IsCancellationRequested){
                    return Result.Canceled;
                }
                
                if(isMatchSuccess){
                    MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Conclude);
                    Result canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(canInit != Result.Success){
                        Debug.LogErrorFormat("Fail InitConnectConfig: {0}", canInit);
                        return canInit;
                    }

                    Result canConnect = await OpenConnection(token);
                    if(canConnect != Result.Success){
                        Debug.LogErrorFormat("Fail OpenConnection :{0}", canConnect);
                        return canConnect;
                    }

                    await MatchMakeManager.Instance.OnSaveLobbyID();

                    return Result.Success;
                }
            }
            //Failed due to no-playing-user or server problems.
            return matchmakingResultCode;
        }
        /// <summary>
        /// Just search Lobby<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<Result> StartJustSearch(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes, uint minLobbyMember){
            matchmakingResultCode = Result.None;
            this.userAttributes = userAttributes;
            //For host migration
            useManualFinishMatchMake = minLobbyMember > 0;
            requiredMembers = minLobbyMember;

            var timer = TimeoutTimer();
            //Serach
            MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Start);
            Result canJoin = await JoinExistingLobby(lobbyCondition, token);
            if(canJoin == Result.Success){
                // Wait for SocketName to use p2p connection
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Wait);
                //Wait for closing lobby or timeout
                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);
                
                if(timerCTS.IsCancellationRequested){
                    return Result.TimedOut;
                }
                if(token.IsCancellationRequested){
                    return Result.Canceled;
                }

                if(isMatchSuccess){
                    MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Conclude);
                    Result canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(canInit != Result.Success){
                        Debug.LogErrorFormat("Fail InitConnectConfig: {0}", canInit);
                        return canInit;
                    }
    
                    Result canConnect = await OpenConnection(token);
                    if(canConnect != Result.Success){
                        Debug.LogErrorFormat("Fail OpenConnection :{0}", canConnect);
                        return canConnect;
                    }
                    
                    await MatchMakeManager.Instance.OnSaveLobbyID();
                }
                // Failure is Result.LobbyClosed or Result.UserKicked or Result.NetworkDisconnected from OnLobbyMemberStatusReceived();
                return isMatchSuccess ? Result.Success : matchmakingResultCode;
            }
            //This is NOT Success. Failed due to no-playing-user or server problems.
            return canJoin;
        }
        /// <summary>
        /// Create lobby as host<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<Result> StartJustCreate(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes, uint minLobbyMember){
            matchmakingResultCode = Result.None;
            this.userAttributes = userAttributes;
            useManualFinishMatchMake = minLobbyMember > 0;
            requiredMembers = minLobbyMember;
            
            var timer = TimeoutTimer();

            MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Start);
            Result canCreate = await CreateLobby(lobbyCondition, token);
            if(canCreate == Result.Success){
                // Wait for other player to join
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Wait);
                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);

                if(timerCTS.IsCancellationRequested){
                    return Result.TimedOut;
                }
                if(token.IsCancellationRequested){
                    return Result.Canceled;
                }

                if(isMatchSuccess){
                    MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Conclude);
                    Result canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(canInit != Result.Success){
                        Debug.LogErrorFormat("Fail InitConnectConfig: {0}", canInit);
                        return canInit;
                    }

                    Result canConnect = await OpenConnection(token);
                    if(canConnect != Result.Success){
                        Debug.LogErrorFormat("Fail OpenConnection :{0}", canConnect);
                        return canConnect;
                    }

                    await MatchMakeManager.Instance.OnSaveLobbyID();
                }
                // Failure is Result.LobbyClosed or Result.UserKicked or Result.NetworkDisconnected from OnLobbyMemberStatusReceived();
                return isMatchSuccess ? Result.Success : matchmakingResultCode;
            }
            //This is NOT Success. Failed due to no-playing-user or server problems.
            return canCreate;
        }
        /// <summary>
        /// Join the Lobby with specific id to that lobby. <br />
        /// To return to a disconnected lobby.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        internal async UniTask<Result> JoinLobbyBySavedLobbyId(string LobbyID, CancellationToken token){
            matchmakingResultCode = Result.None;
            //Search
            MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Recconect);
            Result canSearch = await RetriveLobbyByLobbyId(LobbyID, token);

            if(canSearch != Result.Success){
                #if SYNICSUGAR_LOG
                    Debug.LogErrorFormat("JoinLobbyBySavedLobbyId: RetriveLobbyByLobbyId is failer.: {0}.", canSearch);
                #endif
                await MatchMakeManager.Instance.OnDeleteLobbyID();
                return canSearch; //This is NOT Success. Can't retrive Lobby data from EOS.
            }
            //Join when lobby has members than more one.
            Result canJoin = await TryJoinSearchResults(token, true);
        #if SYNICSUGAR_LOG
            Debug.LogFormat("JoinLobbyBySavedLobbyId: TryJoinSearchResults is '{0}'.", canJoin);
        #endif
            if(canJoin != Result.Success){
                await MatchMakeManager.Instance.OnDeleteLobbyID();
                return canJoin; //This is NOT Success. The lobby was already closed.
            }
            //For the host migration
            AddNotifyLobbyMemberStatusReceived();
            //Create new instance with Reconnecter flag.
            p2pInfo.Instance.userIds = new UserIds(true);
            //Prep Connection
            Result canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
            if(canInit != Result.Success){
                Debug.LogError("Fail InitConnectConfig");
                return canInit; //This is NOT Success. 
            }

            Result canConnect = await OpenConnectionForReconnecter(token);
            if(canConnect != Result.Success){
                return canConnect; //This is NOT Success. 
            }
            
            return Result.Success;
        }
        /// <summary>
        /// Wait for timeout. Leave Lobby, then the end of this task stop main task.
        /// </summary>
        /// <returns></returns>
        async UniTask TimeoutTimer(){
            if(timerCTS.Token.CanBeCanceled){
                timerCTS.Cancel();
            }
            timerCTS = new CancellationTokenSource();
            await UniTask.Delay(timeoutMS, cancellationToken: timerCTS.Token);
            if(waitingMatch && !waitLeave){
                Result canLeave = await LeaveLobby(true, timerCTS.Token);
        #if SYNICSUGAR_LOG
                Debug.Log("Cancel matching by timeout");
        #endif
                if(canLeave == Result.Success){
                    MatchMakeManager.Instance.matchingToken?.Cancel();
                }
            }
        }
//Host
#region Create
        /// <summary>
        /// Create a Lobby. <br />
        /// *<c>StartMatching()</c> can search for an existing lobby and create one if there isn't.
        /// </summary>
        /// <param name="lobbyCondition"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> CreateLobby(Lobby lobbyCondition, CancellationToken token){
            // Check if there is current session. Leave it.
            if (CurrentLobby.isValid()){
#if SYNICSUGAR_LOG
                Debug.LogWarningFormat("Create Lobby: Leaving Current Lobby '{0}'", CurrentLobby.LobbyId);
#endif
                await LeaveLobby(true, token);
            }

            //Lobby Option
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxLobbyMembers = lobbyCondition.MaxLobbyMembers,
                PermissionLevel = lobbyCondition.PermissionLevel,
                BucketId = lobbyCondition.BucketId,
                PresenceEnabled = false,
                RejoinAfterKickRequiresInvite = lobbyCondition.RejoinAfterKickRequiresInvite,
                AllowInvites = lobbyCondition.bAllowInvites,
                EnableRTCRoom = lobbyCondition.bEnableRTCRoom
            };

            // Init for async
            waitingMatch = true;
            isMatchSuccess = false;

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            //Set lobby data
            CurrentLobby = lobbyCondition;
            CurrentLobby._BeingCreated = true;
            CurrentLobby.LobbyOwner = EOSManager.Instance.GetProductUserId();

            lobbyInterface.CreateLobby(ref createLobbyOptions, null, OnCreateLobbyCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);

            if(!isMatchSuccess){
                Debug.LogError("Create Lobby: can't new lobby");
                return matchmakingResultCode;
            }
            // For the host migration
            AddNotifyLobbyMemberStatusReceived();
            // To get Member attributes
            AddNotifyLobbyMemberUpdateReceived();
            //Need add condition for serach
            Result canModify = await AddSerachLobbyAttribute(lobbyCondition, token);
            if(canModify != Result.Success){
                Debug.LogError("Create Lobby: can't add lobby search attributes");
                return matchmakingResultCode;
            }
            
            return Result.Success;
        }
        void OnCreateLobbyCompleted(ref CreateLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                matchmakingResultCode = (Result)info.ResultCode;
                Debug.LogErrorFormat("Created Lobby: error code: {0}", info.ResultCode);
                waitingMatch = false;
                return;
            }
            
            if (string.IsNullOrEmpty(info.LobbyId) || !CurrentLobby._BeingCreated){
                waitingMatch = false;
                return;
            }
            matchmakingResultCode = Result.Success;
            CurrentLobby.LobbyId = info.LobbyId;

            //For self
            MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(EOSManager.Instance.GetProductUserId()), true);

            isMatchSuccess = true;
            waitingMatch = false;
        }
        /// <summary>
        /// Set attribute for search and Host attributes.. This process is only for Host player.
        /// </summary>
        /// <param name="lobbyCondition"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> AddSerachLobbyAttribute(Lobby lobbyCondition, CancellationToken token){
            if (!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Change Lobby: This isn't lobby owner.");
#endif
                return Result.InvalidAPICall;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            //Create modify handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("AddSerachLobbyAttribute: Could not create lobby modification. Error code: {0}", result);
                return (Result)result;
            }
            //Set Backet ID
            LobbyModificationSetBucketIdOptions  bucketIdOptions = new(){
                BucketId = lobbyCondition.BucketId
            };
            result = lobbyHandle.SetBucketId(ref bucketIdOptions);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("AddSerachLobbyAttribute: Could not set bucket id. Error code: {0}", result);
                return (Result)result;
            }

            // Set attribute to handle in local
            foreach(var attribute in lobbyCondition.Attributes){
                LobbyModificationAddAttributeOptions attributeOptions = new LobbyModificationAddAttributeOptions(){
                    Attribute = attribute.AsLobbyAttribute(),
                    Visibility = attribute.Visibility
                };

                result = lobbyHandle.AddAttribute(ref attributeOptions);
                if (result != ResultE.Success){
                    Debug.LogErrorFormat("AddSerachLobbyAttributey: could not add attribute. Error code: {0}", result);
                    return (Result)result;
                }
            }
            foreach(var attr in userAttributes){
                var attrOptions = new LobbyModificationAddMemberAttributeOptions(){
                    Attribute = attr.AsLobbyAttribute(),
                    Visibility = LobbyAttributeVisibility.Public
                };
                result = lobbyHandle.AddMemberAttribute(ref attrOptions);

                if (result != ResultE.Success){
                    Debug.LogErrorFormat("AddSerachLobbyAttribute: could not add Host's member attribute. Error code: {0}", result);
                    return (Result)result;
                }
            }
            //Add attribute with handle
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };
            // Init for async
            waitingMatch = true;
            isMatchSuccess = false;
            
            lobbyInterface.UpdateLobby(ref updateOptions, null, OnAddSearchAttribute);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
            lobbyHandle.Release();

            return matchmakingResultCode; //"isMatchSuccess" is changed in async and callback method with result.
        }
        void OnAddSearchAttribute(ref UpdateLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                matchmakingResultCode = (Result)info.ResultCode;
                waitingMatch = false;
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                return;
            }
            matchmakingResultCode = Result.Success;
            OnLobbyUpdated(info.LobbyId);
            CurrentLobby._BeingCreated = false;
            //RTC
            RTCManager.Instance.AddNotifyParticipantStatusChanged();

            isMatchSuccess = true;
            waitingMatch = false;
        }
#endregion
//Guest
#region Search
        /// <summary>
        /// Find and join the lobby. <br />
        /// If the lobby fills up or is closed by the host, the lobby becomes unsearch-lobby and has SocketName for p2p instead of search attributes.<br />
        /// *<c>StartMatching()</c> can search for an existing lobby and create one if there isn't. <br />
        /// <c>ReconnectParticipatingLobby()</c> can re-join by lobby-ID.
        /// </summary>
        /// <param name="lobbyCondition"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> JoinExistingLobby(Lobby lobbyCondition, CancellationToken token){
            //Serach
            Result canRetrive = await RetriveLobbyByAttribute(lobbyCondition, token);
            if(canRetrive != Result.Success){
                return canRetrive; //Need to create own session
            }
            //Join
            Result canJoin = await TryJoinSearchResults(token);
            if(canJoin != Result.Success){
                return canJoin;  //Need to create own session
            }
            // To get SocketName safety
            AddNotifyLobbyUpdateReceived();
            // For the host migration
            AddNotifyLobbyMemberStatusReceived();
            // To get Member attributes
            AddNotifyLobbyMemberUpdateReceived();

            return Result.Success;
        }
        /// <summary>
        /// For use in normal matching. Retrive Lobby by Attributes. 
        /// Retrun true on getting Lobby data.
        /// </summary>
        /// <param name="lobbyCondition"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> RetriveLobbyByAttribute(Lobby lobbyCondition, CancellationToken token){
            //Create Search handle on local
            // Create new handle 
            CreateLobbySearchOptions searchOptions = new CreateLobbySearchOptions(){
                MaxResults = MAX_SEARCH_RESULT
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch lobbySearchHandle);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("Search Lobby: could not create SearchByAttribute. Error code: {0}", result);
                return (Result)result;
            }

            CurrentSearch = lobbySearchHandle;

            //Set Backet ID
            Epic.OnlineServices.Lobby.AttributeData bucketAttribute = new (){
                Key = "bucket",
                Value = new AttributeDataValue(){ AsUtf8 = lobbyCondition.BucketId }
            };
            LobbySearchSetParameterOptions paramOptions = new LobbySearchSetParameterOptions(){
                Parameter = bucketAttribute,
                ComparisonOp = ComparisonOp.Equal.AsEpic()
            };

            result = lobbySearchHandle.SetParameter(ref paramOptions);
            
            if (result != ResultE.Success){
                Debug.LogErrorFormat("Retrieve Lobby: failed to add bucketID. Error code: {0}", result);
                return (Result)result;
            }
            
            // Set other attributes
            foreach (var attribute in lobbyCondition.Attributes){
                Epic.OnlineServices.Lobby.AttributeData data = attribute.AsLobbyAttribute();
                paramOptions.Parameter = data;
                paramOptions.ComparisonOp = attribute.ComparisonOperator.AsEpic(); 

                result = lobbySearchHandle.SetParameter(ref paramOptions);

                if (result != ResultE.Success){
                    Debug.LogErrorFormat("Retrieve Lobby: failed to add option attribute. Error code: {0}", result);
                    return (Result)result;
                }
            }

            //Search with handle
            LobbySearchFindOptions findOptions = new LobbySearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            waitingMatch = true;
            lobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
            //Can retrive data
            return Result.Success;
        }
        /// <summary>
        /// Get Lobby from EOS by LobbyID<br />
        /// Can search closed-lobby.
        /// </summary>
        /// <param name="lobbyId">Id to get</param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> RetriveLobbyByLobbyId(string lobbyId, CancellationToken token){
            //Create Search handle on local
            // Create new handle 
            CreateLobbySearchOptions searchOptions = new CreateLobbySearchOptions();
            searchOptions.MaxResults = 1;

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch lobbySearchHandle);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("Lobby Search: could not create SearchByLobbyId. Error code: {0}", result);
                return (Result)result;
            }

            CurrentSearch = lobbySearchHandle;

            // Set Lobby ID
            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions(){
                LobbyId = lobbyId
            };
            result = lobbySearchHandle.SetLobbyId(ref setLobbyOptions);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("Search Lobby: failed to update SearchByLobbyId with lobby id. Error code: {0}", result);
                return (Result)result;
            }

            //Search with handle
            LobbySearchFindOptions findOptions = new LobbySearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            waitingMatch = true;
            lobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
            //Can retrive data
            return (Result)result;
        }

        void OnLobbySearchCompleted(ref LobbySearchFindCallbackInfo info){
                matchmakingResultCode = (Result)info.ResultCode;
                waitingMatch = false;
            if (info.ResultCode != ResultE.Success) {
                Debug.LogErrorFormat("Search Lobby: error code: {0}", info.ResultCode);
                return;
            }
        }
#endregion
#region Join
        /// <summary>
        /// Check result amounts, then if it has 1 or more, try join the lobby.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="needCheckMemberCount">For Reconenction process. If member is 0, it means ClosedLobby.</param>
        /// <returns></returns>
        async UniTask<Result> TryJoinSearchResults(CancellationToken token, bool needCheckMemberCount = false){ 
            if (CurrentSearch == null){
                Debug.LogError("TryJoinSearchResults: Failure on creating CurrentSearch data.");
                return Result.InvalidParameters;
            }

            var lobbySearchGetSearchResultCountOptions = new LobbySearchGetSearchResultCountOptions(); 
            uint searchResultCount = CurrentSearch.GetSearchResultCount(ref lobbySearchGetSearchResultCountOptions);
            
            if(searchResultCount == 0){
                return Result.NotFound;
            }
            //For reconnecter
            if(needCheckMemberCount){
                Result result = HasMembers();
                if(result == Result.Success){   
                    return Result.LobbyClosed;
                }
            }
            SearchResults.Clear();

            LobbySearchCopySearchResultByIndexOptions indexOptions = new LobbySearchCopySearchResultByIndexOptions();
            isMatchSuccess = false; //Init
            for (uint i = 0; i < searchResultCount; i++){
                Lobby lobbyObj = new Lobby();
                indexOptions.LobbyIndex = i;

                ResultE result = CurrentSearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails lobbyHandle);

                if (result == ResultE.Success && lobbyHandle != null){
                    waitingMatch = true;
                    JoinLobby(lobbyHandle);
                    
                    await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
                    
                    if(isMatchSuccess){
                        SearchResults.Add(lobbyObj, lobbyHandle);
                        break;
                    }
                    if(token.IsCancellationRequested){
                        return Result.Canceled;
                    }
                }
            }
            return Result.Success;
        }
        /// <summary>
        /// For Reconnection. <br />
        /// The lobby to try to reconnect has members? <br />
        /// Can't go back to the empty lobby.
        /// </summary>
        /// <returns></returns>
        Result HasMembers(){
            LobbySearchCopySearchResultByIndexOptions indexOptions = new LobbySearchCopySearchResultByIndexOptions(){ LobbyIndex = 0 };
            ResultE result = CurrentSearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails lobbyDetails);

            if (result != ResultE.Success){
                Debug.LogError("TryJoinSearchResults: Reconnecter can't create lobby handle to check member count.");
                return (Result)result;
            }
            LobbyDetailsGetMemberCountOptions countOptions = new LobbyDetailsGetMemberCountOptions();
            uint MemberCount = lobbyDetails.GetMemberCount(ref countOptions);
            lobbyDetails.Release();

            if(MemberCount == 0){
                Debug.LogError("TryJoinSearchResults: The lobby had been closed. There is no one.");
                return Result.LobbyClosed;
            }

            return Result.Success;
        }
        
        /// <summary>
        /// Call this from TryJoinSearchResults.<br />
        /// If can join a lobby, then update local lobby infomation in callback.
        /// </summary>
        void JoinLobby(LobbyDetails lobbyDetails){
            JoinLobbyOptions joinOptions = new JoinLobbyOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                LobbyDetailsHandle = lobbyDetails,
                PresenceEnabled = false
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSPlatformInterface().GetLobbyInterface();
            lobbyInterface.JoinLobby(ref joinOptions, null, OnJoinLobbyCompleted);
        }
        void OnJoinLobbyCompleted(ref JoinLobbyCallbackInfo info){
            matchmakingResultCode = (Result)info.ResultCode;
            if (info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("Join Lobby: error code: {0}", info.ResultCode);
                waitingMatch = false;
                return;
            }

            // If has joined in other lobby
            if (CurrentLobby.isValid() && !string.Equals(CurrentLobby.LobbyId, info.LobbyId)){
                LeaveLobby(true).Forget();
            }

            CurrentLobby.InitFromLobbyHandle(info.LobbyId);
            //Member Attribute
            AddUserAttributes();
            //RTC
            RTCManager.Instance.AddNotifyParticipantStatusChanged();
            string LocalId = EOSManager.Instance.GetProductUserId().ToString();
            foreach(var member in CurrentLobby.Members){
                MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(member.Key), true);
            }
            if(userAttributes != null && userAttributes.Count > 0){
                foreach(var m in CurrentLobby.Members){
                    if(m.Key != LocalId){
                        MatchMakeManager.Instance.MemberUpdatedNotifier.MemberAttributesUpdated(UserId.GetUserId(m.Key));
                    }
                }
            }

            waitingMatch = false;
        }
#endregion
#region Kick
        bool isKicking, canKick;
        /// <summary>
        /// Currently preventing duplicate calls.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal async UniTask<bool> KickTargetMember(UserId target, CancellationToken token){
            if(!CurrentLobby.isHost()){
                Debug.LogError("KickTargetMember: This user is not Host.");
                return false;
            }
            if(isKicking){
                Debug.LogError("KickTargetMember: This user is kicking member now.");
                return false;
            }
            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            var options = new KickMemberOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = CurrentLobby.LobbyOwner,
                TargetUserId = target.AsEpic
            };
            isKicking = true;
            canKick = false;
            lobbyInterface.KickMember(ref options, null, OnKickMember);
            await UniTask.WaitUntil(() => !isKicking, cancellationToken: token);
            return canKick;
        }
        void OnKickMember(ref KickMemberCallbackInfo info){
            if(info.LobbyId == CurrentLobby.LobbyId){
                canKick = info.ResultCode == ResultE.Success;
            }else{
                matchmakingResultCode = (Result)info.ResultCode;
                Debug.LogError("This is other lobby result.");
            }
            isKicking = false;
        }
#endregion
//Common
#region Notification
        /// <summary>
        /// For Host and Guest. To get the notification for a user to join and disconnect.
        /// </summary>
        /// <returns></returns>
        void AddNotifyLobbyMemberStatusReceived(){
            if(LobbyMemberStatusNotifyId == 0){
                var options = new AddNotifyLobbyMemberStatusReceivedOptions();
                LobbyMemberStatusNotifyId = EOSManager.Instance.GetEOSLobbyInterface().AddNotifyLobbyMemberStatusReceived(ref options, null, OnLobbyMemberStatusReceived);

                if (LobbyMemberStatusNotifyId == 0){
                    Debug.LogError("LobbyMemberStatusReceived: could not subscribe, bad notification id returned.");
                }
            }
        }
        void RemoveNotifyLobbyMemberStatusReceived(){
            EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(LobbyMemberStatusNotifyId);
            LobbyMemberStatusNotifyId = 0;
        }
        void OnLobbyMemberStatusReceived(ref LobbyMemberStatusReceivedCallbackInfo info){
            if (!info.TargetUserId.IsValid()){
                Debug.LogError("Lobby Notification: Current player is invalid.");
                return;
            }
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            // Target is local user and dosen't need to be updated.
            if (info.TargetUserId == productUserId){
                if (info.CurrentStatus == LobbyMemberStatus.Closed ||
                    info.CurrentStatus == LobbyMemberStatus.Kicked ||
                    info.CurrentStatus == LobbyMemberStatus.Disconnected){
                    OnKickedFromLobby(info.LobbyId);
                    switch(info.CurrentStatus){
                        case LobbyMemberStatus.Closed:
                            matchmakingResultCode = Result.LobbyClosed;
                        break;
                        case LobbyMemberStatus.Kicked:
                            matchmakingResultCode = Result.UserKicked;
                        break;
                        case LobbyMemberStatus.Disconnected:
                            matchmakingResultCode = Result.NetworkDisconnected;
                        break;
                    }
                    //Fail to MatchMake.
                    waitingMatch = false;
                    return;
                }
            }
            OnLobbyUpdated(info.LobbyId);
            
            //For MatchMaking
            if(waitingMatch){ //This flag is shared by both host and guest, and is false after getting SocketName.
                if(info.TargetUserId == productUserId && info.CurrentStatus == LobbyMemberStatus.Promoted){
            #if SYNICSUGAR_LOG
                Debug.Log("OnLobbyMemberStatusReceived: This local user becomes new Host.");
            #endif
                    //This local player manage lobby, So dosen't need update notify.
                    RemoveNotifyLobbyUpdateReceived();
                }
                if(useManualFinishMatchMake){
                    MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(info.TargetUserId), info.CurrentStatus == LobbyMemberStatus.Joined, CurrentLobby.Members.Count >= requiredMembers);
                }else{
                    //Lobby is full. Stop additional member and change search attributes to SoketName.
                    MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(info.TargetUserId), info.CurrentStatus == LobbyMemberStatus.Joined);
                    //Auto comlete match
                    if(CurrentLobby.isHost() && CurrentLobby.Members.Count == CurrentLobby.MaxLobbyMembers){
                        SwitchLobbyAttribute();
                    }
                }
                return;
            }

            //In game
            // Hosts changed?
            if (info.CurrentStatus == LobbyMemberStatus.Promoted){
                p2pInfo.Instance.userIds.HostUserId = UserId.GetUserId(info.TargetUserId);

                #if SYNICSUGAR_LOG
                    Debug.Log($"MemberStatusNotyfy: {info.TargetUserId} is promoted to host.");
                #endif
            }else if(info.CurrentStatus == LobbyMemberStatus.Left) {
                #if SYNICSUGAR_LOG
                    Debug.Log($"MemberStatusNotyfy: {info.TargetUserId} left from lobby.");
                #endif
                p2pInfo.Instance.ConnectionNotifier.Leaved(UserId.GetUserId(info.TargetUserId), Reason.Left);
                p2pInfo.Instance.userIds.RemoveUserId(info.TargetUserId);
            }else if(info.CurrentStatus == LobbyMemberStatus.Disconnected){
                #if SYNICSUGAR_LOG
                    Debug.Log($"MemberStatusNotyfy: {info.TargetUserId} diconnect from lobby.");
                #endif
                p2pInfo.Instance.ConnectionNotifier.Disconnected(UserId.GetUserId(info.TargetUserId), Reason.Disconnected);
                p2pInfo.Instance.userIds.MoveTargetUserIdToLefts(info.TargetUserId);
            }else if(info.CurrentStatus == LobbyMemberStatus.Joined){
                p2pInfo.Instance.userIds.MoveTargetUserIdToRemoteUsersFromLeft(info.TargetUserId);
                p2pInfo.Instance.ConnectionNotifier.Connected(UserId.GetUserId(info.TargetUserId));
                // Send Id list.
                if(p2pInfo.Instance.IsHost()){
                    ConnectPreparation.SendUserList(UserId.GetUserId(info.TargetUserId));
                }
            }
        }
        /// <summary>
        /// For Guest to retrive the SoketName just after joining lobby.
        /// On getting the ScoketName, discard this event.
        /// </summary>
        /// <returns></returns>
        void AddNotifyLobbyUpdateReceived(){
            if(LobbyUpdateNotififyId == 0){
                var options = new AddNotifyLobbyUpdateReceivedOptions();
                LobbyUpdateNotififyId = EOSManager.Instance.GetEOSLobbyInterface().AddNotifyLobbyUpdateReceived(ref options, null, OnLobbyUpdateReceived);
            
                if (LobbyUpdateNotififyId == 0){
                    Debug.LogError("LobbyUpdateReceived: could not subscribe, bad notification id returned.");
                }
            }
        }
        void RemoveNotifyLobbyUpdateReceived(){
            EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyUpdateReceived(LobbyUpdateNotififyId);
            LobbyUpdateNotififyId = 0;
        }

        void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo info){
            if(info.LobbyId != CurrentLobby.LobbyId){
                Debug.LogError("Lobby Updated: this is other lobby data.");
                return;
            }

            OnLobbyUpdated(info.LobbyId);

            if(CurrentLobby.PermissionLevel == LobbyPermissionLevel.Joinviapresence){
                //No need to get lobby update info (to get socket name)
                RemoveNotifyLobbyUpdateReceived();
                
                isMatchSuccess = true;
                waitingMatch = false;
            }
        }
        /// <summary>
        /// To get info to be updated on Member attributes.
        /// </summary>
        /// <returns></returns>
        void AddNotifyLobbyMemberUpdateReceived(){
            if(LobbyMemberUpdateNotifyId == 0){
                var options = new AddNotifyLobbyMemberUpdateReceivedOptions();
                LobbyMemberUpdateNotifyId = EOSManager.Instance.GetEOSLobbyInterface().AddNotifyLobbyMemberUpdateReceived(ref options, null, OnLobbyMemberUpdate);
            
                if (LobbyMemberUpdateNotifyId == 0){
                    Debug.LogError("LobbyMemberUpdateReceived: could not subscribe, bad notification id returned.");
                }
            }
        }
        void RemoveNotifyLobbyMemberUpdateReceived(){
            EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberUpdateReceived(LobbyMemberUpdateNotifyId);
            LobbyMemberUpdateNotifyId = 0;
        }
        void OnLobbyMemberUpdate(ref LobbyMemberUpdateReceivedCallbackInfo info){
            if(info.LobbyId != CurrentLobby.LobbyId){
                Debug.LogError("OnLobbyMemberUpdate: this is other lobby data.");
                return;
            }

            OnLobbyUpdated(info.LobbyId);
                 
            MatchMakeManager.Instance.MemberUpdatedNotifier.MemberAttributesUpdated(UserId.GetUserId(info.TargetUserId));
        }
#endregion
#region Modify
        /// <summary>
        /// Add scoketName and remove search attributes.
        /// This process is the preparation for p2p connect and re-Connect. <br />
        /// Use lobbyID to connect on the problem, so save lobbyID in local somewhere.
        /// </summary>
        /// <returns></returns>
        internal void SwitchLobbyAttribute(){
            if (!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("SwitchLobbyAttribute: This user isn't lobby owner.");
#endif
                return;
            }
            if(CurrentLobby.Members.Count < requiredMembers){
#if SYNICSUGAR_LOG
                Debug.LogError("SwitchLobbyAttribute: This lobby doesn't meet member condition.");
#endif
                return;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            //Create modify handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);

            if (result != ResultE.Success){
                matchmakingResultCode = (Result)result;
                Debug.LogErrorFormat("SwitchLobbyAttribute: Could not create lobby modification. Error code: {0}", result);
                return;
            }
            //Change permission level
            LobbyModificationSetPermissionLevelOptions permissionOptions = new LobbyModificationSetPermissionLevelOptions(){
                PermissionLevel = LobbyPermissionLevel.Joinviapresence
            };
            result = lobbyHandle.SetPermissionLevel(ref permissionOptions);
            if (result != ResultE.Success){
                matchmakingResultCode = (Result)result;
                Debug.LogErrorFormat("SwitchLobbyAttribute: can't switch permission level. Error code: {0}", result);
                return;
            }

            // Set SocketName
            string socket = !string.IsNullOrEmpty(socketName) ? socketName : EOSp2pExtenstions.GenerateRandomSocketName();
            Epic.OnlineServices.Lobby.AttributeData socketAttribute = new(){
                Key = "socket",
                Value = new AttributeDataValue(){ AsUtf8 = socket }
            };
            
            LobbyModificationAddAttributeOptions addAttributeOptions = new LobbyModificationAddAttributeOptions(){
                Attribute = socketAttribute,
                Visibility = LobbyAttributeVisibility.Public
            };

            result = lobbyHandle.AddAttribute(ref addAttributeOptions);
            if (result != ResultE.Success){
                matchmakingResultCode = (Result)result;
                Debug.LogErrorFormat("SwitchLobbyAttribute: could not add socket name. Error code: {0}", result);
                return;
            }
            // Set attribute to handle in local
            foreach(var attribute in CurrentLobby.Attributes){
                LobbyModificationRemoveAttributeOptions removeAttributeOptions = new LobbyModificationRemoveAttributeOptions(){
                    Key = attribute.Key
                };

                result = lobbyHandle.RemoveAttribute(ref removeAttributeOptions);
                if (result != ResultE.Success){
                    matchmakingResultCode = (Result)result;
                    Debug.LogErrorFormat("SwitchLobbyAttribute: could not remove attribute. Error code: {0}", result);
                    return;
                }
            }
            //For Host's attribute.
            foreach(var attr in userAttributes){
                var attrOptions = new LobbyModificationAddMemberAttributeOptions(){
                    Attribute = attr.AsLobbyAttribute(),
                    Visibility = LobbyAttributeVisibility.Public
                };
                result = lobbyHandle.AddMemberAttribute(ref attrOptions);

                if (result != ResultE.Success){
                    matchmakingResultCode = (Result)result;
                    Debug.LogErrorFormat("AddSerachLobbyAttribute: could not add Host's member attribute. Error code: {0}", result);
                    return;
                }
            }
            //Change lobby attributes with handle
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, OnSwitchLobbyAttribute);
            lobbyHandle.Release();
        }
        void OnSwitchLobbyAttribute(ref UpdateLobbyCallbackInfo info){
            matchmakingResultCode = (Result)info.ResultCode;
            if (info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                waitingMatch = false;
                return;
            }

            OnLobbyUpdated(info.LobbyId);
            isMatchSuccess = true;
            waitingMatch = false;
        }
        /// <summary>
        /// For join. Host add self attributes on adding serach attribute.
        /// </summary>
        void AddUserAttributes(){
            if(userAttributes == null || userAttributes.Count == 0){
                return;
            }
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            ResultE result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);
            if(result != ResultE.Success){
                Debug.Log("AddUserAttributes: can't get modify handle.");
                return;
            }

            foreach(var attr in userAttributes){
                var attrOptions = new LobbyModificationAddMemberAttributeOptions(){
                    Attribute = attr.AsLobbyAttribute(),
                    Visibility = LobbyAttributeVisibility.Public
                };
                result = lobbyHandle.AddMemberAttribute(ref attrOptions);

                if (result != ResultE.Success){
                    matchmakingResultCode = (Result)result;
                    Debug.LogErrorFormat("AddMemberAttribute: could not add member attribute. Error code: {0}", result);
                    return;
                }
            }
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, OnAddedUserAttributes);
            lobbyHandle.Release();
        }
        void OnAddedUserAttributes(ref UpdateLobbyCallbackInfo info){
            matchmakingResultCode = (Result)info.ResultCode;
            if (info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                return;
            }
        #if SYNICSUGAR_LOG
            Debug.Log("OnAddedUserAttributes: added User attributes.");
        #endif
        }
        void OnLobbyUpdated(string lobbyId){
            if (!string.IsNullOrEmpty(lobbyId) && CurrentLobby.LobbyId == lobbyId){
                CurrentLobby.InitFromLobbyHandle(lobbyId);
            #if SYNICSUGAR_LOG
                Debug.Log($"OnLobbyUpdated: Update Lobby with {lobbyId}");
            #endif
            }
        }
        /// <summary>
        /// To check disconencted user's conenction state after p2p.
        /// </summary>
        /// <param name="id"></param>
        internal void UpdateMemberAttributeAsHeartBeat(int index){
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            ResultE result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);
            if(result != ResultE.Success){
                Debug.LogError("UpdateMemberAttributeAsHeartBeat: can't get modify handle.");
                return;
            }
            var attrOptions = new LobbyModificationAddMemberAttributeOptions(){
                Attribute = new Epic.OnlineServices.Lobby.AttributeData() { Key = "HeartBeat", Value = index },
                Visibility = LobbyAttributeVisibility.Public
            };
            result = lobbyHandle.AddMemberAttribute(ref attrOptions);

            if (result != ResultE.Success){
                matchmakingResultCode = (Result)result;
                Debug.LogErrorFormat("UpdateMemberAttributeAsHeartBeat: could not add member attribute. Error code: {0}", result);
                return;
            }
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, OnAddedUserAttributes);
            lobbyHandle.Release();
        }
#endregion
#region Cancel MatchMake
    /// <summary>
    /// Host close matchmaking. Guest Cancel matchmaking.
    /// </summary>
    /// <param name="matchingToken"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal async UniTask<Result> CloseMatchMaking(CancellationTokenSource matchingToken, CancellationToken token){
        if(!CurrentLobby.isValid()){
            Debug.LogError($"Cancel MatchMaking: this user has not participated a lobby.");
            return Result.InvalidAPICall;
        }
        //Remove notify
        if(!CurrentLobby.isHost()){
            RemoveNotifyLobbyUpdateReceived();
        }
        RemoveNotifyLobbyMemberStatusReceived();
        RemoveNotifyLobbyMemberUpdateReceived();

        //Destroy or Leave the current lobby.
        if(CurrentLobby.isHost()){
            Result canDestroy = await DestroyLobby(token);
            
            if(canDestroy == Result.Success){
                matchingToken?.Cancel();
            }else{
                Debug.LogError($"Cancel MatchMaking: has something problem when destroying the lobby");
            }

            return canDestroy;
        }

        Result canLeave = await LeaveLobby(true, token);
        
        if(canLeave == Result.Success){
            matchingToken?.Cancel();
        }else{
            Debug.LogError($"Cancel MatchMaking: has something problem when leave from the lobby");
        }
        return canLeave;
    }
    /// <summary>
    /// Cancel MatcgMaking and leave the lobby.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>If true, user can leave or destroy the lobby. </returns>
    internal async UniTask<Result> CancelMatchMaking(CancellationTokenSource matchingToken, CancellationToken token){
        if(!CurrentLobby.isValid()){
            matchmakingResultCode = Result.InvalidAPICall;
            Debug.LogError($"Cancel MatchMaking: this user has not participated a lobby.");
            return Result.InvalidAPICall;
        }
        //Remove notify
        if(!CurrentLobby.isHost()){
            RemoveNotifyLobbyUpdateReceived();
        }
        RemoveNotifyLobbyMemberStatusReceived();
        RemoveNotifyLobbyMemberUpdateReceived();

        //Destroy or Leave the current lobby.
        if(CurrentLobby.isHost()){
            if(CurrentLobby.Members.Count == 1){
                Result canDestroy = await DestroyLobby(token);
                
                if(canDestroy == Result.Success){
                    matchingToken?.Cancel();
                }else{
                    Debug.LogError($"Cancel MatchMaking: has something problem when destroying the lobby");
                }

                return canDestroy;
            }
        }

        Result canLeave = await LeaveLobby(true, token);
        
        if(canLeave == Result.Success){
            matchingToken?.Cancel();
        }else{
            Debug.LogError($"Cancel MatchMaking: has something problem when leave from the lobby");
        }
        return canLeave;
    }
#endregion
#region Leave
        //SynicSugar does not expect user to be in more than one Lobby at the same time.
        //So when joining in a new Lobby, the user needs to exit an old one.
        //It is not necessary to synchronize in most situations and can add Forget().
        //About Guests, when the lobby is Destroyed by Host, they Leave the lobby automatically.
        /// <summary>
        /// Leave the Participating Lobby.<br />
        /// When a game is over, call DestroyLobby() instead of this. .
        /// </summary>
        /// <param name="inMatchMaking"></param>
        /// <param name="token"></param>
        internal async UniTask<Result> LeaveLobby(bool inMatchMaking = false, CancellationToken token = default(CancellationToken)){
            if (CurrentLobby == null || string.IsNullOrEmpty(CurrentLobby.LobbyId) || !EOSManager.Instance.GetProductUserId().IsValid()){
                Debug.LogWarning("Leave Lobby: user is not in a lobby.");
                return Result.InvalidAPICall;
            }

            LeaveLobbyOptions options = new LeaveLobbyOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            waitLeave = true;
            canLeave = false;
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            lobbyInterface.LeaveLobby(ref options, null, OnLeaveLobbyCompleted);

            await UniTask.WaitUntil(() => !waitLeave, cancellationToken: token);
            if(!inMatchMaking){
                await MatchMakeManager.Instance.OnDeleteLobbyID();
            }

            return cancelResultCode;
        }
        void OnLeaveLobbyCompleted(ref LeaveLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                cancelResultCode = (Result)info.ResultCode;
                Debug.LogFormat("Leave Lobby: error code: {0}", info.ResultCode);
                canLeave = false;
                waitLeave = false;
                return;
            }
            if(waitingMatch){
                //To delete all member objects.
                foreach(var member in CurrentLobby.Members){
                    MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(member.Key), false);
                }
            }
            cancelResultCode = Result.Success;
            CurrentLobby.Clear();
            canLeave = true;
            waitLeave = false;
        }
        void OnKickedFromLobby(string lobbyId){
            if (CurrentLobby.isValid() && CurrentLobby.LobbyId.Equals(lobbyId, StringComparison.OrdinalIgnoreCase)){
                RemoveNotifyLobbyMemberStatusReceived();
                RemoveNotifyLobbyMemberUpdateReceived();
            }
            CurrentLobby.Clear();
        }
#endregion
#region Destroy
        /// <summary>
        /// When a game is over, call this. Guest leaves Lobby by update notify.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>On destroy success, return true.</returns>
        internal async UniTask<Result> DestroyLobby(CancellationToken token){
            if(!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Destroy Lobby: This user is not Host.");
#endif
                return Result.InvalidAPICall;
            }
            DestroyLobbyOptions options = new DestroyLobbyOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                LobbyId = CurrentLobby.LobbyId
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();

            waitLeave = true;
            canLeave = false;
            lobbyInterface.DestroyLobby(ref options, null, OnDestroyLobbyCompleted);
            
            await UniTask.WaitUntil(() => !waitLeave, cancellationToken: token);

            await MatchMakeManager.Instance.OnDeleteLobbyID();
            RemoveNotifyLobbyMemberStatusReceived();
            RemoveNotifyLobbyMemberUpdateReceived();
            return cancelResultCode;
        }
        void OnDestroyLobbyCompleted(ref DestroyLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                cancelResultCode = (Result)info.ResultCode;
                waitLeave = false;
                Debug.LogErrorFormat("Destroy Lobby: error code: {0}", info.ResultCode);
                return;
            }
            
            if(waitingMatch){
                //To delete member object.
                MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(EOSManager.Instance.GetProductUserId()), false);
            }
            CurrentLobby.Clear();
            cancelResultCode = Result.Success;
            canLeave = true;
            waitLeave = false;
        }
#endregion
#region Offline
        /// <summary>
        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="delay"></param>
        /// <param name="userAttributes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async UniTask CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, List<AttributeData> userAttributes, CancellationToken token){
            matchmakingResultCode = Result.None;
            this.userAttributes = userAttributes;

            if(delay.StartMatchmakingDelay > 0){
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Start);
                await UniTask.Delay((int)delay.StartMatchmakingDelay, cancellationToken: token);
            }
            //Create Lobby
            CurrentLobby = lobbyCondition;
            CurrentLobby.LobbyId = "OFFLINEMODE";
            CurrentLobby.LobbyOwner = EOSManager.Instance.GetProductUserId();
            CurrentLobby.Members.Add(UserId.GetUserId(EOSManager.Instance.GetProductUserId()).ToString(), new MemberState() { Attributes = userAttributes });
            CurrentLobby.hasConnectedRTCRoom = false;

            MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(UserId.GetUserId(EOSManager.Instance.GetProductUserId()), true);
            MatchMakeManager.Instance.MemberUpdatedNotifier.MemberAttributesUpdated(UserId.GetUserId(EOSManager.Instance.GetProductUserId()));

            if(delay.WaitForOpponentsDelay > 0){
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Wait);
                await UniTask.Delay((int)delay.WaitForOpponentsDelay, cancellationToken: token);
            }
            if(delay.FinishMatchmakingDelay > 0){
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Conclude);
                await UniTask.Delay((int)delay.FinishMatchmakingDelay, cancellationToken: token);
            }
            //Set User info
            p2pConnectorForOtherAssembly.Instance.ScoketName = "OFFLINEMODE";
            p2pInfo.Instance.userIds.HostUserId = UserId.GetUserId(CurrentLobby.LobbyOwner);
            p2pInfo.Instance.userIds.AllUserIds.Add(p2pInfo.Instance.LocalUserId);
            p2pInfo.Instance.userIds.CurrentAllUserIds.Add(p2pInfo.Instance.LocalUserId);
            p2pInfo.Instance.userIds.CurrentConnectedUserIds.Add(p2pInfo.Instance.LocalUserId);
            
            await MatchMakeManager.Instance.OnSaveLobbyID();
            if(delay.ReadyForConnectionDelay > 0){
                MatchMakeManager.Instance.MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
                await UniTask.Delay((int)delay.ReadyForConnectionDelay, cancellationToken: token);
            }
            matchmakingResultCode = Result.Success;
        }
        internal async UniTask DestroyOfflineLobby(){
            matchmakingResultCode = Result.None;
            await MatchMakeManager.Instance.OnDeleteLobbyID();
            CurrentLobby.Clear();
            matchmakingResultCode = Result.Success;
        }
#endregion
        /// <summary>
        /// Init p2pManager's room info with new lobby data.
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Result InitConnectConfig(ref UserIds userIds){
            //Prep RTC(Voice Chat)
            RTCManager.Instance.AddNotifyParticipantUpdated();
            //Crate copy handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            ResultE result = lobbyInterface.CopyLobbyDetailsHandle(ref options, out LobbyDetails lobbyHandle);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("InitConnectConfig: can't get lobby detail handle. {0}", result);
                return (Result)result;
            }
            //Copy details from local handle
            LobbyDetailsCopyInfoOptions dataOptions = new LobbyDetailsCopyInfoOptions();
            result = lobbyHandle.CopyInfo(ref dataOptions, out LobbyDetailsInfo? dataInfo);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("InitConnectConfig: can't copy lobby details on local. {0}", result);
                lobbyHandle.Release();
                return (Result)result;
            }
            //Get member count
            LobbyDetailsGetMemberCountOptions countOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = lobbyHandle.GetMemberCount(ref countOptions);
            //Get other use's id
            LobbyDetailsGetMemberByIndexOptions memberOptions = new LobbyDetailsGetMemberByIndexOptions();
            for(uint i = 0; i < memberCount; i++){
                memberOptions.MemberIndex = i;
                UserId targetId = UserId.GetUserId(lobbyHandle.GetMemberByIndex(ref memberOptions));

                userIds.AllUserIds.Add(targetId);

                if(userIds.LocalUserId != targetId){
                    userIds.RemoteUserIds.Add(targetId);
                }
            }
            userIds.CurrentAllUserIds = new List<UserId>(userIds.AllUserIds);
            userIds.CurrentConnectedUserIds = new List<UserId>(userIds.AllUserIds);
            //Get lobby's attribute count
            LobbyDetailsCopyAttributeByKeyOptions attrOptions = new LobbyDetailsCopyAttributeByKeyOptions();
            attrOptions.AttrKey = "socket";
            result = lobbyHandle.CopyAttributeByKey(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? socket);
            if(result != ResultE.Success || socket?.Data == null){
                Debug.LogErrorFormat("InitConnectConfig: can't copy lobby attribute on local. {0}", result);
                lobbyHandle.Release();
                return (Result)result;
            }
            p2pConnectorForOtherAssembly.Instance.ScoketName = EOSLobbyExtensions.GenerateLobbyAttribute(socket).STRING;
            //For options
            userIds.HostUserId = UserId.GetUserId(CurrentLobby.LobbyOwner);
            lobbyHandle.Release();
            return (Result)result;
        }
        /// <summary>
        /// Open conenction and init some objects
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> OpenConnection(CancellationToken token){
            if(p2pConfig.Instance.relayControl != RelayControl.AllowRelays){
                p2pConfig.Instance.SetRelayControl(p2pConfig.Instance.relayControl);
            }
            await p2pInfo.Instance.natRelay.Init();
            RemoveNotifyLobbyMemberUpdateReceived();
            p2pConnectorForOtherAssembly.Instance.OpenConnection(true);
        #if SYNICSUGAR_LOG
            Debug.Log("OpenConnection: Open Connection.");
        #endif
            Result canConnect = await ConnectPreparation.WaitConnectPreparation(token, initconnectTimeoutMS);
            if(canConnect != Result.Success){
                return Result.ConnectEstablishFailed;
            }
            //Host sends AllUserIds list, Guest Receives AllUserIds.
            if(p2pInfo.Instance.IsHost()){
        #if SYNICSUGAR_LOG
            Debug.Log("OpenConnection: Sends UserList as Host.");
        #endif
                await ConnectPreparation.SendUserListToAll(token);
            }else{
        #if SYNICSUGAR_LOG
            Debug.Log("OpenConnection: Wait for UserList as Guest.");
        #endif
                ConnectPreparation basicInfo = new();
                await basicInfo.ReciveUserIdsPacket(token);
            }
            p2pInfo.Instance.pings.Init();
            return Result.Success;
        }
        /// <summary>
        /// Open connection in strict and wait for AllUserLists(that has the same order in all local) from Host.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<Result> OpenConnectionForReconnecter(CancellationToken token){
            if(p2pConfig.Instance.relayControl != RelayControl.AllowRelays){
                p2pConfig.Instance.SetRelayControl(p2pConfig.Instance.relayControl);
            }
            await p2pInfo.Instance.natRelay.Init();
            p2pConnectorForOtherAssembly.Instance.OpenConnection(true);
            Result canConnect = await ConnectPreparation.WaitConnectPreparation(token, initconnectTimeoutMS);
            if(canConnect != Result.Success){
                return Result.ConnectEstablishFailed;
            }
            //Wait for user ids list from host.
            ConnectPreparation basicInfo = new();
            await basicInfo.ReciveUserIdsPacket(token);

            p2pInfo.Instance.pings.Init();
            return Result.Success;
        }
        /// <summary>
        /// For library user to save ID.
        /// </summary>
        /// <returns></returns>
        internal string GetCurrentLobbyID(){
            return CurrentLobby.LobbyId;
        }
        
        internal int GetCurrentLobbyMemberCount(){
            //When Host create lobby, they can't count self. This is called before adding member attributes.
           return CurrentLobby._BeingCreated ? 1 : CurrentLobby.Members.Count;
        }
        internal int GetLobbyMemberLimit(){
           return (int)CurrentLobby.MaxLobbyMembers;
        }
    }
}