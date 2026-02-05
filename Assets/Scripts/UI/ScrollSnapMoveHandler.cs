using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollSnap))]
public class ScrollSnapMoveHandler : Selectable
{
    [SerializeField, ReadOnly] private ScrollSnap scrollSnapBase;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        scrollSnapBase = GetComponent<ScrollSnap>();
    }
#endif

    public override void OnMove(AxisEventData eventData)
    {
        if (IsActive() == false || IsInteractable() == false)
        {
            base.OnMove(eventData);
            return;
        }

        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                if (scrollSnapBase.direction == ScrollSnap.ScrollDirection.Horizontal)
                    scrollSnapBase.PreviousScreen();
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Right:
                if (scrollSnapBase.direction == ScrollSnap.ScrollDirection.Horizontal)
                    scrollSnapBase.NextScreen();
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Up:
                if (scrollSnapBase.direction == ScrollSnap.ScrollDirection.Vertical)
                    scrollSnapBase.PreviousScreen();
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Down:
                if (scrollSnapBase.direction == ScrollSnap.ScrollDirection.Vertical)
                    scrollSnapBase.NextScreen();
                else
                    base.OnMove(eventData);
                break;
        }
    }
}