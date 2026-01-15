using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NXEModal : MonoBehaviour
{
    public bool isOpen;
    public bool canBeClosed = true;

    public ActionsConfig DisplayActions;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] protected Text titleText;

    [Header("Timings")]
    [SerializeField] private float textTransitionTime = 0.25f;
    [SerializeField] private Ease textTransitionEase = Ease.OutQuad;
    [SerializeField] private float fadeTransitionTime = 0.25f;
    [SerializeField] private Ease fadeTransitionEase = Ease.OutQuad;

    [Header("Input")][CanBeNull, SerializeField] private Selectable defaultSelectable;

    private NXEModal subModal;

    public NXEModal SubModal => subModal;

    public static NXEModal CreateAndShow(NXEModal modal)
    {
        var instance = Instantiate(modal);
        instance.titleText.transform.localScale = new Vector3(1, 0, 1);
        instance.canvasGroup.alpha = 0;
        instance.Show();

        return instance;
    }

    public static NXEModal Create(NXEModal modal)
    {
        var instance = Instantiate(modal);
        instance.titleText.transform.localScale = new Vector3(1, 0, 1);
        instance.canvasGroup.alpha = 0;

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

        if (defaultSelectable)
            EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);

        FindFirstObjectByType<NXEActionsDisplay>().SetConfig(DisplayActions);
    }

    public void OpenSubModal(NXEModal modal)
    {
        subModal = Create(modal);
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
                    subModal.Hide(() =>
                    {
                        Destroy(subModal.gameObject);
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
                Hide(() => Destroy(gameObject));
                return NXEModalCloseResult.NormalClose;
            }

            return NXEModalCloseResult.None;
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