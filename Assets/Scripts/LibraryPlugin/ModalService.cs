using System;
using UnityEngine;

namespace LibraryPlugin
{
    public class CreateModalArgs
    {
        public GameObject ChildrenRoot;
        public string Name;
        public bool CanBeClosed;
        public bool DisplaySelectAction;
    }
    
    public class ModalService
    {
        public static event Func<CreateModalArgs, string> OnRequestCreateModal;
        public static event Action<string> OnRequestCloseModal;
        public static event Action<string, Action> OnRequestSetCloseCallback;        

        public string CreateModal(CreateModalArgs args)
        {
            return OnRequestCreateModal?.Invoke(args);
        }

        public void CloseModal(string modalId)
        {
            OnRequestCloseModal?.Invoke(modalId);
        }
        
        public void SetCloseCallback(string modalId, Action callback)
        {
            OnRequestSetCloseCallback?.Invoke(modalId, callback);
        }
    }
}