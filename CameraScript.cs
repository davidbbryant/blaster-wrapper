using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        //Force camera to specific aspect ratio
        mainCamera.aspect = 16.0f / 9.0f;
    }
}
