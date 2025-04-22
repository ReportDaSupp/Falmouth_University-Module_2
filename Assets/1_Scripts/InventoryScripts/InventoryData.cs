// Assets/Scripts/InventoryScripts/InventoryData.cs
using UnityEngine;

[CreateAssetMenu(fileName="NewInventory", menuName="Inventory/InventoryData")]
public class InventoryData : ScriptableObject {
    public int width = 3, height = 3;
    public InventorySlotData[] slots;

    private void OnEnable() {
        if (slots == null || slots.Length != width * height) {
            slots = new InventorySlotData[width * height];
            for(int i=0;i<slots.Length;i++){
                slots[i] = new InventorySlotData {
                    active = true,
                    item = null,
                    origin = new Vector2Int(i % width, i / width)
                };
            }
        }
    }

    public InventorySlotData GetSlot(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return slots[y * width + x];
    }
}