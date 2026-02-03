using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Extensions
{
    public static class EventTriggerTriggerEventExtensions
    {
        public static EventTrigger.TriggerEvent Listen(this EventTrigger.TriggerEvent ev, UnityAction<BaseEventData> callback)
        {
            ev.AddListener(callback);
            return ev;
        }
    }
}