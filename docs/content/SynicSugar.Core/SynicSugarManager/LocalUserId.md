+++
title = "LocalUserId"
weight = 1
+++

## LocalUserId
<small>*Namespace: SynicSugar* <br>
*Class: SynicSugarManager* </small>

public UserId LocalUserId;

### Description
The UserId for LocalUser.<br>
The user is an OFFLINEUSER until the user actually log in, after which the user is assigned an ID.<br>
Default's UserId is based on EOS's Product User ID.<br>

```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar;
using SynicSugar.Auth;

public class Login : MonoBehaviour {     
    async UniTaskVoid Start(){
        Debug.Log(SynicSugarManger.Instance.LocalUserId); // = "OFFLINEUSER"


        var result = await EOSConnect.Login();

        if(result == Result.Success){
            Debug.Log(SynicSugarManger.Instance.LocalUserId); // By default, a unique 32-character string assigned to each user by EOS.
            return;
        }

        Debug.Log(SynicSugarManger.Instance.LocalUserId); // = "OFFLINEUSER"
    }
}
```