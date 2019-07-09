using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NCToolset
{
    /// <summary>
    /// NcMultipleTabCounter class logs the touch counts via checkCounter() function. 
    /// When it reaches the maximum tab count, it calls the event handlers using UnityEvent.
    /// </summary>

    public class NcMultipleTabCounter : MonoBehaviour
    {
        public int secretTabCount = 5;
        public float timeThreshold = 0.5f;

        int currentTabCount = 1;
        float previousClickedtime;
        public float m_timeThreshold { get; private set; }

        [Header("things to Run")]
        public UnityEvent m_MyEvent;


        // Start is called before the first frame update
        void Start()
        {
            m_timeThreshold = timeThreshold;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void checkCounter()
        {
            if (Time.time - previousClickedtime < m_timeThreshold)
            {

                currentTabCount++;
            }

            else
            {
                currentTabCount = 1;
            }

            if (secretTabCount <= currentTabCount && m_MyEvent != null)
            {
                m_MyEvent.Invoke();
            }

            previousClickedtime = Time.time;
        }

    }
}
