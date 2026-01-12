using DG.Tweening;
using UnityEngine;
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
        
        rectTransform.DOScale(new Vector3(1, 1, 1), textTransitionTime).SetEase(textTransitionEase);
        
        canvasGroup.DOFade(1, fadeTransitionTime)
            .SetEase(fadeTransitionEase)
            .SetDelay(textTransitionTime);
    }

    public void Close()
    {
        var rectTransform = titleText.transform as RectTransform;

        var scaleTween = rectTransform
            .DOScale(new Vector3(1, 0, 1), textTransitionTime)
            .SetEase(textTransitionEase);
        
        var fadeTween = canvasGroup
            .DOFade(0, fadeTransitionTime)
            .SetEase(fadeTransitionEase);

        if (fadeTransitionTime > textTransitionTime)
            fadeTween.OnComplete(() => Destroy(gameObject));
        else
            scaleTween.OnComplete(() => Destroy(gameObject));
    }
}