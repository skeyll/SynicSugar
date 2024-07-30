using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SynicSugar.Samples.Tank {
    /// <summary>
    /// GUI for local players to control themselves.
    /// Since mobile support of inputs is difficult(I always use new input system), in this sample this is done in GUI.
    /// </summary>
    public class TankPadGUI : MonoBehaviour {
        [SerializeField] PadsForMovement movements;
        [SerializeField] PadsForAction actions;
        void Awake(){
            SwitchGUISState(PadState.None);
        }
        /// <summary>
        /// To register RPC events to button for LocalPlayer.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="stop"></param>
        internal void RegisterButtonEvents(Action<Direction> move, Action stop, Action charge, Action release){
            movements.RegisterEvents(move, stop);
            actions.RegisterEvents(charge, release);
        }
        /// <summary>
        /// Change GUI buttons active.
        /// </summary>
        /// <param name="state"></param>
        internal void SwitchGUISState(PadState state){
            //Active except Result and Stay.
            movements.SwitchButtonActive(state != PadState.None);
            //Active during in-game.
            actions.SwitchButtonActive(state == PadState.ALL);
        }
    }
    [Serializable]
    public class PadsForMovement {
        public GameObject Up, Left, Down, Right;

        /// <summary>
        /// Register events.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="stop"></param>
        internal void RegisterEvents(Action<Direction> move, Action stop) {
            GameObject[] objects = { Up, Left, Down, Right };
            
            for (int i = 0; i < objects.Length; i++) {
                int keyIndex = i;
                TankEventRegisterExtenstions.RegisterEvents(objects[i], EventTriggerType.PointerDown, move, (Direction)keyIndex);
                TankEventRegisterExtenstions.RegisterEvents(objects[i], EventTriggerType.PointerUp, stop);
            }
        }
        internal void SwitchButtonActive(bool isActivate){
            GameObject[] objects = { Up, Left, Down, Right };
            
            for (int i = 0; i < objects.Length; i++) {
                objects[i].SetActive(isActivate);
            }
        }
    }
    [Serializable]
    public class PadsForAction {
        public GameObject TriggerToFire;
        internal void RegisterEvents(Action charge, Action release) {
            TankEventRegisterExtenstions.RegisterEvents(TriggerToFire, EventTriggerType.PointerDown, charge);
            TankEventRegisterExtenstions.RegisterEvents(TriggerToFire, EventTriggerType.PointerUp, release);
        }

        internal void SwitchButtonActive(bool isActivate){
            TriggerToFire.SetActive(isActivate);
        }
    }
}