using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SynicSugar.Samples.Tank
{
    [Serializable]
    public class PadsForAction
    {
        public GameObject TriggerToFire;
        internal void RegisterEvents(Action charge, Action release)
        {
            TankEventRegisterExtenstions.RegisterEvents(TriggerToFire, EventTriggerType.PointerDown, charge);
            TankEventRegisterExtenstions.RegisterEvents(TriggerToFire, EventTriggerType.PointerUp, release);
        }

        internal void SwitchButtonActive(bool isActivate)
        {
            TriggerToFire.SetActive(isActivate);
        }
    }
}