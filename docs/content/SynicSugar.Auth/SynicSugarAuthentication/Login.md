+++
title = "Login"
weight = 0
+++

## Login
<small>*Namespace: SynicSugar.Auth* <br>
*Class: SynicSugarAuthentication* </small>

public static async UniTask&lt;Result&gt; Login(CancellationToken token = default(CancellationToken))
public static async UniTask&lt;Result&gt; Login(string displayName, CancellationToken token = default(CancellationToken))


### Description
Sign in SynicSugar.　This API always logs in using an anonymous method such as DeviceID. <br>

Log in to EOS by default. After a successful connection, the login token is managed on the SynicSugar side. Please check SynicSugarManger.Instance.State.IsLoggedIn to see if the connection is in progress


```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.Login;

public class Login : MonoBehaviour {     
    async UniTaskVoid Start(){
        Result result = await SynicSugarAuthentication.Login();

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
            Result result = await SynicSugarAuthentication.Login(cts.Token);

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