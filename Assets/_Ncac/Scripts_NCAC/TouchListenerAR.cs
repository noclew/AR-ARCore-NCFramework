using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace NCAC
{
    using NCToolset;
    using GoogleARCore;
    //TouchListner For AR Core

    public class TouchListenerAR : MonoBehaviour
    {

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
            TouchManager.TouchDown += OnTouchDown;
            TouchManager.Pinch += OnPinch;
            TouchManager.Stretch += OnStretch;
            TouchManager.DoubleTab += OnDoubleTab;
        }

        private void OnDisable()
        {
        }

        void OnTouchDown(object sender, NcCustomInput customInput)
        {
            //ARCore
            TrackableHit hit;
            TrackableHitFlags raycastFilter =
                TrackableHitFlags.PlaneWithinBounds |
                TrackableHitFlags.PlaneWithinPolygon;
            Touch touch = customInput.touches[0];

            //raycast to the AR core detected planes.
            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                Debug.Log("plane touched (from touch Listener)");
                /////////////////////////////////////change the touchlistnerAR onTouch function. now it directly sends.
                //SceneController.Instance.SetDetectedPlane(hit.Trackable as DetectedPlane);
                TrackingController.Instance.OnTrackableTouched(hit);
            }
        }

        void OnPinch(object sender, NcCustomInput customInput)
        {

        }

        void OnStretch(object sender, NcCustomInput customInput)
        {

        }

        void OnDoubleTab(object sender, NcCustomInput customInput)
        {

        }

    }

}
