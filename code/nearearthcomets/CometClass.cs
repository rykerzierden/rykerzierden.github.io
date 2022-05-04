using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Numerics;
using static UnityEngine.Mathf;


public class Comet
{
    public string Name { get; set; }
    public float Epoch { get; set; }
    public float TP { get; set; }
    public float Eccentricity { get; set; }
    public float InclinationAngle { get; set; }
    public float PeriapsisArgument { get; set; }
    public float RAAN { get; set; }
    public float PerigeeDistance { get; set; }
    public float ApogeeDistance { get; set; }
    public float Period { get; set; }
    public float MOID { get; set; }

    public float a;

    public float b;

    private float [,] DCM;

    public UnityEngine.Vector2 Center = new UnityEngine.Vector2();

    public float[] Thetas;

    public List<UnityEngine.Vector3> Orbit = new List<UnityEngine.Vector3>();

    public float[] SunDistances;

    public Bounds OrbitBounds = new Bounds();

    public List<GameObject> CometLine = new List<GameObject>();

    public List<GameObject> CometInstances = new List<GameObject>();

    public bool Hold = false;

    public int LookUpIndex;
    public Comet(System.Data.DataRow CometRow, int index)
    {
        this.Name = (string) CometRow["Object"];
        this.Epoch = Convert.ToSingle((string)CometRow["Epoch (TDB)"]);
        this.TP = Convert.ToSingle((string)CometRow["TP (TDB)"]);
        this.Eccentricity = Convert.ToSingle((string)CometRow["e"]);
        this.InclinationAngle = Convert.ToSingle((string)CometRow["i (deg)"]);
        this.PeriapsisArgument = Convert.ToSingle((string)CometRow["w (deg)"]);
        this.RAAN = Convert.ToSingle((string)CometRow["Node (deg)"]);
        this.PerigeeDistance = Convert.ToSingle((string)CometRow["q (AU)"]);
        this.ApogeeDistance = Convert.ToSingle((string)CometRow["Q (AU)"]);
        this.Period = Convert.ToSingle((string)CometRow["P (yr)"]);
        this.MOID = Convert.ToSingle((string)CometRow["MOID (AU)"]);
        this.a = (this.ApogeeDistance + this.PerigeeDistance) / 2.0f;
        this.b = this.a * Sqrt(1 - Pow(this.Eccentricity, 2));
        this.Center = new UnityEngine.Vector2(-this.a * this.Eccentricity,0f);
        this.LookUpIndex = index;
    }
    public Comet()
    {

    }

    public void CalculateOrbit(int NumDataPoints)
    {
        // calculate DCM
        this.CalculateDCM();

        // initialize theta array
        Thetas = new float[NumDataPoints];
        for(int i = 0; i < NumDataPoints; i++)
        {
            Thetas[i] = (i) * 360f / (NumDataPoints - 1);
        }

        // calculate each point in orbit
        foreach (float Theta in Thetas)
        {
            UnityEngine.Vector3 PointBeforeRotation = new UnityEngine.Vector3(this.Center.x + a * Cos(Theta * Deg2Rad), this.Center.y + b * Sin(Theta * Deg2Rad), 0);
            float RotatedX = PointBeforeRotation.x * DCM[0, 0] + PointBeforeRotation.y * DCM[0, 1] + PointBeforeRotation.z * DCM[0, 2];
            float RotatedY = PointBeforeRotation.x * DCM[1, 0] + PointBeforeRotation.y * DCM[1, 1] + PointBeforeRotation.z * DCM[1, 2];
            float RotatedZ = PointBeforeRotation.x* DCM[2, 0] +PointBeforeRotation.y * DCM[2, 1] + PointBeforeRotation.z * DCM[2, 2];
            Orbit.Add(new UnityEngine.Vector3(RotatedX, RotatedZ, RotatedY));
        }

        this.FindOrbitBounds();
        
    }

    public void CalculateSunDistances()
    {
        if(this.Orbit.Count == 0)
        {
            return;
        }
        SunDistances = new float[Orbit.Count];
        for(int i = 0; i < Orbit.Count; i++)
        {
            SunDistances[i] = Orbit[i].magnitude;
        }


    }

    public void CalculateDCM()
    {
        DCM = new float[3,3];


        DCM[0, 0] = Sin(this.RAAN * Deg2Rad) * Sin(this.InclinationAngle * Deg2Rad) * Sin(this.PeriapsisArgument * Deg2Rad) + Cos(this.RAAN * Deg2Rad) * Cos(this.PeriapsisArgument * Deg2Rad);
        DCM[0, 1] = -Sin(this.RAAN * Deg2Rad) * Cos(this.InclinationAngle * Deg2Rad) * Cos(this.PeriapsisArgument * Deg2Rad) - Cos(this.RAAN * Deg2Rad) * Sin(this.PeriapsisArgument * Deg2Rad);
        DCM[0, 2] = Sin(this.RAAN * Deg2Rad) * Sin(this.InclinationAngle * Deg2Rad);
        DCM[1, 0] = Cos(this.RAAN * Deg2Rad) * Cos(this.InclinationAngle * Deg2Rad) * Sin(this.PeriapsisArgument * Deg2Rad) + Sin(this.RAAN * Deg2Rad) * Cos(this.PeriapsisArgument * Deg2Rad);
        DCM[1,1] = Cos(this.RAAN * Deg2Rad) * Cos(this.InclinationAngle * Deg2Rad) * Cos(this.PeriapsisArgument * Deg2Rad) - Sin(this.RAAN * Deg2Rad) * Sin(this.PeriapsisArgument * Deg2Rad);
        DCM[1, 2] = -Cos(this.RAAN * Deg2Rad) * Sin(this.InclinationAngle * Deg2Rad);
        DCM[2, 0] = Sin(this.InclinationAngle * Deg2Rad) * Sin(this.PeriapsisArgument * Deg2Rad);
        DCM[2, 1] = Sin(this.InclinationAngle * Deg2Rad) * Cos(this.PeriapsisArgument * Deg2Rad);
        DCM[2, 2] = Cos(this.InclinationAngle * Deg2Rad);

    }

    private void FindOrbitBounds()
    {
        Bounds OrbitBounds = new Bounds(UnityEngine.Vector3.zero, UnityEngine.Vector3.zero);
        foreach (var pt in Orbit)
        {
            OrbitBounds.Encapsulate(pt);
        }
        
        float max = OrbitBounds.size.MaxComponent();
        OrbitBounds.size = new UnityEngine.Vector3(max, max, max);
        this.OrbitBounds = OrbitBounds;
    }

}
