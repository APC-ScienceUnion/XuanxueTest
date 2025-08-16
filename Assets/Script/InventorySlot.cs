using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    public class InventorySlot : MonoBehaviour
    {
        [Header("UI组件")]
        public Button slotButton;
        public Image itemIcon;
        public TextMeshProUGUI itemName;
        
        private Item currentItem;
        private int slotIndex;
        
        private void Awake()
        {
            // 如果没有手动分配，尝试自动获取组件
            if (slotButton == null)
                slotButton = GetComponent<Button>();
            if (itemIcon == null)
                itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (itemName == null)
                itemName = transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
                
            // 设置按钮点击事件
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(OnSlotClicked);
            }
        }
        
        public void Initialize(int index)
        {
            slotIndex = index;
            ClearSlot();
        }
        
        public void SetItem(Item item)
        {
            currentItem = item;
            
            if (item == null || item.IsEmpty)
            {
                ClearSlot();
                return;
            }
            
            // 显示物品
            if (itemIcon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.color = Color.white;
                itemIcon.gameObject.SetActive(true);
            }
            
            if (itemName != null)
            {
                itemName.text = item.name;
                itemName.gameObject.SetActive(true);
            }
        }
        
        public void ClearSlot()
        {
            currentItem = null;
            
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.color = Color.clear;
                itemIcon.gameObject.SetActive(false);
            }
            
            if (itemName != null)
            {
                itemName.text = "";
                itemName.gameObject.SetActive(false);
            }
        }
        
        public bool IsEmpty => currentItem == null || currentItem.IsEmpty;
        public Item GetItem() => currentItem;
        
        private void OnSlotClicked()
        {
            if (!IsEmpty)
            {
                
                // 检查是否是魔将玄武物品
                if (IsMJXWItem(currentItem))
                {
                    HandleMJXWClick();
                }
                else
                {
                    // 通知背包系统显示物品检视面板
                    InventorySystem.Instance?.ShowItemInspector(currentItem);
                }
            }
        }
        
        /// <summary>
        /// 判断是否是魔将玄武物品
        /// </summary>
        private bool IsMJXWItem(Item item)
        {
            // 根据物品名称判断是否是魔将玄武
            return item != null && !string.IsNullOrEmpty(item.name) && 
                   item.name.Contains("魔将玄武");
        }
        
        /// <summary>
        /// 处理魔将玄武点击
        /// </summary>
        private void HandleMJXWClick()
        {
            
            // 查找Stage5Controller
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                // 调用魔将玄武占卜功能
                stage5Controller.UseMJXWDivination();
            }
            else
            {
                // 作为后备，显示普通的物品检视面板
                InventorySystem.Instance?.ShowItemInspector(currentItem);
            }
        }
    }
} 