using System;
using DG.Tweening;
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

public class NXEActionsEffects : MonoBehaviour
{
    [SerializeField] private GameObject selectActionButton;
    [SerializeField] private GameObject selectAltActionButton;
    [SerializeField] private GameObject CancelActionButton;

    [Header("Transition Settings")]

    [Header("Text")]
    [SerializeField] private float textTransitionTime = 0.25f;
    [SerializeField] private Ease textTransitionEase = Ease.OutQuad;

    [Header("Button")]
    [SerializeField] private float buttonTransitionTime = 0.25f;
    [SerializeField] private Ease buttonTransitionEase = Ease.OutQuad;
    [SerializeField] private Ease buttonScaleTransitionEase = Ease.OutQuad;

    [Header("Selection")]
    [SerializeField] private float selectionTransitionTime = 0.25f;
    [SerializeField] private Ease selectionTransitionEase = Ease.OutQuad;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectAudioClip;
    [SerializeField] private AudioClip cancelAudioClip;

    public void ActionSelect() 
    {
        AnimateAction(selectActionButton);
        audioSource.PlayOneShot(selectAudioClip);
    }
    public void ActionSelectAlt()
    {
        AnimateAction(selectAltActionButton);
        audioSource.PlayOneShot(selectAudioClip);
    }
    public void ActionCancel()
    {
        AnimateAction(CancelActionButton);
        audioSource.PlayOneShot(cancelAudioClip);
    }
    
    private void AnimateAction(GameObject obj)
    {
        var selectEffect = obj.transform.Find("Icon/PressEffect");
        var img = selectEffect.GetComponent<Image>();

        selectEffect.DOKill();

        selectEffect
            .DOScale(2, selectionTransitionTime)
            .ChangeStartValue(Vector3.one)
            .SetEase(selectionTransitionEase);

        img.DOFade(0, selectionTransitionTime)
            .ChangeStartValue(Color.white)
            .SetEase(selectionTransitionEase);
    }


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
            
        textComponent.DOKill();
        image.DOKill();
            
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
                
                image.transform.DOScale(Vector3.one, buttonTransitionTime)
                    .ChangeStartValue(Vector3.zero)
                    .SetEase(buttonScaleTransitionEase);
            }

            image.DOFade(1, buttonTransitionTime)
                .SetEase(buttonTransitionEase);

            textComponent.DOFade(1, textTransitionTime)
                .SetEase(textTransitionEase);
        }
        else if(wasActive)
        {
            textComponent.DOFade(0, textTransitionTime)
                .SetEase(textTransitionEase)
                .OnComplete(() => obj.SetActive(false));

            image.transform.DOScale(Vector3.zero, buttonTransitionTime)
                    .ChangeStartValue(Vector3.one)
                    .SetEase(buttonScaleTransitionEase);
            
            image.DOFade(0, buttonTransitionTime)
                    .SetEase(buttonTransitionEase);
        }
    }
}
