using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

    [HideInInspector]
    public List<Transform> drones = new List<Transform>();
    public List<Drone> droneList = new List<Drone>();

    [SerializeField] List<GameObject> shapeList = new List<GameObject>();
    
    [SerializeField] int indexTask = 0;
    int nCheckShow = 0;
    public static DroneManager instance;
    List<List<Data>> datas = new List<List<Data>>();

    public CameraController cameraController;
    public UI_Controller uiController;
    List<int> checkState = new List<int>();
    bool isEnd = false;
    private void Awake()
    {
        indexTask = -1;
        instance = this;
        cameraController = GetComponent<CameraController>();
    }
    public int indexFrame = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < shapeList.Count; i++)
        {
            //shapeList[i].transform.GetComponent<TypeShape>().ExportFile();
            datas.Add(DataGame.ReadFile(shapeList[i].transform.GetComponent<TypeShape>()._name));
        }
        for(int i=0; i<DataGame.datas.Count; i++)
            datas.Insert(datas.Count - 1, DataGame.datas[i]);
        //return;   

        int n = transform.childCount;
        for (int i = 0; i < n; i++) {
            drones.Add(transform.GetChild(i));
            droneList.Add(transform.GetChild(i).GetComponent<Drone>());
            droneList[i].SetValue(i);
        }
        cameraController.SetUpCamera(TYPE_CAMERA.DEFAULT, null);
        StartCoroutine(SetUpHungarian(0));
        StartCoroutine(CheckProfiler(0.5f));
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
    public Vector3 ComputeRepulsiveForce(int i)
    {
        Vector3 force = Vector3.zero;
        Vector3 pi = drones[i].position;

        for (int j = 0; j < drones.Count; j++)
        {
            if (j == i) continue;

            Vector3 pj = drones[j].position;
            float d = Vector3.Distance(pi, pj);

            if (d > 0f && d < distanceCheckDrone)
            {
                // công thức: k_rep * (1/d - 1/d0) * 1/d^2
                float term = (1f / d - 1f / distanceCheckDrone);
                float magnitude = term / (d * d);

                Vector3 dir = (pi - pj).normalized;
                force += dir * magnitude;
            }
        }

        return force;
    }
    public void ShowDrone(int id)
    {
        if (checkState.Contains(id)) return;
        checkState.Add(id);
        nCheckShow++;
        if(nCheckShow == drones.Count)
        {
            checkState.Clear();
            nCheckShow = 0;
            if (indexFrame == datas[indexTask][0].positions.Count - 1)
            {
                if (indexTask < datas.Count - 1)
                {
                    StartCoroutine(SetUpHungarian(5));
                }
                else
                {
                    isEnd = true;
                }
            }
            else
            {
                indexFrame++;
            }
        }
    }
    IEnumerator SetUpHungarian(float delay)
    {
        yield return new WaitForSeconds(delay);
        indexTask++;
        indexFrame = 0;
        Hungarian.SetUpHungarian(datas[indexTask], ref drones);
    }
    public IEnumerator CheckProfiler(float timeDelay)
    {
        // Khoảng thời gian giữa 2 sample
        WaitForSeconds wait = new WaitForSeconds(timeDelay);

        // Mảng chứa danh sách vị trí của tất cả drone ở mỗi mẫu
        List<List<Vector3>> samples = new List<List<Vector3>>();

        // Lặp cho đến khi isEnd = true
        while (!isEnd)
        {
            List<Vector3> snapshot = new List<Vector3>(drones.Count);
            for (int i = 0; i < drones.Count; i++)
            {
                snapshot.Add(drones[i].position);
            }
            samples.Add(snapshot);

            yield return wait;
        }

        // Build CSV
        var sb = new StringBuilder();

        // Header: Time, d0_x,d0_y,d0_z, d1_x,d1_y,d1_z, ...
        sb.Append("time");
        for (int i = 0; i < drones.Count; i++)
            sb.AppendFormat(",d{0}_x,d{0}_y,d{0}_z", i);
        sb.AppendLine();

        // Rows
        for (int s = 0; s < samples.Count; s++)
        {
            float t = s * timeDelay;
            sb.Append(t.ToString("F3"));    // thời gian tính từ start
            var snap = samples[s];
            for (int i = 0; i < snap.Count; i++)
            {
                var v = snap[i];
                sb.AppendFormat(",{0:F4},{1:F4},{2:F4}", v.x, v.y, v.z);
            }
            sb.AppendLine();
        }

        // Ghi file
        string path = Path.Combine(Application.dataPath + "/Resources/", "drone_profiler.csv");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[Profiler] Exported {samples.Count} samples to: {path}");
    }
}
