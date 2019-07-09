using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace NCToolset
{
    using NCAC;
    using GoogleARCore;
    //TouchListner For AR Core

    public class TouchListener : MonoBehaviour
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
            TouchManager.TouchDown -= OnTouchDown;
            TouchManager.Pinch -= OnPinch;
            TouchManager.Stretch -= OnStretch;
            TouchManager.DoubleTab -= OnDoubleTab;
        }

        void OnTouchDown(object sender, NcCustomInput customInput)
        {
            GameObject go = NcHelpers.GetTouchedObject(customInput.touches[0]);
            if(go != null) print(string.Format(">> {0} is touched down!", go.name));
        }

        void OnTouchUp (object sender, NcCustomInput customInput)
        {
            GameObject go = NcHelpers.GetTouchedObject(customInput.touches[0]);
            print(string.Format(">> {0} is touched up!", go));
        }

        void OnPinch(object sender, NcCustomInput customInput)
        {
            GameObject go1 = NcHelpers.GetTouchedObject(customInput.touches[0]);
            GameObject go2 = NcHelpers.GetTouchedObject(customInput.touches[1]);

            if (go1 != null && go1 == go2)
            {
                NcHelpers.AdjustLocalScale(go1, (float)customInput.diffInDistBtwTwoFingers);
            }

            //call OnPinch on the touched object
            go1.SendMessage("OnPinch");

        }

        void OnStretch(object sender, NcCustomInput customInput)
        {
            GameObject go1 = NcHelpers.GetTouchedObject(customInput.touches[0]);
            GameObject go2 = NcHelpers.GetTouchedObject(customInput.touches[1]);
            //string msg = string.Format("{0} and {1} are clicked", go1.name, go2.name);
            //AndyHelper.ShowToastMsg(msg);

            if (go1 != null && go1 == go2)
            {
                NcHelpers.AdjustLocalScale(go1, (float)customInput.diffInDistBtwTwoFingers);
            }

            // call OnStretch on the touched object
            go1.SendMessage("OnStretch");

        }

        void OnDoubleTab(object sender, NcCustomInput customInput)
        {
            GameObject go = NcHelpers.GetTouchedObject(customInput.touches[0]);

            if (go != null) go.SendMessage("Double Tabbed on: " + go.name);

            // Call OnDoubleTab on the touched object
            if (go != null) NcHelpers.ResetLocalScale(go);
        }

        
    }

}
