using UnityEngine;

namespace SynicSugar.Samples {
    public class SceneChangerForGUI : MonoBehaviour {
        [EnumAction(typeof(SCENELIST))]
        public void ChangeGameScene(int scene){
            SceneChanger.ChangeGameScene((SCENELIST)scene);
        }
    }
}