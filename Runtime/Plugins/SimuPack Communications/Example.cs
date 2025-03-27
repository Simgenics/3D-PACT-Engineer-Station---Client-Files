using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DisplayServer;

public class Example : MonoBehaviour
{
    private Communications communications;
    public string ManualValveIdentifier;
    private GameObject simuPactInterfaceObject;
    private PointData pdDisplayPosition;

    public double valvePosition;
    public double valvePositionDemand;



    // Start is called before the first frame update
    void Start()
    {
        simuPactInterfaceObject = GameObject.Find("SimuPactInterface");

        communications = simuPactInterfaceObject?.GetComponent<Communications>();

        //Example Interface read connections {COMPONENT}01EKF10CQ006-IO|:|SpecificProperty
        pdDisplayPosition = communications?.GetDataPoint("{COMPONENT}01EKF10CQ006|:|Output");
    }

    // Update is called once per frame
     void Update()
    {
        if(pdDisplayPosition == null) pdDisplayPosition = communications?.GetDataPoint("{COMPONENT}01EKF10CQ006|:|Output");
        if (pdDisplayPosition != null ) valvePosition = pdDisplayPosition.Value;

        //valvePositionDemand = 50;


        //Example Interface read connections {COMPONENT}00ISR10WC001_3DP|:|OpStart
        //pdDisplayPosition = communications?.GetDataPoint("{COMPONENT}00ISR10WC001_3DP |:|OpStart");

        //Example Interface write connection
        //communications?.SetProperty("{COMPONENT}" + "00ISR10WC001_3DP" + " |:|OpStart", valvePositionDemand);
    }

    [Obsolete]
    public void UpdateValue()
    {
        // if (pdDisplayPosition == null) pdDisplayPosition = communications?.GetDataPoint("{COMPONENT}01EKF10CQ006|:|Output");
        // communications?.SetProperty("{COMPONENT}01EKF10CQ006|:|Output", valvePositionDemand);
    }
}
