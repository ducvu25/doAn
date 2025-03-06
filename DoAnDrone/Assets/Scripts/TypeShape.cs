using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeShape : MonoBehaviour
{
    // 64
    [System.Serializable]
    public struct ShapeItem
    {
        public string name;
        public List<Transform> transIndex;
        public Color color;

        public ShapeItem(string n, List<Transform> t, Color c)
        {
            name = n;
            transIndex = t;
            color = c;
        }
    }
    public List<ShapeItem> items = new List<ShapeItem>(); 
    // Start is called before the first frame update
    void Start()
    {
        //List<Transform> p = new List<Transform>();
        //for (int i = 0; i < transform.GetChild(0).childCount; i++)
        //{
        //    p.Add(transform.GetChild(0).GetChild(i));
        //}
        //items.Add(new ShapeItem("Ngoi Sao", p, Color.yellow));



    }

}
