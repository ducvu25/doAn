using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public void ExportFile()
    {
        string filePath = Application.dataPath + "/Resources/" + _name + ".csv";
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Name,IsRandom, NumberFrame, Infor");

        int id = 0;
        int nFrame = 1;
        foreach (var item in items)
        {
            foreach (var t in item.transIndex)
            {
                var c = item.color;
                // Format: tên, r, g, b, a, isRandom, x, y, z
                sb.AppendFormat(
                    "{0},{1},{2},({3};{4};{5}); ({6};{7};{8};{9})\n",
                    id++,
                    item.isRandom,
                    nFrame,
                    t.position.x, t.position.y, t.position.z,
                    c.r, c.g, c.b, c.a
                );
            }
        }

        // Ghi ra file
        Debug.Log($"Exported {items.Count} shapes → CSV to: {filePath}");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
    public List<ShapeItem> items = new List<ShapeItem>(); 
}
