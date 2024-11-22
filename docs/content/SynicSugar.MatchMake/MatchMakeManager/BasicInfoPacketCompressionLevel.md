+++
title = "BasicInfoPacketCompressionLevel"
weight = 1
+++
## BasicInfoPacketCompressionLevel
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

```cs
[SerializeField, Range(1, 11)]
int _basicInfoPacketCompressionLevel;

public int BasicInfoPacketCompressionLevel
{ 
    get { return _basicInfoPacketCompressionLevel; }
    set { _basicInfoPacketCompressionLevel = Mathf.Clamp(value, 1, 11); }
}
```


### Description
Specify the compression level for BasicInfo packet containing the user list and disconnected user's indices. This variable sets the compression degree for the basic information packet. The packet is sent just once before matching and includes a list of UserIDs as 32-character strings for up to 64 users, a byte list representing the indices of disconnected users, and a timestamp represented as a uint. <br><br>

The data size before serialization with MemoryPack reaches 2308 bytes for 64 users and 64 disconnected users. Even with about 35 users, it may exceed 1170 bytes; therefore, it is compressed further with BrotliCompressor.<br><br>

The default compression level of 1 prioritizes data transmission speed and allows for stable transmission for approximately 45 users. While the BrotliCompressor can be set to a maximum level of 11, the compression rate of strings tends to be lower than expected due to the prefix structure of UserIDs, meaning that even at maximum level, it is possible that 64 users' data cannot be sent. This packet is essential for matching success; if synchronization fails, matchmaking will not succeed. Nonetheless, it is designed to be sent as a standard packet to prioritize performance in typical usage instead of LargePacket.<br><br>

SynicSugar adopts a full-mesh topology in pursuit of reconnecting and host migration features, as well as the low-latency of individual connection. When considering a large number of users in a full mesh, there are limitations on maintaining frame rates and the comfort of gameplay.<br><br>

Therefore, while there is a possibility that this setting may change to transmission via LargePacket in the future, this likelihood is low based on the library's concept.


```cs
using UnityEngine;
using SynicSugar.MatchMake;

public class Matchmake : MonoBehaviour {
    private void Start(){
        MatchMakeManager.Instance.BasicInfoPacketCompressionLevel = 11;
    }
}
```