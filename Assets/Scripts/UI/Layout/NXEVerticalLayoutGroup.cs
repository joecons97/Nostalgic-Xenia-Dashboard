using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteAlways]
public class NXEVerticalLayoutGroup : LayoutGroup
{
    private List<NXEBlade> rows = new List<NXEBlade>();
    private Vector3[] targetPositions;

    public List<NXEBlade> Rows => rows;

    [Header("Layout")] [SerializeField] private float titleDownscale = 0.8f;
    [SerializeField] private float titleOffset = -50;
    [SerializeField] private int focusedIndex = 0;


    [Header("Transitions")] [SerializeField]
    private float transitionTime = 8f;

    [SerializeField] private Ease transitionEase = Ease.OutQuad;

    [Header("Audio")] [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cycleUpAudio;
    [SerializeField] private AudioClip cycleDownAudio;
    [SerializeField] private AudioClip cycleLeftAudio;
    [SerializeField] private AudioClip cycleRightAudio;

    public RectTransform RectTransform => transform as RectTransform;

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
        if (enabled == false)
            return;

        if (Rows.Count < 2)
            return;

        Rows[^1].transform.SetAsFirstSibling();
        Rows[^1].RectTransform.anchoredPosition = new Vector3(0, -titleOffset * (1 / titleDownscale), 0);
        Layout();

        audioSource.PlayOneShot(cycleDownAudio);
    }

    public void MoveUp()
    {
        if (enabled == false)
            return;

        if (Rows.Count < 2)
            return;

        Rows[FocusedIndex].transform.SetAsLastSibling();
        float textScale = Mathf.Pow(titleDownscale, Rows.Count);
        Rows[FocusedIndex].RectTransform.anchoredPosition = new Vector3(0, targetPositions[^1].y + (titleOffset * textScale), 0);
        Layout();

        audioSource.PlayOneShot(cycleUpAudio);
    }

    public void Hide()
    {
        Rows[FocusedIndex].enabled = false;
        transform.parent.GetComponent<CanvasGroup>().DOFade(0, transitionTime).SetDelay(transitionTime);
        enabled = false;
    }

    public void Show()
    {
        transform.parent.GetComponent<CanvasGroup>().DOFade(1, transitionTime)
            .OnComplete(() =>
                Rows[FocusedIndex].enabled = true);

        enabled = true;
    }

    public void Select()
    {
        Rows[FocusedIndex].Select();
    }

    public void SelectAlt()
    {
        Rows[FocusedIndex].SelectAlt();
    }

    public void Cancel()
    {
        Rows[FocusedIndex].Cancel();
    }

    public void MoveLeft(float speed = 1)
    {
        if (enabled)
        {
            CollectRows();
            if (Rows[focusedIndex].MoveLeft(speed))
                audioSource.PlayOneShot(cycleLeftAudio);

            Layout();
        }
        else
        {
            Rows[focusedIndex].MoveLeftPassthrough(speed);
        }
    }

    public void MoveRight(float speed = 1)
    {
        if (enabled)
        {
            CollectRows();
            if (Rows[focusedIndex].MoveRight(speed))
                audioSource.PlayOneShot(cycleRightAudio);

            Layout();
        }
        else
        {
            Rows[focusedIndex].MoveRightPassthrough(speed);
        }
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

        int count = rows.Count;

        targetPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            var currentY = 0.0f;
            var tile = rows[i];

            // Calculate how many steps this tile is from the focused one
            int stepsFromFocus = i - focusedIndex;

            float textScale = Mathf.Pow(titleDownscale, stepsFromFocus);
            tile.TitleTransform.localScale = new Vector3(textScale, textScale, 1);

            if (stepsFromFocus > 0)
            {
                var index = i - 1;
                currentY = targetPositions[index].y + (titleOffset * textScale);

                tile.UnFocus(stepsFromFocus);
            }
            else
            {
                currentY = 0;
                if (stepsFromFocus < 0)
                    tile.UnFocus(stepsFromFocus, NXEBladeUnfocusParams.FullHide);
                else
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

            if (Application.isPlaying)
            {
                // Smooth transitions during play
                tile.RectTransform.DOKill();
                tile.RectTransform
                    .DOAnchorPos(targetPositions[i], transitionTime)
                    .SetEase(transitionEase);
            }
            else
            {
                // Immediate positioning in editor
                tile.RectTransform.anchoredPosition = targetPositions[i];
            }
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