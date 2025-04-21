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
    public Data transTarget;

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
            Vector3 dir = (transTarget.data[DroneManager.instance.indexFrame].position - transform.position);
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
    public void SetTask(Data data)
    {
        transTarget = data;
        inforDrone.state = STATE_DRONE.FOLLOW;
        inforDrone.target = data.data[0].position;
        showLight = false;
        colorShow = data.data[0].color;
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
            Vector3 attractDir = inforDrone.huong.normalized;

            // 2. Hướng đẩy khỏi drone vừa chạm
            Vector3 repelDir = (transform.position - collision.transform.position).normalized;

            // 3. Tính góc giữa hai vectơ
            float angle = Vector3.Angle(attractDir, repelDir);

            // 4. Ngưỡng dead‑zone (gần 180° mới coi là triệt tiêu)
            const float deadZoneAngle = 160f;

            if (angle >= deadZoneAngle)
            {
                // Dead‑zone: thêm kick pháp tuyến
                Vector3 tangent = Vector3.Cross(repelDir, Vector3.up).normalized;
                float tangentKick = 1.0f;
                inforDrone.v += tangent * tangentKick;
                // 5. Cập nhật rigidbody
                rb.velocity = inforDrone.v;
            }

        }
    }
}
