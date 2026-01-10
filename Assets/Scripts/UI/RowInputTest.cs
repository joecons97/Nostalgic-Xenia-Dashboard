using UnityEngine;

public class RowInputTest : MonoBehaviour
{
    public NXEBlade test;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) test.MoveRight();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) test.MoveLeft();
    }
}
