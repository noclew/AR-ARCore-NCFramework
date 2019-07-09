using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace NCToolset
{
    public static class NcHelpers
    {
        public static void HideObject<T>(T obj)
        {
            GameObject go = obj as GameObject;
            
            if( typeof(Transform) == obj.GetType() ) { go = (obj as Transform).gameObject; }

            if (go == null) return;
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (Collider c in go.GetComponentsInChildren<Collider>()) c.enabled = false;
        }


        public static void ShowObject<T>(T obj)
        {
            GameObject go = obj as GameObject;

            if (typeof(Transform) == obj.GetType()) { go = (obj as Transform).gameObject; }

            if (go == null) return;
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) r.enabled = true;
            foreach (Collider c in go.GetComponentsInChildren<Collider>()) c.enabled = true;
        }

        /// <summary>
        /// returns the ref to an object touched on the screen
        /// </summary>
        /// <param name="touch"> Touch to detect </param>
        /// <param name="cam"> Camera to use for raycasting </param>
        /// <returns></returns>
        public static GameObject GetTouchedObject(Touch touch, Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            Ray touchRay = cam.ScreenPointToRay(touch.position);
            RaycastHit hit;
            if (Physics.Raycast(touchRay, out hit))
            {
                return hit.transform.gameObject;
            }
            else return null;
        }

        /// <summary>
        /// Converts Camel typed string to words, e.g. mYouGood >> You Good
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ConvertCamelString(string text)
        {
            MatchCollection res = Regex.Matches(text, @"([A-Z][a-z]+)");
            var words = res.Cast<Match>().Select(m => m.Value);
            return string.Join(" ", words);
        }

        #region Helper Methods for scaling
        public static void ResetLocalScale(GameObject go)
        {
            NcObjectProperty goProperty = go.GetComponent<NcObjectProperty>();
            if (goProperty == null || !goProperty.enableTFScaling) return;
            goProperty.transform.localScale = goProperty.oriLocalScale;

        }
        /// <summary>
        /// Adjusts the local scale of a game object. The target object must have NcObjectProperty component. 
        /// </summary>
        /// <param name="go"> Target object </param>
        /// <param name="distDiff"> Scaling factor </param>
        public static void AdjustLocalScale(GameObject go, float distDiff)
        {
            NcObjectProperty goProperty = go.GetComponent<NcObjectProperty>();
            if (goProperty == null || !goProperty.enableTFScaling) return;

            //find the differece in touch distances btw the current and previous touches
            float scaleFactorDiff = distDiff * goProperty.scaleSpeed;
            //since difference is large, we scale scalefactor
            goProperty.scaleFactor = (goProperty.scaleFactor * 100 + scaleFactorDiff) / 100;

            //scaleFactor ranges from 0.1 to 100.
            goProperty.scaleFactor = Mathf.Clamp(goProperty.scaleFactor, 0.1f, 100f);
            goProperty.transform.localScale = goProperty.oriLocalScale * goProperty.scaleFactor;
        }
        #endregion
    }
}