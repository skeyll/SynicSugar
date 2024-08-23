// Copyright Epic Games, Inc. All Rights Reserved.
// Some items are original to SynicSugar; if you implement your own Cores, these result must be able to convert from these.
namespace SynicSugar {
 	public enum Result : int {
		ConnectEstablishFailed = -4,
		/// <summary>
		/// Lobby Closed by Host and kicked out from the current lobby.
		/// </summary>
		LobbyClosed = -3,
		/// <summary>
		/// Failed due to call SynicSugar API with lack conditions
		/// </summary>
		InvalidAPICall = -2,
		/// <summary>
		/// Initial state. SynicSugar has no data.
		/// </summary>
		None = -1,
		/// <summary>
		/// Successful result. no further error processing needed
		/// </summary>
		Success = 0,
		/// <summary>
		/// Failed due to no connection
		/// </summary>
		NoConnection = 1,
		/// <summary>
		/// Failed login due to invalid credentials
		/// </summary>
		InvalidCredentials = 2,
		/// <summary>
		/// Failed due to invalid or missing user
		/// </summary>
		InvalidUser = 3,
		/// <summary>
		/// Failed due to invalid or missing authentication token for user (e.g. not logged in)
		/// </summary>
		InvalidAuth = 4,
		/// <summary>
		/// Failed due to invalid access
		/// </summary>
		AccessDenied = 5,
		/// <summary>
		/// If the client does not possess the permission required
		/// </summary>
		MissingPermissions = 6,
		/// <summary>
		/// If the token provided does not represent an account
		/// </summary>
		TokenNotAccount = 7,
		/// <summary>
		/// Throttled due to too many requests
		/// </summary>
		TooManyRequests = 8,
		/// <summary>
		/// Async request was already pending
		/// </summary>
		AlreadyPending = 9,
		/// <summary>
		/// Invalid parameters specified for request
		/// </summary>
		InvalidParameters = 10,
		/// <summary>
		/// Invalid request
		/// </summary>
		InvalidRequest = 11,
		/// <summary>
		/// Failed due to unable to parse or recognize a backend response
		/// </summary>
		UnrecognizedResponse = 12,
		/// <summary>
		/// Incompatible client for backend version
		/// </summary>
		IncompatibleVersion = 13,
		/// <summary>
		/// Not configured correctly for use
		/// </summary>
		NotConfigured = 14,
		/// <summary>
		/// Already configured for use.
		/// </summary>
		AlreadyConfigured = 15,
		/// <summary>
		/// Feature not available on this implementation
		/// </summary>
		NotImplemented = 16,
		/// <summary>
		/// Operation was canceled (likely by user)
		/// </summary>
		Canceled = 17,
		/// <summary>
		/// The requested information was not found
		/// </summary>
		NotFound = 18,
		/// <summary>
		/// An error occurred during an asynchronous operation, and it will be retried. Callbacks receiving this result will be called again in the future.
		/// </summary>
		OperationWillRetry = 19,
		/// <summary>
		/// The request had no effect
		/// </summary>
		NoChange = 20,
		/// <summary>
		/// The request attempted to use multiple or inconsistent API versions
		/// </summary>
		VersionMismatch = 21,
		/// <summary>
		/// A maximum limit was exceeded on the client, different from <see cref="TooManyRequests" />
		/// </summary>
		LimitExceeded = 22,
		/// <summary>
		/// Feature or client ID performing the operation has been disabled.
		/// </summary>
		Disabled = 23,
		/// <summary>
		/// Duplicate entry not allowed
		/// </summary>
		DuplicateNotAllowed = 24,
		/// <summary>
		/// Required parameters are missing. DEPRECATED: This error is no longer used.
		/// </summary>
		MissingParametersDEPRECATED = 25,
		/// <summary>
		/// Sandbox ID is invalid
		/// </summary>
		InvalidSandboxId = 26,
		/// <summary>
		/// Request timed out
		/// </summary>
		TimedOut = 27,
		/// <summary>
		/// A query returned some but not all of the requested results.
		/// </summary>
		PartialResult = 28,
		/// <summary>
		/// Client is missing the whitelisted role
		/// </summary>
		MissingRole = 29,
		/// <summary>
		/// Client is missing the whitelisted feature
		/// </summary>
		MissingFeature = 30,
		/// <summary>
		/// The sandbox given to the backend is invalid
		/// </summary>
		InvalidSandbox = 31,
		/// <summary>
		/// The deployment given to the backend is invalid
		/// </summary>
		InvalidDeployment = 32,
		/// <summary>
		/// The product ID specified to the backend is invalid
		/// </summary>
		InvalidProduct = 33,
		/// <summary>
		/// The product user ID specified to the backend is invalid
		/// </summary>
		InvalidProductUserID = 34,
		/// <summary>
		/// There was a failure with the backend service
		/// </summary>
		ServiceFailure = 35,
		/// <summary>
		/// Cache directory is not set in platform options.
		/// </summary>
		CacheDirectoryMissing = 36,
		/// <summary>
		/// Cache directory is not accessible.
		/// </summary>
		CacheDirectoryInvalid = 37,
		/// <summary>
		/// The request failed because resource was in an invalid state
		/// </summary>
		InvalidState = 38,
		/// <summary>
		/// Request is in progress
		/// </summary>
		RequestInProgress = 39,
		/// <summary>
		/// Application is suspended
		/// </summary>
		ApplicationSuspended = 40,
		/// <summary>
		/// Network is disconnected
		/// </summary>
		NetworkDisconnected = 41,
		/// <summary>
		/// Account locked due to login failures
		/// </summary>
		AuthAccountLocked = 1001,
		/// <summary>
		/// Account locked by update operation.
		/// </summary>
		AuthAccountLockedForUpdate = 1002,
		/// <summary>
		/// Refresh token used was invalid
		/// </summary>
		AuthInvalidRefreshToken = 1003,
		/// <summary>
		/// Invalid access token, typically when switching between backend environments
		/// </summary>
		AuthInvalidToken = 1004,
		/// <summary>
		/// Invalid bearer token
		/// </summary>
		AuthAuthenticationFailure = 1005,
		/// <summary>
		/// Invalid platform token
		/// </summary>
		AuthInvalidPlatformToken = 1006,
		/// <summary>
		/// Auth parameters are not associated with this account
		/// </summary>
		AuthWrongAccount = 1007,
		/// <summary>
		/// Auth parameters are not associated with this client
		/// </summary>
		AuthWrongClient = 1008,
		/// <summary>
		/// Full account is required
		/// </summary>
		AuthFullAccountRequired = 1009,
		/// <summary>
		/// Headless account is required
		/// </summary>
		AuthHeadlessAccountRequired = 1010,
		/// <summary>
		/// Password reset is required
		/// </summary>
		AuthPasswordResetRequired = 1011,
		/// <summary>
		/// Password was previously used and cannot be reused
		/// </summary>
		AuthPasswordCannotBeReused = 1012,
		/// <summary>
		/// Authorization code/exchange code has expired
		/// </summary>
		AuthExpired = 1013,
		/// <summary>
		/// Consent has not been given by the user
		/// </summary>
		AuthScopeConsentRequired = 1014,
		/// <summary>
		/// The application has no profile on the backend
		/// </summary>
		AuthApplicationNotFound = 1015,
		/// <summary>
		/// The requested consent wasn't found on the backend
		/// </summary>
		AuthScopeNotFound = 1016,
		/// <summary>
		/// This account has been denied access to login
		/// </summary>
		AuthAccountFeatureRestricted = 1017,
		/// <summary>
		/// The overlay failed to load the Account Portal. This can range from general overlay failure, to overlay failed to connect to the web server, to overlay failed to render the web page.
		/// </summary>
		AuthAccountPortalLoadError = 1018,
		/// <summary>
		/// An attempted login has failed due to the user needing to take corrective action on their account.
		/// </summary>
		AuthCorrectiveActionRequired = 1019,
		/// <summary>
		/// Pin grant code initiated
		/// </summary>
		AuthPinGrantCode = 1020,
		/// <summary>
		/// Pin grant code attempt expired
		/// </summary>
		AuthPinGrantExpired = 1021,
		/// <summary>
		/// Pin grant code attempt pending
		/// </summary>
		AuthPinGrantPending = 1022,
		/// <summary>
		/// External auth source did not yield an account
		/// </summary>
		AuthExternalAuthNotLinked = 1030,
		/// <summary>
		/// External auth access revoked
		/// </summary>
		AuthExternalAuthRevoked = 1032,
		/// <summary>
		/// External auth token cannot be interpreted
		/// </summary>
		AuthExternalAuthInvalid = 1033,
		/// <summary>
		/// External auth cannot be linked to his account due to restrictions
		/// </summary>
		AuthExternalAuthRestricted = 1034,
		/// <summary>
		/// External auth cannot be used for login
		/// </summary>
		AuthExternalAuthCannotLogin = 1035,
		/// <summary>
		/// External auth is expired
		/// </summary>
		AuthExternalAuthExpired = 1036,
		/// <summary>
		/// External auth cannot be removed since it's the last possible way to login
		/// </summary>
		AuthExternalAuthIsLastLoginType = 1037,
		/// <summary>
		/// Exchange code not found
		/// </summary>
		AuthExchangeCodeNotFound = 1040,
		/// <summary>
		/// Originating exchange code session has expired
		/// </summary>
		AuthOriginatingExchangeCodeSessionExpired = 1041,
		/// <summary>
		/// The account has been disabled and cannot be used for authentication
		/// </summary>
		AuthAccountNotActive = 1050,
		/// <summary>
		/// MFA challenge required
		/// </summary>
		AuthMFARequired = 1060,
		/// <summary>
		/// Parental locks are in place
		/// </summary>
		AuthParentalControls = 1070,
		/// <summary>
		/// Korea real ID association required but missing
		/// </summary>
		AuthNoRealId = 1080,
		/// <summary>
		/// An outgoing friend invitation is awaiting acceptance; sending another invite to the same user is erroneous
		/// </summary>
		FriendsInviteAwaitingAcceptance = 2000,
		/// <summary>
		/// There is no friend invitation to accept/reject
		/// </summary>
		FriendsNoInvitation = 2001,
		/// <summary>
		/// Users are already friends, so sending another invite is erroneous
		/// </summary>
		FriendsAlreadyFriends = 2003,
		/// <summary>
		/// Users are not friends, so deleting the friend is erroneous
		/// </summary>
		FriendsNotFriends = 2004,
		/// <summary>
		/// Remote user has too many invites to receive new invites
		/// </summary>
		FriendsTargetUserTooManyInvites = 2005,
		/// <summary>
		/// Local user has too many invites to send new invites
		/// </summary>
		FriendsLocalUserTooManyInvites = 2006,
		/// <summary>
		/// Remote user has too many friends to make a new friendship
		/// </summary>
		FriendsTargetUserFriendLimitExceeded = 2007,
		/// <summary>
		/// Local user has too many friends to make a new friendship
		/// </summary>
		FriendsLocalUserFriendLimitExceeded = 2008,
		/// <summary>
		/// Request data was null or invalid
		/// </summary>
		PresenceDataInvalid = 3000,
		/// <summary>
		/// Request contained too many or too few unique data items, or the request would overflow the maximum amount of data allowed
		/// </summary>
		PresenceDataLengthInvalid = 3001,
		/// <summary>
		/// Request contained data with an invalid key
		/// </summary>
		PresenceDataKeyInvalid = 3002,
		/// <summary>
		/// Request contained data with a key too long or too short
		/// </summary>
		PresenceDataKeyLengthInvalid = 3003,
		/// <summary>
		/// Request contained data with an invalid value
		/// </summary>
		PresenceDataValueInvalid = 3004,
		/// <summary>
		/// Request contained data with a value too long
		/// </summary>
		PresenceDataValueLengthInvalid = 3005,
		/// <summary>
		/// Request contained an invalid rich text string
		/// </summary>
		PresenceRichTextInvalid = 3006,
		/// <summary>
		/// Request contained a rich text string that was too long
		/// </summary>
		PresenceRichTextLengthInvalid = 3007,
		/// <summary>
		/// Request contained an invalid status state
		/// </summary>
		PresenceStatusInvalid = 3008,
		/// <summary>
		/// The entitlement retrieved is stale, requery for updated information
		/// </summary>
		EcomEntitlementStale = 4000,
		/// <summary>
		/// The offer retrieved is stale, requery for updated information
		/// </summary>
		EcomCatalogOfferStale = 4001,
		/// <summary>
		/// The item or associated structure retrieved is stale, requery for updated information
		/// </summary>
		EcomCatalogItemStale = 4002,
		/// <summary>
		/// The one or more offers has an invalid price. This may be caused by the price setup.
		/// </summary>
		EcomCatalogOfferPriceInvalid = 4003,
		/// <summary>
		/// The checkout page failed to load
		/// </summary>
		EcomCheckoutLoadError = 4004,
		/// <summary>
		/// Session is already in progress
		/// </summary>
		SessionsSessionInProgress = 5000,
		/// <summary>
		/// Too many players to register with this session
		/// </summary>
		SessionsTooManyPlayers = 5001,
		/// <summary>
		/// Client has no permissions to access this session
		/// </summary>
		SessionsNoPermission = 5002,
		/// <summary>
		/// Session already exists in the system
		/// </summary>
		SessionsSessionAlreadyExists = 5003,
		/// <summary>
		/// Session lock required for operation
		/// </summary>
		SessionsInvalidLock = 5004,
		/// <summary>
		/// Invalid session reference
		/// </summary>
		SessionsInvalidSession = 5005,
		/// <summary>
		/// Sandbox ID associated with auth didn't match
		/// </summary>
		SessionsSandboxNotAllowed = 5006,
		/// <summary>
		/// Invite failed to send
		/// </summary>
		SessionsInviteFailed = 5007,
		/// <summary>
		/// Invite was not found with the service
		/// </summary>
		SessionsInviteNotFound = 5008,
		/// <summary>
		/// This client may not modify the session
		/// </summary>
		SessionsUpsertNotAllowed = 5009,
		/// <summary>
		/// Backend nodes unavailable to process request
		/// </summary>
		SessionsAggregationFailed = 5010,
		/// <summary>
		/// Individual backend node is as capacity
		/// </summary>
		SessionsHostAtCapacity = 5011,
		/// <summary>
		/// Sandbox on node is at capacity
		/// </summary>
		SessionsSandboxAtCapacity = 5012,
		/// <summary>
		/// An anonymous operation was attempted on a non anonymous session
		/// </summary>
		SessionsSessionNotAnonymous = 5013,
		/// <summary>
		/// Session is currently out of sync with the backend, data is saved locally but needs to sync with backend
		/// </summary>
		SessionsOutOfSync = 5014,
		/// <summary>
		/// User has received too many invites
		/// </summary>
		SessionsTooManyInvites = 5015,
		/// <summary>
		/// Presence session already exists for the client
		/// </summary>
		SessionsPresenceSessionExists = 5016,
		/// <summary>
		/// Deployment on node is at capacity
		/// </summary>
		SessionsDeploymentAtCapacity = 5017,
		/// <summary>
		/// Session operation not allowed
		/// </summary>
		SessionsNotAllowed = 5018,
		/// <summary>
		/// Session operation not allowed
		/// </summary>
		SessionsPlayerSanctioned = 5019,
		/// <summary>
		/// Request filename was invalid
		/// </summary>
		PlayerDataStorageFilenameInvalid = 6000,
		/// <summary>
		/// Request filename was too long
		/// </summary>
		PlayerDataStorageFilenameLengthInvalid = 6001,
		/// <summary>
		/// Request filename contained invalid characters
		/// </summary>
		PlayerDataStorageFilenameInvalidChars = 6002,
		/// <summary>
		/// Request operation would grow file too large
		/// </summary>
		PlayerDataStorageFileSizeTooLarge = 6003,
		/// <summary>
		/// Request file length is not valid
		/// </summary>
		PlayerDataStorageFileSizeInvalid = 6004,
		/// <summary>
		/// Request file handle is not valid
		/// </summary>
		PlayerDataStorageFileHandleInvalid = 6005,
		/// <summary>
		/// Request data is invalid
		/// </summary>
		PlayerDataStorageDataInvalid = 6006,
		/// <summary>
		/// Request data length was invalid
		/// </summary>
		PlayerDataStorageDataLengthInvalid = 6007,
		/// <summary>
		/// Request start index was invalid
		/// </summary>
		PlayerDataStorageStartIndexInvalid = 6008,
		/// <summary>
		/// Request is in progress
		/// </summary>
		PlayerDataStorageRequestInProgress = 6009,
		/// <summary>
		/// User is marked as throttled which means he can't perform some operations because limits are exceeded.
		/// </summary>
		PlayerDataStorageUserThrottled = 6010,
		/// <summary>
		/// Encryption key is not set during SDK init.
		/// </summary>
		PlayerDataStorageEncryptionKeyNotSet = 6011,
		/// <summary>
		/// User data callback returned error (<see cref="PlayerDataStorage.WriteResult.FailRequest" /> or <see cref="PlayerDataStorage.ReadResult.FailRequest" />)
		/// </summary>
		PlayerDataStorageUserErrorFromDataCallback = 6012,
		/// <summary>
		/// User is trying to read file that has header from newer version of SDK. Game/SDK needs to be updated.
		/// </summary>
		PlayerDataStorageFileHeaderHasNewerVersion = 6013,
		/// <summary>
		/// The file is corrupted. In some cases retry can fix the issue.
		/// </summary>
		PlayerDataStorageFileCorrupted = 6014,
		/// <summary>
		/// EOS Auth service deemed the external token invalid
		/// </summary>
		ConnectExternalTokenValidationFailed = 7000,
		/// <summary>
		/// EOS Auth user already exists
		/// </summary>
		ConnectUserAlreadyExists = 7001,
		/// <summary>
		/// EOS Auth expired
		/// </summary>
		ConnectAuthExpired = 7002,
		/// <summary>
		/// EOS Auth invalid token
		/// </summary>
		ConnectInvalidToken = 7003,
		/// <summary>
		/// EOS Auth doesn't support this token type
		/// </summary>
		ConnectUnsupportedTokenType = 7004,
		/// <summary>
		/// EOS Auth Account link failure
		/// </summary>
		ConnectLinkAccountFailed = 7005,
		/// <summary>
		/// EOS Auth External service for validation was unavailable
		/// </summary>
		ConnectExternalServiceUnavailable = 7006,
		/// <summary>
		/// EOS Auth External Service configuration failure with Dev Portal
		/// </summary>
		ConnectExternalServiceConfigurationFailure = 7007,
		/// <summary>
		/// EOS Auth Account link failure. Tried to link Nintendo Network Service Account without first linking Nintendo Account. DEPRECATED: The requirement has been removed and this error is no longer used.
		/// </summary>
		ConnectLinkAccountFailedMissingNintendoIdAccountDEPRECATED = 7008,
		/// <summary>
		/// The social overlay page failed to load
		/// </summary>
		SocialOverlayLoadError = 8000,
		/// <summary>
		/// Client has no permissions to modify this lobby
		/// </summary>
		LobbyNotOwner = 9000,
		/// <summary>
		/// Lobby lock required for operation
		/// </summary>
		LobbyInvalidLock = 9001,
		/// <summary>
		/// Lobby already exists in the system
		/// </summary>
		LobbyLobbyAlreadyExists = 9002,
		/// <summary>
		/// Lobby is already in progress
		/// </summary>
		LobbySessionInProgress = 9003,
		/// <summary>
		/// Too many players to register with this lobby
		/// </summary>
		LobbyTooManyPlayers = 9004,
		/// <summary>
		/// Client has no permissions to access this lobby
		/// </summary>
		LobbyNoPermission = 9005,
		/// <summary>
		/// Invalid lobby session reference
		/// </summary>
		LobbyInvalidSession = 9006,
		/// <summary>
		/// Sandbox ID associated with auth didn't match
		/// </summary>
		LobbySandboxNotAllowed = 9007,
		/// <summary>
		/// Invite failed to send
		/// </summary>
		LobbyInviteFailed = 9008,
		/// <summary>
		/// Invite was not found with the service
		/// </summary>
		LobbyInviteNotFound = 9009,
		/// <summary>
		/// This client may not modify the lobby
		/// </summary>
		LobbyUpsertNotAllowed = 9010,
		/// <summary>
		/// Backend nodes unavailable to process request
		/// </summary>
		LobbyAggregationFailed = 9011,
		/// <summary>
		/// Individual backend node is as capacity
		/// </summary>
		LobbyHostAtCapacity = 9012,
		/// <summary>
		/// Sandbox on node is at capacity
		/// </summary>
		LobbySandboxAtCapacity = 9013,
		/// <summary>
		/// User has received too many invites
		/// </summary>
		LobbyTooManyInvites = 9014,
		/// <summary>
		/// Deployment on node is at capacity
		/// </summary>
		LobbyDeploymentAtCapacity = 9015,
		/// <summary>
		/// Lobby operation not allowed
		/// </summary>
		LobbyNotAllowed = 9016,
		/// <summary>
		/// While restoring a lost connection lobby ownership changed and only local member data was updated
		/// </summary>
		LobbyMemberUpdateOnly = 9017,
		/// <summary>
		/// Presence lobby already exists for the client
		/// </summary>
		LobbyPresenceLobbyExists = 9018,
		/// <summary>
		/// Operation requires lobby with voice enabled
		/// </summary>
		LobbyVoiceNotEnabled = 9019,
		/// <summary>
		/// User callback that receives data from storage returned error.
		/// </summary>
		TitleStorageUserErrorFromDataCallback = 10000,
		/// <summary>
		/// User forgot to set Encryption key during platform init. Title Storage can't work without it.
		/// </summary>
		TitleStorageEncryptionKeyNotSet = 10001,
		/// <summary>
		/// Downloaded file is corrupted.
		/// </summary>
		TitleStorageFileCorrupted = 10002,
		/// <summary>
		/// Downloaded file's format is newer than client SDK version.
		/// </summary>
		TitleStorageFileHeaderHasNewerVersion = 10003,
		/// <summary>
		/// ModSdk process is already running. This error comes from the EOSSDK.
		/// </summary>
		ModsModSdkProcessIsAlreadyRunning = 11000,
		/// <summary>
		/// ModSdk command is empty. Either the ModSdk configuration file is missing or the manifest location is empty.
		/// </summary>
		ModsModSdkCommandIsEmpty = 11001,
		/// <summary>
		/// Creation of the ModSdk process failed. This error comes from the SDK.
		/// </summary>
		ModsModSdkProcessCreationFailed = 11002,
		/// <summary>
		/// A critical error occurred in the external ModSdk process that we were unable to resolve.
		/// </summary>
		ModsCriticalError = 11003,
		/// <summary>
		/// A internal error occurred in the external ModSdk process that we were unable to resolve.
		/// </summary>
		ModsToolInternalError = 11004,
		/// <summary>
		/// A IPC failure occurred in the external ModSdk process.
		/// </summary>
		ModsIPCFailure = 11005,
		/// <summary>
		/// A invalid IPC response received in the external ModSdk process.
		/// </summary>
		ModsInvalidIPCResponse = 11006,
		/// <summary>
		/// A URI Launch failure occurred in the external ModSdk process.
		/// </summary>
		ModsURILaunchFailure = 11007,
		/// <summary>
		/// Attempting to perform an action with a mod that is not installed. This error comes from the external ModSdk process.
		/// </summary>
		ModsModIsNotInstalled = 11008,
		/// <summary>
		/// Attempting to perform an action on a game that the user doesn't own. This error comes from the external ModSdk process.
		/// </summary>
		ModsUserDoesNotOwnTheGame = 11009,
		/// <summary>
		/// Invalid result of the request to get the offer for the mod. This error comes from the external ModSdk process.
		/// </summary>
		ModsOfferRequestByIdInvalidResult = 11010,
		/// <summary>
		/// Could not find the offer for the mod. This error comes from the external ModSdk process.
		/// </summary>
		ModsCouldNotFindOffer = 11011,
		/// <summary>
		/// Request to get the offer for the mod failed. This error comes from the external ModSdk process.
		/// </summary>
		ModsOfferRequestByIdFailure = 11012,
		/// <summary>
		/// Request to purchase the mod failed. This error comes from the external ModSdk process.
		/// </summary>
		ModsPurchaseFailure = 11013,
		/// <summary>
		/// Attempting to perform an action on a game that is not installed or is partially installed. This error comes from the external ModSdk process.
		/// </summary>
		ModsInvalidGameInstallInfo = 11014,
		/// <summary>
		/// Failed to get manifest location. Either the ModSdk configuration file is missing or the manifest location is empty
		/// </summary>
		ModsCannotGetManifestLocation = 11015,
		/// <summary>
		/// Attempting to perform an action with a mod that does not support the current operating system.
		/// </summary>
		ModsUnsupportedOS = 11016,
		/// <summary>
		/// The anti-cheat client protection is not available. Check that the game was started using the anti-cheat bootstrapper.
		/// </summary>
		AntiCheatClientProtectionNotAvailable = 12000,
		/// <summary>
		/// The current anti-cheat mode is incorrect for using this API
		/// </summary>
		AntiCheatInvalidMode = 12001,
		/// <summary>
		/// The ProductId provided to the anti-cheat client helper executable does not match what was used to initialize the EOS SDK
		/// </summary>
		AntiCheatClientProductIdMismatch = 12002,
		/// <summary>
		/// The SandboxId provided to the anti-cheat client helper executable does not match what was used to initialize the EOS SDK
		/// </summary>
		AntiCheatClientSandboxIdMismatch = 12003,
		/// <summary>
		/// (ProtectMessage/UnprotectMessage) No session key is available, but it is required to complete this operation
		/// </summary>
		AntiCheatProtectMessageSessionKeyRequired = 12004,
		/// <summary>
		/// (ProtectMessage/UnprotectMessage) Message integrity is invalid
		/// </summary>
		AntiCheatProtectMessageValidationFailed = 12005,
		/// <summary>
		/// (ProtectMessage/UnprotectMessage) Initialization failed
		/// </summary>
		AntiCheatProtectMessageInitializationFailed = 12006,
		/// <summary>
		/// (RegisterPeer) Peer is already registered
		/// </summary>
		AntiCheatPeerAlreadyRegistered = 12007,
		/// <summary>
		/// (UnregisterPeer) Peer does not exist
		/// </summary>
		AntiCheatPeerNotFound = 12008,
		/// <summary>
		/// (ReceiveMessageFromPeer) Invalid call: Peer is not protected
		/// </summary>
		AntiCheatPeerNotProtected = 12009,
		/// <summary>
		/// The DeploymentId provided to the anti-cheat client helper executable does not match what was used to initialize the EOS SDK
		/// </summary>
		AntiCheatClientDeploymentIdMismatch = 12010,
		/// <summary>
		/// EOS Connect DeviceID auth method is not supported for anti-cheat
		/// </summary>
		AntiCheatDeviceIdAuthIsNotSupported = 12011,
		/// <summary>
		/// EOS RTC room cannot accept more participants
		/// </summary>
		TooManyParticipants = 13000,
		/// <summary>
		/// EOS RTC room already exists
		/// </summary>
		RoomAlreadyExists = 13001,
		/// <summary>
		/// The user kicked out from the room
		/// </summary>
		UserKicked = 13002,
		/// <summary>
		/// The user is banned
		/// </summary>
		UserBanned = 13003,
		/// <summary>
		/// EOS RTC room was left successfully
		/// </summary>
		RoomWasLeft = 13004,
		/// <summary>
		/// Connection dropped due to long timeout
		/// </summary>
		ReconnectionTimegateExpired = 13005,
		/// <summary>
		/// EOS RTC room was left due to platform release
		/// </summary>
		ShutdownInvoked = 13006,
		/// <summary>
		/// EOS RTC operation failed because the user is in the local user's block list
		/// </summary>
		UserIsInBlocklist = 13007,
		/// <summary>
		/// The number of available Snapshot IDs have all been exhausted.
		/// </summary>
		ProgressionSnapshotSnapshotIdUnavailable = 14000,
		/// <summary>
		/// The KWS user does not have a parental email associated with the account. The parent account was unlinked or deleted
		/// </summary>
		ParentEmailMissing = 15000,
		/// <summary>
		/// The KWS user is no longer a minor and trying to update the parent email
		/// </summary>
		UserGraduated = 15001,
		/// <summary>
		/// EOS Android VM not stored
		/// </summary>
		AndroidJavaVMNotStored = 17000,
		/// <summary>
		/// Patch required before the user can use the privilege
		/// </summary>
		PermissionRequiredPatchAvailable = 18000,
		/// <summary>
		/// System update required before the user can use the privilege
		/// </summary>
		PermissionRequiredSystemUpdate = 18001,
		/// <summary>
		/// Parental control failure usually
		/// </summary>
		PermissionAgeRestrictionFailure = 18002,
		/// <summary>
		/// Premium Account Subscription required but not available
		/// </summary>
		PermissionAccountTypeFailure = 18003,
		/// <summary>
		/// User restricted from chat
		/// </summary>
		PermissionChatRestriction = 18004,
		/// <summary>
		/// User restricted from User Generated Content
		/// </summary>
		PermissionUGCRestriction = 18005,
		/// <summary>
		/// Online play is restricted
		/// </summary>
		PermissionOnlinePlayRestricted = 18006,
		/// <summary>
		/// The application was not launched through the Bootstrapper. Desktop crossplay functionality is unavailable.
		/// </summary>
		DesktopCrossplayApplicationNotBootstrapped = 19000,
		/// <summary>
		/// The redistributable service is not installed.
		/// </summary>
		DesktopCrossplayServiceNotInstalled = 19001,
		/// <summary>
		/// The desktop crossplay service failed to start.
		/// </summary>
		DesktopCrossplayServiceStartFailed = 19002,
		/// <summary>
		/// The desktop crossplay service is no longer running for an unknown reason.
		/// </summary>
		DesktopCrossplayServiceNotRunning = 19003,
		/// <summary>
		/// An unexpected error that we cannot identify has occurred.
		/// </summary>
		UnexpectedError = 0x7FFFFFFF
	}
}