using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NCToolset;

namespace NCAC
{
    public class MenuController : MonoBehaviour
    {
        [Header("Toggle Button Initialize / Optional")]
        public RectTransform MainViewPanel;
        public RectTransform AdminPanel;
        public RectTransform ManualInitPanel;


        [System.Flags]
        public enum MenuState
        {
            None = 0x0,
            MainViewer = 0x1,
            Manualinit = 0x4,
            AdminMenu = 0x8,
        }

        MenuState menuState;

        public bool IsAutoReinitializing
        {
            get { return TrackingController.Instance.m_isImageAligmentEnabled; }
            set { TrackingController.Instance.m_isImageAligmentEnabled = value; }
        }

        public bool IsPlaneVisualizerEnabled
        {
            get { return TrackingController.Instance.m_isPlaneVisEnabled; }
            set { TrackingController.Instance.m_isPlaneVisEnabled = value; }
        }

        public bool IsDebuggingScreenEnabled
        {
            get { return TrackingController.Instance.m_isDebugScreenEnabled; }
            set { TrackingController.Instance.m_isDebugScreenEnabled = value; }
        }
        public void Awake()
        {
            menuState = MenuState.MainViewer;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /////////////////////////////////////////////////////////// Functions to be called in Main viewer
        public void _OnOpenAdminButtonCliced()
        {
            AdminPanel.gameObject.SetActive(true);
            MainViewPanel.gameObject.SetActive(false);
        }

        /////////////////////////////////////////////////////////// Functions to be called in manual Init menu
        public void OnPlaceRefPlaneButtonPressed()
        {

        }

        public void OnPlaceAnchorButtonPressed()
        {

        }

        public void _OnExitManualInitButtonPressed()
        {
            TrackingController.Instance.m_isInManualInitMode = false;

            AdminPanel.gameObject.SetActive(true);
            ManualInitPanel.gameObject.SetActive(false);

        }

        /////////////////////////////////////////////////////////// Functions to be called in admin menu
        public void _OnExitButtonInAdminMenuPressed()
        {
            AdminPanel.gameObject.SetActive(false);
            MainViewPanel.gameObject.SetActive(true);
        }
        public void OnToggle_AutoReinit(Toggle tg)
        {
            TrackingController.Instance.m_isImageAligmentEnabled = tg.isOn;
        }

        public void _OnManualInitButtonPressed()
        {
            MainViewPanel.gameObject.SetActive(false);
            AdminPanel.gameObject.SetActive(false);
            ManualInitPanel.gameObject.SetActive(true);

            TrackingController.Instance.m_isInManualInitMode = true;
            StartCoroutine(PauseAutoReinit());
            StartCoroutine(EnablePlaneVisualizerWhileInit());
        }

        public void OnToggle_PlaneVis(Toggle tg)
        {
            TrackingController.Instance.m_isPlaneVisEnabled = tg.isOn;
        }

        public void OnToggle_DebugScreen(Toggle tg)
        {
            TrackingController.Instance.m_isDebugScreenEnabled = tg.isOn;
        }

        public void OnToggle_ShowAnchorVisualizer(Toggle tg)
        {
            TrackingController.Instance.m_isAnchorVisEnabled = tg.isOn;
        }


        IEnumerator PauseAutoReinit()
        {
            // if the auto reinitialization was enabled
            if (TrackingController.Instance.m_isImageAligmentEnabled)
            {
                TrackingController.Instance.m_isImageAligmentEnabled = false;
                while (TrackingController.Instance.m_isInManualInitMode)
                {
                    yield return null;
                }
                TrackingController.Instance.m_isImageAligmentEnabled = true;
            }

        }

        IEnumerator EnablePlaneVisualizerWhileInit()
        {
            yield return null;
            if (!TrackingController.Instance.m_isPlaneVisEnabled)
            {
                TrackingController.Instance.m_isPlaneVisEnabled = true;
                while (TrackingController.Instance.m_isInManualInitMode)
                {
                    yield return null;
                }
                TrackingController.Instance.m_isPlaneVisEnabled = false;
            }
        }
    }
}