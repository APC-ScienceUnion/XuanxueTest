using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    [System.Serializable]
    public class Item
    {
        public int id;
        public string name;
        public string description;
        public Sprite icon;
        
        public Item()
        {
            id = 0;
            name = "";
            description = "";
            icon = null;
        }
        
        public Item(int id, string name, string description, Sprite icon = null)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.icon = icon;
        }
        
        public bool IsEmpty => id == 0;
    }
} 

