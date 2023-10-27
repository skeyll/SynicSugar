+++
title = "QueryFileList"
weight = 2
+++
## QueryFileList
<small>*Namespace: SynicSugar.TitleStorage*</small>

public static async UniTask&lt;List&lt;string&gt;&gt; QueryFileList(string[] tags, CancellationToken token = default(CancellationToken))<br>
public static async UniTask&lt;List&lt;string&gt;&gt; QueryFileList(List&lt;string&gt; tags, CancellationToken token = default(CancellationToken))


### Description
Query the file List from backend. Hold FileSizeBytes to read file.<br>
When we know the filename to want to get, can also call ReadFile not to call this.


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
    }
}
```