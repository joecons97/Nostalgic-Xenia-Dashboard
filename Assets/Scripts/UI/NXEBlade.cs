using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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

    public RectTransform TitleTransform => titleText.rectTransform;
    public RectTransform RectTransform => transform as RectTransform;

    private int lastValidatedTilesLength;

    public void MoveLeft()
    {
        var previousTile = tiles[layoutGroup.FocusedIndex];

        layoutGroup.MoveLeft();

        var newTile = tiles[layoutGroup.FocusedIndex];
        if (previousTile != newTile)
        {
            previousTile.OnUnFocus();
            newTile.OnFocus();
        }
        
        UpdatePagerText();
    }

    public void MoveRight()
    {
        var previousTile = tiles[layoutGroup.FocusedIndex];

        layoutGroup.MoveRight();

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
        pagerText.text = $"{layoutGroup.FocusedIndex + 1} of {tiles.Length}";
    }

    public void Select()
    {
        tiles[layoutGroup.FocusedIndex].OnSelect();
    }

    public void AltSelect()
    {
        tiles[layoutGroup.FocusedIndex].OnAltSelect();
    }

    public void Focus()
    {
        titleText.color = Color.white;
        
        canvasGroup.DOKill();
        if (Application.isPlaying)
        {
            //Comment for later self: this is a hack to prevent the blade from moving up and the with the focus animation
            //Should reset when play mode ends
            if(layoutGroup.transform.parent == transform)
                layoutGroup.transform.SetParent(transform.parent, false);
            
            canvasGroup
                .DOFade(1, fadeInTransitionTime)
                .SetDelay(fadeOutTransitionTime);
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
            if(layoutGroup.transform.parent == transform)
                layoutGroup.transform.SetParent(transform.parent, false);

            canvasGroup.DOFade(0, fadeOutTransitionTime);
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

    [ContextMenu("Rebuild")]
    private void Rebuild()
    {
        List<GameObject> toDestroy = new();
        for (int i = 0; i < layoutGroup.transform.childCount; i++)
        {
            var child = layoutGroup.transform.GetChild(i);
            toDestroy.Add(child.gameObject);
        }

        UnityEditor.EditorApplication.delayCall += () =>
        {
            foreach (var o in toDestroy)
            {
                DestroyImmediate(o);
            }

            foreach (var nxeTile in tiles)
            {
                var p = Instantiate(nxeTile, layoutGroup.transform);
            }
        };
    }
}