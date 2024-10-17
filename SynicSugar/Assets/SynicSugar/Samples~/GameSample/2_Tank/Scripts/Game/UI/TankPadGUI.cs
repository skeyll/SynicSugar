using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SynicSugar.Samples.Tank
{
    /// <summary>
    /// GUI for local players to control themselves.
    /// Since mobile support of inputs is difficult(I always use new input system), in this sample this is done in GUI.
    /// </summary>
    public class TankPadGUI : MonoBehaviour
    {
        [SerializeField] private PadsForMovement movements;
        [SerializeField] private PadsForAction actions;
        void Awake()
        {
            SwitchGUISState(PadState.None);
        }
        /// <summary>
        /// To register RPC events to button for LocalPlayer.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="stop"></param>
        internal void RegisterButtonEvents(Action<Direction> move, Action stop, Action charge, Action release)
        {
            movements.RegisterEvents(move, stop);
            actions.RegisterEvents(charge, release);
        }
        /// <summary>
        /// Change GUI buttons active.
        /// </summary>
        /// <param name="state"></param>
        internal void SwitchGUISState(PadState state)
        {
            //Active except Result and Stay.
            movements.SwitchButtonActive(state != PadState.None);
            //Active during in-game.
            actions.SwitchButtonActive(state == PadState.ALL);
        }
    }
}