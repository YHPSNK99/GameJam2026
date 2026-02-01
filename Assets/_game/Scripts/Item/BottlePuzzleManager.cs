using UnityEngine;

public class BottlePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("Orden correcto: ejemplo 0,1,2 significa botella 0 luego 1 luego 2")]
    [SerializeField] private int[] correctOrder;

    private int currentStep = 0;

    [Header("Door")]
    [SerializeField] private DoorController door;

    public void RegisterBottle(int bottleID)
    {
        // Puzzle ya completado
        if (currentStep >= correctOrder.Length)
            return;

        // Correcto
        if (bottleID == correctOrder[currentStep])
        {
            Debug.Log("Correct bottle: " + bottleID);

            currentStep++;

            // Puzzle completado
            if (currentStep >= correctOrder.Length)
            {
                Debug.Log("Puzzle completed!");
                door.OpenDoor();
            }
        }
        else
        {
            Debug.Log("Wrong bottle! Reset puzzle.");
            ResetPuzzle();
        }
    }

    private void ResetPuzzle()
    {
        currentStep = 0;
    }
}
