using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class InventoryItemView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public Vector2Int origin { get; set; }
    public ItemDefinition itemData { get; private set; }
    
    public Vector2Int GrabOffset   { get; private set; }
    public IEnumerable<Vector2Int> CurrentShape => itemData.shape;
    public RectTransform RectTransform => rect;

    private InventoryBackpackController controller;
    private RectTransform rect;
    private Vector2 offset;            // pixel‐offset for smooth drag
    private Vector2 cellSize;          // size of one grid cell in px
    private Vector2Int grabOffset;     // which tile you grabbed (grid coords)

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

        // default in case we miss
        GrabOffset = Vector2Int.zero;

        // Raycast UI under mouse
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

        // Immediately show ghost
        controller.UpdateGhost(this, e.position);
    }

    public void OnDrag(PointerEventData e)
    {
        rect.position = e.position - offset;
        controller.UpdateGhost(this, e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        // 1) Raycast into the UI (ghost won’t block now)
        var results = new List<RaycastResult>();
        controller.graphicRaycaster.Raycast(
            new PointerEventData(EventSystem.current) { position = e.position },
            results
        );

        InventorySlotView hitSlot = null;
        foreach (var r in results)
            if ((hitSlot = r.gameObject.GetComponent<InventorySlotView>()) != null)
                break;

        // 2) If we found a slot, compute the new origin and try to place
        if (hitSlot != null)
        {
            var newOrigin = hitSlot.Data.origin - GrabOffset;
            if (controller.TryPlaceItem(this, newOrigin))
            {
                // snap this view into place
                rect.anchoredPosition = controller.SlotToLocalPosition(newOrigin);
                controller.HideGhost();
                return;
            }
        }

        // 3) Fallback: revert to old origin
        rect.anchoredPosition = controller.SlotToLocalPosition(origin);
        controller.HideGhost();
    }
}
