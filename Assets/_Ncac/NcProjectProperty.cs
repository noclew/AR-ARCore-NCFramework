using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NcProjectProperty : MonoBehaviour
{
    [Header("Project Parameters")]
    public string AppName;
    public string AppVersion;
    public string BuildNumber;
    public string BuildDate;
    public string UnityVersion;
    public string ARCoreVersion;
    public string NCACVersion;

    public string mAppName { get; set; }
    public string mAppVersion {get; set;}
    public string mBuildNumber { get; set; }
    public string mBuildDate { get; set; }
    public string mUnityVersion { get; set; }
    public string mArCoreVersion { get; set; }
    public string mNcacVersion { get; set;}

    private void Awake()
    {
        mAppName = AppName;
        mAppVersion = AppVersion;
        mBuildNumber = BuildNumber;
        mBuildDate = BuildDate;

        if (UnityVersion == null || UnityVersion == "") UnityVersion = Application.unityVersion;
        mUnityVersion = UnityVersion;

        mArCoreVersion = ARCoreVersion;
        mNcacVersion = NCACVersion;
    }
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
