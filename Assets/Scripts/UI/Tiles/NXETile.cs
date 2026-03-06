using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NXETile : MonoBehaviour
{
    public ActionsConfig DisplayActions;
    public Selectable DefaultSelection;

    protected virtual void OnFocus() { }
    public virtual void OnUnFocus() { }
    public virtual void OnSelect() { }
    public virtual void OnSelectAlt() { }
    public virtual void OnCancel() { }
    public virtual void OnMoveRight(float speed = 1) { }
    public virtual void OnMoveLeft(float speed = 1) { }

    public void Focus()
    {
        if (DefaultSelection)
        {
            //Hacky but stops the selection sound from playing on modal open
            var eventTrigger = DefaultSelection.GetComponent<EventTrigger>();
            if (eventTrigger != null)
            {
                eventTrigger.enabled = false;
                DefaultSelection.Select();
                _ = UniTask.NextFrame(destroyCancellationToken).ContinueWith(() =>
                {
                    if (eventTrigger)
                        eventTrigger.enabled = true;
                });
            }
            else
            {
                DefaultSelection.Select();
            }
        }

        OnFocus();
    }
}
