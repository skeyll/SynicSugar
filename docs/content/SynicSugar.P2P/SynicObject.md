+++
title = "SynicObject"
weight = 2
+++

## SynicObject
<small>*Namespace: SynicSugar.P2P*</small>


### Description
Generate object with UserID.


### Properity
| API | description |
|---|---|
| [Instantiate](../SynicObject/instantiate) | Generate a object for UserID |
| [AllSpawn](../SynicObject/allspawn) | Generate objects for all users(include disconencted user) |
| [AllSpawnForCurrent](../SynicObject/allspawnforcurrent) | Generate objects for all current Users in Lobby |


```cs
using SynicSugar.P2P;

public class p2pSample {
    [SerializeField] GameObject playerPrefab;
    void ObjectSample(){
        SynicObject.Instantiate(userID, playerPrefab);
        SynicObject.AllSpawn(playerPrefab);
    }
}
```