using UnityEngine;
using UnityEngine.InputSystem;

public class NXEInputHandler : MonoBehaviour
{
    InputAction navigateAction;
    
    [Header("References")]
    [SerializeField] private NXEVerticalLayoutGroup verticalLayout;
    
    [Header("Repeat Settings")]
    [SerializeField] private float initialDelay = 0.5f; // Time before repeat starts
    [SerializeField] private float repeatRate = 0.1f; // Time between repeats
    
    private Vector2 currentInput = Vector2.zero;
    private Vector2 lastExecutedDirection = Vector2.zero;
    private float holdTime = 0f;
    private float nextRepeatTime = 0f;
    private bool wasHolding = false;
    
    void Start()
    {
        navigateAction = InputSystem.actions.FindAction("Navigate");
        
        //navigateAction.performed += OnNavigate;
       // navigateAction.canceled += OnNavigate;
    }

   private void Update()
    {
        if (navigateAction != null)
        {
            currentInput = navigateAction.ReadValue<Vector2>();
        }
        
        bool isHolding = currentInput.sqrMagnitude > 0.1f;

        if (isHolding)
        {
            Vector2 currentDirection = GetDiscreteDirection(currentInput);
            
            // Check if this is a new press or direction change
            if (!wasHolding || currentDirection != lastExecutedDirection)
            {
                // New input - immediate response
                holdTime = 0f;
                nextRepeatTime = initialDelay;
                ExecuteNavigation(currentInput);
                lastExecutedDirection = currentDirection;
            }
            else
            {
                // Holding same direction - handle repeat
                holdTime += Time.deltaTime;

                if (holdTime >= nextRepeatTime)
                {
                    ExecuteNavigation(currentInput);
                    nextRepeatTime = holdTime + repeatRate;
                }
            }
        }
        else if (wasHolding)
        {
            // Just released
            holdTime = 0f;
            lastExecutedDirection = Vector2.zero;
        }

        wasHolding = isHolding;
    }


    private Vector2 GetDiscreteDirection(Vector2 input)
    {
        // Convert analog input to discrete direction
        Vector2 direction = Vector2.zero;
        
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            direction.x = Mathf.Sign(input.x);
        }
        else if (Mathf.Abs(input.y) > 0.1f)
        {
            direction.y = Mathf.Sign(input.y);
        }
        
        return direction;
    }

    private void ExecuteNavigation(Vector2 input)
    {
        // Determine primary direction
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            // Horizontal movement
            if (verticalLayout != null)
            {
                if (input.x > 0)
                    verticalLayout.MoveRight();
                else if (input.x < 0)
                    verticalLayout.MoveLeft();
            }
        }
        else
        {
            // Vertical movement
            if (verticalLayout != null)
            {
                if (input.y > 0)
                    verticalLayout.MoveUp();
                else if (input.y < 0)
                    verticalLayout.MoveDown();
            }
        }
    }
}
