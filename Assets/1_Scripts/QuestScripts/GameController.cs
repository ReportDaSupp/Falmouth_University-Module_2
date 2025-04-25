// Assets/Scripts/GameController.cs

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Player & Inventory")]
    public InventoryBackpackController inventory;

    [Header("UI")]
    public TMP_Text   goldText;
    public TMP_Text   xpText;

    [HideInInspector] public QuestDefinition activeQuest;
    private List<QuestDefinition> completedQuests = new();
    private int gold, xp;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void StartQuest(QuestDefinition quest)
    {
        activeQuest = quest;
        QuestTrackerController.Instance.Refresh();
    }
    
    public void CompleteQuest()
    {
        if (activeQuest == null) return;
        completedQuests.Add(activeQuest);
        gold += activeQuest.rewardGold;
        xp   += activeQuest.rewardXP;
        goldText.text = $"Gold: {gold}";
        xpText.text   = $"XP: {xp}";
        activeQuest = null;
        //questStatusText.text = "No active quest.";
        QuestTrackerController.Instance.Refresh();
    }
    
    public bool HasCompleted(QuestDefinition q) => completedQuests.Contains(q);
    
    public void TryDeliver(QuestGiver giver)
    {
        if (activeQuest == null || giver.pendingQuest != activeQuest) return;
        
        int count = inventory.CountItem(activeQuest.requiredItem);
        if (count >= activeQuest.requiredQuantity)
        {
            inventory.RemoveItems(activeQuest.requiredItem, activeQuest.requiredQuantity);
            
            CompleteQuest();
            Debug.Log("Quest Completed");
        }
        else
        {
            Debug.Log("You don't have enough quest items!");
        }
    }
    
    public bool HasActiveQuest(QuestDefinition quest)
    {
        return activeQuest != null && activeQuest == quest;
    }
}