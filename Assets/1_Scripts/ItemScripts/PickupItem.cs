using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    [Header("Item Data")]
    public ItemDefinition itemDefinition;

    [Header("UI Prompt")]
    public GameObject promptUI;

    private InventoryBackpackController inventory;
    private bool playerInRange;

    void Awake()
    {
        inventory = FindObjectOfType<InventoryBackpackController>();
        promptUI.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            promptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            TryPickUp();
    }

    private void TryPickUp()
    {
        var fit = inventory.FindFirstFit(itemDefinition);
        if (fit.HasValue)
        {
            inventory.AddItemToGrid(itemDefinition, fit.Value);
            Destroy(gameObject);   // remove from world
        }
        else
        {
            // optional: flash “Inventory full” message instead
            Debug.Log("Your backpack is full!");
        }
    }
}