using JetBrains.Annotations;
using UnityEngine;

public class NXEModalOpenerTile : NXETile
{
    [SerializeField] private NXEModal modal;
    
    [CanBeNull] private NXEModal currentModal;
    
    public override void OnSelect()
    {
        if (currentModal == null)
        {
            currentModal = NXEModal.CreateAndShow(modal);
        }
    }

    public override void OnCancel()
    {
        if(currentModal != null)
        {
            var result = currentModal.Close();

            if (result == NXEModalCloseResult.NormalClose)
            {
                currentModal = null;
            }
        }
    }
}
