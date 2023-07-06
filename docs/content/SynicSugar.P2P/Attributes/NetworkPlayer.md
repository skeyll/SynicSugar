+++
title = "NetworkPlayer"
weight = 0
+++
## NetworkPlayer
<small>*Namespace: SynicSugar.P2P* </small>

[AttributeUsage(AttributeTargets.Class, Inherited = false)]<br>
public sealed class NetworkPlayerAttribute : Attribute


### Description
Each instance has a unique UserID of EOS. The process of the owner of this instance has is synchronized with other peers.<br>
If pass true, *ConnectHub* has GetUserInstance<T>() for the class.


### Constructor

| API | description |
|---|---|
| NetworkPlayer()| useGetInstance's default value is false |
| NetworkPlayer(bool useGetInstance) | To use ConnectHub.Instance.GetUserInstance<T>(UserID TargetID) passes true |


```cs
using SynicSugar.P2P;

[NetworkPlayer(true)]
public partial class NetworkSample {
}
```