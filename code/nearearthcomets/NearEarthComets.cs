using IVLab.ABREngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class NearEarthComets : MonoBehaviour
{
    public System.Data.DataTable CometData;
    public string CometDataFileName = "Near-Earth_Comets_-_Orbital_Elements.csv";
    public List<Comet> Comets = new List<Comet>();
    public List<Bounds> OrbitBounds = new List<Bounds>();
    public float MinCometLineWidth = 0.005f;
    public float MaxCometLineWidth = 0.2f;
    public float SmallestDisplayedCometDistance = -1f;
    public float GreatestDisplayedCometDistance = -1f;
    public int CometPrecision = 50;
    public float MaxLineValue = 1.0f;
    public float MinLineValue = 0.3f;
    public float MaxHaloValue = 0.5f;
    public float MinHaloValue = 0.2f;

    public GameObject LeftController;
    public GameObject SampleCube;
    public GameObject SampleCyl;
    public GameObject EarthModel;
    public GameObject SampleLine;
    public GameObject[] SampleAsteroids;
    public List<GameObject> Legend = new List<GameObject>();
    public float scale = 0.4f;
    public Text Selection;
    public GameObject SampleTextbox;
    public Comet Earth = new Comet();
    public List<float> CometHues = new List<float>();
    public List<Comet> DisplayedComets = new List<Comet>();
    public List<Comet> CometsToDelete = new List<Comet>();
    public List<GameObject> LineInstances = new List<GameObject>();
    private List<GameObject> CometInstances = new List<GameObject>();
    private int CurrentCometIndex = 0;
    public bool CometsNeedUpdate = true;
    public int HoldLimit = 2;
    public int HeldComets = 0;
    private bool SwitchCompleted = false;
    private bool ModeCompleted = false;
    public float OrbitScaleFactor = 1.0f;
    private float LastOrbitScaleFactor = 1.0f;
    public Transform OrbitSetup;
    public DisplayMode CurrentMode = DisplayMode.COMETSANDLINES;

    public InputActionProperty SwitchCometR;
    public InputActionProperty SwitchCometL;
    public InputActionProperty SelectR;
    public InputActionProperty SelectL;
    public InputActionProperty ClearR;
    public InputActionProperty ClearL;
    public InputActionProperty ChangeModeR;
    public InputActionProperty ChangeModeL;

    public enum DisplayMode
    {
        COMETSANDLINES = 0,
        COMETSONLY = 1,
        LINESONLY = 2
    }



    // Start is called before the first frame update
    void Start()
    {
        //SwitchCometR.action.performed += NextComet;
        CurrentCometIndex = 0;
        Earth = CreateEarth();
        LoadData();
        DisplayedComets.Add(Comets[CurrentCometIndex]);
        ShowComet(Earth, 30, EarthModel, 0.6f);

        SwitchCometR.action.performed += SwitchComet;
        SwitchCometL.action.performed += SwitchComet;
        SwitchCometR.action.canceled += JoystickRelease;
        SwitchCometL.action.canceled += JoystickRelease;
        SelectR.action.performed += ToggleHold;
        SelectL.action.performed += ToggleHold;
        ClearR.action.performed += Clear;
        ClearL.action.performed += Clear;
        ChangeModeR.action.performed += ChangeMode;
        ChangeModeL.action.performed += ChangeMode;
        //ChangeModeR.action.canceled += JoystickRelease;
        //ChangeModeL.action.canceled += JoystickRelease;

        for (int i = 0; i < HoldLimit; i++)
        {
            float CurrentCometHue = i * 1.0f / HoldLimit;
            CometHues.Add((CurrentCometHue));
        }
    //ShowComet(Comets[0],50,SampleAsteroids[0]);
    //ShowComet(Comets[1], 50, SampleAsteroids[1]);
    //ShowComet(Comets[2], 50, SampleAsteroids[2]);
    //ShowCometLine(Comets[0],100,new Color(1.0f,1.0f,1.0f,1.0f));

    //EARTH SETUP



    //this.LookupIndex = LookupIndex;


    //Comets[1].CalculateOrbit(20);
    //Comets[1].CalculateSunDistances();
    //for (int o = 0; o < Comets[1].Orbit.Count; o++)
    //{
    //    GameObject ThisCyl = GameObject.Instantiate(SampleCyl);
    //    Debug.Log(("Hi there"));
    //    float ratio = Comets[1].SunDistances[o] / Comets[1].SunDistances.Max<float>();

    //    ThisCyl.transform.localScale = new Vector3(scale * (1f / ratio) * ThisCyl.transform.localScale.x, scale * (1f / ratio) * ThisCyl.transform.localScale.y, scale * (1f / ratio) * ThisCyl.transform.localScale.z);
    //    ThisCyl.transform.position = Comets[1].Orbit[o];

    //}

}

    // Update is called once per frame
    void Update()
    {

        if (LastOrbitScaleFactor != OrbitScaleFactor)
        {
            Refresh();
            LastOrbitScaleFactor = OrbitScaleFactor;
            OrbitSetup.localScale = OrbitScaleFactor * Vector3.one;


        }
        
        //ABREngine.Instance.Render();

        //if (Input.GetKeyDown("space"))
        //{
        //    NextComet();
        //}
        //else if (Input.GetKeyDown("b"))
        //{
        //    Hold(Comets[CurrentCometIndex]);
        //}
        //else if (Input.GetKeyDown("v"))
        //{
        //    Clear();
        //}

        if (CometsNeedUpdate)
        {
            foreach(GameObject LegendObject in Legend.ToArray())
            {
                Destroy(LegendObject);
                Legend.Remove(LegendObject);
            }
            foreach (Comet CurrentComet in CometsToDelete.ToList())
            {
                HideComet(CurrentComet);
                HideCometLine(CurrentComet);
                CometsToDelete.Remove(CurrentComet);
            }
            foreach (Comet CurrentComet in DisplayedComets.ToList())
            {
                HideComet(CurrentComet);
                HideCometLine(CurrentComet);
                CometsToDelete.Remove(CurrentComet);
            }
            foreach (Comet CurrentComet in DisplayedComets)
            {
                if (CurrentComet.Hold)
                {
                    switch (CurrentMode)
                    {
                        case DisplayMode.COMETSANDLINES:
                            ShowComet(CurrentComet, CometPrecision, SampleAsteroids[DisplayedComets.IndexOf(CurrentComet) % SampleAsteroids.Length], CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            ShowCometLine(CurrentComet, CometPrecision, CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            break;
                        case DisplayMode.COMETSONLY:
                            ShowComet(CurrentComet, CometPrecision, SampleAsteroids[DisplayedComets.IndexOf(CurrentComet) % SampleAsteroids.Length], CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            break;
                        case DisplayMode.LINESONLY:
                            ShowCometLine(CurrentComet, CometPrecision, CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            break;
                    }
                }
                else if (Comets.IndexOf(CurrentComet) == CurrentCometIndex)
                {
                    switch (CurrentMode)
                    {
                        case DisplayMode.COMETSANDLINES:
                            ShowComet(CurrentComet, CometPrecision, SampleAsteroids[DisplayedComets.IndexOf(CurrentComet) % SampleAsteroids.Length], CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            ShowCometLine(CurrentComet, CometPrecision, CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            break;
                        case DisplayMode.COMETSONLY:
                            ShowComet(CurrentComet, CometPrecision, SampleAsteroids[DisplayedComets.IndexOf(CurrentComet) % SampleAsteroids.Length], CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            break;
                        case DisplayMode.LINESONLY:
                            ShowCometLine(CurrentComet, CometPrecision, CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                            break;
                    }
                }
                else
                {
                    HideComet(CurrentComet);
                    HideCometLine(CurrentComet);
                }

            }
            Selection.text = "";
            Selection.text += "Current Display Mode: " + CurrentMode + "\n";
            Selection.text += "Selection: " + Comets[CurrentCometIndex].Name + "\n";
            Selection.text += "<        " + (CurrentCometIndex+1) + " / " + Comets.Count + "       >";
            Selection.color = Color.HSVToRGB(CometHues[DisplayedComets.IndexOf(Comets[CurrentCometIndex])], 1.0f, 1.0f);

            for(int c = 0; c < DisplayedComets.Count; c++)
            {
                GameObject ThisText = Instantiate(SampleTextbox,LeftController.transform);
                ThisText.transform.localPosition += new Vector3(0f, Mathf.Sin(Mathf.Deg2Rad * 60)  * 0.062f, Mathf.Cos(Mathf.Deg2Rad * 60)  * 0.062f);
                ThisText.transform.localPosition += new Vector3(0.152f*(c/3),-Mathf.Sin(Mathf.Deg2Rad * 60) *(c%3) * 0.062f, -Mathf.Cos(Mathf.Deg2Rad * 60) * (c % 3) * 0.062f);
                ThisText.SetActive(true);
                Text ThisTextBox = ThisText.GetComponentInChildren<Text>();
                ThisTextBox.text = "";
                ThisTextBox.text += "Comet: " + DisplayedComets[c].Name + "\n";
                ThisTextBox.text += "MOID: " + DisplayedComets[c].MOID + " AU" + "\n";
                ThisTextBox.text += "         " + (Comets.IndexOf(DisplayedComets[c]) + 1) + " / " + Comets.Count + "        ";
                ThisTextBox.color = Color.HSVToRGB(CometHues[c], 1.0f, 1.0f);
                Legend.Add(ThisText);
            }
            CometsNeedUpdate = false;
        }

    }
    private void OnDestroy()
    {
        SwitchCometR.action.performed -= SwitchComet;
        SwitchCometL.action.performed -= SwitchComet;
        SwitchCometR.action.canceled -= JoystickRelease;
        SwitchCometL.action.canceled -= JoystickRelease;
        SelectR.action.performed -= ToggleHold;
        SelectL.action.performed -= ToggleHold;
        ClearR.action.performed -= Clear;
        ClearL.action.performed -= Clear;
        //ChangeModeR.action.canceled -= JoystickRelease;
        //ChangeModeL.action.canceled -= JoystickRelease;
    }
    void LoadData()
    {
        CometData = ReadCSVAsDataTable(Path.Combine(Application.streamingAssetsPath, CometDataFileName), true);
        int LookupIndex = 0;

        foreach (System.Data.DataRow CometRow in CometData.Rows)
        {
            Comets.Add(new Comet(CometRow, LookupIndex));
            LookupIndex++;
        }
    }



    void CreateDataImpression()
    {
        RawDataset OrbitRaw = RawDatasetAdapter.PointsToPoints(Comets[0].Orbit, Comets[0].OrbitBounds, null, null);
        Debug.Log("BOUNDS:" + Comets[0].OrbitBounds);

        // Import the point data into ABR
        KeyData OrbitInfo = ABREngine.Instance.Data.ImportRawDataset(OrbitRaw);

        // Create a layer for selected "before" points (purple)
        SimpleGlyphDataImpression OrbitGlyph = new SimpleGlyphDataImpression();
        OrbitGlyph.useRandomOrientation = false;
        OrbitGlyph.keyData = OrbitInfo;
        OrbitGlyph.glyph = ABREngine.Instance.VisAssets.GetDefault<GlyphVisAsset>() as GlyphVisAsset;
        OrbitGlyph.glyphSize = 0.05f;
        OrbitGlyph.colormap = ColormapVisAsset.SolidColor(new Color(155f / 255f, 55f / 255f, 238f / 255f, 1.0f));
        ABREngine.Instance.RegisterDataImpression(OrbitGlyph);
    }


    System.Data.DataTable ReadCSVAsDataTable(string FilePath, bool HasHeaders)
    {
        System.Data.DataTable RetTable = new System.Data.DataTable();
        try
        {
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(FilePath))
            {
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    if (HasHeaders)
                    {
                        string[] Headers = line.Split(',');
                        foreach (string Header in Headers)
                        {
                            RetTable.Columns.Add(Header);
                        }
                        HasHeaders = false;
                    }
                    else
                    {
                        RetTable.LoadDataRow(line.Split(','), true);
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
            return null;
        }
        return RetTable;
    }
    void ShowComet(Comet Comet, int NumPoints,GameObject DefaultObject,float HaloHue)
    {
        Comet.CalculateOrbit(NumPoints);
        Comet.CalculateSunDistances();

        List<float> OrbitDistances = new List<float>();
        Comet EarthCopy = CreateEarth();
        EarthCopy.CalculateOrbit(100);
        foreach (Vector3 CometPoint in Comet.Orbit)
        {
            float MinDist = -1f;
            foreach (Vector3 EarthPoint in EarthCopy.Orbit)
            {
                float ThisDist = Mathf.Abs(Vector3.Distance(EarthPoint, CometPoint));
                if (MinDist < 0)
                {
                    MinDist = ThisDist;
                }
                else if (ThisDist < MinDist)
                {
                    MinDist = ThisDist;
                }
                if (SmallestDisplayedCometDistance < 0 || GreatestDisplayedCometDistance < 0)
                {
                    SmallestDisplayedCometDistance = MinDist;
                    GreatestDisplayedCometDistance = MinDist;
                }
                else if (MinDist < SmallestDisplayedCometDistance)
                    SmallestDisplayedCometDistance = MinDist;
                else if (MinDist > GreatestDisplayedCometDistance)
                    GreatestDisplayedCometDistance = MinDist;
            }
            OrbitDistances.Add(MinDist);
        }
        float Max = OrbitDistances.Max();
        float Min = OrbitDistances.Min();



        //textbox.text = "Name: " + Comets[0].Name + "\nPeriod: " + Comets[0].Period;
        GameObject[] TheseCubes = new GameObject[NumPoints];
        for (int o = 0; o < NumPoints; o++)
        {



            GameObject ThisObject = Instantiate(DefaultObject);
            ThisObject.SetActive(true);
            TheseCubes[o] = ThisObject;
            //Debug.Log(("Hi there"));
            //float ratio = Comets[0].SunDistances[o] / Comets[0].SunDistances.Max<float>();

            //ThisCube.transform.localScale = new Vector3(scale * (1f / ratio) * ThisCube.transform.localScale.x, scale * (1f / ratio) * ThisCube.transform.localScale.y, scale * (1f / ratio) * ThisCube.transform.localScale.z);
            ThisObject.transform.position = Comet.Orbit[o];
            SerializedObject halo = new SerializedObject(ThisObject.GetComponent("Halo"));
            halo.FindProperty("m_Size").floatValue = 0.12f;
            halo.FindProperty("m_Enabled").boolValue = true;
            halo.FindProperty("m_Color").colorValue = Color.HSVToRGB(HaloHue,1.0f, Mathf.Max(new float[] { (1f - ((OrbitDistances[o] - Min) / (MaxHaloValue - MinHaloValue))) * MaxHaloValue, MinHaloValue }));
            halo.ApplyModifiedProperties();
            ThisObject.AddComponent<Rigidbody>();
            ThisObject.AddComponent<BoxCollider>();
            ThisObject.GetComponent<Rigidbody>().isKinematic = true;
            ThisObject.AddComponent<CometReference>();
            ThisObject.GetComponent<CometReference>().Comet = Comet;
            ThisObject.transform.parent = OrbitSetup;
            //ThisObject.transform.localScale = OrbitScaleFactor * DefaultObject.transform.localScale;
        }
        CometInstances.AddRange(TheseCubes);
        Comet.CometInstances.AddRange(TheseCubes);
    }

    Comet CreateEarth()
    {
        Comet NewEarth = new Comet();
        NewEarth.Name = "Earth";
        NewEarth.Epoch = 0;
        NewEarth.TP = 0;
        NewEarth.Eccentricity = 0.0167f;
        NewEarth.InclinationAngle = 0;
        NewEarth.PeriapsisArgument = 102.95f;
        NewEarth.RAAN = -11.26f;
        NewEarth.PerigeeDistance = 0.98f;
        NewEarth.ApogeeDistance = 1.016f;
        NewEarth.Period = 1;
        NewEarth.MOID = 0;
        NewEarth.a = (NewEarth.ApogeeDistance + NewEarth.PerigeeDistance) / 2.0f;
        NewEarth.b = NewEarth.a * Mathf.Sqrt(1 - Mathf.Pow(NewEarth.Eccentricity, 2f));
        NewEarth.Center = new Vector2(-NewEarth.a * NewEarth.Eccentricity, 0f);
        return NewEarth;
    }

    void ShowCometLine(Comet Comet, int NumPoints, float CometHue)
    {


        Comet.CalculateOrbit(NumPoints);
        //CometLine.positionCount = NumPoints;
        //CometLine.SetPositions(Comet.Orbit.ToArray());
        List<float> OrbitDistances = new List<float>();
        Comet EarthCopy = CreateEarth();
        EarthCopy.CalculateOrbit(100);
        foreach(Vector3 CometPoint in Comet.Orbit)
        {
            float MinDist = -1f;
            foreach (Vector3 EarthPoint in EarthCopy.Orbit)
            {
                float ThisDist = Mathf.Abs(Vector3.Distance(EarthPoint, CometPoint));
                if (MinDist < 0)
                {
                    MinDist = ThisDist;
                }
                else if (ThisDist < MinDist)
                {
                    MinDist = ThisDist;
                }
                if (SmallestDisplayedCometDistance < 0 || GreatestDisplayedCometDistance < 0)
                {
                    SmallestDisplayedCometDistance = MinDist;
                    GreatestDisplayedCometDistance = MinDist;
                }
                else if (MinDist < SmallestDisplayedCometDistance)
                    SmallestDisplayedCometDistance = MinDist;
                else if (MinDist > GreatestDisplayedCometDistance)
                    GreatestDisplayedCometDistance = MinDist;
            }
            OrbitDistances.Add(MinDist);
        }
        float Max = OrbitDistances.Max();
        float Min = OrbitDistances.Min();


        List<GameObject> CometLines = new List<GameObject>();
        for (int i = 0; i < NumPoints - 1; i++)
        {
            GameObject CometLine = Instantiate(SampleLine);
            CometLine.GetComponent<LineRenderer>().enabled = true;
            CometLine.GetComponent<LineRenderer>().positionCount = 2;
            CometLine.GetComponent<LineRenderer>().SetPositions(new Vector3[] { OrbitScaleFactor * Comet.Orbit[i], OrbitScaleFactor * Comet.Orbit[i + 1] });
            CometLine.GetComponent<LineRenderer>().useWorldSpace = true;
            CometLine.GetComponent<LineRenderer>().startWidth = OrbitScaleFactor * Mathf.Max(new float[] { (1f-((OrbitDistances[i] - Min) / (GreatestDisplayedCometDistance - SmallestDisplayedCometDistance))) * MaxCometLineWidth, MinCometLineWidth });
            CometLine.GetComponent<LineRenderer>().endWidth = OrbitScaleFactor * Mathf.Max(new float[] {(1f-(OrbitDistances[i+1] - Min) / (GreatestDisplayedCometDistance - SmallestDisplayedCometDistance)) * MaxCometLineWidth, MinCometLineWidth });
            CometLine.GetComponent<LineRenderer>().startColor = Color.HSVToRGB(CometHue,1.0f,Mathf.Max(new float[] { (1f - ((OrbitDistances[i] - Min) / (MaxLineValue - MinLineValue))) * MaxLineValue, MinLineValue }));
            CometLine.GetComponent<LineRenderer>().endColor = Color.HSVToRGB(CometHue, 1.0f, Mathf.Max(new float[] { (1f - ((OrbitDistances[i+1] - Min) / (MaxLineValue - MinLineValue))) * MaxLineValue, MinLineValue }));
            CometLine.transform.parent = OrbitSetup;
            //CometLine.transform.localScale = OrbitScaleFactor * SampleLine.transform.localScale;
            CometLines.Add(CometLine);
        }

        //Comets[Comet.LookUpIndex].CometLine = CometLines;
        Comet.CometLine.AddRange(CometLines);
        LineInstances.AddRange(CometLines);
    }

    void HideComet(Comet Comet)
    {
        //Debug.Log("HIDING COMET AT INDEX " + Comet.LookUpIndex);
        //foreach (GameObject[] TheseCubes in CometInstances.ToList())
        //{
        //    if (TheseCubes[0].GetComponent<CometReference>().Comet == Comet)
        //    {
        //        foreach (GameObject Cube in TheseCubes)
        //        {
        //            Destroy(Cube);
        //        }
        //        CometInstances.Remove(TheseCubes);
        //    }
        //}
        foreach (GameObject CometInstance in Comet.CometInstances.ToList())
        {
            Comet.CometInstances.Remove(CometInstance);
            CometInstances.Remove(CometInstance);
            Destroy(CometInstance);

        }
    }
    void HideCometLine(Comet Comet)
    {
        //GameObject Line in Comets[Comet.LookUpIndex].CometLine
        foreach (GameObject Line in Comet.CometLine.ToList())
        {
            Comet.CometLine.Remove(Line);
            LineInstances.Remove(Line);
            Destroy(Line.GetComponent<LineRenderer>());
            Destroy(Line);
        }
    }

    void SwitchComet(InputAction.CallbackContext context)
    {
        if (!SwitchCompleted)
        {
            float XValue = context.ReadValue<Vector2>().x;
            if (XValue > 0)
            {
                NextComet();
            }
            else
            {
                LastComet();
            }
            SwitchCompleted = true;
        }
    }
    void ChangeMode(InputAction.CallbackContext context)
    {
        if (!ModeCompleted)
        {
            float YValue = context.ReadValue<Vector2>().y;
            if (YValue > 0)
            {
                NextMode();
            }
            else
            {
                LastMode();
            }
            ModeCompleted = true;
        }
    }
    void NextMode()
    {
        if (CurrentMode < DisplayMode.LINESONLY)
        {
            CurrentMode++;
        }
        else
        {
            CurrentMode = DisplayMode.COMETSANDLINES;
        }
        Refresh();
    }
    void LastMode()
    {
        if (CurrentMode > DisplayMode.COMETSANDLINES)
        {
            CurrentMode--;
        }
        else
        {
            CurrentMode = DisplayMode.LINESONLY;
        }
        Refresh();
    }
    void ToggleHold(InputAction.CallbackContext context)
    {
        if (Comets[CurrentCometIndex].Hold)
        {
            UnHold(Comets[CurrentCometIndex]);
        }
        else
        {
            Hold(Comets[CurrentCometIndex]);
        }
    }
    void Clear(InputAction.CallbackContext context)
    {
        foreach (Comet Comet in Comets)
        {
            Comet.Hold = false;
            HideComet(Comet);
        }
        DisplayedComets.Clear();
        HeldComets = 0;
        //CurrentCometIndex = 0;
        CometsNeedUpdate = true;
        DisplayedComets.Add(Comets[CurrentCometIndex]);
    }
    void JoystickRelease(InputAction.CallbackContext context)
    {
        SwitchCompleted = false;
        ModeCompleted = false;
    }

    void NextComet()
    {
        Debug.Log("NEXT COMET");
        Debug.Log(CurrentCometIndex);
        // InputAction.CallbackContext 
        //Debug.Log("Switching Comets FROM " + Comets[CometIndex].Name);

        if (!Comets[CurrentCometIndex].Hold)
        {
            DisplayedComets.Remove(Comets[CurrentCometIndex]);
            CometsToDelete.Add(Comets[CurrentCometIndex]);
        }

        
        if(CurrentCometIndex >= Comets.Count - 1)
        {
            CurrentCometIndex = 0;
        }
        else
        {
            CurrentCometIndex = (CurrentCometIndex + 1);
        }
        //Debug.Log("Switching Comets TO " + Comets[CometIndex].Name);
        if (!Comets[CurrentCometIndex].Hold)
        {
            DisplayedComets.Add(Comets[CurrentCometIndex]);
        }
        CometsNeedUpdate = true;
    }

    void LastComet()
    {
        Debug.Log("LAST COMET");
        Debug.Log(CurrentCometIndex);
        Debug.Log("Switching Comets FROM " + Comets[CurrentCometIndex].Name);
        if (!Comets[CurrentCometIndex].Hold)
        {
            DisplayedComets.Remove(Comets[CurrentCometIndex]);
            CometsToDelete.Add(Comets[CurrentCometIndex]);
        }

        if (CurrentCometIndex -1 < 0)
        {
            CurrentCometIndex = Comets.Count -1;
        }
        else
        {
            CurrentCometIndex = ((CurrentCometIndex - 1));
        }
        Debug.Log("Switching Comets TO " + Comets[CurrentCometIndex].Name);
        if (!Comets[CurrentCometIndex].Hold)
        {
            DisplayedComets.Add(Comets[CurrentCometIndex]);
        }
        CometsNeedUpdate = true;
        Debug.Log(CurrentCometIndex);
    }

    void Hold(Comet Comet)
    {
        if((HoldLimit - HeldComets > 1) && !Comet.Hold)
        {
           // Debug.Log("HOLD " + Comets[CometIndex].Name);
           // Comets[Comet.LookUpIndex].Hold = true;
            Comet.Hold = true;
            HeldComets++;
            NextComet();
        }   
    }
    void UnHold(Comet Comet)
    {
        if ((HoldLimit - HeldComets > 0) && Comet.Hold)
        {
            // Debug.Log("HOLD " + Comets[CometIndex].Name);
            // Comets[Comet.LookUpIndex].Hold = true;
            Comet.Hold = false;
            HeldComets--;
            NextComet();
            Refresh();
        }
    }

    void Refresh()
    {
        foreach (GameObject LegendObject in Legend.ToArray())
        {
            Destroy(LegendObject);
            Legend.Remove(LegendObject);
        }
        foreach (Comet CurrentComet in DisplayedComets.ToList())
        {
            HideComet(CurrentComet);
            HideCometLine(CurrentComet);
        }
        foreach (Comet CurrentComet in DisplayedComets)
        {
            switch (CurrentMode)
            {
                case DisplayMode.COMETSANDLINES:
                    ShowComet(CurrentComet, CometPrecision, SampleAsteroids[DisplayedComets.IndexOf(CurrentComet) % SampleAsteroids.Length], CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                    ShowCometLine(CurrentComet, CometPrecision, CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                    break;
                case DisplayMode.COMETSONLY:
                    ShowComet(CurrentComet, CometPrecision, SampleAsteroids[DisplayedComets.IndexOf(CurrentComet) % SampleAsteroids.Length], CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                    break;
                case DisplayMode.LINESONLY:
                    ShowCometLine(CurrentComet, CometPrecision, CometHues[DisplayedComets.IndexOf(CurrentComet)]);
                    break;
            }
        }
        Selection.text = "";
        Selection.text += "Current Display Mode: " + CurrentMode + "\n";
        Selection.text += "Selection: " + Comets[CurrentCometIndex].Name + "\n";
        Selection.text += "<        " + (CurrentCometIndex + 1) + " / " + Comets.Count + "       >";
        Selection.color = Color.HSVToRGB(CometHues[DisplayedComets.IndexOf(Comets[CurrentCometIndex])], 1.0f, 1.0f);
        for (int c = 0; c < DisplayedComets.Count; c++)
        {
            GameObject ThisText = Instantiate(SampleTextbox, LeftController.transform);
            ThisText.transform.localPosition += new Vector3(0f, Mathf.Sin(Mathf.Deg2Rad * 60) * 0.062f, Mathf.Cos(Mathf.Deg2Rad * 60) * 0.062f);
            ThisText.transform.localPosition += new Vector3(0.152f * (c / 3), -Mathf.Sin(Mathf.Deg2Rad * 60) * (c % 3) * 0.062f, -Mathf.Cos(Mathf.Deg2Rad * 60) * (c % 3) * 0.062f);
            ThisText.SetActive(true);
            Text ThisTextBox = ThisText.GetComponentInChildren<Text>();
            ThisTextBox.text = "";
            ThisTextBox.text += "Comet: " + DisplayedComets[c].Name + "\n";
            ThisTextBox.text += "MOID: " + DisplayedComets[c].MOID + " AU" + "\n";
            ThisTextBox.text += "         " + (Comets.IndexOf(DisplayedComets[c]) + 1) + " / " + Comets.Count + "        ";
            ThisTextBox.color = Color.HSVToRGB(CometHues[c], 1.0f, 1.0f);
            Legend.Add(ThisText);
        }
    }



}
