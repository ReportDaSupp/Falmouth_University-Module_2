using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class QuestTrackerController : MonoBehaviour
{
    public static QuestTrackerController Instance { get; private set; }

    [Header("UI")]
    public GameObject trackerPanel;
    public Transform entryContainer;
    public GameObject entryPrefab;
    public Button toggleButton;

    private List<NPCQuestProfile> profiles = new();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        Refresh();
        toggleButton.onClick.AddListener(ToggleTracker);
    }

    public void Refresh()
    {
        foreach (Transform t in entryContainer) Destroy(t.gameObject);

        foreach (var prof in profiles)
            foreach (var q in prof.quests)
            {
                Debug.Log("profile" + q.name);
                string status;
                if (GameController.Instance.HasCompleted(q))
                    status = "[Done]";
                else if (GameController.Instance.activeQuest == q)
                {
                    Debug.Log("ActiveQuest");
                    status = "[In-Progress]";
                    var e = Instantiate(entryPrefab, entryContainer);
                    e.transform.Find("Title").GetComponentInChildren<TMPro.TMP_Text>().text = q.questTitle;
                    e.transform.Find("Status").GetComponentInChildren<TMPro.TMP_Text>().text = status;
                }
                else
                    status = "[Available]";
            }
    }

    public void AddQuestProfile(NPCQuestProfile profile)
    {
        profiles.Add(profile);
    }
    
    public void ToggleTracker()
    {
        trackerPanel.SetActive(!trackerPanel.activeSelf);
    }
}