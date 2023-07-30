+++
title = "UserId"
weight = 3
+++

## UserId
<small>*Namespace: SynicSugar.P2P*</small>


### Description
ValueObject for UserID to mediate between SynicSugar and EOSSDK.


### Constructor
| API | description |
|---|---|
| UserId(ProductUserId id) | Generate UserID with EOS's UserID |
| UserId(UserId id) | Generate UserID with this type's UserID |
| UserId(string idString) | Generate UserID with string UserID |


### Function 
| API | description |
|---|---|
| AsEpic | Convert SynicSugar UserID to EOS's UserID |


```cs
using SynicSugar.P2P;

public class p2pSample {
    UserID id;
    void UserIDsSample(){
        Epic.OnlineServices.ProductUserId epicId = p2pConfig.Instance.userIds.LocalUserId.AsEpic;
    }
}
```