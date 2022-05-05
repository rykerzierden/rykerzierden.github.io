using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingPoint : MonoBehaviour
{

    public MotionModule TrackingModule;
    public float TimeBetweenUpdates = 0.1f;
    private float TimeTracker = 0f;
    public int TrackingModuleID = 68;
    public HapticEngine ActiveHapticEngine;
    public Vector3 LastAcceleration;
    public Quaternion LastRotation;
    public bool firstPass = true;
    public bool foundModule = false;
    public string ModuleString;
    public int PassesPerUpdate = 1;
    public int count;
    public float dt;
    public InputActionProperty Calibrate;
    // Start is called before the first frame update
    void Start()
    {

        Calibrate.action.performed += CalibratePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!foundModule && ActiveHapticEngine.serialReady)
        {
            if (ActiveHapticEngine.SerialActiveInputModules.ContainsKey(TrackingModuleID))
            {
                TrackingModule = ActiveHapticEngine.SerialActiveInputModules[TrackingModuleID];
                ModuleString = TrackingModule.ToString();
            }
            else
            {

                Debug.LogWarning("Couldn't find tracking module with ID " + TrackingModuleID);
                foundModule = true;
            }
        }
        // Update the module information
        if (ActiveHapticEngine.serialReady && TimeTracker <= 0f)
        {
            ActiveHapticEngine.UpdateSerialRead();
            TimeTracker = TimeBetweenUpdates;

        }
        else if (!ActiveHapticEngine.serialReady)
        {

            Debug.LogError("Can't update tracking module as serial is not initialized!");
            return;
        }
        else
        {
            TimeTracker -= Time.deltaTime;
        }

        if (!firstPass)
        {
            //TrackingModule.Rotation = Quaternion.Euler(new Vector3(TrackingModule.Rotation.eulerAngles.x, TrackingModule.Rotation.eulerAngles.z, TrackingModule.Rotation.eulerAngles.y));
            TrackingModule.Acceleration = new Vector3(TrackingModule.Acceleration.x, TrackingModule.Acceleration.z, TrackingModule.Acceleration.y);
            this.transform.rotation = TrackingModule.Rotation;
            LastRotation = TrackingModule.Rotation;
            LastAcceleration = TrackingModule.Acceleration;
        }
        else
        {
            //count++;
            //AccelerationSum += TrackingModule.Acceleration;
            //if (count >= PassesPerUpdate)
            //{
            //    // Set initial acceleration as this is the first frame calculation
            //    LastAcceleration = TrackingModule.Acceleration;
            firstPass = false;
            this.transform.localRotation = (TrackingModule.Rotation);

            //    AccelerationSum = Vector3.zero;
            //    count = 0;
            //    dt = 0;
            //}
        }


    }
    void CalibratePosition(InputAction.CallbackContext context)
    {
        //ActiveHapticEngine.CalibrateIMUs();
        //this.transform.localEulerAngles = Vector3.zero;
        //LastRotation = this.transform.localRotation;
    }
}
