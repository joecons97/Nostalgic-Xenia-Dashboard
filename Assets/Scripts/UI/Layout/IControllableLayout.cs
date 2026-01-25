public interface IControllableLayout
{
    void SetEnabled(bool enabled);
    void MoveLeft(float speed = 1);
    void MoveRight(float speed = 1);
    void MoveUp();
    void MoveDown();
    void Select();
    void SelectAlt();
    void Cancel();
}