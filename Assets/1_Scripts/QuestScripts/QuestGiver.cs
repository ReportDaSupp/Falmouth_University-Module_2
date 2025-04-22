// Assets/Scripts/QuestGiver.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class QuestGiver : MonoBehaviour
{
    public QuestDefinition quest;       // assign in Inspector
    public GameObject promptUI;         // “Press E to talk” UI
    public GameObject questWindowUI;    // panel with UI for quest details

    // UI references inside questWindowUI:
    public TMP_Text    titleText;
    public TMP_Text    descText;
    public Image   portraitImage;
    public Button  acceptButton;
    public Button  closeButton;

    private bool playerInRange;

    void Awake()
    {
        promptUI.SetActive(false);
        questWindowUI.SetActive(false);

        acceptButton.onClick.AddListener(OnAccept);
        closeButton.onClick .AddListener(CloseWindow);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            promptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            promptUI.SetActive(false);
            CloseWindow();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (GameController.Instance.HasActiveQuest(quest))
                GameController.Instance.TryDeliver(this);
            else
                OpenWindow();
        }
    }

    void OpenWindow()
    {
        promptUI.SetActive(false);
        questWindowUI.SetActive(true);
        titleText.text    = quest.questTitle;
        descText.text     = quest.questDescription + 
                            $"\n\nFetch: {quest.requiredQuantity}× {quest.requiredItem.itemName}";
        portraitImage.sprite = quest.questGiverPortrait;
    }

    void CloseWindow()
    {
        questWindowUI.SetActive(false);
        promptUI.SetActive(playerInRange);
    }

    void OnAccept()
    {
        GameController.Instance.StartQuest(quest);
        CloseWindow();
    }
}