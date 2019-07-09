using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCToolset
{
    public class NcDebugScreen : MonoBehaviour
    {
        [Header ("Parameters to display debug info")]
        public int labelCount = 40;
        public float labelWidthRatio = 0.9f;
        public float topMarginRatio = 0.075f;
        public float leftMarginRatio = 0.05f;

        public Color defaultTextColor = Color.black;
        public Color32 textBackgroundColor;

        public static int iMsgCount { get; private set; }
        static GUIStyle mGuiStyle = null;

        //private member. These will update per frame
        int labelWidth = 0;
        int labelHeight = 0;
        int TopMargin = 10;
        int LeftMargin = 10;

        private ScreenOrientation PREV_ORIENTATION;

        #region Singleton Stuff
        public static NcDebugScreen Instance { get; private set; }

        private void Awake()
        {
            if (NcDebugScreen.Instance == null) NcDebugScreen.Instance = this;
            if (NcDebugScreen.Instance != this) Destroy(this); 
        }
        #endregion

        // Start is called before the first frame update
        private void Start()
        {
            // check if the scene controller is not specified. 
            PREV_ORIENTATION = Screen.orientation;
            iMsgCount = 0;
            initiateGUIStyle();
        }

        private void initiateGUIStyle()
        {
            mGuiStyle = new GUIStyle();
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.5f));
            texture.Apply();

            mGuiStyle.normal.background = texture;
            mGuiStyle.normal.textColor = Color.red;
            mGuiStyle.fontSize = (int)((Screen.height / labelCount) * 0.75f);
        }
        // Update is called once per frame
        void Update()
        {
            // If the main controller is debugging, and the orientation has been changed, update the debug screen params.
            if (NCAC.TrackingController.Instance.m_isDebugScreenEnabled || PREV_ORIENTATION != Screen.orientation)
            {
                UpdateDebugScreenParams();
                PREV_ORIENTATION = Screen.orientation;
            }
            
        }

        public void ShowDebugMsg(string msg, GUIStyle style = null, Color? color = null)
        {
            //print(iMsgCount);
            if (style == null) { style = mGuiStyle; }
            if (color != null) { style.normal.textColor = (Color)color; }
            GUI.Label(new Rect( LeftMargin, TopMargin + ( (iMsgCount) * labelHeight), labelWidth, labelHeight), msg, mGuiStyle);
            iMsgCount += 1;
        }

        public void ResetDebugScreen()
        {
            iMsgCount = 0;
            ChangeTextColor( defaultTextColor);
        }

        public void ChangeTextColor(Color color)
        {
            mGuiStyle.normal.textColor = color;
        }

        void UpdateDebugScreenParams()
        {
            LeftMargin = (int)(Screen.width * leftMarginRatio);
            TopMargin = (int)(Screen.height * topMarginRatio);

            labelWidth = (int)(Screen.width * labelWidthRatio);
            labelHeight = (int)(Screen.height / labelCount);
            mGuiStyle.fontSize = (int)((Screen.height / labelCount) * 0.75f);
        }
    }
}