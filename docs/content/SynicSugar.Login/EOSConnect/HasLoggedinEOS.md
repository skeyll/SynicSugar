+++
title = "HasLoggedinEOSWithConnect"
weight = 0
+++

## HasLoggedinEOS
<small>*Namespace: SynicSugar.Login* <br>
*Class: EOSConnect* </small>

public static bool HasLoggedinEOS()


### Description
This user has currently logged in EOS or not.

```cs
using UnityEngine;
using SynicSugar.Login;

public class Login : MonoBehaviour {
    void Start(){
        bool hasLoggedin = await EOSConnect.HasLoggedinEOS();

        if(!hasLoggedin){
            // login EOS
            return;
        }
    }
}
```