using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class NXEBladeLayoutGroup : LayoutGroup
{
    [Header("Blade Layout Settings")]
    [SerializeField] private Vector2 baseTileSize = new Vector2(400f, 400f);
    [SerializeField] private float scaleMultiplier = 0.9f; // Each tile is 90% of previous
    [SerializeField] private float overlapAmount = 0.25f; // 25% of tile hidden behind previous
    [SerializeField] private float heightLowerAmount = 50f;
    [SerializeField] private float leftPadding = 50f; // Distance from left edge
    
    [Header("Selection")]
    [SerializeField] private int focusedIndex = 0;
    [SerializeField] private float transitionSpeed = 8f;
    
    private List<RectTransform> tiles = new List<RectTransform>();
    private Vector3[] targetPositions;
    private Vector2[] targetSizes;
    
    public List<RectTransform> Tiles => tiles;
    
    public RectTransform RectTransform => transform as RectTransform;

    public int FocusedIndex
    {
        get => focusedIndex;
        set
        {
            focusedIndex = Mathf.Clamp(value, 0, transform.childCount - 1);
            CalculateLayout();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CalculateLayout();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        CollectTiles();
    }

    public override void CalculateLayoutInputVertical()
    {
        CollectTiles();
    }

    public override void SetLayoutHorizontal()
    {
        CalculateLayout();
    }

    public override void SetLayoutVertical()
    {
        // Handled in SetLayoutHorizontal
    }

    private void CollectTiles()
    {
        tiles.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child != null && child.gameObject.activeSelf)
            {
                tiles.Add(child);
                child.GetComponent<Canvas>().sortingOrder = transform.childCount - i;
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseTileSize.x);
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseTileSize.y);
            }
        }
    }

    private void CalculateLayout()
    {
        if (tiles.Count == 0)
        {
            CollectTiles();
        }

        if (tiles.Count == 0) return;

        int count = tiles.Count;
        targetPositions = new Vector3[count];
        targetSizes = new Vector2[count];

        float currentX = leftPadding;
        float currentY = 0f;

        for (int i = 0; i < count; i++)
        {
            RectTransform tile = tiles[i];
            
            // Calculate how many steps this tile is from the focused one
            int stepsFromFocus = i - focusedIndex;
            
            // Calculate scale based on position relative to focus
            float scale = Mathf.Pow(scaleMultiplier, stepsFromFocus);
            targetSizes[i] = new Vector2(scale, scale);

            // Set pivot to left-top for easier positioning
            tile.pivot = new Vector2(0f, 1);
            tile.anchorMin = new Vector2(0f, 0.5f);
            tile.anchorMax = new Vector2(0f, 0.5f);

            var overlap = 1 - Mathf.Pow(1 - overlapAmount, stepsFromFocus);
            if (stepsFromFocus == 1)
                overlap = 0.01f;
            
            // Position calculation
            if (i == focusedIndex)
            {
                // Focused tile is at the left
                currentX = leftPadding;
            }
            else if (stepsFromFocus > 0)
            {
                // Tiles to the right of focus
                // Previous tile's right edge minus the overlap
                Vector2 prevSize = Vector2.Scale(baseTileSize, targetSizes[i - 1]);
                currentX = currentX + prevSize.x * (1f - overlap);
                currentY = -(heightLowerAmount * (1 - scale));
            }
            else
            {
                // Tiles to the left of focus (if focus isn't at index 0)
                // These would be off-screen or handled differently
                currentX = leftPadding - (baseTileSize.x * (-stepsFromFocus * 2));
            }

            targetPositions[i] = new Vector3(currentX, currentY, 0f);

            // Size will be applied in ApplyLayout for smooth transitions
        }

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            RectTransform tile = tiles[i];
            
            if (Application.isPlaying)
            {
                // Smooth transitions during play
                //TODO Replace with DOTween
                tile.anchoredPosition = Vector2.Lerp(tile.anchoredPosition, targetPositions[i], Time.deltaTime * transitionSpeed);
                
                // Smoothly interpolate size
                Vector2 currentSize = tile.localScale;
                //TODO Replace with DOTween
                Vector2 newSize = Vector2.Lerp(currentSize, targetSizes[i], Time.deltaTime * transitionSpeed);
                tile.localScale = new Vector3(newSize.x, newSize.y, 1);
            }
            else
            {
                // Immediate positioning in editor
                tile.anchoredPosition = targetPositions[i];
                tile.localScale = new Vector3(targetSizes[i].x, targetSizes[i].y, 1);
            }
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            ApplyLayout();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CalculateLayout();
    }
#endif

    // Helper methods for navigation
    public void MoveLeft()
    {
        FocusedIndex = Mathf.Max(0, focusedIndex - 1);
    }

    public void MoveRight()
    {
        FocusedIndex = Mathf.Min(tiles.Count - 1, focusedIndex + 1);
    }

    public RectTransform GetFocusedTile()
    {
        if (focusedIndex >= 0 && focusedIndex < tiles.Count)
        {
            return tiles[focusedIndex];
        }
        return null;
    }
}