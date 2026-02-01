using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Visual")]
    [SerializeField] private GameObject doorObject;

    [Header("Collider")]
    [SerializeField] private Collider2D doorCollider;

    public void OpenDoor()
    {
        Debug.Log("Door opened!");

        if (doorObject)
            doorObject.SetActive(false);

        if (doorCollider)
            doorCollider.enabled = false;
    }
}
