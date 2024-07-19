using UnityEngine.SceneManagement;

namespace SynicSugar.Samples {
    internal static class SceneChanger {
        internal static void ChangeGameScene(SCENELIST scene){
            SceneManager.LoadScene(scene.ToString());
        }
    }
}