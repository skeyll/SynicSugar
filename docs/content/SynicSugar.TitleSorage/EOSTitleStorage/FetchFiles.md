+++
title = "FetchFiles"
weight = 3
+++
## FetchFile
<small>*Namespace: SynicSugar.TitleStorage*</small>

public static async UniTask&lt;bool&gt; FetchFiles(string[] fileNames)


### Description
Exsist targets? If not, Download them from EOS server. <br>
After call this, we can load the target as Resources or AssetBundle.


```cs
using System.Collections.Generic;
using SynicSugar.TitleStorage;
using UnityEngine;

public class TitleStorageSample : MonoBehaviour {
    [SerializeField] Text currentProgress;
    async void Start() {
        List<string> tags = new();
        tags.Add("audio");
        tags.Add("image");

        var fileList = await QueryFileList(tags);
        await FetchFiles(fileList.ToArray());
    }
}
```