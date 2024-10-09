This sample is made with resources and code from Unity's Tank Tutorial.
That Tutorial is a sample published in 2015 and is a 2-player game played with a single keyboard.
The  sample in SynicSugar has been created to a 2-8 player online game.
The players are matched by RoomID, and once 8 people are gathered or Host closes the room, the game scene transitions to the game scene.
The preparation stage prepares the connection and the game, and once each player presses Ready, the actual game begins.
The game includes simple lag compensation, object pooling, and other processes required in modern games.
It also includes a process to revive the game with Syinc if it disconnected, and can be used as a sample of a simple competitive game.

The components edited in SynicSugar are under the MIT license of SynicSugar. However, please note that other components are subject to the Unity EULA.


Known issue (as the game)
 - Player is able to slip through obstacle object.
(*This can be reduced if the movement process is changed to Update. Maybe need more detection of the Hit.)
- Sometimes objects on the ground make player movement strange.


Tutorial
https://learn.unity.com/project/tanks-tutorial
Assets
https://assetstore.unity.com/packages/essentials/tutorial-projects/tanks-tutorial-46209
Unity Asset Store EULA
https://unity.com/legal/as-terms

