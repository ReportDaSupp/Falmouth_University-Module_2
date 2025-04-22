// Assets/Scripts/InventoryScripts/InventorySlotData.cs
using UnityEngine;

[System.Serializable]
public class InventorySlotData {
    public bool active = true;
    public ItemDefinition item = null;
    public Vector2Int origin;
    public Vector2Int itemOrigin;
    
    public void Clear() {
        item = null;
        itemOrigin = new Vector2Int(-1, -1);
    }
}