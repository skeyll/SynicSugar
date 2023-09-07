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

        bool waitingMatch;
        bool waitLeave, canLeave;
        LobbySearch CurrentSearch;
        Dictionary<Lobby, LobbyDetails> SearchResults = new Dictionary<Lobby, LobbyDetails>();
        //User config
        uint MAX_SEARCH_RESULT;
        int timeoutMS;
        List<AttributeData> userAttributes;
        //Notification
        /// <summary>
        /// Join, Leave, Kicked, Promote or so on...
        /// </summary>
        NotifyEventHandle LobbyMemberStatusNotification;
        /// <summary>
        /// Lobby attributes
        /// </summary>
        NotifyEventHandle LobbyUpdateNotification;
        /// <summary>
        /// Member attributes.
        /// </summary>
        NotifyEventHandle LobbyMemberUpdateNotification;
        

        bool isMatchSuccess;
        string socketName = System.String.Empty;

        internal EOSLobby(uint maxSearch, int timeout){
            MAX_SEARCH_RESULT = maxSearch;
            //For Unitask
            timeoutMS = timeout * 1000;
        }

        /// <summary>
        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="saveFn">To save LobbyID. If null, save ID to local by PlayerPrefs</param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<bool> StartMatching(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes){
            MatchMakeManager.Instance.LastResultCode = Result.None;
            this.userAttributes = userAttributes;
            //Start timer 
            var timer = TimeoutTimer(token);
            //Serach
            MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
            MatchMakeManager.Instance.UpdateStateDescription(MatchState.Search);
            bool canJoin = await JoinExistingLobby(lobbyCondition, token);

            if(canJoin){
                // Wait for SocketName to use p2p connection
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.matchState.acceptCancel?.Invoke();

                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);

                if(token.IsCancellationRequested){
                    MatchMakeManager.Instance.LastResultCode = Result.None;
                    throw new OperationCanceledException();
                }

                if(isMatchSuccess){
                    MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
                    bool canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(!canInit){
                        Debug.LogError("Fail InitConnectConfig");
                        return false;
                    }

                    await OpenConnection(token);

                    await MatchMakeManager.Instance.OnSaveLobbyID();
                }
                return isMatchSuccess;
            }
            //If player cannot join lobby as a guest, creates a lobby as a host and waits for other player.
            //Create
            MatchMakeManager.Instance.UpdateStateDescription(MatchState.Wait);
            bool canCreate = await CreateLobby(lobbyCondition, token);
            if(canCreate){
                // Wait for other player to join
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.matchState.acceptCancel?.Invoke();

                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);
                
                //Matching cancel
                if(token.IsCancellationRequested){
                    throw new OperationCanceledException();
                }
                
                if(isMatchSuccess){
                    MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
                    bool canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(!canInit){
                        Debug.LogError("Fail InitConnectConfig");
                        return false;
                    }

                    await OpenConnection(token);

                    await MatchMakeManager.Instance.OnSaveLobbyID();

                    return true;
                }
            }
            //Failed due to no-playing-user or server problems.
            return false;
        }
        /// <summary>
        /// Just search Lobby<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<bool> StartJustSearch(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes){
            MatchMakeManager.Instance.LastResultCode = Result.None;
            this.userAttributes = userAttributes;
            var timer = TimeoutTimer(token);
            //Serach
            MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
            MatchMakeManager.Instance.UpdateStateDescription(MatchState.Search);
            bool canJoin = await JoinExistingLobby(lobbyCondition, token);
            if(canJoin){
                // Wait for SocketName to use p2p connection
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.matchState.acceptCancel?.Invoke();
                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);

                //Matching cancel
                if(token.IsCancellationRequested){
                    throw new OperationCanceledException();
                }

                if(isMatchSuccess){
                    MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
                    bool canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(!canInit){
                        Debug.LogError("Fail InitConnectConfig");
                        return false;
                    }
    
                    await OpenConnection(token);
                    
                    await MatchMakeManager.Instance.OnSaveLobbyID();
                }
                return isMatchSuccess;
            }
            //Failed due to no-playing-user or server problems.
            return false;
        }
        /// <summary>
        /// Create lobby as host<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<bool> StartJustCreate(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes){
            MatchMakeManager.Instance.LastResultCode = Result.None;
            this.userAttributes = userAttributes;
            var timer = TimeoutTimer(token);

            MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
            MatchMakeManager.Instance.UpdateStateDescription(MatchState.Wait);
            bool canCreate = await CreateLobby(lobbyCondition, token);
            if(canCreate){
                // Wait for other player to join
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.matchState.acceptCancel?.Invoke();
                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token), timer);

                //Matching cancel
                if(token.IsCancellationRequested){
                    throw new OperationCanceledException();
                }

                if(isMatchSuccess){
                    MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
                    bool canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
                    if(!canInit){
                        Debug.LogError("Fail InitConnectConfig");
                        return false;
                    }

                    await OpenConnection(token);

                    await MatchMakeManager.Instance.OnSaveLobbyID();
                    return true;
                }
            }
            //Failed due to no-playing-user or server problems.
            return false;
        }
        /// <summary>
        /// Join the Lobby with specific id to that lobby. <br />
        /// To return to a disconnected lobby.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        internal async UniTask<bool> JoinLobbyBySavedLobbyId(string LobbyID, CancellationToken token){
            MatchMakeManager.Instance.LastResultCode = Result.None;
            //Search
            bool canSerch = await RetriveLobbyByLobbyId(LobbyID, token);

        #if SYNICSUGAR_LOG
            Debug.LogFormat("JoinLobbyBySavedLobbyId: RetriveLobbyByLobbyId is '{0}'.", canSerch ? "Success" : "Failure");
        #endif
            if(!canSerch){
                await MatchMakeManager.Instance.OnDeleteLobbyID();
                return false; //The lobby was already closed.
            }
            //Join
            bool canJoin = await TryJoinSearchResults(token);
        #if SYNICSUGAR_LOG
            Debug.LogFormat("JoinLobbyBySavedLobbyId: TryJoinSearchResults is '{0}'.", canJoin ? "Success" : "Failure");
        #endif
            if(!canJoin){
                await MatchMakeManager.Instance.OnDeleteLobbyID();
                return false; //The lobby was already closed.
            }
            //For the host migration
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnLobbyMemberStatusReceived), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });
            //Prep Connection
            bool canInit = InitConnectConfig(ref p2pInfo.Instance.userIds);
            if(!canInit){
                Debug.LogError("Fail InitConnectConfig");
                return false;
            }

            p2pInfo.Instance.userIds.isJustReconnected = true;

            await OpenConnection(token);
            
            return true;
        }
        /// <summary>
        /// Wait for timeout. Leave Lobby, then the end of this task stop main task.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask TimeoutTimer(CancellationToken token){
            await UniTask.Delay(timeoutMS, cancellationToken: token);
            if(waitingMatch && !waitLeave){
                bool canLeave = await LeaveLobby(true, token);
        #if SYNICSUGAR_LOG
                Debug.Log("Cancel matching by timeout");
        #endif
                if(canLeave){
                    MatchMakeManager.Instance.matchingToken?.Cancel();
                }
                
                MatchMakeManager.Instance.LastResultCode = Result.TimedOut;
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
        async UniTask<bool> CreateLobby(Lobby lobbyCondition, CancellationToken token){
            // Check if there is current session. Leave it.
            if (CurrentLobby.isValid()){
#if SYNICSUGAR_LOG
                Debug.LogWarningFormat("Create Lobby: Leaving Current Lobby '{0}'", CurrentLobby.LobbyId);
#endif
                LeaveLobby(true, token).Forget();
            }

            //Lobby Option
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxLobbyMembers = lobbyCondition.MaxLobbyMembers,
                PermissionLevel = lobbyCondition.PermissionLevel,
                BucketId = lobbyCondition.BucketId,
                PresenceEnabled = false,
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
                return false;
            }
            // For the host migration
            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnLobbyMemberStatusReceived), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });
            // To get Member attributes
            LobbyMemberUpdateNotification = new NotifyEventHandle(AddNotifyLobbyMemberUpdateReceived(lobbyInterface, OnLobbyMemberUpdate), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberUpdateReceived(handle);
            });
            //Need add condition for serach
            bool canModify = await AddSerachLobbyAttribute(lobbyCondition, token);
            if(!canModify){
                Debug.LogError("Create Lobby: can't add lobby search attributes");
                return false;
            }
            
            return true;
        }
        void OnCreateLobbyCompleted(ref CreateLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
                Debug.LogErrorFormat("Created Lobby: error code: {0}", info.ResultCode);
                waitingMatch = false;
                return;
            }
            
            if (string.IsNullOrEmpty(info.LobbyId) || !CurrentLobby._BeingCreated){
                waitingMatch = false;
                return;
            }

            CurrentLobby.LobbyId = info.LobbyId;
            //RTC
            RTCManager.Instance.AddNotifyParticipantStatusChanged();

            isMatchSuccess = true;
            waitingMatch = false;
        }
        /// <summary>
        /// Set attribute for search. This process is only for Host player.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<bool> AddSerachLobbyAttribute(Lobby lobbyCondition, CancellationToken token){
            if (!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Change Lobby: This isn't lobby owner.");
#endif
                return false;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            //Create modify handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);

            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("AddSerachLobbyAttribute: Could not create lobby modification. Error code: {0}", result);
                return false;
            }
            //Set Backet ID
            LobbyModificationSetBucketIdOptions  bucketIdOptions = new(){
                BucketId = lobbyCondition.BucketId
            };
            result = lobbyHandle.SetBucketId(ref bucketIdOptions);

            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("AddSerachLobbyAttribute: Could not set bucket id. Error code: {0}", result);
                return false;
            }

            // Set attribute to handle in local
            foreach(var attribute in lobbyCondition.Attributes){
                LobbyModificationAddAttributeOptions attributeOptions = new LobbyModificationAddAttributeOptions(){
                    Attribute = attribute.AsLobbyAttribute(),
                    Visibility = attribute.Visibility
                };

                result = lobbyHandle.AddAttribute(ref attributeOptions);
                if (result != ResultE.Success){
                    MatchMakeManager.Instance.LastResultCode = (Result)result;
                    Debug.LogErrorFormat("Change Lobby: could not add attribute. Error code: {0}", result);
                    return false;
                }
            }
            // Add for User Attribute
            AddUserAttributes(lobbyHandle);

            //Add attribute with handle
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };
            // Init for async
            waitingMatch = true;
            isMatchSuccess = false;
            
            lobbyInterface.UpdateLobby(ref updateOptions, null, OnAddSerchAttribute);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);

            return isMatchSuccess; //"isMatchSuccess" is changed in async and callback method with result.
        }
        void OnAddSerchAttribute(ref UpdateLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                waitingMatch = false;
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                return;
            }

            OnLobbyUpdated(info.LobbyId);
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
        async UniTask<bool> JoinExistingLobby(Lobby lobbyCondition, CancellationToken token){
            //Serach
            bool canRetrive = await RetriveLobbyByAttribute(lobbyCondition, token);
            if(!canRetrive){
                return false; //Need to create own session
            }
            //Join
            bool canJoin = await TryJoinSearchResults(token);
            if(!canJoin){
                return false;  //Need to create own session
            }
            //Register Notification
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            // To get SocketName safety
            LobbyUpdateNotification = new NotifyEventHandle(AddNotifyLobbyUpdateReceived(lobbyInterface, OnLobbyUpdateReceived), (ulong handle) => {
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyUpdateReceived(handle);
            });
            // For the host migration
            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnLobbyMemberStatusReceived), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });
            // To get Member attributes
            LobbyMemberUpdateNotification = new NotifyEventHandle(AddNotifyLobbyMemberUpdateReceived(lobbyInterface, OnLobbyMemberUpdate), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberUpdateReceived(handle);
            });

            return true;
        }
        /// <summary>
        /// For use in normal matching. Retrive Lobby by Attributes. 
        /// Retrun true on getting Lobby data.
        /// </summary>
        /// <param name="lobbyCodition"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<bool> RetriveLobbyByAttribute(Lobby lobbyCondition, CancellationToken token){
            //Create Search handle on local
            // Create new handle 
            CreateLobbySearchOptions searchOptions = new CreateLobbySearchOptions(){
                MaxResults = MAX_SEARCH_RESULT
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch lobbySearchHandle);

            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("Search Lobby: could not create SearchByAttribute. Error code: {0}", result);
                return false;
            }

            CurrentSearch = lobbySearchHandle;

            //Set Backet ID
            Epic.OnlineServices.Lobby.AttributeData bucketAttribute = new (){
                Key = "bucket",
                Value = new AttributeDataValue(){ AsUtf8 = lobbyCondition.BucketId }
            };
            LobbySearchSetParameterOptions paramOptions = new LobbySearchSetParameterOptions(){
                Parameter = bucketAttribute,
                ComparisonOp = ComparisonOp.Equal
            };

            result = lobbySearchHandle.SetParameter(ref paramOptions);
            
            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("Retrieve Lobby: failed to add bucketID. Error code: {0}", result);
                return false;
            }
            
            // Set other attributes
            foreach (var attribute in lobbyCondition.Attributes){
                Epic.OnlineServices.Lobby.AttributeData data = attribute.AsLobbyAttribute();
                paramOptions.Parameter = data;
                paramOptions.ComparisonOp = attribute.ComparisonOperator; 

                result = lobbySearchHandle.SetParameter(ref paramOptions);

                if (result != ResultE.Success){
                    MatchMakeManager.Instance.LastResultCode = (Result)result;
                    Debug.LogErrorFormat("Retrieve Lobby: failed to add option attribute. Error code: {0}", result);
                    return false;
                }
            }

            //Search with handle
            LobbySearchFindOptions findOptions = new LobbySearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            waitingMatch = true;
            lobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
            //Can retrive data
            return true;
        }
        /// <summary>
        /// Get Lobby from EOS by LobbyID<br />
        /// Can get closed-lobby.
        /// </summary>
        /// <param name="lobbyId">Id to get</param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask<bool> RetriveLobbyByLobbyId(string lobbyId, CancellationToken token){
            //Create Search handle on local
            // Create new handle 
            CreateLobbySearchOptions searchOptions = new CreateLobbySearchOptions();
            searchOptions.MaxResults = 1;

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            ResultE result = lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch lobbySearchHandle);

            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("Lobby Search: could not create SearchByLobbyId. Error code: {0}", result);
                return false;
            }

            CurrentSearch = lobbySearchHandle;

            // Set Lobby ID
            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions(){
                LobbyId = lobbyId
            };
            result = lobbySearchHandle.SetLobbyId(ref setLobbyOptions);

            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("Search Lobby: failed to update SearchByLobbyId with lobby id. Error code: {0}", result);
                return false;
            }

            //Search with handle
            LobbySearchFindOptions findOptions = new LobbySearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            waitingMatch = true;
            lobbySearchHandle.Find(ref findOptions, null, OnLobbySearchCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
            //Can retrive data
            return true;
        }

        void OnLobbySearchCompleted(ref LobbySearchFindCallbackInfo info){
            waitingMatch = false;
            if (info.ResultCode != ResultE.Success) {
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
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
        /// <returns></returns>
        async UniTask<bool> TryJoinSearchResults(CancellationToken token){ 
            if (CurrentSearch == null){
                Debug.LogError("Exmaine Lobby: CurrentSearch is null");
                return false;
            }

            var lobbySearchGetSearchResultCountOptions = new LobbySearchGetSearchResultCountOptions(); 
            uint numSearchResult = CurrentSearch.GetSearchResultCount(ref lobbySearchGetSearchResultCountOptions);
            
            if(numSearchResult == 0){
                return false;
            }
            SearchResults.Clear();

            LobbySearchCopySearchResultByIndexOptions indexOptions = new LobbySearchCopySearchResultByIndexOptions();
            isMatchSuccess = false; //Init
            for (uint i = 0; i < numSearchResult; i++){
                Lobby lobbyObj = new Lobby();
                indexOptions.LobbyIndex = i;

                ResultE result = CurrentSearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails lobbyHandle);

                if (result == ResultE.Success && lobbyHandle != null){
                    lobbyObj.InitFromLobbyDetails(lobbyHandle);

                    if (lobbyObj == null || !lobbyObj.isValid()){
                        continue;
                    }
                    waitingMatch = true;
                    JoinLobby(lobbyHandle);
                    
                    await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token);
                    
                    if(isMatchSuccess){
                        SearchResults.Add(lobbyObj, lobbyHandle);
                        break;
                    }
                    if(token.IsCancellationRequested){
                        throw new OperationCanceledException();
                    }
                }
            }
            return true;
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
            if (info.ResultCode != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
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
            if(userAttributes != null && userAttributes.Count > 0){
                foreach(var m in CurrentLobby.Members){
                    MatchMakeManager.Instance.LobbyMemberUpdateNotifier.OnMemberAttributesUpdated(UserId.GetUserId(m.Key));
                }
            }

            waitingMatch = false;
        }
#endregion
//Common
#region Notification
        /// <summary>
        /// For Host and Guest. To get the notification for a user to join and disconnect.
        /// </summary>
        /// <param name="lobbyInterface"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        ulong AddNotifyLobbyMemberStatusReceived(LobbyInterface lobbyInterface, OnLobbyMemberStatusReceivedCallback callback){
            var options = new AddNotifyLobbyMemberStatusReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberStatusReceived(ref options, null, callback);
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
                    MatchMakeManager.Instance.LastResultCode = info.CurrentStatus == LobbyMemberStatus.Kicked ? Result.UserKicked : Result.None;
                    //Fail to MatchMake.
                    waitingMatch = false;
                    return;
                }
            }
            OnLobbyUpdated(info.LobbyId);
            
            //For MatchMake on the first
            if(waitingMatch){ //This flag is shared by both host and guest, and is false after getting SocketName.
                if(info.TargetUserId == productUserId && info.CurrentStatus == LobbyMemberStatus.Promoted){
                    //??? Need timeout process to wait for other user as host?

                    //This local player manage lobby, So dosen't need update notify.
                    LobbyUpdateNotification.Dispose();
                }
                //Memo: Should we change the monitoring conditions to the number of Lobby members in SynicSugar 
                //      instead of the number of Lobby members on the server in order to allow joining in gaming?
                //Lobby is full. Stop additional member and change search attributes to SoketName.
                if (CurrentLobby.isHost() && CurrentLobby.MaxLobbyMembers == CurrentLobby.Members.Count){
                    SwitchLobbyAttribute();
                    return;
                }
                return;
            }

            //In game
            // Hosts changed?
            if (info.CurrentStatus == LobbyMemberStatus.Promoted){
                p2pInfo.Instance.userIds.HostUserId = UserId.GetUserId(CurrentLobby.LobbyOwner);

                #if SYNICSUGAR_LOG
                    Debug.Log($"MemberStatusNotyfy: {info.TargetUserId} is promoted to host.");
                #endif
                if(!CurrentLobby.isHost()){
                    //MEMO: Now, if user disconnect from Lobby and then change hosts, the user become newbie.
                    //Guest Don't need to hold user id 
                    // p2pInfo.Instance.userIds.LeftUsers = new List<UserId>();
                }
            }else if(info.CurrentStatus == LobbyMemberStatus.Left) {
                #if SYNICSUGAR_LOG
                    Debug.Log($"MemberStatusNotyfy: {info.TargetUserId} left from lobby.");
                #endif
                p2pInfo.Instance.userIds.RemoveUserId(info.TargetUserId);
            }else if(info.CurrentStatus == LobbyMemberStatus.Disconnected){
                #if SYNICSUGAR_LOG
                    Debug.Log($"MemberStatusNotyfy: {info.TargetUserId} diconnect from lobby.");
                #endif
                p2pInfo.Instance.userIds.MoveTargetUserIdToLefts(info.TargetUserId);
                p2pInfo.Instance.ConnectionNotifier.OnDisconnected(UserId.GetUserId(info.TargetUserId), Reason.Disconnected);
            }else if(info.CurrentStatus == LobbyMemberStatus.Joined){
                p2pInfo.Instance.userIds.MoveTargetUserIdToRemoteUsersFromLeft(info.TargetUserId);
                p2pInfo.Instance.ConnectionNotifier.OnConnected(UserId.GetUserId(info.TargetUserId));
            }
        }
        /// <summary>
        /// For Guest to retrive the SoketName just after joining lobby.
        /// On getting the ScoketName, discard this event.
        /// </summary>
        /// <param name="lobbyInterface"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        ulong AddNotifyLobbyUpdateReceived(LobbyInterface lobbyInterface, OnLobbyUpdateReceivedCallback callback){
            var options = new AddNotifyLobbyUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyUpdateReceived(ref options, null, callback);
        }
        void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo info){
            if(info.LobbyId != CurrentLobby.LobbyId){
                Debug.LogError("Lobby Updated: this is other lobby data.");
                return;
            }

            OnLobbyUpdated(info.LobbyId);

            if(CurrentLobby.PermissionLevel == LobbyPermissionLevel.Joinviapresence){
                //No need to get lobby update info (to get socket name)
                LobbyUpdateNotification.Dispose();
                
                isMatchSuccess = true;
                waitingMatch = false;
            }
        }
        /// <summary>
        /// To get info to be updated on Member attributes.
        /// </summary>
        /// <param name="lobbyInterface"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        ulong AddNotifyLobbyMemberUpdateReceived(LobbyInterface lobbyInterface, OnLobbyMemberUpdateReceivedCallback callback){
            var options = new AddNotifyLobbyMemberUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberUpdateReceived(ref options, null, callback);
        }
        void OnLobbyMemberUpdate(ref LobbyMemberUpdateReceivedCallbackInfo info){
            if(info.LobbyId != CurrentLobby.LobbyId){
                Debug.LogError("OnLobbyMemberUpdate: this is other lobby data.");
                return;
            }

            OnLobbyUpdated(info.LobbyId);
            
            MatchMakeManager.Instance.LobbyMemberUpdateNotifier.OnMemberAttributesUpdated(UserId.GetUserId(info.TargetUserId));
        }
#endregion
#region Modify
        /// <summary>
        /// Add scoketName and remove search attributes.
        /// This process is the preparation for p2p connect and re-Connect. <br />
        /// Use lobbyID to connect on the problem, so save lobbyID in local somewhere.
        /// </summary>
        /// <returns></returns>
        void SwitchLobbyAttribute(){
            if (!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Change Lobby: This user isn't lobby owner.");
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
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("SwitchLobbyAttribute: Could not create lobby modification. Error code: {0}", result);
                return;
            }
            //Change permission level
            LobbyModificationSetPermissionLevelOptions permissionOptions = new LobbyModificationSetPermissionLevelOptions();
            permissionOptions.PermissionLevel = LobbyPermissionLevel.Joinviapresence;
            result = lobbyHandle.SetPermissionLevel(ref permissionOptions);
            if (result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
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
                MatchMakeManager.Instance.LastResultCode = (Result)result;
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
                    MatchMakeManager.Instance.LastResultCode = (Result)result;
                    Debug.LogErrorFormat("SwitchLobbyAttribute: could not remove attribute. Error code: {0}", result);
                    return;
                }
            }
            //Change lobby attributes with handle
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, OnSwitchLobbyAttribute);
        }
        void OnSwitchLobbyAttribute(ref UpdateLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
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
                    MatchMakeManager.Instance.LastResultCode = (Result)result;
                    Debug.LogErrorFormat("AddMemberAttribute: could not add member attribute. Error code: {0}", result);
                    return;
                }
            }
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions(){
                LobbyModificationHandle = lobbyHandle
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, OnAddedUserAttributes);
        }
        void AddUserAttributes(LobbyModification lobbyHandle){
            if(userAttributes == null || userAttributes.Count == 0){
                return;
            }
            ResultE result = ResultE.Success;

            foreach(var attr in userAttributes){
                var attrOptions = new LobbyModificationAddMemberAttributeOptions(){
                    Attribute = attr.AsLobbyAttribute(),
                    Visibility = LobbyAttributeVisibility.Public
                };
                result = lobbyHandle.AddMemberAttribute(ref attrOptions);

                if (result != ResultE.Success){
                    MatchMakeManager.Instance.LastResultCode = (Result)result;
                    Debug.LogErrorFormat("AddMemberAttribute: could not add member attribute. Error code: {0}", result);
                    return;
                }
            }
        }
        void OnAddedUserAttributes(ref UpdateLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                return;
            }

            OnLobbyUpdated(info.LobbyId);

        #if SYNICSUGAR_LOG
            Debug.Log("OnAddedUserAttributes: Guest added User attributes.");
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
#endregion
#region Cancel
    /// <summary>
    /// Cancel MatcgMaking and leave the lobby.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>If true, user can leave or destroy the lobby. </returns>
    internal async UniTask<bool> CancelMatchMaking(CancellationTokenSource matchingToken, CancellationToken token){
        if(!CurrentLobby.isValid()){
            MatchMakeManager.Instance.LastResultCode = Result.InvalidAPICall;
            Debug.LogError($"Cancel MatchMaking: this user has not participated a lobby.");
            return false;
        }
        //Remove notify
        if(!CurrentLobby.isHost()){
            LobbyUpdateNotification.Dispose();
        }
        LobbyMemberStatusNotification.Dispose();
        LobbyMemberUpdateNotification.Dispose();

        //Destroy or Leave the current lobby.
        if(CurrentLobby.isHost()){
            if(CurrentLobby.Members.Count == 1){
                bool canDestroy = await DestroyLobby(token);
                
                if(canDestroy){
                    matchingToken?.Cancel();
                }else{
                    Debug.LogError($"Cancel MatchMaking: has something problem when destroying the lobby");
                }

                return canDestroy;
            }
        }

        bool canLeave = await LeaveLobby(true, token);
        
        if(canLeave){
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
        internal async UniTask<bool> LeaveLobby(bool inMatchMaking = false, CancellationToken token = default(CancellationToken)){
            if (CurrentLobby == null || string.IsNullOrEmpty(CurrentLobby.LobbyId) || !EOSManager.Instance.GetProductUserId().IsValid()){
                Debug.LogWarning("Leave Lobby: user is not in a lobby.");
                return false;
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

            return canLeave;
        }
        void OnLeaveLobbyCompleted(ref LeaveLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
                Debug.LogFormat("Leave Lobby: error code: {0}", info.ResultCode);
                canLeave = false;
                waitLeave = false;
                return;
            }

            CurrentLobby.Clear();
            canLeave = true;
            waitLeave = false;
        }
        void OnKickedFromLobby(string lobbyId){
            if (CurrentLobby.isValid() && CurrentLobby.LobbyId.Equals(lobbyId, StringComparison.OrdinalIgnoreCase)){
                CurrentLobby.Clear();
                LobbyMemberStatusNotification.Dispose();
                LobbyMemberUpdateNotification.Dispose();
            }
        }
#endregion
#region Destroy
        /// <summary>
        /// When a game is over, call this. Guest leaves Lobby by update notify.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="deleteFn">Delete LobbyID for re-connect</param>
        /// <returns>On destroy success, return true.</returns>
        internal async UniTask<bool> DestroyLobby(CancellationToken token){
            if(!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Destroy Lobby: This user is not Host.");
#endif
                return false;
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
            LobbyMemberStatusNotification.Dispose();
            LobbyMemberUpdateNotification.Dispose();
            CurrentLobby.Clear();

            return canLeave;
        }
        void OnDestroyLobbyCompleted(ref DestroyLobbyCallbackInfo info){
            if (info.ResultCode != ResultE.Success){
                waitLeave = false;
                MatchMakeManager.Instance.LastResultCode = (Result)info.ResultCode;
                Debug.LogErrorFormat("Destroy Lobby: error code: {0}", info.ResultCode);
                return;
            }
            
            canLeave = true;
            waitLeave = false;
        }
#endregion
        /// <summary>
        /// Init p2pManager's room info with new lobby data.
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        bool InitConnectConfig(ref UserIds userIds){
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
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("InitConnectConfig: can't get lobby detail handle. {0}", result);
                return false;
            }
            //Copy details from local handle
            LobbyDetailsCopyInfoOptions dataOptions = new LobbyDetailsCopyInfoOptions();
            result = lobbyHandle.CopyInfo(ref dataOptions, out LobbyDetailsInfo? dataInfo);

            if(result != ResultE.Success){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("InitConnectConfig: can't copy lobby details on local. {0}", result);
                lobbyHandle.Release();
                return false;
            }
            //Get member count
            LobbyDetailsGetMemberCountOptions countOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = lobbyHandle.GetMemberCount(ref countOptions);
            //Get other use's id
            LobbyDetailsGetMemberByIndexOptions memberOptions = new LobbyDetailsGetMemberByIndexOptions();
            userIds.RemoteUserIds = new List<UserId>();
            for(uint i = 0; i < memberCount; i++){
                memberOptions.MemberIndex = i;
                if(userIds.LocalUserId.AsEpic != lobbyHandle.GetMemberByIndex(ref memberOptions)){
                    userIds.RemoteUserIds.Add(UserId.GetUserId(lobbyHandle.GetMemberByIndex(ref memberOptions)));
                }
            }
            //Get lobby's attribute count
            LobbyDetailsCopyAttributeByKeyOptions attrOptions = new LobbyDetailsCopyAttributeByKeyOptions();
            attrOptions.AttrKey = "socket";
            result = lobbyHandle.CopyAttributeByKey(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? socket);
            if(result != ResultE.Success || socket?.Data == null){
                MatchMakeManager.Instance.LastResultCode = (Result)result;
                Debug.LogErrorFormat("InitConnectConfig: can't copy lobby attribute on local. {0}", result);
                lobbyHandle.Release();
                return false;
            }
            p2pConnectorForOtherAssembly.Instance.ScoketName = EOSLobbyExtensions.GenerateLobbyAttribute(socket).STRING;
            //For options
            userIds.HostUserId = UserId.GetUserId(CurrentLobby.LobbyOwner);
            userIds.LeftUsers = new();
            lobbyHandle.Release();
            return true;
        }
        async UniTask OpenConnection(CancellationToken token){
            p2pConnectorForOtherAssembly.Instance.OpenConnection(p2pConfig.Instance.FirstConnection == p2pConfig.FirstConnectionType.Strict);
            p2pInfo.Instance.infoMethod.Init();
            p2pInfo.Instance.pings.Init();
            switch(p2pConfig.Instance.FirstConnection){
                case p2pConfig.FirstConnectionType.Strict:
                    await p2pInfoMethod.WaitConnectPreparation(token);
                return;
                case p2pConfig.FirstConnectionType.TempDelayedDelivery:
                    p2pInfoMethod.DisableDelayedDeliveryAfterElapsed().Forget();
                return;
                case p2pConfig.FirstConnectionType.Casual:
                case p2pConfig.FirstConnectionType.DelayedDelivery:
                return;
            }
        }
        /// <summary>
        /// For library user to save ID.
        /// </summary>
        /// <returns></returns>
        internal string GetCurrentLobbyID(){
            return CurrentLobby.LobbyId;
        }
        
        internal int GetCurrentLobbyMemberCount(){
           return CurrentLobby.Members.Count;
        }
        internal int GetMaxLobbyMemberCount(){
           return (int)CurrentLobby.MaxLobbyMembers;
        }
    }
}

