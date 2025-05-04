using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class DataGame
{ 
    public static List<List<Data>> datas = new List<List<Data>>();

    public static void SaveData(List<TypeShape> shapeList)
    {
        datas = new List<List<Data>>();
        for (int i = 0; i < shapeList.Count; i++) 
            datas.Add(ReadFile(shapeList[i]._name));
    }
    public static List<Data> ReadFile(string path)
    {
        var result = new List<Data>();
        // đường dẫn tới Resources/yourfile.csv
        string filePath = Application.dataPath + "/Resources/" + path + ".csv";
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return new List<Data>();
        }
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        // Bỏ qua dòng header, bắt đầu từ i=1
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Split thành 4 phần: id, color, numberFrame, Infor
            var parts = line.Split(new[] { ',' }, 4);
            // parts[0] = id (bỏ qua), parts[1] = isRandom, parts[2] = numberFrame, parts[3] = "(x;y;z); (r;g;b;a)"

            var colParts = parts[1].Split(';');
            Color color = new Color(
                float.Parse(colParts[0], CultureInfo.InvariantCulture),
                float.Parse(colParts[1], CultureInfo.InvariantCulture),
                float.Parse(colParts[2], CultureInfo.InvariantCulture),
                float.Parse(colParts[3], CultureInfo.InvariantCulture)
            );

            // 2. Lấy phần Infor
            string[] infos = parts[3].Split(',');
            int numberFrame = int.Parse(parts[2], CultureInfo.InvariantCulture);
            //Debug.Log(infos.Length + " " + numberFrame);
            // 5. Đưa vào Data/DataFrame
            var dataItem = new Data
            {
                color = color,
                positions = new List<Vector3>()
            };
            foreach (string info in infos)
            {
                var posSplit = info.Split(';');

                Vector3 position = new Vector3(
                    float.Parse(posSplit[0]),
                    float.Parse(posSplit[1]),
                    float.Parse(posSplit[2])
                );

                dataItem.positions.Add(position);
            }
            result.Add(dataItem);
        }
        return result;
    }
}
