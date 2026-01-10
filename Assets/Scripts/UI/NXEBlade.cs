using System.Collections.Generic;
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
    [SerializeField] private NXEBladeLayoutGroup layoutGroup;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float transitionSpeed = 8f;

    public RectTransform TitleTransform => titleText.rectTransform;
    public RectTransform RectTransform => transform as RectTransform;

    private float targetOpacity;
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
        targetOpacity = 1;
        titleText.color = Color.white;
    }

    public void UnFocus(int focalIndex, NXEBladeUnfocusParams param = NXEBladeUnfocusParams.None)
    {
        targetOpacity = 0;
        canvasGroup.alpha = targetOpacity;

        titleText.color = param == NXEBladeUnfocusParams.FullHide
            ? new Color(1, 1, 1, 0.0f)
            : new Color(1, 1, 1, 0.5f / focalIndex);
    }

    private void Update()
    {
        if (Application.isPlaying)
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetOpacity, Time.deltaTime * transitionSpeed);
        else
            canvasGroup.alpha = targetOpacity;
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