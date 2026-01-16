using System;
using System.Collections.Generic;
using LibraryPlugin;
using UnityEngine;
using UnityEngine.UI;

public class ModalServiceManager : MonoBehaviour
{
    private Dictionary<string, NXEModal> externallyCreatedModals = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ModalService.OnRequestCreateModal += OnRequestCreateModal;
        ModalService.OnRequestCloseModal += OnRequestCloseModal;
    }

    private string OnRequestCreateModal(CreateModalArgs args)
    {
        Debug.Log("Creating modal");

        var empty = Resources.Load<NXEModal>("EmptyModal");
        var guid = Guid.NewGuid().ToString("N");

        NXEModal modal;
        if(NXEModal.TopMostModal == null)
        {
            modal = NXEModal.CreateAndShow(empty);
        }
        else
        {
            NXEModal.TopMostModal.OpenSubModal(empty);
            modal = NXEModal.TopMostModal.SubModal;
        }

        modal.name = guid;
        modal.canBeClosed = args.CanBeClosed;

        modal.transform.Find("Title").GetComponent<Text>().text = args.Name;

        externallyCreatedModals.Add(guid, modal);

        if (args.ChildrenRoot != null)
        {
            var panel = modal.GetComponentInChildren<VerticalLayoutGroup>();
            args.ChildrenRoot.transform.SetParent(panel.transform, false);
        }

        Debug.Log("Openned modal: " + guid);

        return guid;
    }

    private void OnRequestCloseModal(string guid)
    {
        if (externallyCreatedModals.TryGetValue(guid, out var modal))
        {
            modal.canBeClosed = true;
            NXEModalCloseResult result;
            if (modal.ParentModal != null)
                result = modal.ParentModal.Close();
            else
                result = modal.Close(); 

            if(result != NXEModalCloseResult.None)
                externallyCreatedModals.Remove(guid);
        }
    }

    void OnDestroy()
    {
        ModalService.OnRequestCreateModal -= OnRequestCreateModal;
        ModalService.OnRequestCloseModal -= OnRequestCloseModal;
    }
}
