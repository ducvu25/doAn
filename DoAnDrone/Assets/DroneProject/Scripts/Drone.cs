using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum STATE_DRONE
{
    NORMAIL,
    FOLLOW,
    COMPLED,
    ERRO
}
public class InforDrone
{
    public int id;
    public Vector3 v;
    public STATE_DRONE state;
    public Vector3 huong;
    public float distance;
    public Vector3 target;
    public InforDrone(int id)
    {
        this.id = id;
        v = Vector3.zero;
        state = STATE_DRONE.NORMAIL;
        huong = Vector3.zero;
        distance = 0;
    }
}
public class Drone : MonoBehaviour
{
    public Transform transTarget;

    public InforDrone inforDrone {get; private set;}
    
    Rigidbody rb;
    Light light;
    bool showLight = false;
    Color colorShow;

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
        if(inforDrone.id == -1) return;
        if (inforDrone.state == STATE_DRONE.FOLLOW || inforDrone.state == STATE_DRONE.COMPLED)
        {
            Vector3 dir = (transTarget.position - transform.position);
            inforDrone.distance = dir.magnitude;
            float k = Mathf.Clamp(inforDrone.distance / DroneManager.instance.distanceMoveTarget, 0, 1); 
            Vector3 dirDroneCheck = DroneManager.instance.DirSupervisoryDrone(inforDrone.id);

            inforDrone.huong = (dir.normalized * DroneManager.instance.moveTargetImportance + dirDroneCheck).normalized;
            inforDrone.v = inforDrone.huong * DroneManager.instance.speedDrone * k;
            rb.velocity = inforDrone.v;

            if (k < 0.1f && !showLight) {
                inforDrone.state = STATE_DRONE.COMPLED;
                ShowLight();
                DroneManager.instance.ShowDrone();
            }
            //listPosition.Add(transform.position);
        }
    }
    public void SetValue(int id)
    {
        inforDrone = new InforDrone(id);
    }
    public void SetTask(Transform target, Color color)
    {
        transTarget = target;
        inforDrone.state = STATE_DRONE.FOLLOW;
        inforDrone.target = target.position;
        showLight = false;
        colorShow = color;
        //Debug.Log(true);
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
        if (collision.transform.CompareTag("Drone") && inforDrone.state == STATE_DRONE.FOLLOW)
        {
            if (Vector3.Angle(inforDrone.huong, collision.transform.position) >= 20)
            {
                inforDrone.v = inforDrone.v + new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                rb.velocity = inforDrone.v;
            }
        }
    }
}
