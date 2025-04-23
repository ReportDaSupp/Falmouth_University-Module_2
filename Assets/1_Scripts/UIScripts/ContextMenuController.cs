using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContextMenuController : MonoBehaviour
{
    public static ContextMenuController Instance { get; private set; }

    [Header("UI References")]
    public RectTransform menuPanel;      // assign ContextMenuPanel
    public Button        useButton;      // assign UseButton
    public Button        dropButton;     // assign DropButton
    public Button        inspectButton;  // assign InspectButton

    private InventoryItemView currentItem;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        menuPanel.gameObject.SetActive(false);

        useButton.onClick    .AddListener(OnUse);
        dropButton.onClick   .AddListener(OnDrop);
        inspectButton.onClick.AddListener(OnInspect);

        // Hide when clicking elsewhere
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Show the menu for the given item, at screenPos.
    /// </summary>
    public void Show(InventoryItemView itemView, Vector2 screenPos)
    {
        currentItem = itemView;
        menuPanel.gameObject.SetActive(true);

        // position near cursor
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            menuPanel.parent as RectTransform,
            screenPos,
            null,
            out pos
        );
        menuPanel.anchoredPosition = pos;
    }

    public void Hide()
    {
        menuPanel.gameObject.SetActive(false);
        currentItem = null;
    }

    private void OnUse()
    {
        Debug.Log($"Use {currentItem.itemData.itemName}");
        // TODO: equip or consume
        Hide();
    }

    private void OnInspect()
    {
        Debug.Log($"Inspect {currentItem.itemData.itemName}");
        // TODO: show details
        Hide();
    }

    private void OnDrop()
    {
        if (currentItem == null) return;
        // drop at player feet
        var inv    = FindObjectOfType<InventoryBackpackController>();
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        bool ok = inv.DropItemAtOrigin(
            currentItem.itemData,
            currentItem.origin,
            player.position + Vector3.down * 0.5f  // slight offset
        );
        if (!ok) Debug.LogWarning("Drop failed");
        Hide();
    }
}
