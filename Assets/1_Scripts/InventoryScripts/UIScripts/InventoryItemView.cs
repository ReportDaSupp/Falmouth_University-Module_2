using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class InventoryItemView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image icon;
    public Vector2Int origin { get; set; }
    public ItemDefinition itemData { get; private set; }
    
    public Vector2Int GrabOffset   { get; private set; }
    public IEnumerable<Vector2Int> CurrentShape => itemData.shape;
    public RectTransform RectTransform => rect;

    private InventoryBackpackController controller;
    private RectTransform rect;
    private Vector2 offset;
    private Vector2 cellSize;
    private Vector2Int grabOffset;

    public void Initialize(InventorySlotData slot, InventoryBackpackController ctrl)
    {
        itemData    =   slot.item;
        origin      =   slot.origin;
        controller  =   ctrl;
        rect        =   GetComponent<RectTransform>();
        
        var grid = controller.slotsParent.GetComponent<GridLayoutGroup>();
        cellSize = grid.cellSize;
        
        int minX = itemData.shape.Min(o => o.x);
        int maxX = itemData.shape.Max(o => o.x);
        int minY = itemData.shape.Min(o => o.y);
        int maxY = itemData.shape.Max(o => o.y);
        int widthTiles  = maxX - minX + 1;
        int heightTiles = maxY - minY + 1;

        rect.pivot     = new Vector2(0, 1);  
        rect.sizeDelta = new Vector2(cellSize.x * widthTiles, cellSize.y * heightTiles);
        rect.anchoredPosition = controller.SlotToLocalPosition(origin);

        icon.sprite = itemData.icon;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        offset = e.position - (Vector2)rect.position;
        rect.SetAsLastSibling();
        GrabOffset = Vector2Int.zero;
        
        var pd = new PointerEventData(EventSystem.current)
        {
            position = e.position
        };
        var results = new List<RaycastResult>();
        controller.graphicRaycaster.Raycast(pd, results);

        foreach (var r in results)
        {
            var slotView = r.gameObject.GetComponent<InventorySlotView>();
            if (slotView != null)
            {
                GrabOffset = slotView.Data.origin - origin;
                break;
            }
        }
        
        controller.UpdateGhost(this, e.position);
    }

    public void OnDrag(PointerEventData e)
    {
        rect.position = e.position - offset;
        controller.UpdateGhost(this, e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        var results = new List<RaycastResult>();
        controller.graphicRaycaster.Raycast(
            new PointerEventData(EventSystem.current) { position = e.position },
            results
        );

        InventorySlotView hitSlot = null;
        foreach (var r in results)
            if ((hitSlot = r.gameObject.GetComponent<InventorySlotView>()) != null)
                break;

        if (hitSlot != null)
        {
            var newOrigin = hitSlot.Data.origin - GrabOffset;
            if (controller.TryPlaceItem(this, newOrigin))
            {
                rect.anchoredPosition = controller.SlotToLocalPosition(newOrigin);
                controller.HideGhost();
                return;
            }
        }
        
        rect.anchoredPosition = controller.SlotToLocalPosition(origin);
        controller.HideGhost();
    }
    
    public void OnPointerClick(PointerEventData e)
    {
        Debug.Log($"Item {itemData.itemName} clicked with {e.button}");
        if (e.button == PointerEventData.InputButton.Right)
            ContextMenuController.Instance.Show(this, e.position + new Vector2(cellSize.x * 1f, -(cellSize.y * 1.5f)));
        if (e.button == PointerEventData.InputButton.Left)
            ContextMenuController.Instance.Hide();
    }
}
