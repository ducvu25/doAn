using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public Transform transTarget;
    bool isFollow;

    public int indexDrone;
    
    Rigidbody rb;
    Light light;
    bool showLight = false;
    Color colorShow;
    public bool isWarning = false;
    //List<Vector3> listPosition=new List<Vector3>();
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        light = GetComponentInChildren<Light>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //isFollow = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFollow)
        {
            Vector3 dir = (transTarget.position - transform.position);
            float d = dir.magnitude;
            float k = Mathf.Clamp(d / DroneManager.instance.distanceMoveTarget, 0, 1); 
            Vector3 dirDroneCheck = DroneManager.instance.DirSupervisoryDrone(indexDrone);

            Vector3 v = dir.normalized * DroneManager.instance.moveTargetImportance + dirDroneCheck;
            rb.velocity = v.normalized * DroneManager.instance.speedDrone*k;

            if (k < 0.1f && !showLight) {
                ShowLight();
                DroneManager.instance.ShowDrone();
            }
            //listPosition.Add(transform.position);
        }
    }
    public void SetTask(Transform target, Color color)
    {
        transTarget = target;
        isFollow = true;
        showLight = false;
        colorShow = color;
        //listPosition.Clear();
    }
    void ShowLight()
    {
        showLight = true;
        light.color = colorShow;
    }
    public void SetColor(Color x)
    {
        light.color = x;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Drone") && isFollow)
        {
            Debug.Log(1);
            Drone drone = collision.transform.GetComponent<Drone>();
            if (Vector3.Angle(drone.GetVelocity(), GetVelocity()) >= 160 && !isWarning)
            {
                isWarning = true;
                drone.isWarning = true;
                Transform t1 = transTarget;
                Transform t2 = drone.transTarget;

                transTarget = t2;
                drone.transTarget = t1;
                isWarning = false;
                drone.isWarning = false;
            }
        }
    }
    public Vector3 GetVelocity() => rb.velocity;
    //public void SaveFile(string name)
    //{
    //    string fileName = name + " " + indexDrone + ".csv";
    //    string filePath = Path.Combine(Application.persistentDataPath, fileName);

    //    StringBuilder sb = new StringBuilder();

    //    // Ghi tiêu đề
    //    sb.AppendLine("X,Y,Z");

    //    // Ghi từng tọa độ trong listPosition
    //    foreach (var position in listPosition)
    //    {
    //        sb.AppendLine($"{position.x},{position.y},{position.z}");
    //    }

    //    // Lưu file
    //    File.WriteAllText(filePath, sb.ToString());

    //    Debug.Log($"Saved file to: {filePath}");
    //}
}
