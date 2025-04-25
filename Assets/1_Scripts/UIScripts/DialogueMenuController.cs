using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("UI References")]
    public GameObject   dialoguePanel;
    public TMP_Text     lineText;
    public Button       nextButton;

    private Queue<string> lines;
    private System.Action onComplete;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        dialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(ShowNext);
    }
    
    public void StartDialogue(IEnumerable<string> dialogueLines, System.Action doneCallback = null)
    {
        lines = new Queue<string>(dialogueLines);
        onComplete = doneCallback;
        dialoguePanel.SetActive(true);
        ShowNext();
    }

    private void ShowNext()
    {
        if (lines.Count == 0)
        {
            dialoguePanel.SetActive(false);
            onComplete?.Invoke();
            return;
        }
        lineText.text = lines.Dequeue();
    }
}