using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EarthLocation : MonoBehaviour
{
    public InputActionProperty TriggerPressL;
    public InputActionProperty TriggerPressR;
    public Transform Pin;
    public Transform Earth;
    public Transform OuterSphere;
    public Transform InnerSphere;

    public bool TouchingEarth;
    Vector3 TouchLocation;


    // Start is called before the first frame update
    void Start()
    {
        TriggerPressL.action.performed += SelectLocation;
        TriggerPressR.action.performed += SelectLocation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        TriggerPressR.action.performed -= SelectLocation;
        TriggerPressL.action.performed -= SelectLocation;
    }
    private void OnTriggerEnter(Collider other)
    {
        // if the other collision object is a controller, set TouchingEarth and record the location of the touch
        if (other.GetComponent<TeleportLineToggle>())
        {
            TouchingEarth = true;
            TouchLocation = other.ClosestPoint(this.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TouchingEarth = false;
    }
    void SelectLocation(InputAction.CallbackContext context)
    {
        // if the controller is touching earth while the trigger is pressed, update the star spheres
        if (TouchingEarth)
        {
            Pin.position = TouchLocation;
            Quaternion RotationChange = Quaternion.FromToRotation(Vector3.up, Pin.position - Earth.position);
            Quaternion Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up) * RotationChange;
            Pin.rotation = Rotation;
            OuterSphere.rotation = Rotation;
            InnerSphere.rotation = Rotation;
        }
    }
}
