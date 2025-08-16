using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 物品数据结构（对应JSON）
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public string description;
        public string category;
        public int price;
        public string shop;
        public string iconPath;
    }
    
    /// <summary>
    /// 物品容器（对应JSON根对象）
    /// </summary>
    [System.Serializable]
    public class ItemsContainer
    {
        public ItemData[] items;
    }
    
    /// <summary>
    /// 物品数据库管理器
    /// 负责加载和管理所有游戏物品数据
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        // 单例实例
        public static ItemDatabase Instance { get; private set; }
        
        [Header("数据文件路径")]
        public string itemDataPath = "Data/Items";
        
        // 物品数据存储
        private Dictionary<int, ItemData> itemDataDict = new Dictionary<int, ItemData>();
        private List<ItemData> allItems = new List<ItemData>();
        
        private void Awake()
        {
            // 单例模式初始化
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadItemData();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 从JSON文件加载物品数据
        /// </summary>
        private void LoadItemData()
        {
            itemDataDict.Clear();
            allItems.Clear();
            
            // 从Resources文件夹加载JSON文件
            TextAsset jsonFile = Resources.Load<TextAsset>(itemDataPath);
            if (jsonFile != null)
            {
                try
                {
                    ItemsContainer container = JsonUtility.FromJson<ItemsContainer>(jsonFile.text);
                    
                    foreach (ItemData itemData in container.items)
                    {
                        // 添加到字典（通过ID快速查找）
                        itemDataDict[itemData.id] = itemData;
                        // 添加到列表（用于遍历）
                        allItems.Add(itemData);
                    }
                }
                catch (System.Exception e)
                {
                    CreateDefaultItems(); // 回退到默认物品
                }
            }
            else
            {
                CreateDefaultItems(); // 回退到默认物品
            }
        }
        
        /// <summary>
        /// 创建默认物品数据（作为备用）
        /// </summary>
        private void CreateDefaultItems()
        {
            ItemData defaultItem = new ItemData
            {
                id = 1,
                name = "默认物品",
                description = "这是一个默认物品",
                category = "misc",
                price = 1,
                shop = "默认商店",
                iconPath = ""
            };
            
            itemDataDict[1] = defaultItem;
            allItems.Add(defaultItem);
        }
        
        /// <summary>
        /// 根据ID获取物品数据
        /// </summary>
        public ItemData GetItemData(int itemId)
        {
            if (itemDataDict.ContainsKey(itemId))
            {
                return itemDataDict[itemId];
            }
            return null;
        }
        
        /// <summary>
        /// 创建物品实例（用于InventorySystem）
        /// </summary>
        public Item CreateItem(int itemId)
        {
            ItemData itemData = GetItemData(itemId);
            if (itemData != null)
            {
                // 加载图标（如果路径不为空）
                Sprite icon = null;
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    icon = Resources.Load<Sprite>(itemData.iconPath);
                }
                
                return new Item(itemData.id, itemData.name, itemData.description, icon);
            }
            
            return new Item(); // 返回空物品
        }
        
        /// <summary>
        /// 获取所有物品数据
        /// </summary>
        public List<ItemData> GetAllItems()
        {
            return new List<ItemData>(allItems);
        }
        
        /// <summary>
        /// 根据商店名称获取物品列表
        /// </summary>
        public List<ItemData> GetItemsByShop(string shopName)
        {
            List<ItemData> shopItems = new List<ItemData>();
            
            foreach (ItemData item in allItems)
            {
                if (item.shop == shopName)
                {
                    shopItems.Add(item);
                }
            }
            
            return shopItems;
        }
        
        /// <summary>
        /// 根据分类获取物品列表
        /// </summary>
        public List<ItemData> GetItemsByCategory(string category)
        {
            List<ItemData> categoryItems = new List<ItemData>();
            
            foreach (ItemData item in allItems)
            {
                if (item.category == category)
                {
                    categoryItems.Add(item);
                }
            }
            
            return categoryItems;
        }
        
        /// <summary>
        /// 检查物品是否存在
        /// </summary>
        public bool ItemExists(int itemId)
        {
            return itemDataDict.ContainsKey(itemId);
        }
        
        /// <summary>
        /// 获取物品价格
        /// </summary>
        public int GetItemPrice(int itemId)
        {
            ItemData itemData = GetItemData(itemId);
            return itemData != null ? itemData.price : 0;
        }
        
        /// <summary>
        /// 获取物品名称
        /// </summary>
        public string GetItemName(int itemId)
        {
            ItemData itemData = GetItemData(itemId);
            return itemData != null ? itemData.name : "未知物品";
        }
        
        /// <summary>
        /// 获取物品描述
        /// </summary>
        public string GetItemDescription(int itemId)
        {
            ItemData itemData = GetItemData(itemId);
            return itemData != null ? itemData.description : "暂无描述";
        }
        
        /// <summary>
        /// 重新加载物品数据（开发时使用）
        /// </summary>
        [ContextMenu("重新加载物品数据")]
        public void ReloadItemData()
        {
            LoadItemData();
        }
    }
}