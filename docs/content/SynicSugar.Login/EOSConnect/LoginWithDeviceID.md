+++
title = "LoginWithDeviceID"
weight = 1
+++

## LoginWithDeviceID
<small>*Namespace: SynicSugar.Login* <br>
*Class: EOSConnect* </small>

public static async UniTask&lt;Result&gt;  LoginWithDeviceID(CancellationToken token = default(CancellationToken))
public static async UniTask&lt;Result&gt;  LoginWithDeviceID(string displayName, CancellationToken token = default(CancellationToken))


### Description
Sign in EOS with DeviceID. If can sign in, return true.

```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.Login;

public class Login : MonoBehaviour {     
    async UniTaskVoid Start(){
        Result result = await EOSConnect.LoginWithDeviceID();

        if(result == Result.Success){
            // success
            return;
        }
        Debug.Log($"Fault EOS authentication. {result.detail}");
    }
}
```

```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using SynicSugar.Login;

public class Login : MonoBehaviour {
    async UniTaskVoid Start(){
        CancellationTokenSource cts = new CancellationTokenSource();
        try{
            Result result = await EOSConnect.LoginWithDeviceID(cts.Token);

            if(result == Result.Success){
                // success
                return;
            }
            Debug.Log($"Fault EOS authentication. {result}");
        }catch(OperationCanceledException){
            Debug.Log("Canceled by user");
        }
    }
}
```