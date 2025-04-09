using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    [SerializeField] GameObject goPanelMain;

    [SerializeField] Button btnPause;
    [SerializeField] Button btnInforFollow;
    [SerializeField] Button btnFollow;

    [SerializeField] GameObject goPanelShowInfor;
    [SerializeField] Transform transContentFollow;
    [SerializeField] GameObject goPreShowInfor;
    List<ItemContentShow> itemsShow = new List<ItemContentShow>();

    bool isShowFollow = false;

    [SerializeField] GameObject goPanelFollow;
    [SerializeField] UI_FollowController followController;

    [SerializeField] TextMeshProUGUI txtShowTime;
    int timeShow = 0;

    // Start is called before the first frame update
    void Start()
    {
        btnPause.onClick.AddListener(() =>
        {
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        });
        
        btnInforFollow.onClick.AddListener(OnShowInfor);
        btnFollow.onClick.AddListener(() =>
        {
            SetUpUI(2);
            followController.SetUp(TYPE_CAMERA.DRONE);
        });

        SetUpUI(0);

        StartCoroutine(ShowTime());
    }
    public void SetUpUI(int type)
    {
        DroneManager.instance.cameraController.enabled = type > 1;
        goPanelMain.SetActive(type <= 1);
        goPanelShowInfor .SetActive(type == 1);
        goPanelFollow.SetActive(type == 2);
    }
    // Update is called once per frame
    void Update()
    {
        if (isShowFollow)
        {
            if (itemsShow.Count > 0)
            {
                for (int i = 0; i < itemsShow.Count; i++)
                {
                    itemsShow[i].SetUp(DroneManager.instance.droneList[i].inforDrone);
                }
            }
            else
            {
                InitShowInfor();
            }
        }
    }
    IEnumerator ShowTime()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);
        while (true) {
            timeShow++;
            txtShowTime.text = $"{timeShow / 60: 00}m : {timeShow % 60: 00}s";

            yield return waitOneSecond;
        }
    }
    void InitShowInfor()
    {
        for(int i=0; i<DroneManager.instance.drones.Count; i++)
        {
            GameObject go = Instantiate(goPreShowInfor, transContentFollow);
            ItemContentShow itemContentShow = go.transform.GetComponent<ItemContentShow>();
            itemsShow.Add(itemContentShow);
        }
    }
    void OnShowInfor()
    {
        if (itemsShow.Count == 0)
        {
            InitShowInfor();
        }
        isShowFollow = !isShowFollow;
        goPanelShowInfor.SetActive(isShowFollow);
    }
}
