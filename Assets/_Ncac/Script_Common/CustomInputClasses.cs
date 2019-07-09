using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NCToolset
{
    public class NcCustomInput
    {
        //enum for the touch state
        public enum TouchState { PINCH, STRETCH, TOUCH, UNKNOWN, INACTIVE, DRAG, STILL };

        //does the device support touch?
        public bool isTouchPlatform;

        //to record touch state
        public static TouchState currentTouchState = TouchState.INACTIVE;
        public static TouchState previousTouchState = TouchState.INACTIVE;

        //for mouse click
        public static Vector2 previousMousePos = Vector2.zero;
        public static Vector2 currentMousePos = Vector2.zero;
        public static float currentClickTime = 0;
        public static float previousClickTime = 0;
        public static float mouseDeltaTime = 0;
        public static Vector2 mouseDeltaPos = Vector2.zero;

        //to store touches
        public Touch[] touches;
        public bool[] areTouchesOnGui;
        public bool areAnyTouchesOnGui;
        public int touchCount;
        public float? diffInDistBtwTwoFingers = null;

        public NcCustomInput()
        {
            isTouchPlatform = TouchManager.Instance.isTouchPlatform;

            //If the app is running on Android or Apple Touch-enabled device, 
            if (isTouchPlatform)
            {
                touchCount = Input.touchCount;
                touches = new Touch[Input.touchCount];
                areTouchesOnGui = new bool[Input.touchCount];
                areAnyTouchesOnGui = false;

                for (int i = 0; i < Input.touchCount; i++)
                {
                    touches[i] = Input.GetTouch(i);
                    bool isTouchOnGui = IsTouchOverGUIObjects(Input.GetTouch(i));
                    areTouchesOnGui[i] = isTouchOnGui;
                    if (isTouchOnGui) areAnyTouchesOnGui = true;
                }

                //If the touch counts are two, check their state
                if (touchCount == 2)
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    //check if touches are on objs. If it is not enabled on GUI and both fingers are on non-UI objs, pass
                    //if (!enableOnGUI && (IsTouchOverGUIObjects(touch1) || IsTouchOverGUIObjects(touch2))) return;

                    //find the previous touch positions
                    Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                    Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                    //find the current distance between touches and the one of the previous touches
                    float currentTouchesDist = (touch1.position - touch2.position).magnitude;
                    float previousTouchesDist = (touch1PrevPos - touch2PrevPos).magnitude;

                    //difference in the current and preious distances btw/ touches. + means stretch, - means pinch.
                    diffInDistBtwTwoFingers = currentTouchesDist - previousTouchesDist;

                    //set the touch state of the previous 
                    previousTouchState = currentTouchState;

                    //Updtate the current state
                    currentTouchState = diffInDistBtwTwoFingers > 0 ? TouchState.STRETCH : TouchState.PINCH;
                    currentTouchState = diffInDistBtwTwoFingers == 0 ? TouchState.STILL : currentTouchState;
                }

            }

            //if not in touch platform, only one mouse click is enabled
            else
            {
                touchCount = 1;

                //here we convert mouse to touch
                touches = new[] { CovertMouseTotouch() };

                areTouchesOnGui = new[] { IsTouchOverGUIObjects(touches[0]) };
                areAnyTouchesOnGui = areTouchesOnGui[0];

                //save mouse info for the next update
                previousClickTime = Time.time;
                previousMousePos = touches[0].position;
            }
        }


        private Touch CovertMouseTotouch()
        {
            Touch t = new Touch();
            t.fingerId = 0;
            t.position = Input.mousePosition;

            //delta stuff
            t.deltaPosition = t.position - previousMousePos;
            t.deltaTime = Time.time - previousClickTime;

            if (Input.GetMouseButtonDown(0)) t.phase = TouchPhase.Began;
            else if (Input.GetMouseButtonUp(0)) t.phase = TouchPhase.Ended;
            else if (Input.GetMouseButton(0))
            {
                t.phase = TouchPhase.Moved;
            }
            else t.phase = TouchPhase.Canceled;
            return t;
        }

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
