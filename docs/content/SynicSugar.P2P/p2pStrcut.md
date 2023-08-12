+++
title = "p2pStruct"
weight = 6
+++

## p2pStruct
<small>*Namespace: SynicSugar.P2P*</small>


### Description
**The objects is used internally.**

### LargePacketInfomation
The object to rebuild packets of SyncSynic.

#### Properity
| API | description |
|---|---|
| chunk | Number of divided Packets |
| phase | Synic phases to be synchronized |
| syncSinglePhase | If true, sync a specific phase |
| currentSize | The packets size to have received now |


### SynicContainer
The object to sync synic data to target.

#### Properity
| API | description |
|---|---|
| SynicItemN | phase N values |
