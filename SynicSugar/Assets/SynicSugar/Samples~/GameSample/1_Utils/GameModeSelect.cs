using UnityEngine;
using UnityEngine.SceneManagement;
namespace SynicSugar.Samples{
  public class GameModeSelect : MonoBehaviour {
    public enum GameScene {
        MainMenu, Tank, TankMatchMake, ReadHearts, Chat
    }
    public void ChangeGameScene(string scene){
		  SceneManager.LoadScene(scene);
    }
  }
}