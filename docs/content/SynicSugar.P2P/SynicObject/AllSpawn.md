+++
title = "AllSpawn"
weight = 1
+++
## AllSpawn
<small>*Namespace: SynicSugar.P2P* <br>
*Class: SynicObject* </small>

public static List<GameObject> AllSpawn(GameObject original)<br>
public static List<GameObject> AllSpawn(GameObject original, Transform parent)<br>
public static List<GameObject> AllSpawn(GameObject original, Transform parent, bool instantiateInWorldSpace)<br>
public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation)<br>
public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent)


### Description
Generate GameObjects, then set UserIDs of all members in Lobby to each NetworkPlayer. (include disconnected users)<br>
If original prefab has many components, this will be heavy.


```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample {
    [SerializeField] GameObject playerPrefab;
    void ObjectSample(){
        SynicObject.AllSpawn(playerPrefab);
    }
}
```