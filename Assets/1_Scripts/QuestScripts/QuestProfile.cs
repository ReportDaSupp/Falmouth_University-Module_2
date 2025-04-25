using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName="NPC/Quest Profile", fileName="NPCQuestProfile")]
public class NPCQuestProfile : ScriptableObject
{
    [Header("Identifier")]
    public string npcId;

    [Header("Portrait")]
    public Sprite portrait;

    [Header("Available Quests")]
    public List<QuestDefinition> quests;

    [Header("Quest Dialog Lines")]
    [TextArea] public List<string> introLines;
    [TextArea] public List<string> inProgressLines;
    [TextArea] public List<string> completeLines;
}
