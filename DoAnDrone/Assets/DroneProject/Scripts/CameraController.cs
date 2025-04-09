using UnityEngine;

public enum TYPE_CAMERA
{
    PERSON,
    DEFAULT,
    DRONE
}
public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 5f; // Tốc độ phóng to/thu nhỏ
    public float rotationSpeed = 100f; // Tốc độ xoay
    public float minZoom = 10f; // Zoom tối thiểu
    public float maxZoom = 100f; // Zoom tối đa

    private Camera cam;

    [SerializeField] Camera[] cameras;
    void Start()
    {
        cam = Camera.main;
    }

    public void SetUpCamera(TYPE_CAMERA typeCam, Transform trans)
    {
        for(int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);
        cam = cameras[(int)typeCam];
        cam.gameObject.SetActive(true);
        if (typeCam == TYPE_CAMERA.DRONE)
        {
            cam.transform.parent = trans;
            cam.transform.localPosition = Vector2.zero;
        }
        cam.transform.rotation = Quaternion.identity;
    }
    void Update()
    {
        // Xử lý phóng to/thu nhỏ
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            float newZoom = cam.fieldOfView - scrollInput * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(newZoom, minZoom, maxZoom);
        }

        // Xử lý xoay camera
        if (Input.GetMouseButton(0)) // Nếu giữ nút chuột trái
        {
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            cam.transform.Rotate(-rotationY, rotationX, 0);
        }
    }
}