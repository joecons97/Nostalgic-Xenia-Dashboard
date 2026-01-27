using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class NXEGuideLayoutGroup : LayoutGroup, IControllableLayout
{
    [Header("Guide Blade Settings")] [SerializeField]
    private float visibleWidth = 40f; // How much of each unfocused blade is visible

    [SerializeField] private Vector2 bladeSize = new Vector2(542f, 315f);

    [Header("Selection")] [SerializeField] private int focusedIndex = 0;
    [SerializeField] private float focusScale = 1.2f;
    [SerializeField] private float unfocusedScale = 0.85f;
    [SerializeField] private float unfocusedAlpha = 0.6f;
    [SerializeField] private float transitionFadeTime = 0.15f;
    [SerializeField] private float transitionTime = 0.3f;

    [Header("3D Effect")] [SerializeField] private bool use3DPositioning = false;
    [SerializeField] private float depthPerBlade = 50f;

    [Header("Focus Overlay")] [SerializeField]
    private GuideMenuBlade focusOverlay; // The blue blade overlay (outside this layout)

    [SerializeField] private int sortOrderBase = 500;
    [SerializeField] private float overlayTransitionTime = 0.3f;
    [SerializeField] private int overlaySortingOrder = 100; // Overlay renders above blades but below buttons
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] moveAudioClips;

    private List<GuideMenuBlade> blades = new List<GuideMenuBlade>();
    private List<Canvas> bladeCanvases = new List<Canvas>();
    private Vector3[] targetPositions;
    private Vector3[] targetScales;
    private float[] targetAlphas;
    private int[] targetSortOrders;
    private AudioSource audioSource;

    public int FocusedIndex
    {
        get => focusedIndex;
        set
        {
            focusedIndex = Mathf.Clamp(value, 0, blades.Count - 1);
            CalculateLayout();
        }
    }

    public int BladeCount
    {
        get
        {
            if(blades.Count == 0)
                CollectBlades();
            
            return blades.Count;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetupFocusOverlay();
        CalculateLayout();
        
        var blade = blades[focusedIndex];
        blade.Focus();
    }

    protected override void Start()
    {
        audioSource = GetComponent<AudioSource>();
        base.Start();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        CollectBlades();
    }

    public override void CalculateLayoutInputVertical()
    {
        CollectBlades();
    }

    public override void SetLayoutHorizontal()
    {
        CalculateLayout();
    }

    public override void SetLayoutVertical()
    {
        // Handled in SetLayoutHorizontal
    }

    private void SetupFocusOverlay()
    {
        if (focusOverlay == null) return;

        // Ensure focus overlay has a Canvas for sorting
        Canvas overlayCanvas = focusOverlay.GetComponent<Canvas>();
        if (overlayCanvas == null)
        {
            overlayCanvas = focusOverlay.gameObject.AddComponent<Canvas>();
        }

        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = overlaySortingOrder + sortOrderBase;

        // Set same size as blades
        focusOverlay.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bladeSize.x);
        focusOverlay.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bladeSize.y);
        focusOverlay.RectTransform.pivot = new Vector2(0.5f, 0.5f);
        focusOverlay.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        focusOverlay.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
    }

    private void CollectBlades()
    {
        blades.Clear();
        bladeCanvases.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;

            if (child != null && child.gameObject.activeSelf)
            {
                blades.Add(child.GetComponent<GuideMenuBlade>());

                // Ensure each blade has a Canvas component forQ sort order
                Canvas canvas = child.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = child.gameObject.AddComponent<Canvas>();
                }

                canvas.overrideSorting = true;
                bladeCanvases.Add(canvas);

                // Find ButtonContainer and ensure it has a canvas that renders on top
                Transform buttonContainer = child.Find("ButtonContainer");
                if (buttonContainer != null)
                {
                    Canvas buttonCanvas = buttonContainer.GetComponent<Canvas>();
                    if (buttonCanvas == null)
                    {
                        buttonCanvas = buttonContainer.gameObject.AddComponent<Canvas>();
                    }

                    buttonCanvas.overrideSorting = true;
                    buttonCanvas.sortingOrder = 150 + sortOrderBase; // Above overlay

                    // Ensure it has a CanvasGroup for visibility control
                    if (buttonContainer.GetComponent<CanvasGroup>() == null)
                    {
                        buttonContainer.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }
        }
    }

    private void CalculateLayout()
    {
        if (blades.Count == 0)
        {
            CollectBlades();
        }

        if (blades.Count == 0) return;

        int count = blades.Count;
        targetPositions = new Vector3[count];
        targetScales = new Vector3[count];
        targetAlphas = new float[count];
        targetSortOrders = new int[count];

        // Calculate positions starting from focused blade
        // Work left from focus, then right from focus
        for (int i = 0; i < count; i++)
        {
            var blade = blades[i];

            // Set pivot to center
            blade.RectTransform.pivot = new Vector2(0.5f, 0.5f);
            blade.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            blade.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            // Calculate distance from focus
            int distanceFromFocus = Mathf.Abs(i - focusedIndex);

            // Scale based on focus
            float scale = (i == focusedIndex) ? focusScale : Mathf.Pow(unfocusedScale, distanceFromFocus);
            targetScales[i] = new Vector3(1, scale, 1);

            // Alpha
            targetAlphas[i] = (i == focusedIndex) ? 1f : unfocusedAlpha;

            // Sort order - closer to focus = higher sort order (rendered on top)
            // But all below the overlay
            targetSortOrders[i] = 50 - distanceFromFocus;

            // Set blade size
            blade.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bladeSize.x);
            blade.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bladeSize.y);
        }

        // Now calculate positions based on visible width
        float currentX = 0f;

        // Position focused blade at center
        targetPositions[focusedIndex] = new Vector3(0f, 0f, 0f);

        // Position blades to the right of focus
        for (int i = focusedIndex + 1; i < count; i++)
        {
            if (i == focusedIndex + 1)
            {
                // First blade to the right - position at visible width from center
                currentX = visibleWidth;
            }
            else
            {
                // Subsequent blades - just add visible width
                currentX += visibleWidth;
            }

            float z = use3DPositioning ? -(i - focusedIndex) * depthPerBlade : 0f;
            targetPositions[i] = new Vector3(currentX, 0f, z);
        }

        // Position blades to the left of focus
        for (int i = focusedIndex - 1; i >= 0; i--)
        {
            if (i == focusedIndex - 1)
            {
                // First blade to the left - position at visible width from center
                currentX = -visibleWidth;
            }
            else
            {
                // Subsequent blades - just subtract visible width
                currentX -= visibleWidth;
            }

            float z = use3DPositioning ? -(focusedIndex - i) * depthPerBlade : 0f;
            targetPositions[i] = new Vector3(currentX, 0f, z);
        }

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        for (int i = 0; i < blades.Count; i++)
        {
            var blade = blades[i];
            Canvas canvas = bladeCanvases[i];
            CanvasGroup canvasGroup = blade.GetComponent<CanvasGroup>();

            // Ensure CanvasGroup exists for alpha control
            if (canvasGroup == null)
            {
                canvasGroup = blade.gameObject.AddComponent<CanvasGroup>();
            }

            // Set sort order
            canvas.sortingOrder = targetSortOrders[i] + sortOrderBase;

            // Show/hide buttons based on focus
            Transform buttonContainer = blade.RectTransform.Find("ButtonContainer");
            if (buttonContainer != null)
            {
                CanvasGroup buttonCanvasGroup = buttonContainer.GetComponent<CanvasGroup>();
                if (buttonCanvasGroup != null)
                {
                    if (Application.isPlaying && enabled)
                    {
                        buttonCanvasGroup.DOKill();
                        float targetAlpha = (i == focusedIndex) ? 1f : 0f;

                        buttonCanvasGroup.DOFade(targetAlpha, transitionFadeTime).SetEase(Ease.OutQuad);

                        // Also disable interaction when not visible
                        buttonCanvasGroup.interactable = (i == focusedIndex);
                        buttonCanvasGroup.blocksRaycasts = (i == focusedIndex);
                    }
                    else
                    {
                        buttonCanvasGroup.alpha = (i == focusedIndex) ? 1f : 0f;
                        buttonCanvasGroup.interactable = (i == focusedIndex);
                        buttonCanvasGroup.blocksRaycasts = (i == focusedIndex);
                    }
                }
            }

            if (Application.isPlaying && enabled)
            {
                // Smooth transitions
                blade.DOKill();
                canvasGroup.DOKill();

                if (use3DPositioning)
                {
                    blade.RectTransform.DOAnchorPos3D(targetPositions[i], transitionTime).SetEase(Ease.OutQuad);
                }
                else
                {
                    blade.RectTransform.DOAnchorPos(targetPositions[i], transitionTime).SetEase(Ease.OutQuad);
                }

                blade.RectTransform.DOScale(targetScales[i], transitionTime).SetEase(Ease.OutQuad);
                canvasGroup.DOFade(targetAlphas[i], transitionTime).SetEase(Ease.OutQuad);
            }
            else
            {
                // Immediate positioning in editor
                if (use3DPositioning)
                {
                    blade.RectTransform.anchoredPosition3D = targetPositions[i];
                }
                else
                {
                    blade.RectTransform.anchoredPosition = targetPositions[i];
                }

                blade.RectTransform.localScale = targetScales[i];
                canvasGroup.alpha = targetAlphas[i];
            }
        }

        // Animate the focus overlay to the focused blade
        if (focusOverlay != null)
        {
            if (Application.isPlaying && enabled)
            {
                focusOverlay.DOKill();

                if (use3DPositioning)
                {
                    focusOverlay.RectTransform.DOAnchorPos3D(targetPositions[focusedIndex], overlayTransitionTime).SetEase(Ease.OutQuad);
                }
                else
                {
                    focusOverlay.RectTransform.DOAnchorPos(targetPositions[focusedIndex], overlayTransitionTime).SetEase(Ease.OutQuad);
                }

                focusOverlay.RectTransform.DOScale(targetScales[focusedIndex], overlayTransitionTime).SetEase(Ease.OutQuad);
            }
            else
            {
                // Immediate positioning in editor
                if (use3DPositioning)
                {
                    focusOverlay.RectTransform.anchoredPosition3D = targetPositions[focusedIndex];
                }
                else
                {
                    focusOverlay.RectTransform.anchoredPosition = targetPositions[focusedIndex];
                }

                focusOverlay.RectTransform.localScale = targetScales[focusedIndex];
            }
            
            focusOverlay.SetTitle(GetFocusedBlade().BladeName);
        }
    }

    // Navigation methods
    public void MoveLeft(float speed = 1)
    {
        var previousTile = blades[focusedIndex];
        FocusedIndex = Mathf.Max(0, focusedIndex - 1);
        var newTile = blades[focusedIndex];
        
        if (previousTile != newTile)
        {
            newTile.Focus();
            audioSource.PlayOneShot(moveAudioClips[Random.Range(0, moveAudioClips.Length)]);
        }
    }

    public void MoveRight(float speed = 1)
    {
        var previousTile = blades[focusedIndex];
        FocusedIndex = Mathf.Min(blades.Count - 1, focusedIndex + 1);
        var newTile = blades[focusedIndex];

        if (previousTile != newTile)
        {
            newTile.Focus();
            audioSource.PlayOneShot(moveAudioClips[Random.Range(0, moveAudioClips.Length)]);
        }
    }

    public GuideMenuBlade GetFocusedBlade()
    {
        if (focusedIndex >= 0 && focusedIndex < blades.Count)
        {
            return blades[focusedIndex];
        }

        return null;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetupFocusOverlay();
        CalculateLayout();
    }
#endif
    public void SetEnabled(bool value)
    {
        enabled = value;
    }

    public void MoveUp()
    {
        //Does nothing, Unity handles buttons
    }

    public void MoveDown()
    {
        //Does nothing, Unity handles buttons
    }

    public void Select()
    {
        //Does nothing, Unity handles buttons
    }

    public void SelectAlt()
    {
        //TODO: Implement app closing as well
        FindFirstObjectByType<GuideMenuManager>().CloseGuide();
    }

    public void Cancel()
    {
        FindFirstObjectByType<GuideMenuManager>().CloseGuide();
    }
}