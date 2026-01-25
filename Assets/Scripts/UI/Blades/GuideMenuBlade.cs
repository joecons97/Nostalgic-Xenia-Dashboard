using UnityEngine;
using UnityEngine.UI;

public class GuideMenuBlade : MonoBehaviour
{
    [SerializeField] private string bladeName;
    [SerializeField] private Selectable defaultSelection;

    [SerializeField] private Text[] titleText;
    
    public string BladeName => bladeName;
    public RectTransform RectTransform => transform as RectTransform;

    public void SetTitle(string newTitle)
    {
        bladeName = newTitle;
        foreach (var text in titleText)
        {
            text.text = bladeName;
        }
    }
    
    public void Focus()
    {
        if(defaultSelection)
            defaultSelection.Select();
    }

    private void OnValidate()
    {
        SetTitle(bladeName);
    }
}
