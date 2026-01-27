using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class GuideMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject guideMenuContainer;
    [SerializeField] private GameObject dashboardContainer; // Main NXE dashboard UI
    [SerializeField] private TransparentOverlayWindow overlayWindow;
    [SerializeField] private GlobalKeyboardHook keyboardHook;
    [SerializeField] private CanvasGroup guideCanvasGroup;
    [SerializeField] private NXEGuideLayoutGroup layoutGroup;
    [SerializeField] private Image temporaryOverlayPanel;
    
    [Header("Animation")]
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float scaleFrom = 0.8f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip openGuideAudioClip;
    [SerializeField] private AudioClip closeGuideAudioClip;
    
    public event Action OnGuideClosed;
    public event Action OnGuideOpened;
    
    private bool isGuideOpen = false;
    private bool isInOverlayMode = false; // Track if we're in overlay mode (game launched)
    private AudioSource audioSource;
    private Text[] allChildrenText;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (guideCanvasGroup == null && guideMenuContainer != null)
        {
            guideCanvasGroup = guideMenuContainer.GetComponent<CanvasGroup>();
            if (guideCanvasGroup == null)
            {
                guideCanvasGroup = guideMenuContainer.AddComponent<CanvasGroup>();
            }
        }
        
        // Start with guide hidden
        if (guideMenuContainer != null)
        {
            guideMenuContainer.SetActive(false);
        }
        
        // Dashboard starts visible in normal mode
        if (dashboardContainer != null)
        {
            dashboardContainer.SetActive(true);
        }
        
        // Set up global keyboard hook
        if (keyboardHook == null)
        {
            keyboardHook = FindObjectOfType<GlobalKeyboardHook>();
        }
        
        if (keyboardHook != null)
        {
            keyboardHook.OnGuideButtonPressed += ToggleGuide;
        }
    }

    private void OnValidate()
    {
        allChildrenText = guideMenuContainer.GetComponentsInChildren<Text>();
    }

    void OnDestroy()
    {
        if (keyboardHook != null)
        {
            keyboardHook.OnGuideButtonPressed -= ToggleGuide;
        }
    }
    
    /// <summary>
    /// Call this when launching a game from the dashboard
    /// </summary>
    public void EnterOverlayMode()
    {
        if (isInOverlayMode) return;
        
        isInOverlayMode = true;
        
        // Hide the dashboard
        if (dashboardContainer != null)
        {
            dashboardContainer.SetActive(false);
        }
        
        // Switch to transparent overlay window
        if (overlayWindow != null)
        {
            overlayWindow.SetupTransparentWindow();
            overlayWindow.EnableClickthrough(); // Start with clickthrough enabled
        }
        
        // Start global keyboard hook
        if (keyboardHook != null)
        {
            keyboardHook.StartGlobalHook();
        }
        
        Debug.Log("Entered overlay mode - dashboard hidden, transparent window active");
    }
    
    /// <summary>
    /// Call this when returning to dashboard (e.g., closing a game)
    /// </summary>
    public void ExitOverlayMode()
    {
        if (!isInOverlayMode) return;
        
        isInOverlayMode = false;
        
        // Close guide if open
        if (isGuideOpen)
        {
            CloseGuide();
        }
        
        if (overlayWindow != null)
        {
            overlayWindow.DisableClickthrough(); // Start with clickthrough enabled
        }
        
        // Stop global keyboard hook
        if (keyboardHook != null)
        {
            keyboardHook.StopGlobalHook();
        }
        
        // Show the dashboard
        if (dashboardContainer != null)
        {
            dashboardContainer.SetActive(true);
        }
        
        Debug.Log("Exited overlay mode - returned to dashboard");
        
        // Note: Window will remain transparent and topmost
        // If you need to restore normal window, you'll need to restart the app
        // or implement window style restoration in TransparentOverlayWindow
    }
    
    public void ToggleGuide()
    {
        if (isGuideOpen)
        {
            CloseGuide();
        }
        else
        {
            OpenGuide();
        }
    }
    
    public void OpenGuide()
    {
        if (isGuideOpen) return;
        
        isGuideOpen = true;
        
        // Disable clickthrough so we can interact with the guide
        if (overlayWindow != null)
        {
            overlayWindow.DisableClickthrough();
        }

        layoutGroup.FocusedIndex = Mathf.FloorToInt(layoutGroup.BladeCount / 2f);
        
        // Show and animate guide menu
        if (guideMenuContainer != null)
        {
            guideMenuContainer.SetActive(true);
            guideMenuContainer.transform.localScale = Vector3.one * scaleFrom;
            layoutGroup.transform.localScale = new Vector3(0.5f, 1, 1);
            temporaryOverlayPanel.color = Color.white;

            foreach (var text in allChildrenText)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
                text.DOFade(1, fadeTime).SetDelay(fadeTime);
            }
            
            if (guideCanvasGroup != null)
            {
                guideCanvasGroup.alpha = 0f;
                guideCanvasGroup.DOKill();
                guideCanvasGroup.DOFade(1f, fadeTime).SetEase(Ease.OutQuad);
            }
            
            guideMenuContainer.transform.DOKill();
            layoutGroup.DOKill();
            
            guideMenuContainer.transform.DOScale(Vector3.one, fadeTime).SetEase(Ease.OutBack);
            layoutGroup.transform.DOScale(Vector3.one, fadeTime).SetDelay(fadeTime);
            layoutGroup.GetComponent<CanvasGroup>().DOFade(1,fadeTime).SetDelay(fadeTime).ChangeStartValue(0);
            temporaryOverlayPanel.DOFade(0, fadeTime).SetDelay(fadeTime);
        }
        
        audioSource.PlayOneShot(openGuideAudioClip);
        
        OnGuideOpened?.Invoke();
    }
    
    public void CloseGuide()
    {
        if (!isGuideOpen) return;
        
        isGuideOpen = false;
        
        // Animate guide menu out
        if (guideMenuContainer != null && guideCanvasGroup != null)
        {
            guideCanvasGroup.DOKill();
            guideMenuContainer.transform.DOKill();
            layoutGroup.DOKill();
            
            foreach (var text in allChildrenText)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
                text.DOFade(0, fadeTime);
            }

            layoutGroup.transform.DOScale(new Vector3(0.5f, 1, 1), fadeTime);

            temporaryOverlayPanel.DOFade(1, fadeTime);
            layoutGroup.GetComponent<CanvasGroup>().DOFade(0,fadeTime).ChangeStartValue(1);
            
            guideCanvasGroup.DOFade(0f, fadeTime)
                .SetDelay(fadeTime)
                .SetEase(Ease.InQuad);

            guideMenuContainer.transform.DOScale(Vector3.one * scaleFrom, fadeTime)
                .SetEase(Ease.InBack)
                .SetDelay(fadeTime)
                .OnComplete(() => 
                {
                    guideMenuContainer.SetActive(false);
                    
                    // Re-enable clickthrough when guide is closed (only in overlay mode)
                    if (isInOverlayMode && overlayWindow != null)
                    {
                        overlayWindow.EnableClickthrough();
                    }
                });
        }
        
        audioSource.PlayOneShot(closeGuideAudioClip);
        
        OnGuideClosed?.Invoke();
    }

    public void QuitNxd()
    {
        Application.Quit();
    }

    public bool IsGuideOpen => isGuideOpen;
    public bool IsInOverlayMode => isInOverlayMode;
}