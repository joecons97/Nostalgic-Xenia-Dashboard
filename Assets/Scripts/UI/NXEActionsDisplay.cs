using System;
using DG.Tweening;
using Gilzoide.FlexUi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ActionsConfig
{
    public bool showSelectAction;
    public string selectActionText = "Select";
    public bool showSelectAltAction;
    public string selectAltActionText = "Details";
    public bool showCancelAction;
    public string cancelActionText = "Back";
}

public class NXEActionsDisplay : MonoBehaviour
{
    [SerializeField] private GameObject selectActionButton;
    [SerializeField] private GameObject selectAltActionButton;
    [SerializeField] private GameObject CancelActionButton;

    [FoldoutHeader("Transition Settings")]
    [SerializeField] private float textTransitionTime = 0.25f;
    [SerializeField] private Ease textTransitionEase = Ease.OutQuad;
    [SerializeField] private float buttonTransitionTime = 0.25f;
    [SerializeField] private Ease buttonTransitionEase = Ease.OutQuad;
    [SerializeField] private Ease buttonScaleTransitionEase = Ease.OutQuad;
    

    public void SetConfig(ActionsConfig config)
    {
        HandleText(config.showSelectAction, config.selectActionText, selectActionButton);
        HandleText(config.showSelectAltAction, config.selectAltActionText, selectAltActionButton);
        HandleText(config.showCancelAction, config.cancelActionText, CancelActionButton);
    }
    
    private void HandleText(bool display, string text, GameObject obj)
    {
        bool wasActive = obj.activeSelf;

        var textComponent = obj.GetComponentInChildren<Text>();
        var image = obj.GetComponentInChildren<Image>();
            
        if (display)
        {
            obj.SetActive(true);
            if (wasActive)
            {
                textComponent.DOFade(0, textTransitionTime)
                    .SetEase(textTransitionEase)
                    .OnComplete(() => textComponent.text = text);
            }
            else
            {
                textComponent.text = text;
            }

            image.DOFade(1, buttonTransitionTime)
                    .SetEase(buttonTransitionEase);

            image.transform.DOScale(1, buttonTransitionTime)
                    .SetEase(buttonScaleTransitionEase);

            textComponent.DOFade(1, textTransitionTime)
                    .SetEase(textTransitionEase);
        }
        else if(wasActive)
        {
            textComponent.DOFade(0, textTransitionTime)
                .SetEase(textTransitionEase)
                .OnComplete(() => obj.SetActive(false));

            image.transform.DOScale(0, buttonTransitionTime)
                    .SetEase(buttonScaleTransitionEase);
            
            image.DOFade(0, buttonTransitionTime)
                    .SetEase(buttonTransitionEase);
        }
    }
}
