// Assets/Scripts/InventoryScripts/ItemDefinition.cs
using UnityEngine;

public enum ItemType {};

[CreateAssetMenu(fileName="NewInventoryItem", menuName="Inventory/ItemDefinition")]
public class ItemDefinition : ScriptableObject {
    public string itemName;
    public string description;
    public Sprite icon;
    public Vector2Int[] shape;
    public ItemType[] tags;
}