// Assets/Scripts/InventoryScripts/ItemDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName="NewInventoryItem", menuName="Inventory/ItemDefinition")]
public class ItemDefinition : ScriptableObject {
    public string itemName;
    public Sprite icon;
    public Vector2Int[] shape;
}