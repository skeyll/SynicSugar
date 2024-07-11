+++
title = "EOSConnect"
weight = 0
+++
## EOSConnect

### Description
Sign in EOS. This Authentication is via Connect interface instead of Auth Interface. So, the user can't use EOS Epic Account Services.

### Function 
| API | description |
| --- | --- |
| [HasLoggedinEOS](../EOSConnect/hasloggedineos) | User has logged in or not. |
| [LoginWithDeviceID](../EOSConnect/loginwithdeviceid) | Sign in EOS with DeviceID |
| [DeleteDeviceID](../EOSConnect/deletedeviceid) | Delete DeviceID from local |


```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.Login;

public class Login : MonoBehaviour {     
    async UniTaskVoid Start(){
        bool hasLoggedin = await EOSConnect.HasLoggedinEOS();

        if(hasLoggedin){
            return;
        }

        //(bool isSuccess, Result detail)
        var result = await EOSConnect.LoginWithDeviceID();

        if(result == Result.Success){
            // success
            // Transition to MainMenu or so on...
            return;
        }
        Debug.Log($"Fault EOS authentication. {result.detail}");
    }
}
```