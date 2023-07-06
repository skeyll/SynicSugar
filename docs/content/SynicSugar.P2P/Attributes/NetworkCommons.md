+++
title = "NetworkCommons"
weight = 1
+++
## NetworkCommons
<small>*Namespace: SynicSugar.P2P* </small>

[AttributeUsage(AttributeTargets.Class,  Inherited = false)]<br>
public sealed class NetworkCommonsAttribute : Attribute


### Description
NetworkCommons has no UserID. All peers can call a process in this class to synchronize with other peers.<br>
If pass true, *ConnectHub* has GetUserInstance<T>() for the class.


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