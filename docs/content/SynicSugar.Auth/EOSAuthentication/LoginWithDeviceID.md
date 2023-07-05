+++
title = "LoginWithDeviceID"
weight = 0
+++

## LoginWithDeviceID
<small>*Namespace: SynicSugar.Auth* <br>
*Class: EOSAuthentication* </small>

public static async UniTask&lt;bool&gt; LoginWithDeviceID(CancellationTokenSource token)<br>
public static async UniTask&lt;string&gt; LoginWithDeviceID(CancellationTokenSource token, bool needResult)


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
        //Return "Success" or the others.
        // string isSuccess = await EOSAuthentication.LoginWithDeviceID(cancellationToken, true);

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
    public void Signin(){
        // For task
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        //Not waiting
        EOSAuthentication.LoginWithDeviceID(cancellationToken).Forget();

        //Processing without waiting for sign-in
    }
}
```