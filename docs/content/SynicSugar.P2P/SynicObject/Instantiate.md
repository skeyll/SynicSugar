+++
title = "Instantiate"
weight = 0
+++
## Instantiate
<small>*Namespace: SynicSugar.P2P* <br>
*Class: SynicObject* </small>

public static GameObject Instantiate(UserId id, GameObject original)<br>
public static GameObject Instantiate(UserId id, GameObject original, Transform parent)<br>
public static GameObject Instantiate(UserId id, GameObject original, Transform parent, bool instantiateInWorldSpace)<br>
public static GameObject Instantiate(UserId id, GameObject original, Vector3 position, Quaternion rotation)<br>
public static GameObject Instantiate(UserId id, GameObject original, Vector3 position, Quaternion rotation, Transform parent)


### Description
Generate GameObject, then set UserID to each NetworkPlayer. <br>
If original prefab has many components, this will be heavy.


```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample {
    [SerializeField] GameObject playerPrefab;
    void ObjectSample(){
        SynicObject.Instantiate(userID, playerPrefab);
    }
}
```