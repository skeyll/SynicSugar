using UnityEngine;
using UnityEngine.SceneManagement;
namespace SynicSugar.Samples{
  public class GameModeSelect : MonoBehaviour {
    public enum GameScene {
        MainMenu, TankMatchMake, Tank, ReadHearts
    }
    public void ChangeGameScene(string scene){
		  SceneManager.LoadScene(scene);
    }
  }
}