using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NXEModal : MonoBehaviour
{
    public bool canBeClosed = true;

    public ActionsConfig DisplayActions;

    public event Action OnClosed;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] protected Text titleText;

    [Header("Timings")]
    [SerializeField] private float textTransitionTime = 0.25f;
    [SerializeField] private Ease textTransitionEase = Ease.OutQuad;
    [SerializeField] private float fadeTransitionTime = 0.25f;
    [SerializeField] private Ease fadeTransitionEase = Ease.OutQuad;

    [Header("Input")][CanBeNull, SerializeField] private Selectable defaultSelectable;

    private NXEModal subModal;
    private NXEModal parentModal;

    public NXEModal SubModal => subModal;
    public NXEModal ParentModal => parentModal;

    public static NXEModal TopMostModal => topMostModal;
    private static NXEModal topMostModal = null;

    public static NXEModal CreateAndShow(NXEModal modal)
    {
        var instance = Instantiate(modal);
        
        if(instance.titleText)
            instance.titleText.transform.localScale = new Vector3(1, 0, 1);
        
        instance.canvasGroup.alpha = 0;
        instance.Show();

        return instance;
    }

    public static NXEModal Create(NXEModal modal)
    {
        var instance = Instantiate(modal);
        
        if(instance.titleText)
            instance.titleText.transform.localScale = new Vector3(1, 0, 1);
        
        instance.canvasGroup.alpha = 0;

        return instance;
    }

    [ContextMenu("Show")]
    public void Show()
    {
        if (titleText)
        {
            var rectTransform = titleText.transform as RectTransform;

            titleText.DOFade(1, textTransitionTime).SetEase(textTransitionEase);
            rectTransform.DOScale(new Vector3(1, 1, 1), textTransitionTime).SetEase(textTransitionEase);
        }

        canvasGroup.DOFade(1, fadeTransitionTime)
            .SetEase(fadeTransitionEase)
            .SetDelay(textTransitionTime);

        if (defaultSelectable)
        {
            //Hacky but stops the selection sound from playing on modal open
            var eventTrigger = defaultSelectable.GetComponent<EventTrigger>();
            eventTrigger.enabled = false;
            EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
            _ = UniTask.NextFrame().ContinueWith(() => eventTrigger.enabled = true);
        }

        FindFirstObjectByType<NXEVerticalLayoutGroup>()?.Hide();
        UpdateDisplayActions();
        topMostModal = this;
    }

    public void UpdateDisplayActions()
    {
        FindFirstObjectByType<NXEActionsEffects>().SetConfig(DisplayActions);
    }

    public void OpenSubModal(NXEModal modal)
    {
        subModal = Create(modal);
        subModal.parentModal = this;

        Hide(() =>
        {
            subModal.Show();
            gameObject.SetActive(false);
        });
    }

    public NXEModalCloseResult Close()
    {
        if (subModal != null)
        {
            if (subModal.subModal != null)
            {
                if(subModal.subModal.canBeClosed)
                    subModal.Close();
            }
            else
            {
                if (subModal.canBeClosed)
                {
                    subModal.OnClosed?.Invoke();
                    subModal.Hide(() =>
                    {
                        Destroy(subModal.gameObject);
                        if (topMostModal == subModal)
                            topMostModal = this;
                            
                        subModal = null;
                        gameObject.SetActive(true);
                        Show();
                    });
                }
                return NXEModalCloseResult.SubModalClosed;
            }

            return NXEModalCloseResult.None;
        }
        else
        {
            if (canBeClosed)
            {
                OnClosed?.Invoke();
                Hide(() => Destroy(gameObject));
                
                FindFirstObjectByType<NXEVerticalLayoutGroup>()?.Show();
                return NXEModalCloseResult.NormalClose;
            }

            return NXEModalCloseResult.None;
        }
    }

    private void Hide(TweenCallback onComplete = null)
    {
        TweenerCore<Vector3, Vector3, VectorOptions> scaleTween = null;
        if (titleText)
        {
            var rectTransform = titleText.transform as RectTransform;

            titleText.DOFade(0, textTransitionTime)
                .SetEase(textTransitionEase);
            
            scaleTween = rectTransform
                .DOScale(new Vector3(1, 0, 1), textTransitionTime)
                .SetEase(textTransitionEase);
        }

        var fadeTween = canvasGroup
            .DOFade(0, fadeTransitionTime)
            .SetEase(fadeTransitionEase);

        if (onComplete != null)
        {
            if (textTransitionTime > fadeTransitionTime && scaleTween != null)
                scaleTween.OnComplete(onComplete);
            else
                fadeTween.OnComplete(onComplete);
        }
    }

    private void OnDestroy()
    {
        canvasGroup.DOKill();
        
        if (titleText)
        {
            titleText.DOKill();
            titleText.transform.DOKill();
        }
    }
}