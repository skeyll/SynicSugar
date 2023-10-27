+++
title = "Overview"
weight = 0
+++
### Overview
SynicSugar is  high-level networking library for Unity.<br>
In addition to RPC, SynicSugar includes developer-friendly features like matchmaking and host-migration. <br>
The backend server is EOS, which is available for free of charge. EOS has no CCU limit if used for online games. In other word, all we need for online game development with SynicSugar is just on registring EOS.<br>

In SynicSugar, a user sign-in EOS with DeviceID. SynicSugar isn't required epic brand review, but we cannot use EAS (Epic Account Services). EAS includes features like authentication with store accounts, an account cross-platform and friends. In short, SynicSugar adopts EOS only for multi-play.<br>

The transport is DTLS (Datagram Transport Layer Security), which operates over UDP. The packets are encrypted, and optionally, can have the reliability and the ordered.<br>

##### Note
UDP is favored in many online games due to its speed and packet size.In fact, Mirror also adopts UDP as default transport. However, we should acknowledge of certain problems of UDP. When using UDP, we cannot build for WebGL: browsers operate over TCP. In addition to it, when iOS app's initial review for release, it will be rejected: the review's done in a TCP environment. (We need to request the review over UDP to pass it.) In this view, SynicSugar is also exploring TCP option in future just to solve these problems.<br><br>

### Why SynicSugar is Developed?
SynicSugar has been developed for my game. On that time, Mirror had already gotten stable, and various other network services existed. However, there was still no definitive solution for the kind of small-scale game and mobile-game.<br>

My first choice was Mirror with EOS relay. This approach similar to SynicSugar. Mirror was designed for MMOs, normally using a dedicated server. Even with EOS as the transport, this structure is the same. In other words, Mirror centers connections around the host. This topology doesn't suite my game.<br>

My game is 1v1 pvp (mobile) game. A client might crash or a user could get frustrated and away from keyboard. However, I didn't intend to use a server for a battle and want to adopt P2P. In Mirror, it is difficult to deal with such situations. Mirror's Host is always a server that dosen't throw away the game. (Even if use client-host on p2p, it remains hard.)<br>

Another is Photon. Photon provides netcodes and cloud servers for it. We can get server and supports, but of course, need the charge to use. If we don't need Support, the same can be achieved with Mirror + EOS relay for free. PhotonFusion, the newest one, offers host migration and high-quality synchronization on tick rate, but I don’t need such advanced for my game.<br>
Could a (simple) synchronization system be implemented with EOS for small party and mobile games? This is reason to develop SynicSugar. The concept is a network library for games that are difficult to develop in Mirror.<br>

I start this project as "EOS Sample" only to test EOS SDK for my game. So, some APIs was for my game. Other many ones also has been added as general function to SynicSugar instead of implementing just for my game. I will continue to develop SynicSugar on this base principle. In other words, the library isn't for paper but for actual games. Furthermore, all of these APIs are tested with my games. About optimization, SynicSugar is also improved on actual results. Of course,  SynicSugar will adopt excellent functions. However, I don't have another idea except for my game genre. If you possess good thoughts, please help the development. The more detail is in project.<br><br>


### Support on Unity
SynicSugar support Unity 21.3 and later, which adopts Roslyn Source Generator. SynicSugar use SourceGenerator and IL PostProcesser to generate codes for network process. SynicSugar will not support older Unity. In addition to it, we have to build  with IL2CPP a game that uses SynicSugar for some depend libraries. Currently, SynicSugar officially supports games developed with the traditional MonoBehaviour/Component. For ECS, it isn't yet.<br>

‥‥