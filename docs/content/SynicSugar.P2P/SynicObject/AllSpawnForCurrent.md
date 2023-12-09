+++
title = "AllSpawnForCurrent"
weight = 1
+++
## AllSpawnForCurrent
<small>*Namespace: SynicSugar.P2P* <br>
*Class: SynicObject* </small>

public static List<GameObject> AllSpawnForCurrent(GameObject original)<br>
public static List<GameObject> AllSpawnForCurrent(GameObject original, Transform parent)<br>
public static List<GameObject> AllSpawnForCurrent(GameObject original, Transform parent, bool instantiateInWorldSpace)<br>
public static List<GameObject> AllSpawnForCurrent(GameObject original, Vector3 position, Quaternion rotation)<br>
public static List<GameObject> AllSpawnForCurrent(GameObject original, Vector3 position, Quaternion rotation, Transform parent)


### Description
Generate GameObjects, then set UserIDs of all members in current session to each NetworkPlayer.<br>
If original prefab has many components, this is heavy.


```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample {
    [SerializeField] GameObject playerPrefab;
    void ObjectSample(){
        SynicObject.AllSpawnForCurrent(playerPrefab);
    }
}
```