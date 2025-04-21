using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public float speedDrone;
    public float moveTargetImportance;
    public float conflictDistanceImportance;
    public float distanceCheckDrone;
    public float distanceMoveTarget;

    public List<Transform> drones = new List<Transform>();
    public List<Drone> droneList = new List<Drone>();

    [SerializeField] List<GameObject> shapeList = new List<GameObject>();
    List<TypeShape> typeShapes = new List<TypeShape>();
    List<List<Data>> datas = new List<List<Data>>();
    [SerializeField] int indexTask = 0;
    int nCheckShow = 0;
    public static DroneManager instance;

    public CameraController cameraController;
    public UI_Controller uiController;

    private void Awake()
    {
        instance = this;
        cameraController = GetComponent<CameraController>();
    }
    public int indexFrame = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < shapeList.Count; i++)
        {
            //typeShapes.Add(shapeList[i].transform.GetComponent<TypeShape>());
            //typeShapes[i].ExportFile();
            datas.Add(ReadFile(shapeList[i].transform.GetComponent<TypeShape>()._name));
        }
        //return;   

        int n = transform.childCount;
        for (int i = 0; i < n; i++) {
            drones.Add(transform.GetChild(i));
            droneList.Add(transform.GetChild(i).GetComponent<Drone>());
            droneList[i].SetValue(i);
        }
        cameraController.SetUpCamera(TYPE_CAMERA.DEFAULT, null);
        //PhanNhiem();
        StartCoroutine(SetUpHungarian(0));
    }
    public Vector3 DirSupervisoryDrone(int i)
    {
        Vector3 dir = Vector3.zero;
        for(int j=0; j<drones.Count; j++)
        {
            if(j != i)
            {
                float d = Vector3.Distance(drones[i].position, drones[j].position);
                if(d < distanceCheckDrone)
                {
                    dir += (drones[i].position - drones[j].position);
                }
            }
        }
        return dir.normalized* conflictDistanceImportance;
    }

    public void ShowDrone()
    {
        nCheckShow++;
        if(nCheckShow == drones.Count)
        {
            nCheckShow = 0;
            if(indexFrame == datas[indexTask][0].data.Count - 1)
            {
                indexTask++;
                if (indexTask < datas.Count)
                {
                    StartCoroutine(SetUpHungarian(5));
                }
            }else
                indexFrame++;
        }
    }
    IEnumerator SetUpHungarian(float delay)
    {
        yield return new WaitForSeconds(delay);
        indexFrame = 0;
        Hungarian.SetUpHungarian(datas[indexTask], ref drones);
    }
    public List<Data> ReadFile(string path)
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

            // Split thành 4 phần: id, isRandom, numberFrame, Infor
            var parts = line.Split(new[] { ',' }, 4);
            // parts[0] = id (bỏ qua), parts[1] = isRandom, parts[2] = numberFrame, parts[3] = "(x;y;z); (r;g;b;a)"

            // 1. Parse isRandom
            bool isRandom = bool.Parse(parts[1].Trim());

            // 2. Lấy phần Infor
            string[] infos = parts[3].Split(',');
            // 5. Đưa vào Data/DataFrame
            var dataItem = new Data
            {
                isRandom = isRandom,
                data = new List<Data.DataFrame>()
            };
            foreach(string info in infos) {
                // 3. Tách ra 2 nhóm trong ngoặc: vị trí và màu
                int firstClose = info.IndexOf(')');
                int secondOpen = info.IndexOf('(', firstClose);
                int secondClose = info.LastIndexOf(')');

                string posData = info.Substring(1, firstClose - 1);               // giữa "(" và ")"
                string colData = info.Substring(secondOpen + 1, secondClose - secondOpen - 1);

                // 4. Parse thành float[]
                var posSplit = posData.Split(';');
                var colSplit = colData.Split(';');

                Vector3 position = new Vector3(
                    float.Parse(posSplit[0]),
                    float.Parse(posSplit[1]),
                    float.Parse(posSplit[2])
                );

                Color color = new Color(
                    float.Parse(colSplit[0]),
                    float.Parse(colSplit[1]),
                    float.Parse(colSplit[2]),
                    float.Parse(colSplit[3])
                );

                Data.DataFrame frame = new Data.DataFrame
                {
                    position = position,
                    color = color
                };

                dataItem.data.Add(frame);
            }
            result.Add(dataItem);
        }

        return result;
    }
}
