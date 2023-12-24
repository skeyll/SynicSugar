+++
title = "FetchFile"
weight = 3
+++
## FetchFile
<small>*Namespace: SynicSugar.TitleStorage*</small>

public static async UniTask&lt;bool&gt; FetchFile(string fileName)


### Description
Exsist target? If not, Download it from EOS server. <br />
After call this, we can load the target as Resources or AssetBundle.


```cs
using System.Collections.Generic;
using SynicSugar.TitleStorage;
using UnityEngine;

public class TitleStorageSample : MonoBehaviour {
    [SerializeField] Text currentProgress;
    async void Start() {
        await FetchFile("TestLogo");
    }
}
```