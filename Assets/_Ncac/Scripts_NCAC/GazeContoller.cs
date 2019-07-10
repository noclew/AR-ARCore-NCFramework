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

        public bool m_IsGazeVizEnabled { get; set; }
        public bool m_IsGazeIntersectTrackablePlane { get; set; }

        // the plane that a user is looking at
        public DetectedPlane m_PlaneOfInterest { get; private set; }
        public Transform m_ObjectOfInterest { get; private set; }
        public TrackableHit m_TrackableHit { get; private set; }
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
            m_IsGazeVizEnabled = true;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if (ArCam == null) ArCam = Camera.main;
            NcHelpers.HideObject(m_GazePointer);
        }

        // Update is called once per frame
        void Update()
        {
            m_IsGazeIntersectTrackablePlane = false;
            m_PlaneOfInterest = null;

            #region Gaze towards AR Core trackables
            // if the AR core session is tracking
            if (Session.Status == SessionStatus.Tracking)
            {

                TrackableHit hit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

                // If the gaze from the center of the screen hits AR Core trackable
                if (Frame.Raycast(Screen.width * 0.5f, Screen.height * 0.5f, raycastFilter, out hit))
                {
                    //detect if we are looking at a back of the plane. seems like quaternion * vector means to translate the vector in the given quaternion (roatation) frame.
                    if ((hit.Trackable is DetectedPlane) && Vector3.Dot(ArCam.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
                    {
                        Debug.Log("You are looking at back of the current DetectedPlane");
                    }
                    else
                    {
                        m_IsGazeIntersectTrackablePlane = true;
                        // Record the current trackable hit
                        m_TrackableHit = hit;
                        // Record the current plane that the device is looking at
                        m_PlaneOfInterest = hit.Trackable as DetectedPlane;
                    }

                }
            }

            // if the AR core session is not tracking
            #endregion

            VisualizeGaze();

            //Raycast to find the object at gaze
            Ray gazeRay = ArCam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            //Debug.DrawRay(gazeRay.origin, gazeRay.direction * 10, Color.yellow);
            RaycastHit raycastHit;

            if (Physics.Raycast(gazeRay, out raycastHit))
            {
                m_rayCastHit = raycastHit;
                m_ObjectOfInterest = raycastHit.transform;
            }
            else m_ObjectOfInterest = null;
        }

        void VisualizeGaze()
        {

            if (Session.Status == SessionStatus.Tracking && m_IsGazeVizEnabled)
            {
                NcHelpers.ShowObject(m_GazePointer);
                // if gaze does not intersect with a trackable plane, skip updating the position of gaze visualizer
                if (!m_IsGazeIntersectTrackablePlane)
                {
                    return;
                }
                // if a gaze intersects with a trackable plane, update the pos and rotation of the gaze pointer.
                else
                {
                    Pose pose = m_TrackableHit.Pose;
                    // Lerp a bit for a bit of smoothness. "ArGazePointer.transform.position = pt;" will also just work.
                    m_GazePointer.transform.position = Vector3.Lerp(m_GazePointer.transform.position, pose.position, Time.smoothDeltaTime * gazeFollowingSpeed);
                    m_GazePointer.transform.rotation = pose.rotation;

                }

                
            }
            else
            {
                NcHelpers.HideObject(m_GazePointer);
            }
        }
    }

}