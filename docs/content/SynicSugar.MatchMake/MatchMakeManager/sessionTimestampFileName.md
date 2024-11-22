+++
title = "sessionTimestampFileName"
weight = 1
+++
## sessionTimestampFileName
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public string sessionTimestampFileName = "ss_sessiondata";




### Description
This is the file name to save the session start time. It is stored in Application.persistentDataPath/**sessionTimestampFileName**+.dat at the end of matchmaking. <br>
This file is not always necessary when reconnection, but having this file improves the accuracy of the sessionTime for the timestamp.<br>
This option is intended for use when multiple applications are launched on the same terminal.<br><br>

Save the following file. If this file does not exist locally on reconnection, the sessionTimestamp is calculated based on the Session's elapsed time sent by the Host.

```cs
using System;
using MemoryPack;
namespace SynicSugar.P2P {
    [MemoryPackable]
    public partial class SessionData
    {
        public string LobbyID { get; set; }
        public DateTime SessionStartTimestamp { get; set; }
        
        [MemoryPackConstructor]
        public SessionData(string lobbyId, DateTime sessionStartTimestamp)
        {
            LobbyID = lobbyId;
            SessionStartTimestamp = sessionStartTimestamp;
        }
    }
}
```


```cs
using UnityEngine;
using SynicSugar.MatchMake;

public class Matchmake : MonoBehaviour {
    private void Start(){
        string localuserName = GetLocalUserName();
        MatchMakeManager.Instance.sessionTimestampFileName = localuserName + "_data";
    }
    ///Processes that return a unique name
    private string GetLocalUserName(){
        string username;
        ///...
        return username; 
    }
}
```