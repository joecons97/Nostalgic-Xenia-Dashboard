using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public enum NXEBladeUnfocusParams
{
    None,
    FullHide
}

[ExecuteAlways]
public class NXEBlade : MonoBehaviour
{
    [SerializeField] private string title;
    [SerializeField] private NXETile[] tiles;

    [SerializeField] private Text titleText;
    [SerializeField] private Text pagerText;
    [SerializeField] private NXEBladeLayoutGroup layoutGroup;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeOutTransitionTime = 0.25f;
    [SerializeField] private float fadeInTransitionTime = 0.125f;
    [SerializeField] private Ease transitionEase = Ease.OutQuad;

    [SerializeField, ReadOnly] private List<NXETile> tileInstances = new();

    public RectTransform TitleTransform => titleText.rectTransform;
    public RectTransform RectTransform => transform as RectTransform;
    public NXEBladeLayoutGroup LayoutGroup => layoutGroup;

    public IReadOnlyList<NXETile> Tiles => tileInstances;

    private int lastValidatedTilesLength;

    public void SetTitle(string newTitle)
    {
        title = newTitle;
        if (titleText)
            titleText.text = title;
    }

    public void SetTiles(NXETile[] newTiles)
    {
        tiles = newTiles;
        Rebuild();
    }

    public void MoveLeft(float speed = 1)
    {
        var previousTile = tiles[layoutGroup.FocusedIndex];

        layoutGroup.MoveLeft(speed);

        var newTile = tiles[layoutGroup.FocusedIndex];
        if (previousTile != newTile)
        {
            previousTile.OnUnFocus();
            newTile.OnFocus();
        }

        UpdatePagerText();
    }

    public void MoveRight(float speed = 1)
    {
        var previousTile = tiles[layoutGroup.FocusedIndex];

        layoutGroup.MoveRight(speed);

        var newTile = tiles[layoutGroup.FocusedIndex];
        if (previousTile != newTile)
        {
            previousTile.OnUnFocus();
            newTile.OnFocus();
        }

        UpdatePagerText();
    }

    private void UpdatePagerText()
    {
        if (pagerText == null)
        {
            var obj = GameObject.Find("PagerText");

            if (obj != null)
                pagerText = obj.GetComponent<Text>();
            else
                return;
        }
        pagerText.text = $"{layoutGroup.FocusedIndex + 1} of {tiles.Length}";
    }

    public void Select()
    {
        if (tileInstances.Count == 0)
            GatherExistingTiles();

        tileInstances[layoutGroup.FocusedIndex].OnSelect();
    }

    public void Cancel()
    {
        if (tileInstances.Count == 0)
            GatherExistingTiles();

        tileInstances[layoutGroup.FocusedIndex].OnCancel();
    }

    public void Focus()
    {
        titleText.color = Color.white;

        canvasGroup.DOKill();
        if (Application.isPlaying)
        {
            //Comment for later self: this is a hack to prevent the blade from moving up and the with the focus animation
            //Should reset when play mode ends
            if (layoutGroup.transform.parent == transform)
                layoutGroup.transform.SetParent(transform.parent, false);

            canvasGroup
                .DOFade(1, fadeInTransitionTime)
                .SetDelay(fadeOutTransitionTime)
                .SetEase(transitionEase);
        }
        else
            canvasGroup.alpha = 1;

        UpdatePagerText();
    }

    public void UnFocus(int focalIndex, NXEBladeUnfocusParams param = NXEBladeUnfocusParams.None)
    {
        canvasGroup.DOKill();
        if (Application.isPlaying)
        {
            //Comment for later self: this is a hack to prevent the blade from moving up and the with the unfocus animation
            //Should reset when play mode ends
            if (layoutGroup.transform.parent == transform)
                layoutGroup.transform.SetParent(transform.parent, false);

            canvasGroup.DOFade(0, fadeOutTransitionTime)
                .SetEase(transitionEase)
                .OnComplete(() => layoutGroup.ResetPosition());
        }
        else
            canvasGroup.alpha = 0;

        titleText.color = param == NXEBladeUnfocusParams.FullHide
            ? new Color(1, 1, 1, 0.0f)
            : new Color(1, 1, 1, 0.5f / focalIndex);
    }

    private void OnValidate()
    {
        if (titleText)
            titleText.text = title;

        //TODO Tile Pooling
        if (Application.isPlaying == false)
        {
            if (lastValidatedTilesLength != tiles.Length)
            {
                lastValidatedTilesLength = tiles.Length;
                Rebuild();
            }
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
            Destroy(layoutGroup.gameObject);
    }

    private void GatherExistingTiles()
    {
        tileInstances = layoutGroup.GetComponentsInChildren<NXETile>().ToList();
    }

    [ContextMenu("Rebuild")]
    private void Rebuild()
    {
        if (layoutGroup == null)
            return;

        List<GameObject> toDestroy = new();
        for (int i = 0; i < layoutGroup.transform.childCount; i++)
        {
            var child = layoutGroup.transform.GetChild(i);
            toDestroy.Add(child.gameObject);
        }

        tileInstances.Clear();

#if UNITY_EDITOR
        if (Application.isPlaying)
            ExecuteImmediate();
        else
            ExecuteDelayed();

        void ExecuteDelayed()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                foreach (var o in toDestroy)
                {
                    DestroyImmediate(o);
                }

                foreach (var nxeTile in tiles)
                {
                    if (nxeTile == null)
                        return;

                    tileInstances.Add(Instantiate(nxeTile, layoutGroup.transform));
                }
            };
        }
#else
        ExecuteImmediate();
#endif


        void ExecuteImmediate()
        {
            foreach (var o in toDestroy)
            {
                Destroy(o);
            }

            foreach (var nxeTile in tiles)
            {
                tileInstances.Add(Instantiate(nxeTile, layoutGroup.transform));
            }
        }
    }
}