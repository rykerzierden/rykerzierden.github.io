using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    public InputActionProperty Grab;
    public InputActionProperty Trigger;
    public ModuleGroup ForearmFront;
    public ModuleGroup ForearmBack;
    public ControllerPulser Controller;
    public GameObject Laser;
    private GameObject CurrentLaser;
    public Transform TouchedObject;
    public Transform GrabbedObject;
    public bool Shooting = false;
    public bool SpinningChain = false;
    public float ShootPulseTime = 0.3f;
    private float ShootPulseTimer = 0.3f;
    private int PulsingObject = 0;
    public bool CanGrab = false;
    public bool UsingHaptics = false;
    // Start is called before the first frame update
    void Start()
    {
        Grab.action.performed += GrabObject;
        Grab.action.canceled += ReleaseObject;
        Trigger.action.performed += Activate;
        Trigger.action.canceled += Deactivate;
        ShootPulseTimer = ShootPulseTime;

    }

    // Update is called once per frame
    void Update()
    {
        if (Shooting)
        {
            
            if(ShootPulseTimer > 0f)
            {
                switch (PulsingObject)
                {
                    case 0:
                        if(Controller != null)
                            Controller.ActivatePulse(ModuleGroup.PulseTypes.NONE);
                        ForearmFront.ActivatePulse(ModuleGroup.PulseTypes.NONE);
                        break;
                    case 1:
                        if (Controller != null)
                            Controller.DeactivatePulse();
                        if (ForearmFront != null)
                            ForearmFront.DeactivatePulse();
                        if (ForearmBack != null)
                            ForearmBack.ActivatePulse(ModuleGroup.PulseTypes.NONE);
                        break;
                    case 2:
                        if (ForearmBack != null)
                            ForearmBack.DeactivatePulse();
                        break;

                }
                ShootPulseTimer -= Time.deltaTime;
            }
            else
            {
                PulsingObject++;
                ShootPulseTimer = ShootPulseTime;
                if(PulsingObject > 2)
                {
                    UsingHaptics = false;
                    Shooting = false;
                    PulsingObject = 0;
                }
            }
            

        }
        if (SpinningChain)
        {
            if (Controller != null)
            {
                if (GrabbedObject.GetComponent<Chainsaw>().cutting)
                {
                    Controller.ActivatePulse(ModuleGroup.PulseTypes.NONE);
                }
                else
                {
                    Controller.ActivatePulse(ModuleGroup.PulseTypes.HALF);
                }
            }
            if (ForearmBack != null)
            {
                if (GrabbedObject.GetComponent<Chainsaw>().cutting)
                {
                    ForearmBack.ActivatePulse(ModuleGroup.PulseTypes.NONE);
                }
                else
                {
                    ForearmBack.DeactivatePulse();
                }
            }
            if (ForearmFront != null)
            {
                if (GrabbedObject.GetComponent<Chainsaw>().cutting)
                {
                    ForearmFront.ActivatePulse(ModuleGroup.PulseTypes.NONE);
                }
                else
                {
                    ForearmFront.DeactivatePulse();
                }
            }
        }
    }

    void GrabObject(InputAction.CallbackContext context)
    {
        if (CanGrab && GrabbedObject == null)
        {
            GrabbedObject = TouchedObject;
            GrabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            GrabbedObject.GetComponent<Rigidbody>().detectCollisions = true;
            GrabbedObject.parent = this.transform;
            if(GrabbedObject.GetComponent<Chainsaw>())
            {
                GrabbedObject.localEulerAngles = Vector3.zero;
                GrabbedObject.localPosition = new Vector3(0.05f,0f,0.15f);
            }
            else if (GrabbedObject.GetComponent<LaserGun>())
            {
                GrabbedObject.localEulerAngles = Vector3.zero;
                GrabbedObject.localPosition = new Vector3(0f, 0f, 0.1f);
            }

        }
    }
    void ReleaseObject(InputAction.CallbackContext context)
    {
        if (GrabbedObject != null)
        {
            GrabbedObject.GetComponent<Rigidbody>().isKinematic = false;
            GrabbedObject.GetComponent<Rigidbody>().detectCollisions = true;
            GrabbedObject.parent = null;
            GrabbedObject = null;
            StopSpin();
            Shooting = false;
            SpinningChain = false;
        }
    }
    void Activate(InputAction.CallbackContext context)
    {
        if (GrabbedObject != null)
        { 
            if (GrabbedObject.GetComponent<LaserGun>())
            {
                Shoot();
            }
            else if (GrabbedObject.GetComponent<Chainsaw>())
            {
                Spin();
            }
        }
    }


    void Deactivate(InputAction.CallbackContext context)
    {
        if (GrabbedObject != null && GrabbedObject.GetComponent<Chainsaw>())
        {
            StopSpin();
        }
    }
    void Shoot()
    {
        CurrentLaser = Instantiate(Laser);
        CurrentLaser.SetActive(true);
        ShootPulseTimer = ShootPulseTime;
        PulsingObject = 0;
        CurrentLaser.transform.position = this.transform.position + 0.07f*this.transform.TransformDirection(Vector3.up) + 0.3f*this.transform.TransformDirection(Vector3.forward);
        CurrentLaser.transform.up = this.transform.forward;
        UsingHaptics = true;
        Shooting = true;
    }
    void Spin()
    {
        SpinningChain = true;
        UsingHaptics = true;
        
        
    }
    
    void StopSpin()
    {
        SpinningChain = false;
        UsingHaptics = false;
        if (Controller != null)
        {
            Controller.DeactivatePulse();
        }
        if (ForearmBack != null)
        {
            ForearmBack.DeactivatePulse();
        }
        if (ForearmFront != null)
        {
            ForearmFront.DeactivatePulse();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Chainsaw>() || other.GetComponent<LaserGun>())
        {
            CanGrab = true;
            TouchedObject = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        CanGrab = false;
    }
}
