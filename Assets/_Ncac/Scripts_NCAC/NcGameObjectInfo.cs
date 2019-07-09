using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class saves the initial global position/rotation/scale of the model
/// </summary>
namespace NCAC {

    public class NcGameObjectInfo : MonoBehaviour
    {
        // Initial transform parameters in global space
        public NcTransform OriginalTransformData;

        // Start is called before the first frame update

        private void Start()
        {  
            OriginalTransformData = new NcTransform(transform);        
        }

    }
}
