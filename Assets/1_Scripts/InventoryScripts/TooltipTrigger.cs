using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    InventoryItemView itemView;

    void Awake()
    {
        itemView = GetComponent<InventoryItemView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemView != null && itemView.itemData != null)
            TooltipController.Instance.Show(itemView.itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}