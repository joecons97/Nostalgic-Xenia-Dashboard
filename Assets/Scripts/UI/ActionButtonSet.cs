using UnityEngine;

public class ActionButtonSet : MonoBehaviour
{
    public GameObject SelectAction;
    public GameObject SelectAltAction;
    public GameObject CancelAction;

    public void Use(ActionsConfig actions = null)
    {
        var effects = FindFirstObjectByType<NXEActionsEffects>();
        
        if(effects.actionButtonSet)
            effects.actionButtonSet.gameObject.SetActive(false);
        
        effects.actionButtonSet = this;
        
        if(actions != null)
            effects.SetConfig(actions);
        
        gameObject.SetActive(true);
    }
}
