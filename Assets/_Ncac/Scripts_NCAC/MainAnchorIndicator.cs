using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NCAC
{
    /// <summary>
    /// manages color block for a button indication some AR state
    /// </summary>
    public class MainAnchorIndicator : MonoBehaviour
    {
        public Color trackingColor = Color.green;
        public Color nonTrackingColor = Color.magenta;

        private UnityEngine.UI.Button mButton;
        private ColorBlock mColors_untrack;
        private ColorBlock mColors_track;
        private NcacDetectedPlaneTracker mPlTracker;

        // Start is called before the first frame update
        void Start()
        {
            mButton = GetComponent<UnityEngine.UI.Button>();

            if (mButton != null)
            {
                mColors_track = (ColorBlock)MakeUniformColorBlock(trackingColor);
                mColors_untrack = (ColorBlock)MakeUniformColorBlock(nonTrackingColor);
                mButton.colors = mColors_untrack;
            }
            mPlTracker = TrackingController.Instance.PlaneTracker;
        }

        // Update is called once per frame
        void Update()
        {
            GoogleARCore.Anchor res = mPlTracker.m_Anchor;

            if (mButton != null && res != null)
            {
                mButton.colors = mColors_track;
            }
            else if (mButton != null && res == null)
            {
                mButton.colors = mColors_untrack;
            }

        }

        ColorBlock? MakeUniformColorBlock(Color color)
        {
            if (mButton != null)
            {
                ColorBlock cb = mButton.colors;
                cb = GetComponent<Button>().colors;
                cb.normalColor = color;
                cb.pressedColor = color;
                cb.highlightedColor = color;
                cb.disabledColor = color;
                return cb;
            }

            return null;
        }
    }
}