using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class CameraController : MonoBehaviour
{
    public float speedRotation = 5f;
    public float speedMove = 30f;
    public float shiftBoost = 10f;

    private float yaw = 0f;
    private float pitch = 0f;


    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject (-1) && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            yaw += speedRotation * Input.GetAxis("Mouse X");
            pitch -= speedRotation * Input.GetAxis("Mouse Y");
            
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        float boost = Input.GetKey(KeyCode.LeftShift) ? shiftBoost : 1f;

        Vector3 velocity = GetKeyboardInput();
        if (!velocity.Equals(Vector3.zero))
        {
            velocity *= boost * speedMove * Time.deltaTime;
        }
        transform.Translate(velocity);

    }
    
    private Vector3 GetKeyboardInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 velocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            velocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey (KeyCode.A)){
            velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey (KeyCode.E)){
            velocity += new Vector3(0, 1 , 0);
        }
        if (Input.GetKey (KeyCode.Q)){
            velocity += new Vector3(0, -1, 0);
        }
        return velocity;
    }
}
