+++
title = "LoginWithDeviceID"
weight = 1
+++
## LoginWithDeviceID
public static async UniTask<bool> LoginWithDeviceID(CancellationTokenSource token)

### Description
Sign in EOS with DeviceID. If can sign in, return true.

```cs
using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;

public class AuthLogin : MonoBehaviour {
    public async UniTask Signin(){
        // For task
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        //Wait for signin
        bool isSuccess = await EOSAuthentication.LoginWithDeviceID(cancellationToken);

        if(isSuccess){
            // success
            return;
        }
        // failer
    }
}
```

```cs
using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;

public class AuthLogin : MonoBehaviour {
    public async UniTask Signin(){
        // For task
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        //Not waiting
        EOSAuthentication.LoginWithDeviceID(cancellationToken).Forget();

        //Processing without waiting for sign-in
    }
}
```