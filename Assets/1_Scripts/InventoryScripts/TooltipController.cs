using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }

    [Header("UI References")]
    public RectTransform tooltipPanel;
    public TMP_Text nameText;
    public TMP_Text descText;

    Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        canvas = tooltipPanel.GetComponentInParent<Canvas>();
        tooltipPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.gameObject.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out pos
            );
            tooltipPanel.anchoredPosition = pos + new Vector2(10, 0);
        }
    }
    
    public void Show(ItemDefinition def)
    {
        nameText.text = def.itemName;
        descText.text = def.description;
        tooltipPanel.gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        tooltipPanel.gameObject.SetActive(false);
    }
}