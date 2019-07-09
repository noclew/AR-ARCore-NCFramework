using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCToolset
{
    public class NcObjectProperty : MonoBehaviour
    {
        public Vector3 oriLocalTransfrom { get; private set; }
        public Vector2 oriLocalScale { get; private set; }
        public Quaternion oriLocalRotation { get; private set; }

        public bool bHideOnStart;
        public bool bDeactiveOnStart;
        public bool enableTFScaling;
        public bool enableOFMoving;
        public float scaleSpeed = 1f;
        public float scaleFactor = 1f;

        // Start is called before the first frame update
        void Start()
        {
            oriLocalTransfrom = transform.localPosition;
            oriLocalRotation = transform.localRotation;
            oriLocalScale = transform.localScale;
            if (bHideOnStart) NcHelpers.HideObject(transform);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}