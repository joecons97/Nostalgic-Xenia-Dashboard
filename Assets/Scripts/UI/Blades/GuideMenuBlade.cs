using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
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
        if (defaultSelection)
        {
            //Hacky but stops the selection sound from playing on modal open
            var eventTrigger = defaultSelection.GetComponent<EventTrigger>();
            eventTrigger.enabled = false;
            defaultSelection.Select();
            _ = UniTask.NextFrame(destroyCancellationToken).ContinueWith(() =>
            {
                 if(eventTrigger)
                    eventTrigger.enabled = true;
            });
        }
    }

    private void OnValidate()
    {
        SetTitle(bladeName);
    }
}
