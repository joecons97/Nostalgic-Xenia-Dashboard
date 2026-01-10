using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class NXEVerticalLayoutGroup : LayoutGroup
{
    private List<NXEBlade> rows = new List<NXEBlade>();
    private Vector3[] targetPositions;

    public List<NXEBlade> Rows => rows;

    [SerializeField] private float titleDownscale = 0.8f;
    [SerializeField] private float titleOffset = -50;

    [SerializeField] private int focusedIndex = 0;
    [SerializeField] private float transitionSpeed = 8f;


    public int FocusedIndex
    {
        get => focusedIndex;
        set
        {
            focusedIndex = Mathf.Clamp(value, 0, transform.childCount - 1);
            Layout();
        }
    }

    public void MoveDown()
    {
        FocusedIndex = Mathf.Max(0, focusedIndex - 1);
    }

    public void MoveUp()
    {
        FocusedIndex = Mathf.Min(rows.Count - 1, focusedIndex + 1);
    }

    public override void CalculateLayoutInputVertical()
    {
        Layout();
    }

    public override void SetLayoutHorizontal()
    {
        CollectRows();
    }

    public override void SetLayoutVertical()
    {
        Layout();
    }

    private void CollectRows()
    {
        rows.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetComponent<NXEBlade>();
            if (child != null && child.gameObject.activeSelf)
            {
                rows.Add(child);
            }
        }
    }

    private void Layout()
    {
        if (rows.Count == 0)
        {
            CollectRows();
        }

        if (rows.Count == 0) return;

        var currentY = 0.0f;
        int count = rows.Count;

        targetPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            var tile = rows[i];

            // Calculate how many steps this tile is from the focused one
            int stepsFromFocus = i - focusedIndex;
            var scalePower = stepsFromFocus;
            if(scalePower < 0)
                scalePower = count + stepsFromFocus;
            
            float textScale = Mathf.Pow(titleDownscale, scalePower);
            tile.TitleTransform.localScale = new Vector3(textScale, textScale, 1);

            if (stepsFromFocus != 0)
            {
                var index = i - 1;
                if (index < 0)
                {
                    index = count + index;
                    Debug.Log(index);
                }

                currentY = targetPositions[index].y + (titleOffset * textScale);
                if ((i - 1) < 0)
                    Debug.Log(currentY);
                
                tile.UnFocus(stepsFromFocus);
            }
            else
            {
                currentY = 0;
                tile.Focus();
            }

            targetPositions[i] = new Vector3(0, currentY, 0f);
        }

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            var tile = rows[i];

            //if (Application.isPlaying)
            //{
            //    // Smooth transitions during play
            //    tile.RectTransform.anchoredPosition = Vector2.Lerp(tile.RectTransform.anchoredPosition, targetPositions[i], Time.deltaTime * transitionSpeed);
            //}
            //else
            {
                // Immediate positioning in editor
                tile.RectTransform.anchoredPosition = targetPositions[i];
            }
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            ApplyLayout();

            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveUp();
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveDown();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        Layout();
    }
#endif
}