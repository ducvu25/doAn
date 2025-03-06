using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public float speedDrone;
    public float moveTargetImportance;
    public float conflictDistanceImportance;
    public float distanceCheckDrone;
    public float distanceMoveTarget;

    public List<Transform> drones = new List<Transform>();


    [SerializeField] List<TypeShape> typeShapes = new List<TypeShape>();
    [SerializeField] int indexTask = 0;
    int nCheckShow = 0;
    public static DroneManager instance;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        int n = transform.childCount;
        for (int i = 0; i < n; i++) {
            drones.Add(transform.GetChild(i));
            transform.GetChild(i).GetComponent<Drone>().indexDrone = i;
        }

        //PhanNhiem();
        Hugrarian();
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

    void PhanNhiem()
    {
        int i = 0; 
        for(int j=0; j < typeShapes[indexTask].items.Count; j++)
        {
            List<Transform> t = typeShapes[indexTask].items[j].transIndex;
            for (int j2 = 0; j2 < t.Count; j2++) {
                drones[i].GetComponent<Drone>().SetTask(t[j2], typeShapes[indexTask].items[j].color);
                i++;
            }
        }
    }

    public List<bool> indexColCheck;
    public List<bool> indexRowCheck;
    public List<Vector2> indexChoose = new List<Vector2>();
    void Hugrarian()
    {
        List<Transform> targets = new List<Transform>();
        List<Color> colors = new List<Color>();
        for (int j = 0; j < typeShapes[indexTask].items.Count; j++)
        {
            List<Transform> t = typeShapes[indexTask].items[j].transIndex;
            for (int j2 = 0; j2 < t.Count; j2++)
            {
                targets.Add(t[j2]);
                colors.Add(typeShapes[indexTask].items[j].color);   
            }
        }
        float[,] matrix = InitPrice(targets);
         //{ {80, 120, 125, 140 },
         //                   {20, 115, 145, 60 },
         //                   {40, 100, 85, 45 },
         //                   {65, 35, 25, 75 } };
        int n = drones.Count;
        //ShowMatrix(matrix, n);

        matrix = SubMinRowValue(matrix, n);
        matrix = SubMinColValue(matrix, n);

        bool isStop = false;
        while (!isStop)
        {
            isStop = true;
            indexColCheck = new List<bool>(new bool[n]);
            indexRowCheck = new List<bool>(new bool[n]);
            indexChoose.Clear();

            SelectRowSolution(matrix, n, ref indexColCheck, ref indexChoose);

            SelectColSolution(matrix, n, ref indexColCheck, ref indexRowCheck, ref indexChoose);
            if (indexChoose.Count < n)
            {
                isStop = false;
                matrix = Adjust(matrix, n, ref indexColCheck, ref indexRowCheck, ref indexChoose);
            }
        }
        
        for (int i = 0; i < drones.Count; i++)
        {
            drones[(int)indexChoose[i].x].GetComponent<Drone>().SetTask(targets[(int)indexChoose[i].y], colors[(int)indexChoose[i].y]);
            drones[(int)indexChoose[i].x].GetComponent<Drone>().SetColor(Color.white);
        }
        nCheckShow = 0;
    }
    #region Hugrarian
    void ShowMatrix(float [,] matrix, int n)
    {
        string sCheck = "";
        for (int i = 0; i < n; i++)
        {
            string s = "Line: " + i + " | ";
            for (int j = 0; j < n; j++)
            {
                s += $"{matrix[i, j]:000} |";
            }
            sCheck += s + "\n";
        }
        Debug.Log(sCheck);
    }
    float[,] InitPrice(List<Transform> listTarget) // B1
    {
        int n = drones.Count;
        float[,] matrix = new float[n, n];
        for(int i = 0; i < n; i++)
        {
            for(int j = 0;j < n; j++)
            {
                float d = Vector3.Distance(listTarget[i].position, drones[j].position);
                matrix[i,j] = d;
            }
        }
        return matrix;
    }
    float[,] SubMinRowValue(float[,] matrix, int n) // B2
    {
        for(int i = 0; i<n; i++)
        {
            int indexMin = 0;
            for(int j=1; j<n; j++)
            {
                if (matrix[i, j] < matrix[i, indexMin])
                {
                    indexMin = j;
                }
            }
            float minValue = matrix[i, indexMin];
            for (int j = 0; j < n; j++)
            {
                matrix[i, j] -= minValue;
            }
        }

        //ShowMatrix(matrix, n);
        return matrix;
    }
    float[,] SubMinColValue(float[,] matrix, int n) // B3
    {
        for (int i = 0; i < n; i++)
        {
            int indexMin = 0;
            for (int j = 1; j < n; j++)
            {
                if (matrix[j, i] < matrix[indexMin, i])
                {
                    indexMin = j;
                }
            }
            float minValue = matrix[indexMin, i];
            for (int j = 0; j < n; j++)
            {
                matrix[j, i] -= minValue;
            }
        }
        //ShowMatrix(matrix, n);
        return matrix;
    }
    void SelectRowSolution(float[,] matrix, int n, ref List<bool> indexColCheck, ref List<Vector2> indexChoose)  // B4.1
    {
        bool isStop = false;
        while (!isStop)
        {
            isStop = true;
            for (int i = 0; i < n; i++)
            {
                int indexCheck = -2;
                for (int j = 0; j < n; j++)
                {
                    if (matrix[i, j] == 0 && !indexColCheck[j])
                    {
                        if (indexCheck == -2)
                            indexCheck = j;
                        else
                            indexCheck = -1;
                    }
                }
                if (indexCheck > -1)
                {
                    isStop = false;
                    indexColCheck[indexCheck] = true;
                    indexChoose.Add(new Vector2(i, indexCheck));
                }
            }
        }
    }
    void SelectColSolution(float[,] matrix, int n, ref List<bool> indexColCheck, ref List<bool> indexRowCheck, ref List<Vector2> indexChoose)  // B4.2
    {
        bool isStop = false;
        while (!isStop)
        {
            isStop = true;
            for (int i = 0; i < n; i++)
            {
                if (indexColCheck[i])
                    continue;
                int indexCheck = -2;
                for (int j = 0; j < n; j++)
                {
                    if (matrix[j, i] == 0 && !indexRowCheck[j])
                    {
                        if (indexCheck == -2)
                            indexCheck = j;
                        else
                            indexCheck = -1;
                    }
                }
                if (indexCheck > -1)
                {
                    isStop = false;
                    indexRowCheck[indexCheck] = true;
                    indexChoose.Add(new Vector2(indexCheck, i));
                }
            }
        }
    }
    float[,] Adjust(float[,] matrix, int n, ref List<bool> indexColCheck, ref List<bool> indexRowCheck, ref List<Vector2> indexChoose) // B5
    {
        // B1: Tìm giá trị min trong các số chưa gạch
        float minValue = -1;
        for(int i = 0; i<n; i++)
        {
            if (indexRowCheck[i])
                continue ;
            for(int j = 0; j < n; j++)
            {
                if(indexColCheck[j])
                    continue ;
                if (minValue == -1 || matrix[i, j] < minValue)
                    minValue = matrix[i, j];
            }
        }
        // B2: các số gạch 1 lần giữ nguyên, gạch 2 lần + minValue, còn lại - minValue
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (indexRowCheck[i] && indexColCheck[j])
                {
                    matrix[i, j] += minValue;
                }else if(!indexRowCheck[i] && !indexColCheck[j])
                    matrix[i, j] -= minValue;
            }
        }
        return matrix;
    }
    #endregion
    public void ShowDrone()
    {
        nCheckShow++;
        if(nCheckShow == drones.Count)
        {
            indexTask++;
            if (indexTask < drones.Count)
            {
                for (int i = 0; i < drones.Count; i++)
                {
                    //drones[(int)indexChoose[i].x].GetComponent<Drone>().SaveFile(typeShapes[indexTask-1].name);
                    Invoke("Hugrarian", 5);
                }
            }
        }
    }

}
