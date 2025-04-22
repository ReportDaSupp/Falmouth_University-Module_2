// Assets/Scripts/InventoryScripts/UIScripts/InventorySlotView.cs
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour {
    public Image background;
    public InventorySlotData Data;
    public void Initialize(InventorySlotData slotData) {
        Data = slotData;
        background.color = Data.active ? Color.gray : Color.black;
    }
}