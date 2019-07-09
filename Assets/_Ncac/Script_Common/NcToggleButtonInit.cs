using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NCToolset
{
    /// <summary>
    /// If an attached toggle button has the first event handler to set a perperty of an object,
    /// this class sync the toggle button with the target propety.
    /// </summary>
    public class NcToggleButtonInit : MonoBehaviour
    {
        public bool IsToggleButtonInitiated { get; set; }
        Toggle m_Toggle;
        // Start is called before the first frame update
        private void Awake()
        {
            IsToggleButtonInitiated = false;
        }
        void Start()
        {
            m_Toggle = GetComponent<Toggle>();
            Toggle.ToggleEvent m_Event = m_Toggle.onValueChanged;
            Object obj = m_Event.GetPersistentTarget(0);
            string propName = m_Event.GetPersistentMethodName(0);
            propName = propName.Split('_')[1];

            //print(propName);
            //print(typeof(NCAC.SceneController).GetProperty(propName));

            try
            {
                bool? res = typeof(NCAC.TrackingController).GetProperty(propName).GetValue(obj) as bool?;
                if (res != null)
                {
                    m_Toggle.isOn = (bool)res;
                }
            }
            catch (System.NullReferenceException)
            {
                print("!!!!!!!!!--------- The first callback from the toggle button is not assinged to a property of a class. check that shit");
            }

        }

    }
}