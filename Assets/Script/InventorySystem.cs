using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace XuanZhiShiLian
{
    public class InventorySystem : MonoBehaviour
    {
        // 单例实例
        public static InventorySystem Instance { get; private set; }
        
        [Header("UI预制件配置")]
        public string inventoryPrefabPath = "Prefab/InventoryPanel";
        
        [Header("背包配置")]
        public int maxSlots = 10;
        
        // 内部引用
        private GameObject inventoryPanel;
        private InventoryPanelController inventoryPanelController;
        
        // 背包数据
        private Item[] items;
        
        private void Awake()
        {
            // 单例模式初始化
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInventory();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 监听场景切换事件
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // 自动初始化背包UI（加载场景与不需要显示的场景不初始化）
            string currentScene = SceneManager.GetActiveScene().name;
            if (ShouldShowInventoryInScene(currentScene))
            {
                InitializeInventoryUI();
            }
            else
            {
                HideInventoryPanel();
            }
        }
        
        private void OnDestroy()
        {
            // 取消场景切换事件监听
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void Update()
        {
            // 背包系统常驻显示，无需额外输入处理
        }
        
        private void InitializeInventory()
        {
            // 初始化背包数据
            items = new Item[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                items[i] = new Item(); // 空物品
            }
        }
        
        private void InitializeUI()
        {
            // 检查现有的面板是否仍然有效
            if (inventoryPanel != null && inventoryPanel.activeInHierarchy)
            {
                // 面板存在且有效，刷新显示即可
                RefreshAllSlots();
                return;
            }
            
            // 清理无效引用
            if (inventoryPanel != null && !inventoryPanel.activeInHierarchy)
            {
                inventoryPanel = null;
                inventoryPanelController = null;
            }
            
            // 从Resources文件夹加载预制件
            GameObject prefab = Resources.Load<GameObject>(inventoryPrefabPath);
            if (prefab != null)
            {
                // 查找Canvas（包括Stage5Controller等特殊场景的Canvas）
                Canvas canvas = FindSuitableCanvas();
                
                if (canvas != null)
                {
                    inventoryPanel = Instantiate(prefab, canvas.transform);
                }
                else
                {
                    inventoryPanel = Instantiate(prefab);
                }
                
                // 获取背包面板控制器
                inventoryPanelController = inventoryPanel.GetComponent<InventoryPanelController>();
                
                if (inventoryPanelController != null)
                {
                    
                    // 背包常驻显示
                    inventoryPanel.SetActive(true);
                    
                    // 刷新显示
                    RefreshAllSlots();
                }
            }
        }
        
        /// <summary>
        /// 查找合适的Canvas（优先使用Stage控制器的Canvas）
        /// </summary>
        private Canvas FindSuitableCanvas()
        {
            // 1. 优先查找Stage5Controller的Canvas
            Stage5Controller stage5 = FindObjectOfType<Stage5Controller>();
            if (stage5 != null && stage5.canvas != null)
            {
                return stage5.canvas;
            }
            
            // 2. 查找标记为"Canvas"的GameObject
            GameObject canvasGameObject = GameObject.FindWithTag("Canvas");
            if (canvasGameObject != null)
            {
                Canvas canvas = canvasGameObject.GetComponent<Canvas>();
                if (canvas != null)
                    return canvas;
            }
            
            // 3. 查找任何Canvas组件
            Canvas anyCanvas = FindObjectOfType<Canvas>();
            if (anyCanvas != null)
                return anyCanvas;
            
            return null;
        }
        

        
        public void InitializeInventoryUI()
        {
            // 确保UI组件已初始化
            if (inventoryPanel == null)
            {
                InitializeUI();
            }
            
            if (inventoryPanelController != null)
            {
                RefreshAllSlots();
            }
        }
        
        public bool AddItem(Item item)
        {
            if (item == null)
            {
                return false;
            }
            
            if (item.IsEmpty)
            {
                return false;
            }
            
            // 找到第一个空格子
            for (int i = 0; i < maxSlots; i++)
            {
                if (items[i].IsEmpty)
                {
                    items[i] = item;
                    RefreshSlot(i);
                    return true;
                }
            }
            return false;
        }
        
        public bool RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                return false;
            }
            
            if (items[slotIndex].IsEmpty)
            {
                return false;
            }
            
            string itemName = items[slotIndex].name;
            items[slotIndex] = new Item(); // 重置为空物品
            RefreshSlot(slotIndex);
            return true;
        }
        
        public void ClearInventory()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                items[i] = new Item(); // 重置为空物品
            }
            RefreshAllSlots();
        }
        
        public void ShowItemInspector(Item item)
        {
            if (inventoryPanelController != null)
            {
                inventoryPanelController.ShowItemInspector(item);
            }
        }
        
        public void CloseItemInspector()
        {
            if (inventoryPanelController != null)
            {
                inventoryPanelController.CloseItemInspector();
            }
        }
        
        private void RefreshSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
                return;
                
            if (inventoryPanelController != null)
            {
                inventoryPanelController.SetSlotItem(slotIndex, items[slotIndex]);
            }
        }
        
        private void RefreshAllSlots()
        {
            if (inventoryPanelController != null)
            {
                inventoryPanelController.RefreshAllSlots(items);
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 检查是否从存档加载，如果是则不清空背包（背包数据会在GameManager中恢复）
            if (GameManager.Instance != null && GameManager.Instance.isLoadingFromSave)
            {
                //
            }
            else
            {
                // 切换场景时切换为目标Stage的背包内容
                // 先清空，再按场景名加载对应Stage的背包
                ClearInventory();
                LoadInventoryForScene(scene.name);
            }
            
            // 检查当前场景是否需要显示背包
            if (ShouldShowInventoryInScene(scene.name))
            {
                // 重新初始化背包UI（因为Canvas可能是新的）
                InitializeInventoryUI();
            }
            else
            {
                // 隐藏背包面板
                HideInventoryPanel();
            }
        }
        
        /// <summary>
        /// 检查指定场景是否需要显示背包
        /// </summary>
        private bool ShouldShowInventoryInScene(string sceneName)
        {
            // Load场景（存在LoadController）不显示背包
            if (FindObjectOfType<LoadController>() != null)
            {
                return false;
            }
            // Stage0、1、6不显示背包（Stage2 需要显示）
            string[] noInventoryScenes = { "Stage0", "Stage1", "Stage6" };
            
            foreach (string scene in noInventoryScenes)
            {
                if (sceneName.Equals(scene, System.StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 根据场景名加载对应Stage的背包内容（从存档文件读取）
        /// 非存档恢复场景切换时使用
        /// </summary>
        private void LoadInventoryForScene(string sceneName)
        {
            int stageNumber = GetStageNumberFromSceneName(sceneName);
            if (stageNumber < 0)
            {
                return;
            }
            
            if (GameManager.Instance == null || GameManager.Instance.saveSystem == null)
            {
                return;
            }
            
            var saveData = GameManager.Instance.saveSystem.GetSaveData();
            if (saveData == null || saveData.stageInventories == null)
            {
                return;
            }
            
            var entry = saveData.stageInventories.Find(e => e.stageNumber == stageNumber);
            if (entry != null && entry.items != null)
            {
                int count = Mathf.Min(maxSlots, entry.items.Count);
                Item[] loaded = new Item[maxSlots];
                for (int i = 0; i < maxSlots; i++)
                {
                    if (i < count && entry.items[i] != null && !entry.items[i].IsEmpty)
                    {
                        loaded[i] = entry.items[i].ToItem();
                    }
                    else
                    {
                        loaded[i] = new Item();
                    }
                }
                SetAllItems(loaded);
            }
        }
        
        /// <summary>
        /// 从场景名提取Stage编号（如"Stage4" -> 4），失败返回-1
        /// </summary>
        private int GetStageNumberFromSceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return -1;
            if (!sceneName.StartsWith("Stage")) return -1;
            string numPart = sceneName.Substring("Stage".Length);
            if (int.TryParse(numPart, out int stage))
            {
                return stage;
            }
            return -1;
        }

        /// <summary>
        /// 隐藏背包面板
        /// </summary>
        private void HideInventoryPanel()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
        }
        
        // 公共接口 - 供其他脚本调用
        public Item GetItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
                return new Item();
                
            return items[slotIndex];
        }
        
        public int GetItemCount()
        {
            int count = 0;
            for (int i = 0; i < maxSlots; i++)
            {
                if (!items[i].IsEmpty)
                    count++;
            }
            return count;
        }
        
        public bool IsFull()
        {
            return GetItemCount() >= maxSlots;
        }
        
        public bool IsEmpty()
        {
            return GetItemCount() == 0;
        }
        
        /// <summary>
        /// 检查背包中是否包含指定ID的物品
        /// </summary>
        public bool HasItem(int itemId)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (!items[i].IsEmpty && items[i].id == itemId)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取当前背包中的所有物品（用于存档）
        /// </summary>
        public Item[] GetAllItems()
        {
            Item[] allItems = new Item[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                allItems[i] = new Item(items[i].id, items[i].name, items[i].description, items[i].icon);
            }
            return allItems;
        }
        
        /// <summary>
        /// 设置背包中的所有物品（用于加载存档）
        /// </summary>
        public void SetAllItems(Item[] loadedItems)
        {
            if (loadedItems == null)
            {
                return;
            }
            
            int maxToLoad = Mathf.Min(loadedItems.Length, maxSlots);
            for (int i = 0; i < maxToLoad; i++)
            {
                if (loadedItems[i] != null)
                {
                    items[i] = new Item(loadedItems[i].id, loadedItems[i].name, loadedItems[i].description, loadedItems[i].icon);
                }
                else
                {
                    items[i] = new Item(); // 空物品
                }
            }
            
            // 剩余格子填充为空物品
            for (int i = maxToLoad; i < maxSlots; i++)
            {
                items[i] = new Item();
            }
            
            // 刷新UI显示
            RefreshAllSlots();
        }
        
        /// <summary>
        /// 通过物品ID移除背包中的物品（移除第一个找到的）
        /// </summary>
        public bool RemoveItemById(int itemId)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (!items[i].IsEmpty && items[i].id == itemId)
                {
                    string itemName = items[i].name;
                    items[i] = new Item(); // 重置为空物品
                    RefreshSlot(i);
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 通过物品ID移除背包中的物品（静态方法）
        /// </summary>
        public static bool RemoveItemFromInventory(int itemId)
        {
            if (Instance == null)
            {
                return false;
            }
            
            return Instance.RemoveItemById(itemId);
        }
        
        /// <summary>
        /// 检查背包中是否包含指定ID的物品（静态方法）
        /// </summary>
        public static bool HasItemInInventory(int itemId)
        {
            if (Instance == null)
            {
                return false;
            }
            
            return Instance.HasItem(itemId);
        }
        
        // 便捷方法 - 通过ItemDatabase创建物品并添加到背包（推荐使用）
        public static bool AddItemToInventory(int itemId)
        {
            if (Instance == null)
            {
                return false;
            }
            
            if (ItemDatabase.Instance == null)
            {
                // 确保itemId > 0，避免IsEmpty判断错误
                int validId = itemId > 0 ? itemId : 9999;
                return AddItemToInventory(validId, $"物品{itemId}", "暂无描述");
            }
            
            Item newItem = ItemDatabase.Instance.CreateItem(itemId);
            if (newItem.IsEmpty)
            {
                // 即使数据库中没有，也创建一个基本物品，避免IsEmpty判断错误
                int validId = itemId > 0 ? itemId : 9999;
                return AddItemToInventory(validId, $"物品{itemId}", "暂无描述");
            }
            
            bool result = Instance.AddItem(newItem);
            return result;
        }
        
        // 便捷方法 - 手动创建物品并添加到背包（向后兼容）
        public static bool AddItemToInventory(int id, string name, string description, Sprite icon = null)
        {
            if (Instance == null)
            {
                return false;
            }
            
            Item newItem = new Item(id, name, description, icon);
            return Instance.AddItem(newItem);
        }
    }
} 