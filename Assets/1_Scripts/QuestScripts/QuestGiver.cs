// Assets/Scripts/QuestGiver.cs

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class QuestGiver : MonoBehaviour
{
    [Header("NPCs Quests")]
    public NPCQuestProfile profile;
    public GameObject questPrefab;

    [Header("UI Panels")]
    public GameObject talkPromptUI;
    public GameObject questWindowUI;

    [Header("Quest Window References")]
    public Button acceptButton, closeButton;
    public Transform questListContainer;
    
    [Header("Marker Sprites")]
    public Sprite exclamationSprite;
    public Sprite questionSprite;
    private SpriteRenderer markerRenderer;

    bool playerInRange;
    public QuestDefinition pendingQuest;
    private TMP_Text titleText, descText;
    private Image portraitImage;
    private Button questButton;

    void Awake()
    {
        markerRenderer = transform.Find("QuestMarker").GetComponent<SpriteRenderer>();

        talkPromptUI .SetActive(false);
        questWindowUI.SetActive(false);
        
        acceptButton.onClick.AddListener(OnAccept);
        closeButton .onClick.AddListener(CloseWindow);
    }
    
    void Start()
    {
        if (profile == null)
            Debug.LogError($"[{name}] Missing NPCQuestProfile!");
        else
        {
            QuestTrackerController.Instance.AddQuestProfile(profile);
            UpdateMarker();
        }
            
    }
    
    void Update()
    {
        UpdateMarker();

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OpenQuestWindow();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        talkPromptUI.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        talkPromptUI.SetActive(false);
        CloseWindow();
    }

    void SetPendingQuest(QuestDefinition pQuest)
    {
        pendingQuest = pQuest;
    }
    
    void UpdateMarker()
    {
        var gc = GameController.Instance;
        
        if (gc.activeQuest != null && profile.quests.Contains(gc.activeQuest))
        {
            markerRenderer.sprite = questionSprite;
            markerRenderer.enabled = true;
            pendingQuest = gc.activeQuest;
            return;
        }
        
        var avail = profile.quests.Where(q => !gc.HasCompleted(q) && gc.activeQuest == null).ToList();
        if (avail.Count > 0)
        {
            markerRenderer.sprite = exclamationSprite;
            markerRenderer.enabled = true;
            pendingQuest = avail[0];
            return;
        }
        
        markerRenderer.enabled = false;
        pendingQuest = null;
    }

    void OpenQuestWindow()
    {
        // 1) Gather lines from the profile
        var prof = profile;  // your NPCQuestProfile reference
        IEnumerable<string> lines;

        // if no active quest, show intro; else if in-progress, show those; else complete?
        if (GameController.Instance.activeQuest == null)
            lines = prof.introLines;
        else if (GameController.Instance.activeQuest == pendingQuest && !TryDeliver()) 
            lines = prof.inProgressLines;
        else
        {
            lines = prof.completeLines;
            DialogueController.Instance.StartDialogue(lines, () => { return; });
            return;
        }

        // 2) Run dialogue, then open the real window
        DialogueController.Instance.StartDialogue(lines, () =>
        {
            questWindowUI.SetActive(true);
            PopulateQuestSelection();
        });
    }

    void PopulateQuestSelection()
    {
        foreach (Transform child in questListContainer)
            Destroy(child.gameObject);
        
        foreach (var quest in profile.quests)
        {
            if (!GameController.Instance.HasCompleted(quest))
            {
                var go = Instantiate(questPrefab, questListContainer);
                titleText = go.transform.Find("TitleText").GetComponentInChildren<TMPro.TMP_Text>();
                titleText.text = quest.questTitle;
                descText = go.transform.Find("DescriptionText").GetComponentInChildren<TMPro.TMP_Text>();
                descText.text = $"Fetch {quest.requiredQuantity}× {quest.requiredItem.itemName}";
                questButton = go.GetComponentInChildren<Button>(); //.Find("QuestItemPrefab").GetComponentInChildren<Button>();
                questButton.onClick.AddListener(delegate { SetPendingQuest(quest); });
                portraitImage.sprite = quest.questGiverPortrait;
                acceptButton.gameObject.SetActive(true);
                questWindowUI.SetActive(true);
            }
        }
    }

    void CloseWindow()
    {
        questWindowUI.SetActive(false);
        talkPromptUI .SetActive(playerInRange);
    }

    void OnAccept()
    {
        if (pendingQuest == null) return;
        GameController.Instance.StartQuest(pendingQuest);
        CloseWindow();
    }

    bool TryDeliver()
    {
        var inv = GameController.Instance.inventory;
        int have = inv.CountItem(pendingQuest.requiredItem);
        if (have >= pendingQuest.requiredQuantity)
        {
            inv.RemoveItems(pendingQuest.requiredItem, pendingQuest.requiredQuantity);
            GameController.Instance.CompleteQuest();
            return true;
        }
        else
        {
            Debug.Log("Not enough items yet!");
            return false;
        }
    }
}
