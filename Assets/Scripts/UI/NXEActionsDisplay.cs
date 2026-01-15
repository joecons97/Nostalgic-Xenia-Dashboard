using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ActionsConfig
{
    public bool showSelectAction;
    public string selectActionText;
    public bool showSelectAltAction;
    public string selectAltActionText;
    public bool showCancelAction;
    public string cancelActionText;
}

public class NXEActionsDisplay : MonoBehaviour
{
    [SerializeField] private GameObject selectActionButton;
    [SerializeField] private GameObject selectAltActionButton;
    [SerializeField] private GameObject CancelActionButton;

    public void SetConfig(ActionsConfig config)
    {
        selectActionButton.SetActive(config.showSelectAction);
        selectAltActionButton.SetActive(config.showSelectAltAction);
        CancelActionButton.SetActive(config.showCancelAction);

        if (selectActionButton.activeSelf)
            selectActionButton.GetComponentInChildren<Text>().text = config.selectActionText;
        if (selectAltActionButton.activeSelf)
            selectAltActionButton.GetComponentInChildren<Text>().text = config.selectAltActionText;
        if (CancelActionButton.activeSelf)
            CancelActionButton.GetComponentInChildren<Text>().text = config.cancelActionText;
    }
}
