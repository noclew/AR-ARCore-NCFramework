using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace NCToolset
{
    /// <summary>
    /// This class extracts the project information from "SceneController.Instance", and assign is into the two texts. 
    /// Note that it *needs* a SceneController object, and two text feilds, which accomodate the name and value of a variable in the SceneController object.
    /// </summary>
    public class NcInfoGetter : MonoBehaviour
    {
        public string VarNameInSceneController;

        // Start is called before the first frame update
        void Start()
        {
            Text[] texts = GetComponentsInChildren<Text>();
            if (texts.Length != 2 || VarNameInSceneController == null || VarNameInSceneController == "") return;

            if (NCAC.TrackingController.Instance && NCAC.TrackingController.Instance.GetComponent<NcProjectProperty>())
            {
                NcProjectProperty prop = NCAC.TrackingController.Instance.GetComponent<NcProjectProperty>();
                var appVersion = typeof(NcProjectProperty).GetProperty(VarNameInSceneController).GetValue(prop) as string;
                //GetComponent<UnityEngine.UI.Text>().text = appVersion;

                texts[0].text = NcHelpers.ConvertCamelString(VarNameInSceneController);
                texts[1].text = appVersion;
            }



        }

        // Update is called once per frame
        
    }
}
