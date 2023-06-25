using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;

namespace SynicSugar.MatchMake {
    internal class EOSLobby {
        Lobby CurrentLobby = new Lobby();

        // Search
        bool waitingMatch;
        LobbySearch CurrentSearch;
        Dictionary<Lobby, LobbyDetails> SearchResults = new Dictionary<Lobby, LobbyDetails>();
        //User config
        uint MAX_SEARCH_RESULT;
        int timeoutMS;
        bool allowRecoonect;
        Action saveLobbyId, deleteLobbyId;
        //Notification
        NotifyEventHandle LobbyMemberStatusNotification;
        NotifyEventHandle LobbyUpdateNotification;
        CancellationTokenSource matchingToken = new CancellationTokenSource(); //To cancel matchmake

        bool isMatchSuccess;
        string socketName = System.String.Empty;
        
        // Manager Callbacks
        internal delegate void OnLobbyCallback(Result result);
        internal delegate void OnLobbySearchCallback(Result result);

        internal EOSLobby(uint maxSearch, int timeout, bool allowUserBack){
            MAX_SEARCH_RESULT = maxSearch;
            //For Unitask
            timeoutMS = timeout * 1000;
            allowRecoonect = allowUserBack;
        }
        internal void RegisterLobbyIdEvent(Action save, Action delete){
            saveLobbyId = save;
            deleteLobbyId = delete;
        }

        /// <summary>
        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="saveFn">To save LobbyID. If null, save ID to local by PlayerPrefs</param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<bool> StartMatching(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null){
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
                await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);
                //Matching cancel
                if(token.IsCancellationRequested){ return false; }

                if(isMatchSuccess){
                    InitConnectConfig(ref p2pConfig.Instance.userIds);
                    p2pConnectorForOtherAssembly.Instance.OpenConnection();
                    SaveLobbyId();
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
                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token), UniTask.Delay(timeoutMS, cancellationToken: token.Token));
                //Matching cancel
                if(token.IsCancellationRequested){ return false; }

                if(isMatchSuccess){
                    InitConnectConfig(ref p2pConfig.Instance.userIds);
                    p2pConnectorForOtherAssembly.Instance.OpenConnection();
                    SaveLobbyId();
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
        /// <param name="saveFn">To save LobbyID. If null, save ID to local by PlayerPrefs</param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<bool> StartJustSearch(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null){
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
                await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);

                //Matching cancel
                if(token.IsCancellationRequested){ return false; }

                if(isMatchSuccess){
                    InitConnectConfig(ref p2pConfig.Instance.userIds);
                    p2pConnectorForOtherAssembly.Instance.OpenConnection();
                    SaveLobbyId();
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
        /// <param name="saveFn">To save LobbyID. If null, save ID to local by PlayerPrefs</param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        internal async UniTask<bool> StartJustCreate(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null){
            MatchMakeManager.Instance.matchState.stopAdditionalInput?.Invoke();
            MatchMakeManager.Instance.UpdateStateDescription(MatchState.Wait);
            bool canCreate = await CreateLobby(lobbyCondition, token);
            if(canCreate){
                // Wait for other player to join
                // Chagne these value via MamberStatusUpdate notification.
                isMatchSuccess = false;
                waitingMatch = true;
                MatchMakeManager.Instance.matchState.acceptCancel?.Invoke();
                await UniTask.WhenAny(UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token), UniTask.Delay(timeoutMS, cancellationToken: token.Token));

                //Matching cancel
                if(token.IsCancellationRequested){ return false; }

                if(isMatchSuccess){
                    InitConnectConfig(ref p2pConfig.Instance.userIds);
                    p2pConnectorForOtherAssembly.Instance.OpenConnection();
                    SaveLobbyId();
                    return true;
                }
            }
            //Failed due to no-playing-user or server problems.
            return false;
        }
        /// <summary>
        /// Join the Lobby with saved LobbyID. <br />
        /// Call this at the start of game or match-make.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        public async UniTask<bool> ReconnectParticipatingLobby(string LobbyID, CancellationTokenSource token){
            MatchMakeManager.Instance.UpdateStateDescription(MatchState.Connect);
            bool canSerch = await RetriveLobbyByLobbyId(LobbyID, token);

            if(!canSerch){
                MatchMakeManager.Instance.UpdateStateDescription(MatchState.Fail);
                return false; //The lobby was already closed.
            }
            //Join
            bool canJoin = await ExamineSearchResults(token);
            if(!canJoin){
                return false; //The lobby was already closed.
            }
            //For the host migration
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnMemberStatusReceived), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });
            
            InitConnectConfig(ref p2pConfig.Instance.userIds);
            p2pConfig.Instance.userIds.isJustReconnected = true;
            return true;
        }
//Host
#region Create
        /// <summary>
        /// Create a Lobby. <br />
        /// *<c>StartMatching()</c> can search for an existing lobby and create one if there isn't.
        /// </summary>
        /// <param name="lobbyCondition"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async UniTask<bool> CreateLobby(Lobby lobbyCondition, CancellationTokenSource token, OnLobbyCallback callback = null){
            // Check if there is current session. Leave it.
            if (CurrentLobby.isValid()){
#if SYNICSUGAR_LOG
                Debug.LogWarningFormat("Lobbies (Create Lobby): Leaving Current Lobby '{0}'", CurrentLobby.LobbyId);
#endif
                LeaveLobby(true).Forget();
            }
            //Hold token to cancel matchmake
            matchingToken = token;

            //Lobby Option
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            createLobbyOptions.MaxLobbyMembers = lobbyCondition.MaxLobbyMembers;
            createLobbyOptions.PermissionLevel = lobbyCondition.PermissionLevel;
            createLobbyOptions.BucketId = lobbyCondition.BucketId;
            createLobbyOptions.PresenceEnabled = lobbyCondition.bPresenceEnabled;
            createLobbyOptions.AllowInvites = lobbyCondition.bAllowInvites;

            // Init for async
            waitingMatch = true;
            isMatchSuccess = false;

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            //Set lobby data
            CurrentLobby = lobbyCondition;
            CurrentLobby._BeingCreated = true;
            CurrentLobby.LobbyOwner = EOSManager.Instance.GetProductUserId();

            lobbyInterface.CreateLobby(ref createLobbyOptions, callback, OnCreateLobbyCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);
            if(!isMatchSuccess){
                Debug.LogError("Create Lobby: can't new lobby");
                return false;
            }
            //Register Notification
            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnMemberStatusReceived), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
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
            if (info.ResultCode != Result.Success){
                Debug.LogErrorFormat("Created Lobby: error code: {0}", info.ResultCode);
                waitingMatch = false;
                return;
            }
            
            if (string.IsNullOrEmpty(info.LobbyId) || !CurrentLobby._BeingCreated){
                waitingMatch = false;
                return;
            }

            OnLobbyCallback callback = info.ClientData as OnLobbyCallback;
            callback?.Invoke(Result.Success);
            CurrentLobby.LobbyId = info.LobbyId;

            isMatchSuccess = true;
            waitingMatch = false;
        }
        /// <summary>
        /// Set attribute for search. This process is only for Host player.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async UniTask<bool> AddSerachLobbyAttribute(Lobby lobbyCondition, CancellationTokenSource token, OnLobbyCallback callback = null){
            if (!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Change Lobby: This isn't lobby owner.");
#endif
                return false;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions();
            options.LobbyId = CurrentLobby.LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            //Create modify handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);

            if (result != Result.Success){
                Debug.LogErrorFormat("Change Lobby: Could not create lobby modification. Error code: {0}", result);
                return false;
            }
            //Set Backet ID
            AttributeData bucketAttribute = new AttributeData();
            bucketAttribute.Key = "bucket";
            bucketAttribute.Value = new AttributeDataValue(){
                AsUtf8 = lobbyCondition.BucketId
            };
            
            LobbyModificationAddAttributeOptions addAttributeOptions = new LobbyModificationAddAttributeOptions();

            addAttributeOptions.Attribute = bucketAttribute;
            addAttributeOptions.Visibility = LobbyAttributeVisibility.Public;

            result = lobbyHandle.AddAttribute(ref addAttributeOptions);
            if (result != Result.Success){
                Debug.LogErrorFormat("Change Lobby: could not add backetID. Error code: {0}", result);
                return false;
            }

            // Set attribute to handle in local
            foreach(var attribute in lobbyCondition.Attributes){
                addAttributeOptions.Attribute = attribute.AsLobbyAttribute();
                addAttributeOptions.Visibility = attribute.Visibility;

                result = lobbyHandle.AddAttribute(ref addAttributeOptions);
                if (result != Result.Success){
                    Debug.LogErrorFormat("Change Lobby: could not add attribute. Error code: {0}", result);
                    return false;
                }
            }

            //Add attribute with handle
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
            updateOptions.LobbyModificationHandle = lobbyHandle; 
            // Init for async
            waitingMatch = true;
            isMatchSuccess = false;

            lobbyInterface.UpdateLobby(ref updateOptions, callback, OnAddSerchAttribute);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);

            return isMatchSuccess; //"isMatchSuccess" is changed in async and callback method with result.
        }
        void OnAddSerchAttribute(ref UpdateLobbyCallbackInfo info){
            OnLobbyCallback callback = info.ClientData as OnLobbyCallback;

            if (info.ResultCode != Result.Success){
                waitingMatch = false;
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                callback?.Invoke(info.ResultCode);
                return;
            }

            OnLobbyUpdated(info.LobbyId, callback);
            isMatchSuccess = true;
            waitingMatch = false;
        }
        /// <summary>
        /// Add scoketName and remove search attributes.
        /// This process is the preparation for p2p connect and re-Connect. <br />
        /// Use lobbyID to connect on the problem, so save lobbyID in local somewhere.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        void SwitchLobbyAttribute(OnLobbyCallback callback = null){
            if (!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Change Lobby: This user isn't lobby owner.");
#endif
                return;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions();
            options.LobbyId = CurrentLobby.LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            //Create modify handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyHandle);

            if (result != Result.Success){
                Debug.LogErrorFormat("Change Lobby: Could not create lobby modification. Error code: {0}", result);
                return;
            }
            //Change permission level
            LobbyModificationSetPermissionLevelOptions permissionOptions = new LobbyModificationSetPermissionLevelOptions();
            permissionOptions.PermissionLevel = LobbyPermissionLevel.Joinviapresence;
            result = lobbyHandle.SetPermissionLevel(ref permissionOptions);
            if (result != Result.Success){
                Debug.LogErrorFormat("Change Lobby: can't switch permission level. Error code: {0}", result);
                return;
            }

            // Set SocketName
            string socket = !string.IsNullOrEmpty(socketName) ? socketName : EOSp2pExtenstions.GenerateRandomSocketName();
            AttributeData socketAttribute = new AttributeData();
            socketAttribute.Key = "socket";
            socketAttribute.Value = new AttributeDataValue(){
                AsUtf8 = socket
            };
            
            LobbyModificationAddAttributeOptions addAttributeOptions = new LobbyModificationAddAttributeOptions();

            addAttributeOptions.Attribute = socketAttribute;
            addAttributeOptions.Visibility = LobbyAttributeVisibility.Public;

            result = lobbyHandle.AddAttribute(ref addAttributeOptions);
            if (result != Result.Success){
                Debug.LogErrorFormat("Change Lobby: could not add socket name. Error code: {0}", result);
                return;
            }

            // Set attribute to handle in local
            foreach(var attribute in CurrentLobby.Attributes){
                LobbyModificationRemoveAttributeOptions removeAttributeOptions = new LobbyModificationRemoveAttributeOptions();
                removeAttributeOptions.Key = attribute.Key;

                result = lobbyHandle.RemoveAttribute(ref removeAttributeOptions);
                if (result != Result.Success){
                    Debug.LogErrorFormat("Change Lobby: could not remove attribute. Error code: {0}", result);
                    return;
                }
            }
            //Change lobby attributes with handle
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
            updateOptions.LobbyModificationHandle = lobbyHandle; 

            lobbyInterface.UpdateLobby(ref updateOptions, callback, OnSwitchLobbyAttribute);
        }
        void OnSwitchLobbyAttribute(ref UpdateLobbyCallbackInfo info){
            OnLobbyCallback callback = info.ClientData as OnLobbyCallback;

            if (info.ResultCode != Result.Success){
                Debug.LogErrorFormat("Modify Lobby: error code: {0}", info.ResultCode);
                callback?.Invoke(info.ResultCode);
                waitingMatch = false;
                return;
            }

            OnLobbyUpdated(info.LobbyId, callback);
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
        async UniTask<bool> JoinExistingLobby(Lobby lobbyCondition, CancellationTokenSource token){
            //Hold token to cancel matchmake
            matchingToken = token;
            //Serach
            bool canRetrive = await RetriveLobbyByAttribute(lobbyCondition, token);
            if(!canRetrive){
                return false; //Need to create own session
            }
            //Join
            bool canJoin = await ExamineSearchResults(token);
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
            LobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnMemberStatusReceived), (ulong handle) =>{
                EOSManager.Instance.GetEOSLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(handle);
            });

            return true;
        }
        /// <summary>
        /// For use in normal matching. Retrive Lobby by Attributes. 
        /// Retrun true on getting Lobby data.
        /// </summary>
        /// <param name="lobbyCodition"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async UniTask<bool> RetriveLobbyByAttribute(Lobby lobbyCondition, CancellationTokenSource token, OnLobbySearchCallback callback = null){
            //Create Search handle on local
            // Create new handle 
            CreateLobbySearchOptions searchOptions = new CreateLobbySearchOptions();
            searchOptions.MaxResults = MAX_SEARCH_RESULT; 

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch lobbySearchHandle);

            if (result != Result.Success){
                Debug.LogErrorFormat("Search Lobby: could not create SearchByAttribute. Error code: {0}", result);
                return false;
            }

            CurrentSearch = lobbySearchHandle;

            //Set Backet ID
            AttributeData bucketAttribute = new AttributeData();
            bucketAttribute.Key = "bucket";
            bucketAttribute.Value = new AttributeDataValue(){
                AsUtf8 = lobbyCondition.BucketId
            };
            
            LobbySearchSetParameterOptions paramOptions = new LobbySearchSetParameterOptions();

            paramOptions.Parameter = bucketAttribute;
            paramOptions.ComparisonOp = ComparisonOp.Equal;

            result = lobbySearchHandle.SetParameter(ref paramOptions);
            
            if (result != Result.Success){
                Debug.LogErrorFormat("Retrieve Lobby: failed to add bucketID. Error code: {0}", result);
                return false;
            }
            
            // Set other attributes
            foreach (var attribute in lobbyCondition.Attributes){
                AttributeData data = attribute.AsLobbyAttribute();
                paramOptions.Parameter = data;
                paramOptions.ComparisonOp = attribute.comparisonOption; 

                result = lobbySearchHandle.SetParameter(ref paramOptions);

                if (result != Result.Success){
                    Debug.LogErrorFormat("Retrieve Lobby: failed to add option attribute. Error code: {0}", result);
                    return false;
                }
            }

            //Search with handle
            LobbySearchFindOptions findOptions = new LobbySearchFindOptions();
            findOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            waitingMatch = true;
            lobbySearchHandle.Find(ref findOptions, callback, OnLobbySearchCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);
            //Can retrive data
            return true;
        }
        /// <summary>
        /// Return to last match before the connection is lost.
        /// Need LobbyID that saved in each user local or somewhere.
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async UniTask<bool> RetriveLobbyByLobbyId(string lobbyId, CancellationTokenSource token, OnLobbySearchCallback callback = null){
            //Create Search handle on local
            // Create new handle 
            CreateLobbySearchOptions searchOptions = new CreateLobbySearchOptions();
            searchOptions.MaxResults = 2; //For duplicattion of lobby? Probably not a problem for 1.

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch outLobbySearchHandle);

            if (result != Result.Success){
                Debug.LogErrorFormat("Lobby Search: could not create SearchByLobbyId. Error code: {0}", result);
                return false;
            }

            CurrentSearch = outLobbySearchHandle;

            // Set Lobby ID
            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions();
            setLobbyOptions.LobbyId = lobbyId;

            result = outLobbySearchHandle.SetLobbyId(ref setLobbyOptions);

            if (result != Result.Success){
                Debug.LogErrorFormat("Search Lobby: failed to update SearchByLobbyId with lobby id. Error code: {0}", result);
                return false;
            }

            //Search with handle
            LobbySearchFindOptions searchFindOptions = new LobbySearchFindOptions();
            searchFindOptions.LocalUserId = EOSManager.Instance.GetProductUserId();

            outLobbySearchHandle.Find(ref searchFindOptions, null, OnLobbySearchCompleted);

            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);
            //Can retrive data
            return true;
        }

        void OnLobbySearchCompleted(ref LobbySearchFindCallbackInfo info){
            waitingMatch = false;
            if (info.ResultCode != Result.Success) {
                Debug.LogErrorFormat("Search Lobby: error code: {0}", info.ResultCode);
                return;
            }
        }
#endregion
#region Join
        async UniTask<bool> ExamineSearchResults(CancellationTokenSource token){ 
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

                Result result = CurrentSearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails lobbyHandle);

                if (result == Result.Success && lobbyHandle != null){
                    lobbyObj.InitFromLobbyDetails(lobbyHandle);

                    if (lobbyObj == null || !lobbyObj.isValid()){
                        continue;
                    }
                    waitingMatch = true;
                    JoinLobby(lobbyHandle);
                    
                    await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);

                    if(isMatchSuccess || token.IsCancellationRequested){
                        SearchResults.Add(lobbyObj, lobbyHandle);
                        break;
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// Call this from ExamineSearchResults.
        /// </summary>
        void JoinLobby(LobbyDetails lobbyDetails, Action<Result> callback = null){
            JoinLobbyOptions joinOptions = new JoinLobbyOptions();
            joinOptions.LobbyDetailsHandle = lobbyDetails;
            joinOptions.LocalUserId = EOSManager.Instance.GetProductUserId();
            joinOptions.PresenceEnabled = false;

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSPlatformInterface().GetLobbyInterface();
            lobbyInterface.JoinLobby(ref joinOptions, callback, OnJoinLobbyCompleted);
        }
        void OnJoinLobbyCompleted(ref JoinLobbyCallbackInfo data){
            OnLobbyCallback JoinLobbyCallback = data.ClientData as OnLobbyCallback;

            if (data.ResultCode != Result.Success){
                Debug.LogErrorFormat("Join Lobby: error code: {0}", data.ResultCode);
                waitingMatch = false;
                return;
            }

            // If has joined in other lobby
            if (CurrentLobby.isValid() && !string.Equals(CurrentLobby.LobbyId, data.LobbyId)){
                LeaveLobby(true).Forget();
            }

            CurrentLobby.InitFromLobbyHandle(data.LobbyId);

            waitingMatch = false;
        }

#endregion
//Common
#region Notification
        /// <summary>
        /// For Host and Guest. To get the notification for a user to join and disconnect.
        /// </summary>
        /// <param name="lobbyInterface"></param>
        /// <param name="notificationFn"></param>
        /// <returns></returns>
        ulong AddNotifyLobbyMemberStatusReceived(LobbyInterface lobbyInterface, OnLobbyMemberStatusReceivedCallback notificationFn){
            var options = new AddNotifyLobbyMemberStatusReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberStatusReceived(ref options, null, notificationFn);
        }
        void OnMemberStatusReceived(ref LobbyMemberStatusReceivedCallbackInfo data){
            if (!data.TargetUserId.IsValid()){
                Debug.LogError("Lobby Notification: Current player is invalid.");
                return;
            }
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            // Target is local user and dosen't need to be updated.
            if (data.TargetUserId == productUserId){
                if (data.CurrentStatus == LobbyMemberStatus.Closed ||
                    data.CurrentStatus == LobbyMemberStatus.Kicked ||
                    data.CurrentStatus == LobbyMemberStatus.Disconnected){
                    OnKickedFromLobby(data.LobbyId);
                    //Fail to MatchMake.
                    waitingMatch = false;
                    return;
                }
            }
            OnLobbyUpdated(data.LobbyId);
            
            //For MatchMake on the first
            if(waitingMatch){ //This flag is shared by both host and guest, and is false after getting SocketName.
                if(data.TargetUserId == productUserId && data.CurrentStatus == LobbyMemberStatus.Promoted){
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
            if (data.CurrentStatus == LobbyMemberStatus.Promoted){
                p2pConfig.Instance.userIds.HostUserId = new UserId(data.TargetUserId);

                if(!CurrentLobby.isHost()){
                    //MEMO: Now, if user disconnect from Lobby and then change hosts, the user become newbie.
                    //Guest Don't need to hold user id 
                    p2pConfig.Instance.userIds.LeftUsers = new List<UserId>();
                }
            }else if(data.CurrentStatus == LobbyMemberStatus.Left) {
                p2pConfig.Instance.userIds.RemoveUserId(data.TargetUserId);
            }else if(data.CurrentStatus == LobbyMemberStatus.Disconnected){
                p2pConfig.Instance.userIds.MoveTargetUserIdToLefts(data.TargetUserId);
            }
        }
        /// <summary>
        /// For Guest to retrive the SoketName just after joining lobby.
        /// On getting the ScoketName, discard this event.
        /// </summary>
        /// <param name="lobbyInterface"></param>
        /// <param name="notificationFn"></param>
        /// <returns></returns>
        ulong AddNotifyLobbyUpdateReceived(LobbyInterface lobbyInterface, OnLobbyUpdateReceivedCallback notificationFn){
            var options = new AddNotifyLobbyUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyUpdateReceived(ref options, null, notificationFn);
        }
        void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo data){
            if(data.LobbyId != CurrentLobby.LobbyId){
                Debug.LogError("Lobby Updated: this is other lobby's data.");
                return;
            }

            OnLobbyUpdated(data.LobbyId);

            if(CurrentLobby.PermissionLevel == LobbyPermissionLevel.Joinviapresence){
                //No need to get lobby update info (to get socket name)
                LobbyUpdateNotification.Dispose();
                
                isMatchSuccess = true;
                waitingMatch = false;
            }
        }
#endregion
#region Modify
        void OnLobbyUpdated(string lobbyId, OnLobbyCallback LobbyUpdateCompleted = null){
            if (!string.IsNullOrEmpty(lobbyId) && CurrentLobby.LobbyId == lobbyId){
                CurrentLobby.InitFromLobbyHandle(lobbyId);

                LobbyUpdateCompleted?.Invoke(Result.Success);
            }
        }
#endregion
#region Cancel
    internal async UniTask<bool> CancelMatchMaking(CancellationTokenSource token = default(CancellationTokenSource)){
        if(CurrentLobby.isHost()){
            if(CurrentLobby.Members.Count == 1){
                bool canDestroy = await DestroyLobby(token);
                matchingToken.Cancel();
                if(!canDestroy){
                    Debug.LogError($"Cancel MatchMaking: has something problem when destroying the lobby");
                }

                return canDestroy;
            }
        }

        bool canLeave = await LeaveLobby(true, token);
        
        matchingToken.Cancel();
        if(!canLeave){
            Debug.LogError($"Cancel MatchMaking: has something problem when leave from the lobby");
        }
        return canLeave;
    }
#endregion
#region Leave
        bool waitLeave, canLeave;
        //SynicSugar does not expect user to be in more than one Lobby at the same time.
        //So when joining in a new Lobby, the user needs to exit an old one.
        //It is not necessary to synchronize in most situations and can add Forget().
        //About Guests, when the lobby is Destroyed by Host, they Leave the lobby automatically.
        /// <summary>
        /// Leave the Participating Lobby.<br />
        /// When a game is over, call DestroyLobby() instead of this. .
        /// </summary>
        /// <param name="LeaveLobbyCompleted">Callback when leave lobby is completed</param>
        internal async UniTask<bool> LeaveLobby(bool inMatchMaking = false, CancellationTokenSource token = default(CancellationTokenSource), OnLobbyCallback LeaveLobbyCompleted = null){
            if (CurrentLobby == null || string.IsNullOrEmpty(CurrentLobby.LobbyId) || !EOSManager.Instance.GetProductUserId().IsValid()){
                Debug.LogWarning("Leave Lobby: Not currently in a lobby.");
                return false;
            }

            LeaveLobbyOptions options = new LeaveLobbyOptions();
            options.LobbyId = CurrentLobby.LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            waitLeave = true;
            canLeave = false;
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            lobbyInterface.LeaveLobby(ref options, LeaveLobbyCompleted, OnLeaveLobbyCompleted);

            await UniTask.WaitUntil(() => !waitLeave, cancellationToken: token.Token);
            if(!inMatchMaking){
                DeleteLobbyID();
            }

            return canLeave;
        }
        void OnLeaveLobbyCompleted(ref LeaveLobbyCallbackInfo data){
            if (data.ResultCode != Result.Success){
                Debug.LogFormat("Leave Lobby: error code: {0}", data.ResultCode);
                canLeave = false;
                waitLeave = false;
                return;
            }

            OnLobbyCallback LeaveLobbyCallback = data.ClientData as OnLobbyCallback;

            CurrentLobby.Clear();
            LeaveLobbyCallback?.Invoke(Result.Success);
            canLeave = true;
            waitLeave = false;
        }
        void OnKickedFromLobby(string lobbyId){
            if (CurrentLobby.isValid() && CurrentLobby.LobbyId.Equals(lobbyId, StringComparison.OrdinalIgnoreCase)){
                CurrentLobby.Clear();
                LobbyMemberStatusNotification.Dispose();
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
        internal async UniTask<bool> DestroyLobby(CancellationTokenSource token){
            if(!CurrentLobby.isHost()){
#if SYNICSUGAR_LOG
                Debug.LogError("Destroy Lobby: This user is not Host.");
#endif
                return false;
            }
            DestroyLobbyOptions options = new DestroyLobbyOptions();
            options.LocalUserId = EOSManager.Instance.GetProductUserId();
            options.LobbyId = CurrentLobby.LobbyId;

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();

            waitingMatch = true;
            isMatchSuccess = false;
            lobbyInterface.DestroyLobby(ref options, null, OnDestroyLobbyCompleted);
            
            await UniTask.WaitUntil(() => !waitingMatch, cancellationToken: token.Token);

            DeleteLobbyID();
            LobbyMemberStatusNotification.Dispose();
            CurrentLobby.Clear();

            return isMatchSuccess;
        }
        void OnDestroyLobbyCompleted(ref DestroyLobbyCallbackInfo data){
            if (data.ResultCode != Result.Success){
                waitingMatch = false;
                Debug.LogErrorFormat("Destroy Lobby: error code: {0}", data.ResultCode);
                return;
            }
            
            isMatchSuccess = true;
            waitingMatch = false;
        }
#endregion
#region Re-connect(Waiter)
        
#endregion
        /// <summary>
        /// Init p2pManager's room info with new lobby data.
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        bool InitConnectConfig(ref UserIds userIds){
            //Crate copy handle
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions();
            options.LobbyId = CurrentLobby.LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            Result result = lobbyInterface.CopyLobbyDetailsHandle(ref options, out LobbyDetails lobbyHandle );

            if(result != Result.Success){
                Debug.LogError("Get detail: can't get lobby detail handle.");
                lobbyHandle.Release();
                return false;
            }
            //Copy details from local handle
            LobbyDetailsCopyInfoOptions dataOptions = new LobbyDetailsCopyInfoOptions();
            result = lobbyHandle.CopyInfo(ref dataOptions, out LobbyDetailsInfo? dataInfo);

            if(result != Result.Success){
                Debug.LogError("Get detail: can't copy lobby details on local.");
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
                    userIds.RemoteUserIds.Add(new UserId(lobbyHandle.GetMemberByIndex(ref memberOptions)));
                }
            }
            //Get lobby's attribute count
            LobbyDetailsCopyAttributeByKeyOptions attrOptions = new LobbyDetailsCopyAttributeByKeyOptions();
            attrOptions.AttrKey = "socket";
            result = lobbyHandle.CopyAttributeByKey(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? socket);
            if(result != Result.Success || socket?.Data == null){
                Debug.LogError("Get detail: can't copy lobby attribute on local.");
                lobbyHandle.Release();
                return false;
            }
            p2pConnectorForOtherAssembly.Instance.ScoketName = EOSLobbyExtenstions.GenerateLobbyAttribute(socket).STRING;
            //For options
            userIds.HostUserId = new UserId(CurrentLobby.LobbyOwner);
            userIds.LeftUsers = new List<UserId>();
            lobbyHandle.Release();
            return true;
        }
        /// <summary>
        /// Save lobby data for player to connect unexpectedly left lobby like power off.
        /// </summary>
        void SaveLobbyId(){
            if(!allowRecoonect){ return; }

            if(saveLobbyId != null && deleteLobbyId != null){
                saveLobbyId.Invoke();
                return;
            }
            PlayerPrefs.SetString("eos_lobbyid", CurrentLobby.LobbyId);
        }
        /// <summary>
        /// Delete save data for player not to connect the current lobby after the battle.
        /// </summary>
        void DeleteLobbyID(){
            if(!allowRecoonect){ return; }

            if(saveLobbyId != null && deleteLobbyId != null){
                deleteLobbyId.Invoke();
                return;
            }
            PlayerPrefs.DeleteKey("eos_lobbyid");
        }
    }
}

