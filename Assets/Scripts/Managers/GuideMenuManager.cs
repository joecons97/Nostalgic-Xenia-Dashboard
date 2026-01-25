using UnityEngine;
using DG.Tweening;

public class GuideMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject guideMenuContainer;
    [SerializeField] private GameObject dashboardContainer; // Main NXE dashboard UI
    [SerializeField] private TransparentOverlayWindow overlayWindow;
    [SerializeField] private GlobalKeyboardHook keyboardHook;
    [SerializeField] private CanvasGroup guideCanvasGroup;
    [SerializeField] private NXEGuideLayoutGroup layoutGroup;
    
    [Header("Animation")]
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float scaleFrom = 0.8f;
    
    private bool isGuideOpen = false;
    private bool isInOverlayMode = false; // Track if we're in overlay mode (game launched)
    
    void Start()
    {
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
            
            if (guideCanvasGroup != null)
            {
                guideCanvasGroup.alpha = 0f;
                guideCanvasGroup.DOKill();
                guideCanvasGroup.DOFade(1f, fadeTime).SetEase(Ease.OutQuad);
            }
            
            guideMenuContainer.transform.DOKill();
            guideMenuContainer.transform.DOScale(Vector3.one, fadeTime).SetEase(Ease.OutBack);
        }
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
            
            guideCanvasGroup.DOFade(0f, fadeTime).SetEase(Ease.InQuad);
            guideMenuContainer.transform.DOScale(Vector3.one * scaleFrom, fadeTime)
                .SetEase(Ease.InBack)
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
    }
    
    public bool IsGuideOpen => isGuideOpen;
    public bool IsInOverlayMode => isInOverlayMode;
}