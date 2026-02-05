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
    
    [SerializeField] private float fadeTime = 1f;
    
    private List<ILoadable> completedLoadables = new List<ILoadable>();
    private int expectedLoadables;

    private void Awake()
    {
        if (loadables == null || loadables.Length == 0)
        {
            CompleteSplash();
            return;
        }

        foreach (var loadable in loadables)
        {
            if (loadable?.Value == null)
                continue;

            expectedLoadables++;
            loadable.Value.OnLoadComplete += ValueOnOnLoadComplete;
        }

        if (expectedLoadables == 0)
            CompleteSplash();
    }

    private void OnDestroy()
    {
        if (loadables == null)
            return;

        foreach (var loadable in loadables)
        {
            if (loadable?.Value == null)
                continue;

            loadable.Value.OnLoadComplete -= ValueOnOnLoadComplete;
        }
    }

    private void ValueOnOnLoadComplete(ILoadable obj)
    {
        obj.OnLoadComplete -= ValueOnOnLoadComplete;
        
        if(completedLoadables.Contains(obj) == false)
            completedLoadables.Add(obj);

        if (completedLoadables.Count == expectedLoadables)
            CompleteSplash();
    }

    private void CompleteSplash()
    {
        canvasGroup.DOFade(0, fadeTime);
        dashboardCanvasGroup.DOFade(1, fadeTime).SetDelay(fadeTime);
    }
}
