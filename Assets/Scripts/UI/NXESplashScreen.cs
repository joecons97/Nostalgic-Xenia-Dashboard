using System;
using System.Collections.Generic;
using DG.Tweening;
using Loadables;
using TNRD;
using UnityEngine;

public class NXESplashScreen : MonoBehaviour
{
    [SerializeField] private SerializableInterface<ILoadable>[] loadables;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup dashboardCanvasGroup;
    [SerializeField] private Transform throbber;
    [SerializeField] private Transform spinner;
    
    [SerializeField] private float throbberSize = 1f;
    [SerializeField] private float throbberSpeed = 1f;
    
    [SerializeField] private float spinnerSpeed = 1f;
    [SerializeField] private float fadeTime = 1f;
    
    private List<ILoadable> completedLoadables = new List<ILoadable>();

    private void Awake()
    {
        foreach (var loadable in loadables)
        {
            loadable.Value.OnLoadComplete += ValueOnOnLoadComplete;
        }
    }

    private void ValueOnOnLoadComplete(ILoadable obj)
    {
        obj.OnLoadComplete -= ValueOnOnLoadComplete;
        
        if(completedLoadables.Contains(obj) == false)
            completedLoadables.Add(obj);

        if (completedLoadables.Count == loadables.Length)
        {
            canvasGroup.DOFade(0, fadeTime);
            dashboardCanvasGroup.DOFade(1, fadeTime).SetDelay(fadeTime);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        throbber.DOScale(throbberSize, throbberSpeed)
            .ChangeStartValue(Vector3.one)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);

        spinner.DORotate(new Vector3(0, 0, 360), spinnerSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear) // Ensures constant, smooth speed
            .SetLoops(-1, LoopType.Restart) // -1 makes it loop infinitely
            .SetRelative(); // Rotates relative to current rotation
    }
}
