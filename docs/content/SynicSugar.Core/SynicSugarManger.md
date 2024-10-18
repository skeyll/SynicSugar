+++
title = "SynicSugarManger"
weight = 0
+++
## SynicSugarManger
<small>*Namespace: SynicSugar*</small>

### Description
Modify the backend server and check the status of users.

### Event 
| API | description |
| --- | --- |
| CleanupForEditor | For dev. Invoke this event OnDestory in the editor. |

### Property 
| API | description |
| --- | --- |
| [State](../SynicSugarManger/state) | Check user state |
| [LocalUserId](../SynicSugarManger/LocalUserId) | Check user state |

### Function 
| API | description |
| --- | --- |
| GetFactoryName | Get core name |
| SetCoreFactory | NOT DONE. Change backend and transport  |

```cs
using UnityEngine;
using SynicSugar;

public class FPSManager : MonoBehaviour {     
    private void Start()
    {
        if(!SynicSugarManger.Instance.State.IsLoggedIn){
            Application.targetFrameRate = 60;
        }
    }
}
```