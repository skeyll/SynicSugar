+++
title = "OwnerUserID"
weight = 1
+++
## isLocal
<small>*Namespace: SynicSugar.P2P*</small>

public UserId OwnerUserID


### Description
UserID of the owner of the instance.<br>
For example, when a user attacks others, get this UserID and 

*In future, the Setter Property is changed to Init.*

```cs
using SynicSugar.P2P;
using UnityEngine;
using MemoryPack;

[NetworkPlayer]
public partial class p2pSample {
    void OnCollisionEnter(Collision collision){
        //MEMO:
        //     I plan to create API for such getting the id way.
        //     Please teach me the good idea about this.
        UserId enemyID = collision.gameObject.GetComponent<p2pSample>().OwnerUserID;
        AttackEnemy(value);
    }
    [Rpc]
    public void AttackEnemy(Attack value){
        //
    }
}

[MemoryPackable]
public partial class Attack {
    public UserId id;
    public int damage;
}
```