using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples.Tank
{
    internal static class TankEventRegisterExtenstions
    {
        internal static void RegisterEvents <T>(GameObject uiObject, EventTriggerType type, Action<T> action, T value)
        {
            if (uiObject == null) return;

            EventTrigger trigger = uiObject.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger = uiObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry triger = new EventTrigger.Entry();
            triger.eventID = type;
            triger.callback.AddListener((d) => { action(value); });
            trigger.triggers.Add(triger);
        }

        internal static void RegisterEvents(GameObject uiObject, EventTriggerType type, Action action)
        {
            if (uiObject == null) return;

            EventTrigger trigger = uiObject.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger = uiObject.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry triger = new EventTrigger.Entry();
            triger.eventID = type;
            triger.callback.AddListener((d) => { action(); });
            trigger.triggers.Add(triger);
        }
        internal static void RegisterEvents(Button uiObject, EventTriggerType type, Action action)
        {
            if (uiObject == null) return;

            EventTrigger trigger = uiObject.gameObject.GetComponent<EventTrigger>();
            
            if (trigger == null)
            {
                trigger = uiObject.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry triger = new EventTrigger.Entry();
            triger.eventID = type;
            triger.callback.AddListener((d) => { action(); });
            trigger.triggers.Add(triger);
        }
    }
}