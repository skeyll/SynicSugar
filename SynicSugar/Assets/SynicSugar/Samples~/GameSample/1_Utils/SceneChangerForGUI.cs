using UnityEngine;

namespace SynicSugar.Samples {
    public class SceneChangerForGUI : MonoBehaviour {
        [EnumAction(typeof(Scene))]
        public void ChangeGameScene(int scene){
            SceneChanger.ChangeGameScene((Scene)scene);
        }
    }
}