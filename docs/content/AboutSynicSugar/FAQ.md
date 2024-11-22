+++
title = "FAQ"
weight = 3
+++
# FAQ
<details>
<summary><b>Is there a CCU (Concurrent User) limit?</b></summary>
Probably not. As long as you're not using it in extreme use, it should be fine.<br>

[Service Usage Limitations](https://dev.epicgames.com/docs/epic-online-services/eos-get-started/working-with-the-eos-sdk/conventions-and-limitations#service-usage-limitations)

</details>

<details>
<summary><b>Where can I find the DevTool for DebugLogin?</b></summary>
It's included in the SDK, which can be downloaded from the "Download and configure EOS SDK" section after creating a product page in the Developer portal.<br>
Register test accounts in the Organization section of the Devportal, then launch the Devtool. You need to log in with each account and keep the Tool open in the background during testing.<br>
The DeviceID is the same for both the Editor and built versions, so use DebugDeviceID + DevLogin or similar for debugging.<br><br>
</details>

<details>
<summary><b>What does SourceGenerator do?</b></summary>
It adds classes based on attributes at compile time. The example is such this.

[GenaratedSampleNetworkplayer](https://github.com/skeyll/SynicSugar/blob/main/Resources/GenaratedSampleNetworkplayer.cs).

</details>

<details>
<summary><b>About Transport of P2P.</b></summary>
EOS uses DTLS (Datagram Transport Layer Security), which is an encrypted version of UDP packets. In addition to this, the EOS SDK has an internal mechanism to resend packets based on factors such as order and reliability. When reliability is enabled, packets are generally guaranteed to be delivered successfully.<br><br>
</details>


<details>
<summary><b>Can the transport of p2p be changed?</b></summary>
Currently, it cannot be changed. It's only EOS (Epic Online Services).<br>
However,  SyicSugar will make it possible to change the backend server and the transport by modifying the Core before logging into the server. This feature is not implemented in v0.8.1.<br><br>
</details>

<details>
<summary><b>Why shouldn't user attributes in Lobby be used as user information for P2P?</b></summary>
There are two reasons. First, the Lobby has a lag of up to 5 seconds before information is reflected. Second, due to this lag, I decided to use the user attributes as a heartbeat for disconnections in SynicSugar now.<br>
In EOS, there can be a lag of up to 30 seconds in recognizing a disconnection. This is because the SDK's heartbeat for recognizing EOS's own connection depends on user actions.
While the lag is often shorter when communication is frequent, if there's no activity, the SDK checks for disconnections at specified intervals. This typically takes about twice as long as the time it takes for P2P disconnections to be notified.<br>
Therefore, when there's an anomaly in connection, I decided to take action to check the connection status. I chose to use the Lobby's user attributes, which already had a lag in data reflection, were not originally recommended for use, and only provided a method for updating during matchmaking.<br>
SynicSugar has shortened the time to check communication status by having the Lobby host and the top two users in the user list update the P2P communication user list. During this process, the user information is overwritten with data used for the heartbeat.<br>
As a result, it has become practically impossible to use the Lobby's user attributes for these purpose.<br><br>
</details>

<details>
<summary><b>How is reconnection implemented in SynicSugar?</b></summary>
Reconnection is achieved by saving the LobbyID locally or in a specified location, and then searching for the Lobby using that LobbyID.<br>
In SynicSugar, once matchmaking is completed, the Lobby is closed and hidden from the search. At this point, the lobby can only be searched using the LobbyID or through an invitation from users already in the Lobby. When matchmaking is completed, the LobbyID and the session start time are saved. These details are then used to log into the closed lobby.<br>
Typically, locally saved data is used for the session start time. However, when the Host synchronizes UserIDs or disconnected users within the Lobby, it simultaneously sends a timestamp (uint) of the elapsed time. If there is no local data regarding the session start time, an estimated start time is calculated using this timestamp. Using this, it's also possible to save the LobbyID in the cloud and join that Lobby from a different device.<br><br>
</details>

<details>
<summary><b>How many P2P connections can be made with SynicSugar?</b></summary>
Up to 64 players are possible, but 16 or fewer is recommended.<br>
In SynicSugar, after closing matchmaking, the host synchronizes the user list to sync information about Lobby users. (The user index may differ locally for each user when retrieved from the Lobby information.)<br>
During this process, three things are synchronized: a list of UserIDs (32 characters * number of users), a byte list of disconnected user indexes, and a timestamp expressed as a uint. These are compressed using MemoryPack and Brotli compression set to Fastest mode before transmission via sendpacket(packet limit is 1170). This is to keep the matchmaking time as short as possible.<br>
As a result, depending on the UserIDs, it may not be possible to start connection.  It will likely succeed with up to 45 users by default compression quality.<br><br>
</details>

<details>
<summary><b>Large Packets in SynicSugar.</b></summary>
While the EOS SDK can only send packets up to 1170 bytes, SynicSugar divides packets into 1166-byte segments and attaches its own unique header before transmission. On the receiving end, these segments are stored in Dictionary, organized by CH (Channel). Once all segments of a packet have been received, they are combined into a single byte array and executed as an RPC. The system can send up to 256 packets. If this limit is exceeded, an error is thrown.<br><br>
</details>

<details>
<summary><b>Is it necessary to differentiate between large packets and normal packets in RPC?</b></summary>
Yes. If we don't use large packets, we can't send packets exceeding 1170 bytes due to EOS SDK limitations. We might think that all packets should be large packets, but in reality, we should usually send a packet as standard RPC for the performance.<br>
When large packets are enabled, SynicSugar adds a header and internally sends packets in ReliableOrdered. On the receiving end, packets are stored in a dictionary, and when all necessary packets for reconstruction are received, the original data is deserialized and the RPC is triggered.<br>
The reason for using Reliable transmission is that if some packets can't be sent, a large number of packets would be wasted. Also, if we don't use Ordered, data could become corrupted if packets from the next RPC are received before the current one is complete. That's why these settings are used.<br>
On the other hand, standard packets are sent with just the channel and payload, without additional headers. The packet reliability is determined by the method specified in the P2P config. This means no extra processing is done.<br>
In the EOS SDK, reliably ordered packets are sent and verified by the SDK internally, even if the client hasn't sent packets for an extended period. This should also be kept in mind when considering performance.<br>
Therefore, I recommend using them appropriately based on your needs.<br><br>
</details>

<details>
<summary><b>Can not sync Class or struct specified for Synic.</b></summary>
Synic is converted to a string in JsonUtility before being serialized into bytes, which are then grouped into SyncedItems in ConnectHub. Therefore, it must be serializable in JsonUtility. Synic class needs [System.Serializable] attribute.<br><br>
</details>

<details>
<summary><b>About Mobile Store Reviews.</b></summary>
Mobile store reviews (for both Android and iOS) can be passed even using UDP alone. In the notes section of your review application, please mention that UDP is used and that online features may not function in the review environment. If you're concerned, it might be a good idea to add a tutorial using an SynicSugar Offline Mode.<br><br>
</details>

<details>
<summary><b>What entitlement dose need for macOS notarization?</b></summary>
When using VC (Voice Chat) and network features, add the following permissions and notarize the app:

```YOURGAME.entitlements
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.cs.disable-library-validation</key>
    <true/>
    <key>com.apple.security.cs.disable-executable-page-protection</key>
    <true/>
    <key>com.apple.security.app-sandbox</key>
    <true/>
    <key>com.apple.security.files.user-selected.read-only</key>
    <true/>
    <key>com.apple.security.cs.allow-unsigned-executable-memory</key>
    <true/>
    <key>com.apple.security.device.audio-input</key>
    <true/>
    <key>com.apple.security.device.microphone</key>
    <true/>
    <key>com.apple.security.network.client</key>
    <true/>
    <key>com.apple.security.network.server</key>
    <true/>
</dict>
</plist>
```

Even if the app passes notarization with these permissions, there will be a one-time confirmation prompt when the app launches, asking for permission to access the DeviceID key from the Keychain.
</details>







