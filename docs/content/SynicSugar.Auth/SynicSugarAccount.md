+++
title = "SynicSugarAccount"
weight = 0
+++
## SynicSugarAccount
<small>*Namespace: SynicSugar.Auth*</small>

### Description
Manage account to sing in.

### Function 
| API | description |
| --- | --- |
| [DeleteAccount](../EOSConnect/deletedeviceid) | Delete Account for sing in |


```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.Auth;

public class Login : MonoBehaviour {     
    public async UniTask DeleteDeviceIDRequest()
    {
        var result = await SynicSugarAccount.DeleteAccount();
        if(result == Result.Success)
        {
            SynicSugarDebug.Instance.Log("Delete DeviceID: Success.");
            return;
        }
        SynicSugarDebug.Instance.Log("Delete DeviceID: Failare.");
    }
}
```