+++
title = "HasLoggedinEOSWithConnect"
weight = 0
+++

## HasLoggedinEOSWithConnect
<small>*Namespace: SynicSugar.Auth* <br>
*Class: EOSAuthentication* </small>

public static bool HasLoggedinEOSWithConnect()


### Description
Whether this user has currently logged in EOS.

```cs
using UnityEngine;
using SynicSugar.Auth;

public class AuthLogin : MonoBehaviour {
    void Start(){
        bool hasLoggedin = await EOSAuthentication.HasLoggedinEOSWithConnect();

        if(!hasLoggedin){
            // login EOS
            return;
        }
    }
}
```