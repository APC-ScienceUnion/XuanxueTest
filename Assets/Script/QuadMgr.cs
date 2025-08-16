using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadMgr : MonoBehaviour
{
    public QuadTree tree;
    // Start is called before the first frame update
    void Start()
    {
        tree = new QuadTree(new Bound
            {
                MinX = -5,
                MaxX = 5,
                MinY = -5,
                MaxY = 5
            },
            5, 4,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 randomPos = new Vector3(Random.Range(-4.9f, 4.9f), 0, Random.Range(-4.9f, 4.9f));
            GameObject obj = Instantiate(Resources.Load("ball"), randomPos, Quaternion.identity) as GameObject;
            obj.name = "ball" + randomPos.x;
            tree.Insert(obj);
            QuadTree node = tree.Find(obj, tree);
        }
    }
}

