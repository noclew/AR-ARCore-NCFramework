using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using NCToolset;

namespace NCAC
{
    public class NcacDetectedPlaneTracker
    {
        public DetectedPlane m_refPlane { get; private set; }
        public Anchor m_Anchor;

        //this will be used to compare two anchors to determine they are similar or not
        public float mNewAnchorAngleThres = 5f;
        public float mNewAnchorDistThres = 0.05f;

        string msgSource = " ERR!!>> planeTracker: ";

        public NcacDetectedPlaneTracker()
        {
            m_refPlane = null;
            m_Anchor = null;
        }

        public bool SetReferncePlane(DetectedPlane plane)
        {

            if (plane != null || plane.TrackingState == TrackingState.Tracking)
            {
                m_refPlane = plane;
                Debug.Log("successfully set the reference plane " + plane.GetHashCode());
                return true;
            }

            else
            {
                Debug.Log("could not set the reference plane ");
                return false;
            }
        }


        public bool UpdateMainAnchor(Pose pose, DetectedPlane plane = null)
        {
            if (plane != null) SetReferncePlane(plane);

            // if the reference plane is not being tracked, or it is null, do nothing.
            if (m_refPlane.TrackingState != TrackingState.Tracking || m_refPlane == null)
            {
                string msg = "The ref plane is not in a tracking state or is empty, so updating anchor failed \n" +
                    m_refPlane + " >> " + m_refPlane.TrackingState ;
                Debug.LogError(msgSource + msg);
                AndyHelper.ShowToastMsg(msg);
                return false;
            }

            // if the new anchor position is similar to the current anchor, skip update. 
            if (m_Anchor != null
                && Quaternion.Angle(pose.rotation, m_Anchor.transform.rotation) < mNewAnchorAngleThres
                && Vector3.Distance(pose.position, m_Anchor.transform.position) < mNewAnchorDistThres)
            {
                string msg = msgSource + "The new Anchor position is too similar to the current one";
                return false;
            }

            // create a new anchor
            Anchor ac = m_refPlane.CreateAnchor(pose);

            // verify if the anchor is good
            if (ac != null)
            {
                // If, there's one exsting, delete it. the object will be destroyed in the end of the frame
                if (m_Anchor != null)
                {
                    Debug.Log(msgSource + "existing anchor " + m_Anchor.GetHashCode() + "was deleted");
                    Object.Destroy(m_Anchor);
                    m_Anchor = null;
                }

                m_Anchor = ac;
                Debug.Log(msgSource + "anchor was successfully updated");
                return true;
            }

            else
            {
                Debug.Log(msgSource + "anchor creation failed on the plane");
                return false;
            }




        }

    }
}

