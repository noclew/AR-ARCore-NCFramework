using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCAC
{
    [ RequireComponent( typeof(NcGameObjectInfo) ) ]
    public class NcAugmentedImageInfo : MonoBehaviour
    {
        [Header("AR Target Info")]
        public int m_augmentedImageIndex = -1;
        public float m_width = 0.5f;
        public float m_height = 0.5f;

        [Header("AR Contents List")]
        public Transform m_contentsInWolrdTracking;
        public Transform m_contentsInImageTracking;

        /// <summary>
        /// Initial transform parameters in global space
        /// </summary>
        public NcTransform OriginalTransformData;


        Transform m_ARImageQuad; // a quad visualizaing an AR target
        //public List<Transform> m_linkedObjectList;

        // Start is called before the first frame update
        void Start()
        {
            if (transform.GetChild(0) != null) transform.GetChild(0).gameObject.SetActive(false);
            TrackingController.Instance.RegesterAugmentedImageInfo(this);

            OriginalTransformData = new NcTransform(transform);


        }

        // Update is called once per frame
        void Update()
        {

        }

        void MakeQuad()
        {
            if (transform.GetChild(0) == null)
            {
                m_ARImageQuad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                m_ARImageQuad.transform.Rotate(Vector3.right, 90);
                m_ARImageQuad.transform.parent = this.transform;
                m_ARImageQuad.transform.localPosition = new Vector3(0, 0, 0);
            }
        }
        private void OnValidate()
        {
            if (m_ARImageQuad == null) MakeQuad();
            m_ARImageQuad = transform.GetChild(0);
            m_ARImageQuad.transform.localScale = new Vector3(m_width, m_height, 1);
        }
    }
}