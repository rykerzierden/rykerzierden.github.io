using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MagicPanel : MonoBehaviour
{
    public InputActionProperty ActivatePanelLeft;
    public InputActionProperty ActivatePanelRight;
    public InputActionProperty ZoomRight;
    public InputActionProperty ZoomLeft;
    public InputActionProperty SnapshotRight;
    public InputActionProperty SnapshotLeft;
    public InputActionProperty ResetZoomLeft;
    public InputActionProperty ResetZoomRight;
    public InputActionProperty ToggleStarDepthR;
    public InputActionProperty ToggleLaserPointerR;
    public InputActionProperty ToggleStarDepthL;
    public InputActionProperty ToggleLaserPointerL;

    public Transform RightController;
    public Transform LeftController;
    public Transform MagicCamera;
    public Transform PanelDisplayPlane;

    public Shader DefaultShader;
    public RenderTexture MagicCameraOutput;
    public Material OuterSphereMaterial;
    public Material InnerSphereMaterial;
    public Texture LessStarsTextureOuter;
    public Texture MoreStarsTextureOuter;
    public Texture LessStarsTextureInner;
    public Texture MoreStarsTextureInner;
    public float BezelSize = 0f;
    public float ZoomSpeed = 200f;
    public EarthLocation EarthScript;
    public MeshRenderer LaserIndicator;

    public bool LeftToggled = false;
    public bool RightToggled = false;
    public bool GrabbedLeft = false;
    public bool GrabbedRight = false;
    public bool CanGrab = false;
    public bool ZoomingLeft = false;
    public bool ZoomingRight = false;
    public bool CameraSnapped = false;
    public bool MoreStars = false;
    public Transform PositionOnGrab;
    public float ZoomOnSnap = 0f;
    public Vector3 ControllerDiffOnGrab;
    public Vector3 ScaleOnGrab;
    bool LaserOn = false;
    Vector3 ControllerDiff;
    private float ScaledZoomSpeed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize panel as being 'off'
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;

        // Initialize camera position
        MagicCamera.transform.position = this.transform.position;
        // Set up action properties
        ActivatePanelLeft.action.performed += ToggleLeft;
        ActivatePanelRight.action.performed += ToggleRight;
        ActivatePanelLeft.action.canceled += Release;
        ActivatePanelRight.action.canceled += Release;
        ZoomRight.action.performed += ZoomR;
        ZoomLeft.action.performed += ZoomL;
        ZoomLeft.action.canceled += StopZoom;
        ZoomRight.action.canceled += StopZoom;
        SnapshotRight.action.performed += Snap;
        SnapshotLeft.action.performed += Snap;
        ResetZoomRight.action.performed += ResetZoom;
        ResetZoomLeft.action.performed += ResetZoom;
        ToggleStarDepthR.action.performed += StarTextureSwap;
        ToggleStarDepthL.action.performed += StarTextureSwap;
        ToggleLaserPointerL.action.performed += LaserToggle;
        ToggleLaserPointerR.action.performed += LaserToggle;
        // Set up render texture
        MagicCameraOutput.isPowerOfTwo = true;
        // Fix camera zoom speed
        ScaledZoomSpeed = ZoomSpeed * 200;
        // Start with laser off
        this.GetComponent<LineRenderer>().enabled = false;
        LaserIndicator.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update size and position on grab (changes for left and right hand use of the panel)
        if (GrabbedLeft)
        {
            ControllerDiff = RightController.position - LeftController.position;
            this.transform.localScale = new Vector3((Vector3.Magnitude(ControllerDiff) / Vector3.Magnitude(ControllerDiffOnGrab)) * ScaleOnGrab.x, this.transform.localScale.y, (Vector3.Magnitude(ControllerDiff) / Vector3.Magnitude(ControllerDiffOnGrab)) * ScaleOnGrab.x / (3f / 2f));
            this.transform.localPosition = new Vector3(this.transform.localScale.x / 2 + 0.02f, 0, 0);
        }
        else if (GrabbedRight)
        {
            ControllerDiff = RightController.position - LeftController.position;
            this.transform.localScale = new Vector3((Vector3.Magnitude(ControllerDiff) / Vector3.Magnitude(ControllerDiffOnGrab)) * ScaleOnGrab.x, this.transform.localScale.y, (Vector3.Magnitude(ControllerDiff) / Vector3.Magnitude(ControllerDiffOnGrab)) * ScaleOnGrab.x / (3f / 2f));
            this.transform.localPosition = new Vector3(-this.transform.localScale.x / 2 - 0.02f, 0, 0);
        }

        // Update size and position of the output plane relative to the magic panel
        PanelDisplayPlane.localScale = new Vector3(0.1f - BezelSize * 2, 0, 0.1f - BezelSize * 2);
        PanelDisplayPlane.localPosition = new Vector3(0, +0.51f, 0);



    }
    private void OnDestroy()
    {
        ActivatePanelLeft.action.performed -= ToggleLeft;
        ActivatePanelRight.action.performed -= ToggleRight;
        ActivatePanelLeft.action.canceled -= Release;
        ActivatePanelRight.action.canceled -= Release;
        ZoomRight.action.performed += ZoomR;
        ZoomLeft.action.performed += ZoomL;
        ZoomLeft.action.canceled -= StopZoom;
        ZoomRight.action.canceled -= StopZoom;
        SnapshotRight.action.performed -= Snap;
        SnapshotLeft.action.performed -= Snap;
        ResetZoomRight.action.performed -= ResetZoom;
        ResetZoomLeft.action.performed -= ResetZoom;
        ToggleStarDepthR.action.performed -= StarTextureSwap;
        ToggleStarDepthL.action.performed -= StarTextureSwap;
        ToggleLaserPointerL.action.performed -= LaserToggle;
        ToggleLaserPointerR.action.performed -= LaserToggle;

    }

    void ToggleRight(InputAction.CallbackContext context)
    {
        // Disable case
        if (RightToggled)
        {
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
            if (LaserOn)
            {
                this.gameObject.GetComponent<LineRenderer>().enabled = false;
                LaserIndicator.enabled = false;
            }
            RightToggled = false;
            PanelDisplayPlane.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        // Resize case
        else if (LeftToggled && CanGrab)
        {
            GrabbedLeft = true;
            PositionOnGrab = this.transform;
            ControllerDiffOnGrab = RightController.position - LeftController.position;
            ScaleOnGrab = this.transform.localScale;
        }
        // Enable Case
        else if (!LeftToggled)
        {
            this.gameObject.GetComponent<MeshRenderer>().enabled = true;
            if (LaserOn)
            { 
                this.gameObject.GetComponent<LineRenderer>().enabled = true;
                LaserIndicator.enabled = true;
            }
        this.transform.position = RightController.position;
            this.transform.rotation = RightController.rotation;
            this.transform.parent = RightController;
            this.transform.localEulerAngles = new Vector3(-30, 0, 0);
            this.transform.localPosition = new Vector3(-this.transform.localScale.x / 2 - 0.02f, 0, 0);
            PanelDisplayPlane.gameObject.GetComponent<MeshRenderer>().enabled = true;
            RightToggled = true;
        }
    }
    void ToggleLeft(InputAction.CallbackContext context)
    {
        // Disable case
        if (LeftToggled)
        {
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
            LeftToggled = false;
            if (LaserOn)
            { 
                this.gameObject.GetComponent<LineRenderer>().enabled = false;
                LaserIndicator.enabled = false;
            }
        PanelDisplayPlane.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        // Resize case
        else if (RightToggled && CanGrab)
        {
            GrabbedRight = true;
            PositionOnGrab = this.transform;
            ControllerDiffOnGrab = RightController.position - LeftController.position;
            ScaleOnGrab = this.transform.localScale;
        }
        // Enable Case
        else if (!RightToggled)
        {
            this.gameObject.GetComponent<MeshRenderer>().enabled = true;
            if (LaserOn)
            { 
                this.gameObject.GetComponent<LineRenderer>().enabled = true;
                LaserIndicator.enabled = true;
            }
        this.transform.position = LeftController.position;
            this.transform.rotation = LeftController.rotation;
            this.transform.parent = LeftController;
            this.transform.localEulerAngles = new Vector3(-30, 0, 0);
            this.transform.localPosition = new Vector3(this.transform.localScale.x / 2 + 0.02f, 0, 0);
            PanelDisplayPlane.gameObject.GetComponent<MeshRenderer>().enabled = true;
            LeftToggled = true;
        }
    }
    void ZoomR(InputAction.CallbackContext context)
    {
        if (RightToggled && !CameraSnapped)
        {
            ZoomingRight = true;
            if ((MagicCamera.localPosition.y <= -200 && context.ReadValue<Vector2>().y <= 0) || (MagicCamera.localPosition.y >= -80000 && context.ReadValue<Vector2>().y >= 0))
            {
                float deltaY = ScaledZoomSpeed * Time.deltaTime * context.ReadValue<Vector2>().y;
                MagicCamera.localPosition = new Vector3(MagicCamera.localPosition.x, MagicCamera.localPosition.y - deltaY, MagicCamera.localPosition.z);
            }

        }


    }
    void ZoomL(InputAction.CallbackContext context)
    {
        if (LeftToggled && !CameraSnapped)
        {
            ZoomingLeft = true;
            if ((MagicCamera.localPosition.y <= -200 && context.ReadValue<Vector2>().y <= 0) || (MagicCamera.localPosition.y >= -80000 && context.ReadValue<Vector2>().y >= 0))
            {
                float deltaY = ScaledZoomSpeed * Time.deltaTime * context.ReadValue<Vector2>().y;
                MagicCamera.localPosition = new Vector3(MagicCamera.localPosition.x, MagicCamera.localPosition.y - deltaY, MagicCamera.localPosition.z);
            }

        }


    }

    void StopZoom(InputAction.CallbackContext context)
    {
        ZoomingLeft = false;
        ZoomingRight = false;
    }

    void ResetZoom(InputAction.CallbackContext context)
    {
        MagicCamera.localPosition = Vector3.zero;
    }
    void Snap(InputAction.CallbackContext context)
    {
        if ((LeftToggled || RightToggled) && !EarthScript.TouchingEarth)
        {
            CameraSnapped = !CameraSnapped;
            if (CameraSnapped)
            {
                ZoomOnSnap = MagicCamera.localPosition.y;
                MagicCamera.transform.parent = null;
            }
            else
            {
                MagicCamera.transform.parent = this.transform;
                MagicCamera.position = this.transform.position;
                MagicCamera.localPosition = new Vector3(0, ZoomOnSnap, 0);
                MagicCamera.localEulerAngles = new Vector3(90, 0, 0);
            }
        }
    }

    void StarTextureSwap(InputAction.CallbackContext context)
    {
        MoreStars = !MoreStars;
        if (MoreStars)
        {
            InnerSphereMaterial.mainTexture = MoreStarsTextureInner;
            OuterSphereMaterial.mainTexture = MoreStarsTextureOuter;
        }
        else
        {
            InnerSphereMaterial.mainTexture = LessStarsTextureInner;
            OuterSphereMaterial.mainTexture = LessStarsTextureOuter;
        }
    }

    void LaserToggle(InputAction.CallbackContext context)
    {
        LaserOn = !LaserOn;
        if ((RightToggled || LeftToggled))
        {
            this.GetComponent<LineRenderer>().enabled = LaserOn;
            LaserIndicator.enabled = LaserOn;
        }
    }
    
    void UploadToScreen(Collider screen)
    {
        RenderTexture SnapShot = new RenderTexture(MagicCameraOutput);
        Camera SnapCamera = Camera.Instantiate(MagicCamera.GetComponent<Camera>());
        SnapCamera.transform.position = MagicCamera.position;
        SnapCamera.transform.rotation = MagicCamera.rotation;
        SnapCamera.targetTexture = SnapShot;
        Material SnapMaterial = new Material(DefaultShader);
        SnapMaterial.mainTexture = SnapShot;
        
        screen.GetComponent<Renderer>().material = SnapMaterial;
    }
    void Release(InputAction.CallbackContext context)
    {
        GrabbedLeft = false;
        GrabbedRight = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TeleportLineToggle>())
        {
            CanGrab = true;
        }
        else if (other.GetComponent<DisplayTarget>() && CameraSnapped && (LeftToggled || RightToggled))
        {
            UploadToScreen(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CanGrab = false;
        GrabbedLeft = false;
        GrabbedRight = false;
    }
}
