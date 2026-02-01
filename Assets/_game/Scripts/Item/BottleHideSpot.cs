using UnityEngine;

public class BottleHideSpot : MonoBehaviour
{
    [Header("Bottle ID")]
    public int bottleID;

    [Header("Puzzle Manager")]
    [SerializeField] private BottlePuzzleManager puzzleManager;

    private void Awake()
    {
        if (!puzzleManager)
            puzzleManager = FindFirstObjectByType<BottlePuzzleManager>();
    }

    // Llamar cuando el jugador se esconda aquí
    public void OnPlayerHide()
    {
        if (puzzleManager)
            puzzleManager.RegisterBottle(bottleID);
    }
}
