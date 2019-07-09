using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NCToolset
{
    /// <summary>
    /// TouchManger class for a custom touch control. 
    /// </summary>
    public class TouchManager : MonoBehaviour
    {
        /// <summary>
        /// when enabled, the manager incurs touch event even when the touches are on GUI objects.
        /// </summary>
        public bool PropagateGUIClick = false;


        //params for multiple tabs
        public float doubleTabTimeLimit = 0.2f;
        public float doubleTabDistLimit = 1f;

        //to support multi tabs
        //int multipleTabCount = 0;
        //float previousCheckTime = 0;
        bool isHoldingAction = false;

        //eventHandler = delegate with (obj sender, eventArg arg)
        public static event EventHandler<NcCustomInput> TouchDown;
        public static event EventHandler<NcCustomInput> TouchUp;
        public static event EventHandler<NcCustomInput> Drag;
        public static event EventHandler<NcCustomInput> DoubleTab;
        public static event EventHandler<NcCustomInput> Pinch;
        public static event EventHandler<NcCustomInput> Stretch;

        public bool isTouchPlatform { get; private set; }

        NcCustomInput lastInput;
        NcCustomInput currentInput;

        public bool enableScaleAdjustment = false; //enable scaling functions.
        public bool enableOnGUI = false;

        public int currentTouchCount { get; set; } //stores the current touch count in this frame

        private Vector3 OriginalLocalScale;
        private float scaleSpeed = 1f;
        private float scaleFactor = 1f;

        #region Sington stuff

        public static TouchManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

            else if (Instance != this)
            {
                Destroy(this);
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            isTouchPlatform = IsTouchPlatform();
        }



        // Update is called once per frame
        void Update()
        {
            currentTouchCount = Input.touchCount;

            if (!isTouchPlatform)
                currentTouchCount = (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) ) ? 1 : 0;

            //if no touch, just return
            if (currentTouchCount == 0)
            {
                currentTouchCount = 0;
                return;
            }

            //initiate new custom Touch
            NcCustomInput customInput = new NcCustomInput();

            //if GUI propagation disabled, and any touches are on GUI, return.
            if (!PropagateGUIClick && customInput.areAnyTouchesOnGui) return;

            //if one-finger has been touched
            if (currentTouchCount == 1)
            {

                Touch touch = customInput.touches[0];
                

                //TouchDown || DoubleTab Event
                if (touch.phase == TouchPhase.Began)
                {
                    float deltaTime = touch.deltaTime;
                    float deltaPosMag = touch.deltaPosition.magnitude;
                    if (deltaTime > 0 && deltaTime < doubleTabTimeLimit && deltaPosMag < doubleTabDistLimit)
                    {
                        if (DoubleTab != null)
                        {
                            DoubleTab(this, customInput);
                        }
                    }

                    else
                    {
                        if (TouchDown != null)
                        {
                            TouchDown(this, customInput);
                        }
                    }
                    
                }
                

                //TouchUp Event
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {

                    if (TouchUp != null)
                    {
                        TouchUp(this, customInput);
                    }
                }

                //TouchDrag Event


                //DoubleTab Event


            }

            //if two-fingers have been touched
            else if (currentTouchCount == 2)
            {
                
                if (NcCustomInput.currentTouchState == NcCustomInput.TouchState.STRETCH)
                {
                    if (Stretch != null) Stretch(this, customInput);
                }
                if (NcCustomInput.currentTouchState == NcCustomInput.TouchState.PINCH)
                {
                    if (Stretch != null) Pinch(this, customInput);
                }
            }

        }


        //destroy
        private void LateUpdate()
        {

        }

        public static bool IsTouchPlatform()
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer) return true;
            else return false;

        }

    }




}