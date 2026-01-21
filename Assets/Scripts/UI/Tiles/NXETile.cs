using UnityEngine;
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
        if(DefaultSelection)
            DefaultSelection.Select();
        
        OnFocus();
    }
}
