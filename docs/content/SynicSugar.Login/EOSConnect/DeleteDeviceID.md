+++
title = "DeleteDeviceID"
weight = 2
+++

## DeleteDeviceID
<small>*Namespace: SynicSugar.Login* <br>
*Class: EOSConnect* </small>

public static async UniTask&lt;Result&gt;  DeleteDeviceID(CancellationToken token = default(CancellationToken))

### Description
This doesn't mean Delete an account from EOS. Just delete data from local. We can call this after calling LoginWithDeviceID.<br>
Delete any existing Device ID access credentials for the current user profile on the local device. The deletion is permanent and it is not possible to recover lost game data and progression if the Device ID had not been linked with at least one real external user account. On Android and iOS devices, uninstalling the application will automatically delete any local Device ID credentials created by the application. On Desktop platforms (Linux, macOS, Windows), Device ID credentials are not automatically deleted. <br>

```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.Login;

public class Login : MonoBehaviour {     
    async UniTaskVoid Start(){
        Result result = await EOSConnect.DeleteDeviceID();

        if(result == Result.Success){
            // success
            return;
        }
        Debug.Log($"Fault on delete. {result}");
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
            Result result = await EOSConnect.DeleteDeviceID(cts.Token);

            if(result == Result.Success){
                // success
                return;
            }
            Debug.Log($"Fault on delete. {result}");
        }catch(OperationCanceledException){
            Debug.Log("Canceled by user");
        }
    }
}
```