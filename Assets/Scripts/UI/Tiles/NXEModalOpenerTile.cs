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
            FindFirstObjectByType<NXEVerticalLayoutGroup>()?.Hide();
        }
    }

    public override void OnCancel()
    {
        if(currentModal != null)
        {
            var result = currentModal.Close();

            if (result == NXEModalCloseResult.NormalClose)
            {
                FindFirstObjectByType<NXEVerticalLayoutGroup>()?.Show();
                currentModal = null;
            }
        }
    }
}
