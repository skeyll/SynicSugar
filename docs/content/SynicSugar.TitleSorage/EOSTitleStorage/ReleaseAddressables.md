+++
title = "ReleaseAddressables"
weight = 6
+++
## ReleaseAddressables
<small>*Namespace: SynicSugar.TitleStorage*</small>

public static void ReleaseAddressables()


### Description
Release all used resources. Call before transition to the next scene.


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