using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SynicSugar.Samples.Tank
{
    [Serializable]
    public class PadsForMovement 
    {
        public GameObject Up, Left, Down, Right;

        /// <summary>
        /// Register events.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="stop"></param>
        internal void RegisterEvents(Action<Direction> move, Action stop)
        {
            GameObject[] objects = { Up, Left, Down, Right };

            for (int i = 0; i < objects.Length; i++)
            {
                int keyIndex = i;
                TankEventRegisterExtenstions.RegisterEvents(objects[i], EventTriggerType.PointerDown, move, (Direction)keyIndex);
                TankEventRegisterExtenstions.RegisterEvents(objects[i], EventTriggerType.PointerUp, stop);
            }
        }
        internal void SwitchButtonActive(bool isActivate)
        {
            GameObject[] objects = { Up, Left, Down, Right };
            
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].SetActive(isActivate);
            }
        }
    }
}