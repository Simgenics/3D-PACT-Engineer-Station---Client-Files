using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DisplayServer;
using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using Unity.VisualScripting;

public class Communications : MonoBehaviour
{
    public static Communications Instance;
    private string _OwnerID = Guid.NewGuid().ToString();
    private Stopwatch stopWatch = new Stopwatch();
    public int sampleRate = 100;
    public bool isConnected;
    private string readFromTextFileIP = Application.streamingAssetsPath + "/SimuPactIPAddress.txt";

    public bool DebugMode;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        IPAddress = File.ReadAllText(readFromTextFileIP);
        DisplayClient = new DisplayClient(IPAddress, IPPort, _OwnerID);
        stopWatch.Start();
        //StartAutoConnecting();
    }

    private bool working;

    // Update is called once per frame
    void Update()
    {
        if (working)
        {
            return;
        }
        working = true;

        System.Threading.Tasks.Task.Run(new Action(() => {
            try
            {
                isReconnecting = IsReconnecting;

                if (stopWatch.ElapsedMilliseconds >= sampleRate)
                {
                    RegisterDataPoints();
                    SyncDataPoints();
                    stopWatch.Restart();

                    Reconnect(null);

                    IsConnected = DisplayClient.IsConnected;
                    if (IsConnected)
                    {
                        if (DebugMode) Debug.Log("Connected successfully!");

                    }
                    else
                    {
                        if (IsConnected) Debug.LogWarning("Attempting to reconnect to SimuPact with IP " + IPAddress);
                    }
                }
            }
            catch (Exception e)
            {
                if (DebugMode) Debug.LogWarning(e);
            }
            working = false;
        }));

    }

    public string IPAddress;
    public ushort IPPort = 2954;
    public static bool FirstConnectionAttempComplete = false;
    public bool IsConnected_Model { get { return DisplayClient.IsConnected; } }
    public bool IsReconnecting { get; private set; } = false;
    public bool isReconnecting;
    public int SecondsTillNextReconnectionAttempt
    {
        get
        {
            return System.Convert.ToInt32(Math.Max(0.0, 10.0 - _TimeSinceLastReconnectionAttempt.Elapsed.TotalSeconds));
        }
    }

    public bool IsConnected 
    { 
        get => isConnected;
        set
        {
            if (isConnected == value) 
                return;

            isConnected = value;
            CustomEvent.Trigger(gameObject, "Connection Changed");
        }
    }

    Stopwatch _TimeSinceLastReconnectionAttempt = new Stopwatch();
    System.Threading.Timer _AutoReconnectTimer = null;
    public void StartAutoConnecting()
    {
        Reconnect(null);
        FirstConnectionAttempComplete = true;
        //_AutoReconnectTimer = new System.Threading.Timer(Reconnect);
        //_AutoReconnectTimer.Change(10000, 10000);
    }

    private void Reconnect(System.Object stateInfo)
    {
        //_TimeSinceLastReconnectionAttempt.Restart();
        try
        {
            if (!IsReconnecting)
            {
                IsReconnecting = true;

                DisplayClient.ReconnectConnectToServer();
            }
        }
        catch (Exception e)
        {
            if (DebugMode) Debug.LogWarning(e);
        }
        IsReconnecting = false;


    }

    private DisplayClient DisplayClient;




    private void RegisterDataPoints()
    {
        DisplayClient.RegisterDataPoints();
    }

    private void DeRegisterDataPoints()
    {
        DisplayClient.DeRegisterDataPoints();
    }

    private void SyncDataPoints()
    {
        DisplayClient.SyncDataPoints();
    }

    public PointData GetDataPoint(string data_point_id)
    {
        
        return DisplayClient.GetDataPoint(data_point_id);
    }

    public void ToggleProperty(string property, double set, double reset)
    {
        DisplayClient.ToggleProperty(property, set, reset);
    }

    public void IncrementProperty(string property, double inc, double min, double max)
    {
        DisplayClient.IncrementProperty(property, inc, min, max);
    }

    public void SetProperty(string property, double value)
    {
        DisplayClient.SetProperty(property, value);
    }

    //public void FocusProperty(string property)
    //{
    //    DisplayClient.FocusProperty(property);
    //}

    //public SphereData GetSpherePaths()
    //{
    //    return DisplayClient.GetSpherePaths();
    //}

    //public string GetComponentType(string owner)
    //{
    //    return DisplayClient.GetComponentType(owner);
    //}
}
