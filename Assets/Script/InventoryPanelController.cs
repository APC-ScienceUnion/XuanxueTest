using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 背包面板UI控制器
    /// 负责管理背包面板的所有UI组件交互
    /// 在预制件上手动分配组件引用，避免名称查找的不安全性
    /// </summary>
    public class InventoryPanelController : MonoBehaviour
    {
        [Header("格子组件 - 手动分配10个格子")]
        public InventorySlot[] inventorySlots = new InventorySlot[10];
        
        [Header("物品检视面板组件 - 手动分配")]
        public GameObject itemInspectorPanel;
        public TextMeshProUGUI inspectorTitle;
        public TextMeshProUGUI inspectorDescription;
        public Button inspectorCloseButton;
        
        private void Awake()
        {
            ValidateComponents();
            SetupEvents();
        }
        
        private void ValidateComponents()
        {
            bool allValid = true;
            
            // 验证格子组件
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null)
                {
                    allValid = false;
                }
            }
            
            // 验证检视面板组件
            if (itemInspectorPanel == null)
            {
                allValid = false;
            }
            
            if (inspectorTitle == null)
            {
                allValid = false;
            }
            
            if (inspectorDescription == null)
            {
                allValid = false;
            }
            
            if (inspectorCloseButton == null)
            {
                allValid = false;
            }
        }
        
        private void SetupEvents()
        {
            // 设置检视面板关闭按钮事件
            if (inspectorCloseButton != null)
            {
                inspectorCloseButton.onClick.RemoveAllListeners();
                inspectorCloseButton.onClick.AddListener(CloseItemInspector);
            }
            
            // 初始化所有格子
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] != null)
                {
                    inventorySlots[i].Initialize(i);
                }
            }
            
            // 检视面板默认隐藏
            if (itemInspectorPanel != null)
            {
                itemInspectorPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 设置指定格子的物品
        /// </summary>
        public void SetSlotItem(int slotIndex, Item item)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
            {
                return;
            }
            
            if (inventorySlots[slotIndex] != null)
            {
                inventorySlots[slotIndex].SetItem(item);
            }
        }
        
        /// <summary>
        /// 清空指定格子
        /// </summary>
        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
            {
                return;
            }
            
            if (inventorySlots[slotIndex] != null)
            {
                inventorySlots[slotIndex].ClearSlot();
            }
        }
        
        /// <summary>
        /// 刷新所有格子显示
        /// </summary>
        public void RefreshAllSlots(Item[] items)
        {
            if (items == null)
            {
                return;
            }
            
            int maxSlots = Mathf.Min(inventorySlots.Length, items.Length);
            
            for (int i = 0; i < maxSlots; i++)
            {
                if (inventorySlots[i] != null)
                {
                    inventorySlots[i].SetItem(items[i]);
                }
            }
        }
        
        /// <summary>
        /// 显示物品检视面板
        /// </summary>
        public void ShowItemInspector(Item item)
        {
            if (itemInspectorPanel == null || item == null || item.IsEmpty)
            {
                return;
            }
            
            // 设置检视面板文本内容
            if (inspectorTitle != null)
                inspectorTitle.text = item.name;
                
            if (inspectorDescription != null)
                inspectorDescription.text = item.description;
            
            // 显示检视面板
            itemInspectorPanel.SetActive(true);
            
        }
        
        /// <summary>
        /// 关闭物品检视面板
        /// </summary>
        public void CloseItemInspector()
        {
            if (itemInspectorPanel != null)
            {
                itemInspectorPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 获取指定格子的物品
        /// </summary>
        public Item GetSlotItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
            {
                return new Item(); // 返回空物品
            }
            
            if (inventorySlots[slotIndex] != null)
            {
                return inventorySlots[slotIndex].GetItem();
            }
            
            return new Item(); // 返回空物品
        }
        
        /// <summary>
        /// 检查指定格子是否为空
        /// </summary>
        public bool IsSlotEmpty(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
            {
                return true;
            }
            
            if (inventorySlots[slotIndex] != null)
            {
                return inventorySlots[slotIndex].IsEmpty;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取格子总数
        /// </summary>
        public int GetSlotCount()
        {
            return inventorySlots.Length;
        }
        
    }
} 