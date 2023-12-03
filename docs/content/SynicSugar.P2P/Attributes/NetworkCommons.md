+++
title = "NetworkCommons"
weight = 1
+++
## NetworkCommons
<small>*Namespace: SynicSugar.P2P* </small>

[AttributeUsage(AttributeTargets.Class,  Inherited = false)]<br>
public sealed class NetworkCommonsAttribute : Attribute


### Description
For around game system.<br>
NetworkCommons has no UserID. Instead, this has **isHost** flag in class. All peers can call a process in this class to synchronize with other peers.<br>
SyncVar in Commons can specify isOnlyHost to synchronize only host values. This manages game time and enemy appearances. It may be better to manage the HP of bosses in CommonsClass than in PlayerClass, which has its each UserId.<br>
Unlike PlayerClass that register Instance to ConnectHub with seteting UserId, NetworkCommons need be registered by hand. We can use *[RegisterInstance](../../SynicSugar.P2P/ConnectHub/registerinstance)* to register the instance.<br><br>
If pass true, ConnectHub has GetUserInstance<T>() for the class.


### Constructor

| API | description |
|---|---|
| NetworkCommons()| useGetInstance's default value is false |
| NetworkCommons(bool useGetInstance) | To use ConnectHub.Instance.GetUserInstance<T>() passes true |


```cs
using SynicSugar.P2P;

[NetworkCommons(false)]
public partial class NetworkSample {
}
```