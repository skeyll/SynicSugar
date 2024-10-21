+++
title = "SynicSugarAuthentication"
weight = 0
+++
## SynicSugarAuthentication

### Description
Sign in SynicSugar. <br>
Default: This Authentication is via Connect interface instead of Auth Interface. So, the user can't use EOS Epic Account Services.

### Function 
| API | description |
| --- | --- |
| [Login](../SynicSugarAuthentication/loginwithdeviceid) | Sign in EOS with DeviceID |


```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar;
using SynicSugar.Auth;

public class Login : MonoBehaviour {     
    async UniTaskVoid Start(){

        if(SynicSugarManger.Instance.State.IsLoggedIn){
            return;
        }

        //(bool isSuccess, Result detail)
        var result = await EOSConnect.Login();

        if(result == Result.Success){
            // success
            // Transition to MainMenu or so on...
            return;
        }
        Debug.Log($"Fault EOS authentication. {result.detail}");
    }
}
```