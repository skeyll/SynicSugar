+++
title = "DeleteCache"
weight = 4
+++
## FetchFile
<small>*Namespace: SynicSugar.TitleStorage*</small>

public static async UniTask&lt;bool&gt; DeleteCache()


### Description
Clear previously cached file data.<br>


```cs
using SynicSugar.TitleStorage;
using UnityEngine;

public class TitleStorageSample : MonoBehaviour {
    async void Start() {
        await EOSTitleStorage.ProgressInfo.DeleteCache();
    }
}
```