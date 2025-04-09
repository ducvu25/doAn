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
    public List<Drone> droneList = new List<Drone>();

    [SerializeField] List<GameObject> shapeList = new List<GameObject>();
    List<TypeShape> typeShapes = new List<TypeShape>();
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
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i< shapeList.Count; i++)
            typeShapes.Add(shapeList[i].transform.GetComponent<TypeShape>());

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
            indexTask++;
            if (indexTask < typeShapes.Count)
            {
                StartCoroutine(SetUpHungarian(5));
            }
        }
    }
    IEnumerator SetUpHungarian(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hungarian.SetUpHugrarian(typeShapes[indexTask], ref drones);
    }
}
