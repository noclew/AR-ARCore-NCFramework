using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NCAC
{
    using NCToolset;
    using GoogleARCore;
    public class GazeContoller : MonoBehaviour
    {
        #region public members

        // visualizer for the gaze

        public GameObject m_GazePointer;

        public bool IsGazeVisualizerEnabled { get; set; }

        // the plane that a user is looking at
        public DetectedPlane m_PlaneOfInterest { get; private set; }
        public Transform mObjOfInterest { get; private set; }
        public TrackableHit mTrackableHit { get; private set; }
        public RaycastHit m_rayCastHit { get; private set; }

        //speed of the following point visualizer. Does not effect the actual hit point. 
        public float gazeFollowingSpeed = 10f;
        #endregion

        public Camera ArCam;
        // Speed to move.

        #region Singleton Stuff
        public static GazeContoller Instance { get; private set; }
        private void Awake()
        {
            if (GazeContoller.Instance == null) GazeContoller.Instance = this;
            if (GazeContoller.Instance != this) Destroy(this);
            IsGazeVisualizerEnabled = true;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if (ArCam == null) ArCam = TrackingController.Instance.m_ARCam; 
            NcHelpers.HideObject(m_GazePointer); 
        }

        // Update is called once per frame
        void Update()
        {
            #region Gaze towards AR Core trackables
            // if the AR core session is tracking
            if (Session.Status == SessionStatus.Tracking)
            {
                TrackableHit trackableHit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds;
                
                // If the gaze from the center of the screen hits AR Core trackable
                if (Frame.Raycast(Screen.width * 0.5f, Screen.height * 0.5f, raycastFilter, out trackableHit))
                {
                    // save the current trackable hit
                    mTrackableHit = trackableHit;

                    // postion of the hit point
                    Pose pose = trackableHit.Pose;

                    // Lerp a bit for a bit of smoothness. "ArGazePointer.transform.position = pt;" will also just work.
                    m_GazePointer.transform.position = Vector3.Lerp(m_GazePointer.transform.position, pose.position, Time.smoothDeltaTime * gazeFollowingSpeed);
                    m_GazePointer.transform.rotation = pose.rotation;

                    // Record the plane of interests.
                    m_PlaneOfInterest = trackableHit.Trackable as DetectedPlane;

                }

                // If the gaze does not intersect AR Core trackable
                else
                {
                    //empty the mPlaneOfInterest
                    m_PlaneOfInterest = null;
                }

            }

            // if the AR core session is not tracking
            else
            {
                m_PlaneOfInterest = null;
            }
            #endregion

            VisualizeGaze();

            //Raycast to find the object at gaze
            Ray gazeRay = ArCam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            //Debug.DrawRay(gazeRay.origin, gazeRay.direction * 10, Color.yellow);
            RaycastHit raycastHit;

            if (Physics.Raycast(gazeRay, out raycastHit))
            {
                m_rayCastHit = raycastHit;
                mObjOfInterest = raycastHit.transform;
            }
            else mObjOfInterest = null;
        }

        void VisualizeGaze()
        {
            if (Session.Status == SessionStatus.Tracking && IsGazeVisualizerEnabled)
            {
                NcHelpers.ShowObject(m_GazePointer);
            }
            else NcHelpers.HideObject(m_GazePointer);
        }
    }

}