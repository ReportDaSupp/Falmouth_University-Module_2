using UnityEngine;

// QuestDefinition.asset (ScriptableObject)
[CreateAssetMenu(menuName="Quests/QuestDefinition")]
public class QuestDefinition : ScriptableObject {
    public string questTitle;
    public string questDescription;
    public int requiredQuantity;
    public ItemDefinition requiredItem;
    public Sprite questGiverPortrait;
    public int rewardGold;
    public int rewardXP;
}