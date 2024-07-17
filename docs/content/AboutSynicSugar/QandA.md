+++
title = "Q&A"
weight = 3
+++
# Q&A
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
<summary><b>Large Packets in SynicSugar</b></summary>
While the EOS SDK can only send packets up to 1170 bytes, SynicSugar divides packets into 1166-byte segments and attaches its own unique header before transmission. On the receiving end, these segments are stored in Dictionary, organized by CH (Channel). Once all segments of a packet have been received, they are combined into a single byte array and executed as an RPC. The system can send up to 256 packets. If this limit is exceeded, an error is thrown.<br><br>
</details>

<details>
<summary><b>About Mobile Store Reviews</b></summary>
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







