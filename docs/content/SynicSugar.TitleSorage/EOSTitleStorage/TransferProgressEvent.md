+++
title = "TransferProgressEvent"
weight = 0
+++
## TransferProgressEvent
<small>*Namespace: SynicSugar.TitleStorage*</small>

public TransferProgressEvent ProgressInfo


### Description
Events to display transfer progress on GUI.<br>


### Property
| API | description |
|---|---|
| CurrentFileName | Name of current transfer file |

### Event
| API | description |
|---|---|
| InProgress&lt;string, float&gt; | Called each Fetch |


### Function
| API | description |
|---|---|
| Register | Set events |
| Clear | Clear events |


```cs
using SynicSugar.TitleStorage;
using UnityEngine;
using UnityEngine.UI;

public class TitleStorageSample : MonoBehaviour {
    [SerializeField] Text currentProgress;
    void Start() {
        EOSTitleStorage.ProgressInfo.InProgress += DisplayCurrentProgress;
    }
    void OnDestory() {
        EOSTitleStorage.ProgressInfo.InProgress -= DisplayCurrentProgress;
    }
    public void DisplayCurrentProgress(string currentFileName, float progress){
        currentProgress.text = $"{currentFileName}: {progress}%";
    }
}
```