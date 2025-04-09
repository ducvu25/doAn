using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FollowController : MonoBehaviour
{
    [SerializeField] Button btnExit;
    [SerializeField] Button btnPer;
    [SerializeField] Button btnDrone;

    [SerializeField] ItemContentShow itemContentShow;
    [SerializeField] Button btnPre;
    [SerializeField] Button btnNext;
    int indexDrone = 0;

    TYPE_CAMERA typeCam = TYPE_CAMERA.DEFAULT;
    private void Start()
    {
        btnPre.onClick.AddListener(() =>
        {
            indexDrone--;
            SetUpBtn();
        });
        btnNext.onClick.AddListener(() =>
        {
            indexDrone++;
            SetUpBtn();
        });
        btnPer.onClick.AddListener(() => {
            SetUp(TYPE_CAMERA.PERSON);
        });
        btnDrone.onClick.AddListener(() => {
            SetUp(TYPE_CAMERA.DRONE);
        });
        btnExit.onClick.AddListener(() => {
            SetUp(TYPE_CAMERA.DEFAULT);
            DroneManager.instance.uiController.SetUpUI(0);
        });
    }
    public void SetUp(TYPE_CAMERA typeCam)
    {
        btnPer.gameObject.SetActive(false);
        btnDrone.gameObject.SetActive(false);   
        btnPre.gameObject.SetActive(false);
        btnNext.gameObject.SetActive(false);
        itemContentShow.gameObject.SetActive(false);

        if (typeCam == TYPE_CAMERA.PERSON)
        {
            btnDrone.gameObject.SetActive(true);
            DroneManager.instance.cameraController.SetUpCamera(typeCam, null);
        }
        else if(typeCam == TYPE_CAMERA.DEFAULT)
        {
            btnPer.gameObject.SetActive(true);
            btnDrone.gameObject.SetActive(true);
            DroneManager.instance.cameraController.SetUpCamera(typeCam, null);
        }
        else if(typeCam == TYPE_CAMERA.DRONE)
        {
            itemContentShow.gameObject.SetActive(true);
            btnPer.gameObject.SetActive(true);
            indexDrone = 0;
            SetUpBtn();
        }
        this.typeCam = typeCam;
    }
    void SetUpBtn()
    {
        DroneManager.instance.cameraController.SetUpCamera(typeCam, DroneManager.instance.drones[indexDrone].transform);
        btnPre.gameObject.SetActive(indexDrone > 0);
        btnNext.gameObject.SetActive(indexDrone < DroneManager.instance.drones.Count-1);
        
    }
    private void Update()
    {
        if(typeCam == TYPE_CAMERA.DRONE)
        {
            itemContentShow.SetUp(DroneManager.instance.droneList[indexDrone].inforDrone);
        }
    }
}
