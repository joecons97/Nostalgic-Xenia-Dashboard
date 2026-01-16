using System;
using UnityEngine;

namespace LibraryPlugin
{
    public class CreateModalArgs
    {
        public GameObject ChildrenRoot;
        public string Name;
        public bool CanBeClosed;
    }
    
    public class ModalService
    {
        public static event Func<CreateModalArgs, string> OnRequestCreateModal;
        public static event Action<string> OnRequestCloseModal;

        public string CreateModal(CreateModalArgs args)
        {
            return OnRequestCreateModal.Invoke(args);
        }

        public void CloseModal(string modalId)
        {
            OnRequestCloseModal.Invoke(modalId);
        }
    }
}