// Assets/Scripts/InventoryScripts/InventoryBackpackController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryBackpackController : MonoBehaviour {
    [Header("Data (cloned at runtime)")]
    public InventoryData inventoryData;
    private InventoryData _runtimeData;
    
    [Header("Prefabs & UI")]
    public GameObject slotPrefab;
    public GameObject itemPrefab;
    public GameObject ghostPrefab;
    private RectTransform ghostRect;
    private Image ghostImage;
    private bool ghostVisible = false;
    
    public Transform slotsParent;
    public RectTransform itemsParent;
    public RectTransform ghostParent;
    public GraphicRaycaster graphicRaycaster;

    private void Awake()
    {
        _runtimeData = Instantiate(inventoryData);
        
        var go = Instantiate(ghostPrefab, ghostParent);
        ghostRect  = go.GetComponent<RectTransform>();
        ghostImage = go.GetComponent<Image>();
        go.SetActive(false);
    }

    void Start() {
        BuildSlots();
        RenderItems();
    }

    void BuildSlots() {
        var grid = slotsParent.GetComponent<GridLayoutGroup>();
        grid.constraintCount = _runtimeData.width;

        foreach (Transform c in slotsParent) Destroy(c.gameObject);
        for (int y=0; y<_runtimeData.height; y++){
            for (int x=0; x<_runtimeData.width; x++){
                var slotGO = Instantiate(slotPrefab, slotsParent);
                var slotData = _runtimeData.GetSlot(x,y);
                slotGO.GetComponent<InventorySlotView>().Initialize(slotData);
            }
        }
    }

    public void RenderItems() {
        foreach (Transform c in itemsParent) Destroy(c.gameObject);
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotsParent.GetComponent<RectTransform>());

        var drawn = new HashSet<Vector2Int>();
        foreach (var slot in _runtimeData.slots) {
            if (slot.item == null) continue;

            var origin = slot.itemOrigin;
            if (origin.x < 0 || origin.y < 0) continue;

            if (!drawn.Add(origin)) continue;

            var originSlot = _runtimeData.GetSlot(origin.x, origin.y);
            if (originSlot == null) continue;

            var go = Instantiate(itemPrefab, itemsParent);
            var view = go.GetComponent<InventoryItemView>();
            view.Initialize(originSlot, this);
        }
    }
    
    public bool AddItemToGrid(ItemDefinition def, Vector2Int origin)
    {
        foreach (var off in def.shape)
        {
            var pos = origin + off;
            var slot = _runtimeData.GetSlot(pos.x, pos.y);
            if (slot == null || !slot.active || slot.item != null)
                return false;
        }
        
        foreach (var off in def.shape) {
            var pos = origin + off;
            var slot = _runtimeData.GetSlot(pos.x, pos.y);
            slot.item = def;
            slot.itemOrigin = origin;
        }
        RenderItems();
        return true;
    }
    
    public bool TryPlaceItem(InventoryItemView itemView, Vector2Int newOrigin)
    {
        var def = itemView.itemData;
        var oldOrigin = itemView.origin;
        var shape    = def.shape;
        
        var oldPositions = new HashSet<Vector2Int>(
            shape.Select(offset => oldOrigin + offset)
        );
        var newPositions = shape.Select(offset => newOrigin + offset).ToList();
        
        foreach (var pos in newPositions)
        {
            var slot = _runtimeData.GetSlot(pos.x, pos.y);
            if (slot == null || !slot.active)
                return false;
            
            if (slot.item != null && !oldPositions.Contains(pos))
                return false;
        }
        
        foreach (var pos in oldPositions)
        {
            var oldSlot = _runtimeData.GetSlot(pos.x, pos.y);
            if (oldSlot != null)
                oldSlot.Clear();
        }
        
        foreach (var pos in newPositions)
        {
            var newSlot = _runtimeData.GetSlot(pos.x, pos.y);
            newSlot.item       = def;
            newSlot.itemOrigin = newOrigin;
        }
        
        itemView.origin = newOrigin;
        RenderItems();

        return true;
    }
    
    public void UpdateGhost(InventoryItemView itemView, Vector2 screenPos)
    {
        var pd = new PointerEventData(EventSystem.current) {
            position = screenPos
        };
        var results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pd, results);

        InventorySlotView hit = null;
        foreach (var r in results)
            if ((hit = r.gameObject.GetComponent<InventorySlotView>()) != null)
                break;

        if (hit == null) {
            if (ghostVisible) { HideGhost(); }
            return;
        }
        
        ghostImage.sprite = itemView.icon.sprite;
        
        // compute new origin
        var newOrigin = hit.Data.origin - itemView.GrabOffset;

        // validate footprint…
        bool valid = true;
        foreach (var off in itemView.CurrentShape) {
            var pos = newOrigin + off;
            var s = _runtimeData.GetSlot(pos.x, pos.y);
            if (s == null || !s.active ||
                (s.item != null && s.itemOrigin != itemView.origin))
            {
                valid = false; break;
            }
        }

        // position & color
        ghostRect.sizeDelta = itemView.RectTransform.sizeDelta;
        ghostRect.anchoredPosition = SlotToLocalPosition(newOrigin);
        ghostImage.color = valid
            ? new Color(0,1,0,0.5f)
            : new Color(1,0,0,0.5f);

        if (!ghostVisible) {
            ghostRect.gameObject.SetActive(true);
            ghostVisible = true;
        }
    }
    
    public void HideGhost()
    {
        if (ghostVisible)
        {
            ghostVisible = false;
            ghostRect.gameObject.SetActive(false);
        }
    }
    
    public Vector2 SlotToLocalPosition(Vector2Int gridPos)
    {
        // 1) Find the slot RectTransform
        int idx = gridPos.y * _runtimeData.width + gridPos.x;
        var slotRT = slotsParent.GetChild(idx).GetComponent<RectTransform>();

        // 2) Get its world‑space top‑left corner
        //    (0,0) in its local rect is at its pivot; adjust for top‑left:
        Vector3 localTopLeft = new Vector3(
            -slotRT.rect.width * slotRT.pivot.x,
            slotRT.rect.height * (1 - slotRT.pivot.y),
            0
        );
        Vector3 worldPos = slotRT.TransformPoint(localTopLeft);

        // 3) Convert worldPos → screen point
        var canvas = slotRT.GetComponentInParent<Canvas>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

        // 4) Convert screen point → itemsParent local point
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            itemsParent,
            screenPoint,
            canvas.worldCamera,
            out localPoint
        );

        return localPoint;
    }
    
    public Vector2Int? FindFirstFit(ItemDefinition def)
    {
        for (int y = 0; y < _runtimeData.height; y++)
        {
            for (int x = 0; x < _runtimeData.width; x++)
            {
                var origin = new Vector2Int(x, y);
                if (CanFit(def, origin))
                    return origin;
            }
        }
        return null;
    }
    
    private bool CanFit(ItemDefinition def, Vector2Int origin)
    {
        foreach (var offset in def.shape)
        {
            var pos = origin + offset;
            var slot = _runtimeData.GetSlot(pos.x, pos.y);
            if (slot == null || !slot.active || slot.item != null)
                return false;
        }
        return true;
    }
    
    public int CountItem(ItemDefinition def)
    {
        var origins = new HashSet<Vector2Int>();
        foreach (var slot in _runtimeData.slots)
            if (slot.item == def && slot.itemOrigin.x >= 0)
                origins.Add(slot.itemOrigin);
        return origins.Count;
    }
    
    public int RemoveItems(ItemDefinition def, int quantity)
    {
        int removed = 0;
        // 1) Collect all distinct origins for this item definition
        var origins = new HashSet<Vector2Int>();
        foreach (var slot in _runtimeData.slots)
            if (slot.item == def && slot.itemOrigin.x >= 0)
                origins.Add(slot.itemOrigin);

        // 2) Remove one entire item (all its slots) per origin, up to quantity
        foreach (var origin in origins)
        {
            if (removed >= quantity)
                break;

            // clear every slot that belonged to this item instance
            foreach (var offset in def.shape)
            {
                var pos = origin + offset;
                var s = _runtimeData.GetSlot(pos.x, pos.y);
                if (s != null)
                    s.Clear();
            }
            removed++;
        }

        // 3) Refresh the UI so they disappear
        RenderItems();
        return removed;
    }
    
}