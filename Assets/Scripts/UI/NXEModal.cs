using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NXEModal : MonoBehaviour
{
    public bool isOpen;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text titleText;

    [Header("Timings")] 
    [SerializeField] private float textTransitionTime = 0.25f;
    [SerializeField] private Ease textTransitionEase = Ease.OutQuad;
    [SerializeField] private float fadeTransitionTime = 0.25f;
    [SerializeField] private Ease fadeTransitionEase = Ease.OutQuad;
    
    [Header("Input")] [CanBeNull, SerializeField] private Selectable defaultSelectable;

    private NXEModal subModal;
    

    public static NXEModal CreateAndShow(NXEModal modal)
    {
        var instance = Instantiate(modal);
        instance.titleText.transform.localScale = new Vector3(1, 0, 1);
        instance.canvasGroup.alpha = 0;
        instance.Show();

        return instance;
    }

    [ContextMenu("Show")]
    public void Show()
    {
        var rectTransform = titleText.transform as RectTransform;
        
        titleText.DOFade(1, textTransitionTime).SetEase(textTransitionEase);
        rectTransform.DOScale(new Vector3(1, 1, 1), textTransitionTime).SetEase(textTransitionEase);
        
        canvasGroup.DOFade(1, fadeTransitionTime)
            .SetEase(fadeTransitionEase)
            .SetDelay(textTransitionTime);
        
        if(defaultSelectable)
            EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
    }

    public void OpenSubModal(NXEModal modal)
    {
        Hide(() =>
        {
            subModal = CreateAndShow(modal);
        });
    }

    public NXEModalCloseResult Close()
    {
        if (subModal != null)
        {
            subModal.Hide(() =>
            {
                Destroy(subModal.gameObject);
                subModal = null;
                Show();
            });

            return NXEModalCloseResult.SubModalClosed;
        }
        else
        {
            Hide(() => Destroy(gameObject));
            return NXEModalCloseResult.NormalClose;
        }
    }

    private void Hide(TweenCallback onComplete = null)
    {
        var rectTransform = titleText.transform as RectTransform;

        titleText.DOFade(0, textTransitionTime)
            .SetEase(textTransitionEase);
        
        var scaleTween = rectTransform
            .DOScale(new Vector3(1, 0, 1), textTransitionTime)
            .SetEase(textTransitionEase);
        
        var fadeTween = canvasGroup
            .DOFade(0, fadeTransitionTime)
            .SetEase(fadeTransitionEase);

        if (onComplete != null)
        {
            if (fadeTransitionTime > textTransitionTime)
                fadeTween.OnComplete(onComplete);
            else
                scaleTween.OnComplete(onComplete);
        }
    }

    private void OnDestroy()
    {
        canvasGroup.DOKill();
        titleText.DOKill();
        titleText.transform.DOKill();
    }
}