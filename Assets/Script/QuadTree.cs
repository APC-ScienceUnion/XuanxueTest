using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public struct Bound
{
    public float MinX;
    public float MinY;
    public float MaxX;
    public float MaxY;
}
public class QuadTree
{
    private int MaxObjects{get; set;}
    private int MaxLevel{get; set;}
    public int Level{get; set;}
    private Bound Bounds{get; set;}
    public List<GameObject> Objects{get; set;}
    // node 鎸?nw銆乶e銆乻w銆乻e 椤哄簭鍒掑垎
    private List<QuadTree> Nodes{get; set;}
    // 鏋勯€犲嚱鏁?
    public QuadTree(Bound bounds, int maxObjects, int maxLevels, int level) {
        MaxObjects = maxObjects;
        MaxLevel = maxLevels;
        Bounds = bounds;
        Level = level;
        Objects = new List<GameObject>();
        Nodes = new List<QuadTree>();
    }
    
    /// <summary>
    /// 鎻掑叆涓€涓妭鐐?
    /// </summary>
    public void Insert(GameObject obj) {
        // 濡傛灉瀛樺湪瀛愯妭鐐癸紝灏嗚妭鐐规彃鍏ュ埌鍖归厤鐨勫瓙鑺傜偣涓?
        if (this.Nodes.Count == 4) {
            this.Nodes[this.GetIndex(obj)].Insert(obj);
            return;
        }

        // 鍚﹀垯锛屽皢鑺傜偣瀛樺偍鍦ㄥ綋鍓嶈妭鐐逛腑
        this.Objects.Add(obj);

        // 濡傛灉褰撳墠鑺傜偣鐨勫璞℃暟閲忚秴杩囦簡鏈€澶у閲忥紝骞朵笖褰撳墠灞傜骇灏忎簬鏈€澶у眰绾?
        if (this.Objects.Count > this.MaxObjects && this.Level < this.MaxLevel) {
            // 濡傛灉杩樻病鏈夊瓙鑺傜偣锛屽垯杩涜鍒嗗壊
            if (this.Nodes.Count == 0)
            {
                this.Split();
            }

            // 灏嗘墍鏈夊璞℃坊鍔犲埌瀵瑰簲鐨勫瓙鑺傜偣涓?
            for (int i = 0; i < this.Objects.Count; i++) {
                int idx = this.GetIndex(Objects[i]);
                this.Nodes[idx].Insert(Objects[i]);
            }

            // 娓呯┖褰撳墠鑺傜偣鐨勫璞″垪琛?
            this.Objects.Clear();
        }
    }

    /// <summary>
    /// 鎻掑叆鍒板彾瀛愯妭鐐逛腑
    /// </summary>
    private int GetIndex(GameObject obj)
    {
        var x = obj.transform.position.x;
        var y = obj.transform.position.z;
        var midPointX = (this.Bounds.MaxX + this.Bounds.MinX)/2;
        var midPointY = (this.Bounds.MaxY + this.Bounds.MinY)/2;
        
        if (x <= midPointX && y >= midPointY)
            // nw锛堝乏涓婏級
            return 0;
        if (x >= midPointX && y >= midPointY)
            // ne锛堝彸涓婏級
            return 1;
        if (x < midPointX && y < midPointY)
            // sw锛堝乏涓嬶級
            return 2;
        // sw锛堝彸涓嬶級
        return 3;
    }

    /// <summary>
    /// 鍒嗗壊鑺傜偣
    /// </summary>
    private void Split()
    {
        int nextLevel = this.Level + 1;
        float midX = (this.Bounds.MaxX + this.Bounds.MinX) / 2;
        float midY = (this.Bounds.MaxY + this.Bounds.MinY) / 2;

        // 鍒涘缓 4 涓瓙鑺傜偣
        this.Nodes = new List<QuadTree>(4);

        // nw锛堝乏涓婏級
        this.Nodes.Add(new QuadTree(
            new Bound
            {
                MinX = this.Bounds.MinX,
                MaxX = midX,
                MinY = midY,
                MaxY = this.Bounds.MaxY
            },
            this.MaxObjects,
            this.MaxLevel,
            nextLevel
        ));

        // ne锛堝彸涓婏級
        this.Nodes.Add(new QuadTree(
            new Bound
            {
                MinX = midX,
                MaxX = this.Bounds.MaxX,
                MinY = midY,
                MaxY = this.Bounds.MaxY
            },
            this.MaxObjects,
            this.MaxLevel,
            nextLevel
        ));

        // sw锛堝乏涓嬶級
        this.Nodes.Add(new QuadTree(
            new Bound
            {
                MinX = this.Bounds.MinX,
                MaxX = midX,
                MinY = this.Bounds.MinY,
                MaxY = midY
            },
            this.MaxObjects,
            this.MaxLevel,
            nextLevel
        ));

        // se锛堝彸涓嬶級
        this.Nodes.Add(new QuadTree(
            new Bound
            {
                MinX = midX,
                MaxX = this.Bounds.MaxX,
                MinY = this.Bounds.MinY,
                MaxY = midY
            },
            this.MaxObjects,
            this.MaxLevel,
            nextLevel
        ));
        Debug.DrawLine(new Vector3(midX, 0, this.Bounds.MinY), new Vector3(midX, 0, this.Bounds.MaxY), Color.red,100.0f);
        Debug.DrawLine(new Vector3(this.Bounds.MinX, 0, midY), new Vector3(this.Bounds.MaxX, 0, midY), Color.red,100.0f);
    }

    /// <summary>
    /// 鏌ユ壘瀵硅薄鎵€鍦ㄧ殑鍙跺瓙鑺傜偣
    /// </summary>
    public QuadTree Find(GameObject obj, QuadTree root)
    {
        // 璇ヨ妭鐐归潪鍙跺瓙鑺傜偣
        while (root.Nodes.Count == 4)
        {
            // 鑾峰彇鑺傜偣鎵€鍦ㄧ殑瀛愯妭鐐圭储寮?
            var index = root.GetIndex(obj);
            root = root.Nodes[index];
        }

        return root;
    }
}

