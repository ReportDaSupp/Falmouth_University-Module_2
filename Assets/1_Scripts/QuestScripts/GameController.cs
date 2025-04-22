// Assets/Scripts/GameController.cs
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Player & Inventory")]
    public InventoryBackpackController inventory;

    [Header("UI")]
    public Text   goldText;
    public Text   xpText;
    public Text   questStatusText;   // e.g. “Deliver 1× Fire Orchid”

    private QuestDefinition activeQuest;
    private int gold, xp;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Update()
    {
        // Check for delivery: if player presses E at a quest-giver with activeQuest:
        if (activeQuest != null && Input.GetKeyDown(KeyCode.E))
        {
            // Raycast or trigger check to see if we're at the giver
            // Simplest: rely on QuestGiver OnAccept to re-press E to deliver
        }
    }

    public void StartQuest(QuestDefinition quest)
    {
        activeQuest = quest;
        //questStatusText.text = 
            //$"Quest: {quest.questTitle}\n" +
            //$"Fetch {quest.requiredQuantity}× {quest.requiredItem.itemName}";
    }

    public void TryDeliver(QuestGiver giver)
    {
        if (activeQuest == null || giver.quest != activeQuest) return;

        // Count matching items in inventory
        int count = inventory.CountItem(activeQuest.requiredItem);
        if (count >= activeQuest.requiredQuantity)
        {
            // Remove items
            inventory.RemoveItems(activeQuest.requiredItem, activeQuest.requiredQuantity);

            // Grant rewards
            //gold += activeQuest.rewardGold;
            //xp   += activeQuest.rewardXP;
            //goldText.text = $"Gold: {gold}";
            //xpText.text   = $"XP: {xp}";

            // Clear quest
            activeQuest = null;
            //questStatusText.text = "No active quest.";
            Debug.Log("Quest Completed");
        }
        else
        {
            // Feedback: not enough items
            Debug.Log("You don't have enough quest items!");
        }
    }
    
    public bool HasActiveQuest(QuestDefinition quest)
    {
        return activeQuest != null && activeQuest == quest;
    }
}