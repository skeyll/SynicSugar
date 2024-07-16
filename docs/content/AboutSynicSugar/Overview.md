+++
title = "Overview"
weight = 0
+++
### What is SynicSugar?
SynicSugar is a network library for Unity that uses EOS (Epic Online Services) as a backend server. It communicates through a full mesh topology using a P2P interface.<br>

While primarily a network library, SynicSugar aims to support a full range of features necessary for online game development, from EOS login to title storage usage. Future plans may include support for custom servers, but this is yet to be determined.<br>

The concept is to provide the easiest method of online game development for everyone. SynicSugar is the syntax sugar that implements sync at sonic speed!<br><br>

### Why SynicSugar is Developed?
SynicSugar was initially developed for my own game. At the time, while stable solutions like Mirror existed and various other network services were available, there was no definitive solution for small-scale and mobile games.<br>

I first considered Mirror with EOS relay, an approach similar to SynicSugar. However, Mirror, designed for MMOs with dedicated servers, centers connections around a host's dedicated server. This topology didn't suit my 1v1 PvP mobile game, where clients might crash or users might unexpectedly disconnect.<br>

Photon was another option, offering netcode and cloud servers, but it comes with usage fees. While PhotonFusion provides advanced features like host migration and high-quality synchronization, these weren't necessary for my game.<br>

Could a simple synchronization system be implemented with EOS for small party and mobile games? This led to the development of SynicSugar, a network library for games that are challenging to develop using Mirror.
<br><br>

### Concept and Development Philosophy
SynicSugar began as an "EOS Sample" to test the EOS SDK for my game. While some APIs were initially game-specific, many have been generalized for broader use. I will continue to develop SynicSugar with this approach. In other words, the library is developed based on actual needs for developer, not just theoretical concepts on the paper. All APIs are tested with real games, and optimizations are based on practical results.
I'm open to adopting excellent functions and welcome community input for development ideas beyond my game genre. For more details about future development, please refer to the [roadmap](https://github.com/users/skeyll/projects/5/views/2).