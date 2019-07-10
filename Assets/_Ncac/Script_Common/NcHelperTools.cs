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

            if (typeof(Transform) == obj.GetType()) { go = (obj as Transform).gameObject; }

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


        /// <summary>
        /// math functino to average poses
        /// </summary>
        /// <param name="poses"></param>
        /// <returns></returns>
        public static Pose AveragePose(List<Pose> poses)
        {
            Quaternion[] qArray = new Quaternion[poses.Count];
            Vector3[] vArray = new Vector3[poses.Count];

            for (int i = 0; i < poses.Count; i++)
            {
                qArray[i] = poses[i].rotation;
                vArray[i] = poses[i].position;
            }

            return new Pose(AverageVector(vArray), AverageQuaternion(qArray));
        }
        public static Quaternion AverageQuaternion(Quaternion[] qArray)
        {
            Quaternion qAvg = qArray[0];
            float weight;
            for (int i = 1; i < qArray.Length; i++)
            {
                weight = 1.0f / (float)(i + 1);
                qAvg = Quaternion.Slerp(qAvg, qArray[i], weight);
            }
            return qAvg;
        }

        public static Vector3 AverageVector(Vector3[] vArray)
        {
            int addAmount = 0;
            Vector3 addedVector = Vector3.zero;

            foreach (Vector3 singleVector in vArray)
            {
                //Amount of separate rotational values so far
                addAmount++;
                addedVector += singleVector;
            }

            return addedVector / (float)addAmount;
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