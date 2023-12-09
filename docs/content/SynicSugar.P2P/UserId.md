+++
title = "UserId"
weight = 3
+++

## UserId
<small>*Namespace: SynicSugar.P2P*</small>


### Description
Object for UserID to mediate between SynicSugar and EOSSDK.<br>
This caches all Ids for the duration of a one session.


### Function 
| API | description |
|---|---|
| AsEpic | Convert SynicSugar UserID to EOS's UserID |

### static Function 
| API | description |
|---|---|
| GetUserId(ProductUserId id) | Instantiate UserID from EOS's UserID or return cache |
| GetUserId(UserId id) | Return id cache or null |
| GetUserId(string id) | Return id cache or null |


```cs
using SynicSugar.P2P;

public class p2pSample {
    UserID id;
    void UserIDsSample(){
        Epic.OnlineServices.ProductUserId epicId = p2pConfig.Instance.userIds.LocalUserId.AsEpic;
        UserId id = UserId.GetUserId(epicId);
    }
}
```