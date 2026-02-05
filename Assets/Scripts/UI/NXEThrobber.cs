using DG.Tweening;
using UnityEngine;

public class NXEThrobber : MonoBehaviour
{
    [SerializeField] private Transform throbber;
    [SerializeField] private Transform spinner;
    
    [SerializeField] private float throbberSize = 1f;
    [SerializeField] private float throbberSpeed = 1f;
    
    [SerializeField] private float spinnerSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        throbber.DOScale(throbberSize, throbberSpeed)
            .ChangeStartValue(Vector3.one)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);

        spinner.DORotate(new Vector3(0, 0, 360), spinnerSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear) // Ensures constant, smooth speed
            .SetLoops(-1, LoopType.Restart) // -1 makes it loop infinitely
            .SetRelative(); // Rotates relative to current rotation
    }

    private void OnDestroy()
    {
        throbber.DOKill();
        spinner.DOKill();
    }
}
