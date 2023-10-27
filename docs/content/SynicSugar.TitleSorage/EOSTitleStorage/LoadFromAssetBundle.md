+++
title = "LoadFromAssetBundle"
weight = 5
+++
## LoadFromAssetBundle
<small>*Namespace: SynicSugar.TitleStorage*</small>

public static async UniTask&lt;T&gt; LoadFromAssetBundle&lt;T&gt;(string filePath) where T : class


### Description
Load file with Addressable. <br>
When data exists in local, it is just loaded; if not, it is downloaded from Server and then loaded.<br>
These Resource is managed in batches, and need to call ReleaseAddressables() on changing scene.


```cs
using SynicSugar.TitleStorage;
using UnityEngine;
using UnityEngine.UI;

public class TitleStorageSample : MonoBehaviour {
    [SerializeField] Text currentProgress;
    [SerializeField] Image logo;
    async void Start() {
        logo.sprite = await EOSTitleStorage.LoadFromAssetBundle<Sprite>("TestLogo");
    }
    void OnDestory() {
        EOSTitleStorage.ReleaseAddressables();
    }
}
```