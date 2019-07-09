using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NCToolset
{
    using GoogleARCore;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.

    using Input = GoogleARCore.InstantPreviewInput;
#endif  // UNITY_EDITOR

    public class NcGlobalScalable : MonoBehaviour
    {
        //enum for tracking touch states
        enum TouchState { PINCH, STRETCH, UNKNOWN, INACTIVE, STALE };

        public bool handleScaleInput = false; //Let this GO to handle input touch
        public bool enableScaleAdjustment = false; //enable scaling functions.
        public bool enableOnGUI = false;

        public int currentTouchCount { get; set; } //stores the current touch count in this frame

        private Vector3 OriginalLocalScale;
        private float scaleSpeed = 1f;
        private float scaleFactor = 1f;
        private TouchState currentTouchState = TouchState.INACTIVE;

        //for ARcore
        NCAC.NcacDetectedPlaneTracker planeTracker;

        // Start is called before the first frame update
        void Start()
        {
            OriginalLocalScale = transform.localScale;
            currentTouchCount = 0;

            planeTracker = new NCAC.NcacDetectedPlaneTracker();
            planeTracker.SetReferncePlane(null);
        }

        // Update is called once per frame
        void Update()
        {
            if (handleScaleInput == false) return;

            //if no touch
            if (Input.touchCount == 0)
            {
                currentTouchCount = 0;
                currentTouchState = TouchState.INACTIVE;
            }

            //if one-finger touch
            else if (Input.touchCount == 1)
            {
                currentTouchCount = 1;
                Touch touch1 = Input.GetTouch(0); //get the first touch

                //check if touch is onGUI
                if (!enableOnGUI && IsTouchOverGUIObjects(touch1))
                {
                    string msg = string.Format("fingeID {0} is touched on GUI", touch1.fingerId);
                    AndyHelper.ShowToastMsg(msg);
                    print(msg);
                    return;
                }

                //OnTouchDown Event
                else if (touch1.phase == TouchPhase.Began)
                {
                    TrackableHit hit;
                    TrackableHitFlags raycastFilter =
                        TrackableHitFlags.PlaneWithinBounds |
                        TrackableHitFlags.PlaneWithinPolygon;

                    if (Frame.Raycast(touch1.position.x, touch1.position.y, raycastFilter, out hit))
                    {
                        planeTracker.SetReferncePlane( hit.Trackable as DetectedPlane);
                    }


                }

                //OnTouchUp Event
                else if (touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled)
                {

                }

                //OnTouchDrag Event
                else if (touch1.phase == TouchPhase.Moved)
                {

                }

            }

            //if two-finger touch
            else if (Input.touchCount == 2)
            {
                currentTouchCount = 2;

                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                //check if touches are on objs. If it is not enabled on GUI and both fingers are on non-UI objs, pass
                if (!enableOnGUI && (IsTouchOverGUIObjects(touch1) || IsTouchOverGUIObjects(touch2))) return;

                //find the previous touch positions
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                //find the current distance between touches and the one of the previous touches
                float currentTouchesDist = (touch1.position - touch2.position).magnitude;
                float previousTouchesDist = (touch1PrevPos - touch2PrevPos).magnitude;

                //difference in the current and preious distances btw/ touches. + means stretch, - means pinch.
                float distDiff = currentTouchesDist - previousTouchesDist;
                currentTouchState = distDiff > 0 ? TouchState.STRETCH : TouchState.PINCH;
                currentTouchState = distDiff == 0 ? TouchState.STALE : currentTouchState;

                //find the differece in touch distances btw the current and previous touches
                float scaleFactorDiff = distDiff * scaleSpeed;
                //since difference is large, we scale scalefactor
                scaleFactor = (scaleFactor * 100 + scaleFactorDiff) / 100;

                //scaleFactor ranges from 0.1 to 100.
                scaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 100f);
            }

            ScaleObject(this.transform);
        }

        void ScaleObject(Transform transfrom)
        {
            transform.localScale = OriginalLocalScale * scaleFactor;
        }

        private void LateUpdate()
        {

        }

        bool CheckIfOnGUI(int id)
        {
            if (EventSystem.current.IsPointerOverGameObject(id))
            {
                return true;
            }
            return false;
        }

        private GameObject FindTouchedGameObject(Touch touch)
        {
            //If touch is on GUI, return null
            if (IsTouchOverGUIObjects(touch)) return null;

            else
            {
                //check only the began phase
                if (touch.phase == TouchPhase.Began)
                {
                    //construct a ray from 
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit))
                    {
                        return (hit.collider.gameObject);
                    }
                }

            }

            return null;
        }

        //check if current touch is on *ANY GUI element.
        private bool IsTouchOverGUIObjects(Touch touch)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
