+++
title = "InternalDesign"
weight = 1
+++
Let me introduce you to how SynicSugar uses EOS features to achieve online game.<br><br>

### About EOS
First, EOS offers two types of services: EOS Game Services and EOS Account Services. Game Services can be used just by registering with EOS, but it only provides basic functions necessary for online game development. On the other hand, Account Services requires Epic's review. Moreover, user consent the client policy of your app before login by user Epic account, but it offers more features such as Easy Anti Cheat and friends Interface. <br>
You can check the difference of these services [here](https://dev.epicgames.com/docs/resources-overview).
About agreement, that's [here](https://dev.epicgames.com/en-US/services/terms/agreements)<br>

SynicSugar uses the functions provided by Game Services to enable online connection, and currently, only a part of Game Services is being used. Specifically, SynicSugar currently uses the Connect Interface, Lobby Interface, P2P Interface, and RTC (VC) Interface.<br>
Account Services may be supported in the future, but it will likely be limited to basic features like account linking that doesn't need EOS overlay.<br><br>
Tools for debug login can be downloaded from Developer Portal.<br><br>

### About Login
As mentioned earlier, EOS offers two authentication methods. The Connect Interface allows logging in without user consent or using non-Epic accounts, while the Auth interface requires users to agree to your client policy when logging in.<br>

SynicSugar uses the Connect Interface with Device ID login, enabling users to access EOS as anonymous users. This method employs a unique Device ID issued by the SDK by the Connect Interface. It requires no user consent, and login occurs automatically when the API is called.<br>

The advantage of this approach is that users can start the game immediately without consent or registration. However, this account can be easily lost. On mobile devices, uninstalling the app, or on PC, calling the account delete function will cause the loss of the login key, permanently revoking access to the account.<br>

To address this, I plan to implement a feature in the future that allows linking other login acounts (such as Google or Steam) to the account created with the Device ID. Nevertheless, even after implementing this linking functionality, SynicSugar will continu to use only Device ID login for user experience.<br><br>

### Matchmaking
#### Lobby and Session
There are two methods for Matchmaking: Lobby and Session Interface.<br>

The Lobby Interface can create rooms for up to 64 people. We can set lobby attributes and attributes to each user. The only other information is whether they are Host or Guest. However, Host can kick guests from the lobby and they can use RTC (The max users of an each RTC room is up to 16). Everything about state is managed on EOS backend, so users know only other User's ID and user attributes.<br>

The Session Interface can connect up to 1000 people. In addition to lobby and user attributes, Session can have more states like as "Party" or "Game", and the game state whether the game has started. However, each user has to manage state about the session in the user local. Moreover, for Session, Host IP needs to open to all participants.<br>

Epic probably uses Lobby for the manage the state before the matchmaking, and for actual matchmaking uses Session. This is inferred from the max member limit of RTC. <br><br>

SynicSugar exclusively uses Lobby Interface. This is because SynicSugar targets games for smaller groups. SynicSugar employs a full-mesh network topology, which makes large-scale connections difficult, thus doesn't need matchmaking for more than 64 players.<br>
Ignoring the max matchmaking limit, Lobby Interface offers distinct advantages. It handles RTC, lobby host management, and disconnection notifications on the server side. Moreover, using Lobby enhances security because personal information isn't disclosed until explicitly transitioning to P2P connection.<br><br>

It's worth noting that EOS doesn't strictly delineate these use cases. They offer these two methods to provide more options, so this approach is within that usage.<br><br>

#### Transition from Matchmaking to P2P
While Lobby Interface can't manage game states directly, SynicSugar manages this state using the Lobby presence. When the required number of players is gathered, the lobby is closed by hiding its presence from search results. After this, the lobby is accessible only via Lobby ID or invitation.<br>

Afterwards, Host generates a random string as a socket ID and adds this ID to the lobby attributes. Users in that lobby then connect using this socket ID and their individual User IDs. Users' IP addresses are only disclosed after matchmaking is completely closed. If we use relay for the connection, user IP addresses are never revealed throughout the game.<br>

By completely separating the matchmaking and P2P phases, SynicSugar enables matchmaking with the same attributes and reconnection to disconnected connections. However, this isn't a perfect matchmaking method. New users can't join mid-game, and users can't freely move characters via P2P during matchmaking. (Regarding Voice Chat (VC), it can be manually enabled during matchmaking. By default, it turns on after P2P communication begins.)<br><br>


### P2P Conenction
After matchmaking, actual conenction occurs throught P2P Interface. At this point, Lobby is only used for connect notifications, and dispose almost all notifications about matchmaking.<br><br>

#### Network Topology
SynicSugar adopts a full-mesh topology. This choice is primarily based on the assumption of small-scale games, where bandwidth is less of a concern. Direct connections between users allow for lower latency compared to using relay. Additionally, as all data is shared to all peers, reconnection is simpler.<br>

Full mesh communication makes it easier to detect users who may be cheating, as each user can only handles via their own instance. While game developers cannot directly intervene in P2P, clear identification of cheating players allows for action to be taken outside the game.<br>

#### Preparation
SynicSugar caches most communication essentials before or during matchmaking. This allows zero-allocate conenction in Runtime.<br>

The initial Host creates User ID list from lobby data, which becomes immutable User list for that connection. This list is synchronized to all users before the matchmaking API returns true and control is handed over to the library user. This list remains unique and identical across all local instances and step.<br><br>

#### Synchronization
Actual connections are synchronized by SendPacket at the top of each RPC process and ConvertFromPacket in ConnectHub.<br>

SynicSugar adds necessary network processing at compile-time based on network attributes. When need further modification, SynicSugar uses ILPostProcessor to insert generated processes at the top of existing ones. This allows library users to achieve automatic synchronization by simply applying network attributes and calling processes as usual.<br><br>
Packet receiving is handled by a MonoBehaviour script with a single loop process (like Unity Update) for each timing. It receives data from the buffer a set number of times each frame based on the batch count, then passes this data to ConnectHub to invoke processing. If the buffer is empty, the process waits until the next frame.<br>

Synchronization occurs automatically when an RPC function is called from a local user's instance or when a SyncVar value is changed.<br>

Currently, garbage collection occurs in the sendpacket option specification part of EOS during transmission. However, after the SDK update, basic RPCs are expected to be zero-allocation.<br>

For large packets, while not zero-allocation due to holding packet data in a dictionary before invoking RPC in the receiving local, such situations typically don't require high performance.<br>

Pre-connection settings are stored in the P2PConfig singleton, while post-connection data is held in the P2PInfo singleton.<br><br>

#### Synic
To recreate user data for reconnection, SynicSugar provides Synic. All field variables with the Synic attribute can be synchronized at once by calling SyncSynic(). It has a SynicType argument (OnlySelf, WithTarget, WithOthers), allowing the Host to send data other than its own through SyncSynic. While each user's data is always transmittable by SyncSynic, data from others is automatically discarded unless that user is reconnecting. This allows for safer data retransmission compared to usual methods.<br><br>

#### Disconnect Notification
There are three phases of disconnect notification: P2P Interrupted, P2P Closed, and Lobby Disconnection.<br>

EOS SDK sends small packets to each peer and lobby as regular heartbeats. When the SDK determines a connection loss based on these heartbeats, SynicSugar invokes EarlyDisconnectedNotify as an interruption notification.<br>

More frequent packet sending leads to quicker notifications, but it typically takes about 5 to 20 seconds at most.<br>

After about 10 seconds, a Closed notification is received, indicating that reconnection is no longer possible. Lobby fire disconnected notification 10 seconds later than that Closed notification.<br>

With EarlyDisconnect enabled, immediately after an interruption notification, SynicSugar calls RefreshPing to the disconnected user from several users. The Host then updates Lobby attributes with the disconnected user's index. This ensures all notifications are executed within about 15 seconds.<br>

The recommended settings are:
- Enable UseDisconnectedEarlyNotify
- Set sample pings to 2-3
- Disable AutoRefreshPing
<br>
This configuration allows for the fastest disconnect notification. As Ping (Round-Trip Time) is only a reference value, it's advised to turn off auto-refresh ping.