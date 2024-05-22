using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadUIFollower : MonoBehaviour
{
    public Transform head;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        if (head == null)
        {
            head = GetComponent<Transform>();
        }
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation=Quaternion.LookRotation(cam.transform.forward);
    }
}
