+++
title = "AllSpawn"
weight = 1
+++
## AllSpawn
<small>*Namespace: SynicSugar.P2P* <br>
*Class: SynicObject* </small>

public static List<GameObject> AllSpawn(GameObject original)
public static List<GameObject> AllSpawn(GameObject original, Transform parent)
public static List<GameObject> AllSpawn(GameObject original, Transform parent, bool instantiateInWorldSpace)
public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation)
public static List<GameObject> AllSpawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent)


### Description
Generate GameObjects, then set UserIDs of all members in Lobby to each NetworkPlayer.<br>
If original prefab has many components, this will be heavy.


```cs
using SynicSugar.P2P;

public class p2pSample {
    [SerializeField] GameObject playerPrefab;
    void ObjectSample(){
        SynicObject.AllSpawn(playerPrefab);
    }
}
```