using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCAC
{
    public class AugmentedImageController : MonoBehaviour
    {
        public static AugmentedImageController Instance { get; private set; }
        private Dictionary< int, List<Transform> > m_targetDict;

        private void Awake()
        {
            #region Singleton Stuff
            if (AugmentedImageController.Instance == null) AugmentedImageController.Instance = this;
            if (AugmentedImageController.Instance != this) Destroy(this);
            #endregion
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RegisterARImage(NcAugmentedImageInfo item)
        {
           
        }
    }
}
