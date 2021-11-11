using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public GameManager              gameManager;
    float                           cameraSize = 25.0f;

    float                           x;
    float                           y;

    Vector3                         curPos;
    Vector3                         newPos;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(25, 25, -10);

        x = transform.position.x;
        y = transform.position.y;

        curPos = transform.position;
        newPos = new Vector3(0,0, -10);
    }

    // Update is called once per frame
    void Update()
    {
        curPos = transform.position;
        curPos.x += Input.GetAxis("Horizontal");
        curPos.y += Input.GetAxis("Vertical");
        transform.position = curPos;
        SetCameraSize();
    }

    void SetCameraSize()
    {
        cameraSize += Input.GetAxis("Mouse ScrollWheel") * -3;
        Camera.main.orthographicSize = cameraSize;
    }
}
