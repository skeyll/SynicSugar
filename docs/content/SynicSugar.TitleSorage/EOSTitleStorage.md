+++
title = "EOSTitleStorage"
weight = 0
+++

## EOSTitleStorage
<small>*Namespace: SynicSugar.TitleStorage*</small>


### Description
Download and load data from EOS title storage.<br>
***This API is under test and I'm considering what the API should be. So, this will be changed in the future version.**
*If you use this, (I use TitleStorage in this way now) <br>
1. Build a file(.bundle) as AssetBundle.<br>
2. Upload the AssetBundle to EPIC title storage with EncriptionKey in EOS plugin's Client Credentials.<br>
3. Build Game and delete 2 AssetBundle from APPNAME_Data/StreamingAssets/aa/PLATFORM/ASSETBUNDLENAME from build.<br>
4. call LoadFromAssetBundle(Uploaded Path) in runtime.<br>
**In the future, I will implement the proprietary code to build and read as Addressables.**<br>



### Event
| API | description |
|---|---|
| [ProgressInfo](../EOSTitleStorage/transferprogressevent) | Event to display progress on GUI. |

### Function
| API | description |
|---|---|
| [QueryFileList](../EOSTitleStorage/queryfilelist) | Query the file List from backend |
| [FetchFile](../EOSTitleStorage/fetchfile) | When there is not target in local, Download it from EOS server |
| [FetchFiles](../EOSTitleStorage/fetchfiles) | When there is not target in local, Download it from EOS server |
| [DeleteCache](../EOSTitleStorage/deletecache) | Clear previously cached file data |
| [LoadFromAssetBundle](../EOSTitleStorage/loadfromassetbundle) | Load file with Addressable |
| [ReleaseAddressables](../EOSTitleStorage/releaseaddressables) | Destroy all used Addressable resources |



```cs
using SynicSugar.TitleStorage;
using UnityEngine;
using UnityEngine.UI;

public class TitleStorageSample : MonoBehaviour {
    [SerializeField] Text currentProgress;
    [SerializeField] Image logo;
    async void Start() {
        EOSTitleStorage.ProgressInfo.Register(DisplayCurrentProgress);
        logo.sprite = await EOSTitleStorage.LoadFromAssetBundle<Sprite>("TestLogo");
    }
    void OnDestory() {
        EOSTitleStorage.ProgressInfo.Clear();
        EOSTitleStorage.ProgressInfo.ReleaseAddressables();
    }
    public void DisplayCurrentProgress(string currentFileName, float progress){
        currentProgress.text = $"{currentFileName}: {progress}%";
    }
}
```