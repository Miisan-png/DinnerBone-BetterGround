using UnityEngine;

public enum Camera_Mode { Follow, Split_Screen }

/// ---------- Main camera system controller - handles switching between follow and split modes ----------



public class Camera_Manager : MonoBehaviour
{
    [SerializeField] private Camera main_camera;
    [SerializeField] private Camera split_camera_1;
    [SerializeField] private Camera split_camera_2;
    [SerializeField] private Transform player_1;
    [SerializeField] private Transform player_2;
    [SerializeField] private float split_distance = 15f;
    [SerializeField] private float transition_speed = 2f;

    private Camera_Mode current_mode = Camera_Mode.Follow;
    private Camera_Follow follow_script;
    private Split_Screen_Controller split_script;

    void Start()
    {
        follow_script = GetComponent<Camera_Follow>();
        split_script = GetComponent<Split_Screen_Controller>();

        follow_script.Initialize(main_camera, player_1, player_2);
        split_script.Initialize(split_camera_1, split_camera_2, player_1, player_2);

        SetCameraMode(Camera_Mode.Follow);
    }

    void Update()
    {
        float distance = Vector3.Distance(player_1.position, player_2.position);

        if (distance > split_distance && current_mode == Camera_Mode.Follow)
        {
            SetCameraMode(Camera_Mode.Split_Screen);
        }
        else if (distance <= split_distance && current_mode == Camera_Mode.Split_Screen)
        {
            SetCameraMode(Camera_Mode.Follow);
        }
    }

    private void SetCameraMode(Camera_Mode mode)
    {
        current_mode = mode;

        if (mode == Camera_Mode.Follow)
        {
            main_camera.gameObject.SetActive(true);
            split_camera_1.gameObject.SetActive(false);
            split_camera_2.gameObject.SetActive(false);
            follow_script.enabled = true;
            split_script.enabled = false;
        }
        else
        {
            main_camera.gameObject.SetActive(false);
            split_camera_1.gameObject.SetActive(true);
            split_camera_2.gameObject.SetActive(true);
            follow_script.enabled = false;
            split_script.enabled = true;
        }
    }
}