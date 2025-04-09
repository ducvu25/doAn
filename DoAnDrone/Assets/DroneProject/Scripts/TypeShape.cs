using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeShape : MonoBehaviour
{
    public string _name;
    // 64
    [System.Serializable]
    public struct ShapeItem
    {
        public string name;
        public List<Transform> transIndex;
        public Color color;
        public bool isRandom;

        public ShapeItem(string n, List<Transform> t, Color c, bool i)
        {
            name = n;
            transIndex = t;
            color = c;
            isRandom = i;
        }
    }
    public List<ShapeItem> items = new List<ShapeItem>(); 
}
