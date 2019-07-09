﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCToolset;
using UnityEditor;
using System.Linq;

namespace NCAC
{
    using GoogleARCore;
    using A_DetectedPlaneVisualizer = GoogleARCore.Examples.Common.DetectedPlaneVisualizer;

    /// <summary>
    /// SceneController for Noclew Ar Core extension
    /// </summary>
    /// <remarks>
    /// transferred and edited from the google sample files. For more detail,
    /// refer to the Slither AR core intro by Codelab and the default ARcore examples.
    /// </remarks>

    [RequireComponent(typeof(NcDebugScreen))]
    [RequireComponent(typeof(GazeContoller))]
    public class TrackingController : MonoBehaviour
    {
        #region private class members
        // flag to set the quitting state
        private bool isQuitting = false;

        // GUI style for debugging mode
        private GUIStyle mdebugTextStyle = new GUIStyle();

        // List of detectedPlaneVisualizer Instances
        private List<GoogleARCore.Examples.Common.DetectedPlaneVisualizer> m_PlaneVisList;

        //forTesting
        private List<DetectedPlane> m_AllDetectedPlane = new List<DetectedPlane>();
        private List<DetectedPlane> m_newPlanes = new List<DetectedPlane>();
        #endregion

        // List of augmented images with its index
        private Dictionary<int, NcAugmentedImageInfo> m_augementedImageDict = new Dictionary<int, NcAugmentedImageInfo>();

        #region public class members

        [Header("Image Realignment Setting")]
        // settings for initialization
        public bool enableImageRealignment = false;
        public float colinearPlaneThreshold = 0.1f;

        [Header("Visualization Setting")]
        public bool enableAnchorVisualizer = true;
        public bool enablePlaneVisualizer = true;
        public bool enableGazeVisualizer = true;

        [Header("Debug Screen")]
        public bool enableDebugScreen = true;

        [Header("ArCore default members")]
        // Detected Plane Visualizer
        public GameObject DetectedPlanePrefab;
        //device dim out threshold
        public int TimeoutAfterTrackingIsLost = 30;

        [Header("Visualization Component")]
        //anchor visualizer
        public Transform m_AnchorVisualizer;

        [Header("Ar Contents")]
        // hide basket
        public Transform m_HidingBasket;
        //building model
        public Transform m_ModelContents;

        //a class member to store the main plane and the main anchor associated to it
        public NcacDetectedPlaneTracker PlaneTracker { get; private set; }
        // AppState enum to track the app states


        #endregion

        #region class properties
        // Flags
        // debuging flag
        public bool m_isDebugScreenEnabled { get; set; }

        // Auto initialization Flag
        public bool m_isImgRealignEnabled { get; set; }

        // indicates if the app is in manual initialization mode
        public bool m_isInManualInitMode { get; set; }

        // indicates if plane visualizer is enabled
        public bool m_isPlaneVisEnabled { get; set; }

        // indicates if anchor visualizer is enabled
        public bool m_isAnchorVisEnabled { get; set; }


        // indicates gaze vis
        public bool m_isGazeVisEnabled
        {
            get { return GazeContoller.Instance.IsGazeVisualizerEnabled; }
            set { GazeContoller.Instance.IsGazeVisualizerEnabled = value; }
        }
        //end flags

        //ref to the AR cam
        public Camera m_ARCam;

        //in update, this list get the detected image list
        private List<AugmentedImage> m_updatedImageTarget = new List<AugmentedImage>();
        private float m_distPlaneToImage = -1;
        #endregion

        public static TrackingController Instance { get; private set; }
        private void Awake()
        {
            #region Singleton Stuff
            if (TrackingController.Instance == null) TrackingController.Instance = this;
            if (TrackingController.Instance != this) Destroy(this);
            #endregion

            //class param setting
            // set the ar cam member


            m_isDebugScreenEnabled = enableDebugScreen;
            m_isImgRealignEnabled = enableImageRealignment;
            m_isPlaneVisEnabled = enablePlaneVisualizer;
            m_isAnchorVisEnabled = enableAnchorVisualizer;

            m_isInManualInitMode = false;
            //IsGazeVisualizerEnabled = false;

            if (m_ARCam == null) m_ARCam = Camera.main;

            ;            // initiate plane tracker class
            PlaneTracker = new NcacDetectedPlaneTracker();
            m_PlaneVisList = new List<GoogleARCore.Examples.Common.DetectedPlaneVisualizer>();
        }


        // Start is called before the first frame update
        void Start()
        {

            m_isGazeVisEnabled = enableGazeVisualizer;
            // check on startup that this device is ARcore-compatible
            QuitOnConnectionErrors();
        }

        // Update is called once per frame
        void Update()
        {
            m_distPlaneToImage = 99999;

            if (Input.GetKeyDown("space"))
            {
                print("---------------------ss" + PlaneTracker.m_Anchor.name);
                //m_ModelContents.SetParent(m_HidingBasket);
            }
            /////////////////////////////////////////
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }


            #region check on tracking status
            //if the session is not tracking
            if (Session.Status != SessionStatus.Tracking)
            {
                //Hide the building Model
                //NcHelpers.HideGameObject(bldgModel);

                //allow a device to dim out in 15 sec and siege the update
                Screen.sleepTimeout = TimeoutAfterTrackingIsLost;
                return;
            }
            NcHelpers.HideObject(this);
            //set the screen not to dim out if the device is in tracking
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            #endregion


            //if the main plane is set, and it was subsumed by another, update the main plain
            if (PlaneTracker.m_refPlane != null)
            {
                while (PlaneTracker.m_refPlane.SubsumedBy != null) SetReferencePlane(PlaneTracker.m_refPlane.SubsumedBy);
            }

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            try
            {
                Session.GetTrackables<DetectedPlane>(m_newPlanes, TrackableQueryFilter.New);
                Session.GetTrackables<DetectedPlane>(m_AllDetectedPlane, TrackableQueryFilter.All);
            }
            catch (System.NullReferenceException e)
            {
                Debug.Log(" plane error ");
            }

            // visualize Newerly detected planes 
            // This replaces the original "detected plane generator" component.
            for (int i = 0; i < m_newPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate(DetectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
                A_DetectedPlaneVisualizer visualizer = planeObject.GetComponent<A_DetectedPlaneVisualizer>();
                visualizer.Initialize(m_newPlanes[i]);

                m_PlaneVisList.Add(visualizer);
            }

            #region AR Core Augmented Image Routine
            //get all the detected images in to the list
            try
            {
                Session.GetTrackables<AugmentedImage>(m_updatedImageTarget, TrackableQueryFilter.Updated);
            }
            catch (System.NullReferenceException e)
            {
                Debug.Log(" image error ");
            }

            AugmentedImage detectedFullImage = null;

            //flag for image realignemnt
            bool wasRealingedwithImage = false;
            // If image realignment is enabled and updated images are more than one
            if (m_isImgRealignEnabled && m_updatedImageTarget.Count != 0)
            {
                // get the first fully-tracked image in the current frame
                detectedFullImage = GetFullTrackingImage();

                if (detectedFullImage != null)
                {
                    wasRealingedwithImage = TryUpdatingPlaneAndAnchorOnImage(detectedFullImage, m_AllDetectedPlane);
                }

            }

            #endregion
            // Update the model position
            if (wasRealingedwithImage == false)
            {
                PlaceModelOnAnchor(m_ModelContents, PlaneTracker.m_Anchor);
            }

            else
            {
                PlaceModelOnAnchor(m_ModelContents, PlaneTracker.m_Anchor, detectedFullImage);
            }

            // Visualize the main anchor
            VisualizeAnchor();
        }

        private bool TryUpdatingPlaneAndAnchorOnImage(AugmentedImage image, List<DetectedPlane> planeList)
        {
            DetectedPlane planeCoplanar;
            //Find coplanar Plane from a detected plane list
            if ((planeCoplanar = FindCoplanarPlaneFromImagePose(image, planeList)) != null)
            {
                ////calc Dist to the plane from the image pose
                print(">>>>>>>> dist from image to plane :: " + CalcDistPointToPlane(image.CenterPose.position, planeCoplanar.CenterPose.position, planeCoplanar.CenterPose.up));
                PlaneTracker.SetReferncePlane(planeCoplanar);

                //colorTest
                //foreach (A_DetectedPlaneVisualizer vis in m_PlaneVisList)
                //{
                //    vis.
                //}

                if (PlaneTracker.m_Anchor != null)
                {
                    ///////////////////////////////////////////
                    m_ModelContents.SetParent(m_HidingBasket);
                    // By default, GoogleARCore::Anchor.sc deactivate all its child when its "TrackingState" status is not TrackingState.Tracking
                    // So, we have to activate models, which was nested in the current main achor.
                    m_ModelContents.gameObject.SetActive(true);
                }

                // Update the main anchor to be placed on the center of the image
                PlaneTracker.UpdateMainAnchor(image.CenterPose);
                return true;
            }
            return false;
        }

        DetectedPlane FindCoplanarPlaneFromImagePose(AugmentedImage image, List<DetectedPlane> planeList)
        {
            Dictionary<float, DetectedPlane> copDict = new Dictionary<float, DetectedPlane>();

            Pose imagePose = image.CenterPose;
            if (m_AllDetectedPlane.Count == 0) return null;
            foreach (DetectedPlane plane in planeList)
            {
                float res = Vector3.Dot(plane.CenterPose.up, imagePose.up);
                if (1f - colinearPlaneThreshold < res && res < 1f + colinearPlaneThreshold)
                {
                    float dist = CalcDistPointToPlane(image.CenterPose.position, plane.CenterPose.position, plane.CenterPose.up);

                    if (!copDict.ContainsKey(dist)) copDict.Add(dist, plane);
                }
            }

            if (copDict.Count != 0)
            {
                var list = copDict.Keys.ToList();
                list.Sort();
                m_distPlaneToImage = list[0];
                return copDict[list[0]];
            }

            return null;
        }

        float CalcDistPointToPlane(Vector3 evalPoint, Vector3 planeOrgin, Vector3 planeNormal)
        {
            Vector3 v = evalPoint - planeOrgin;
            float dist = Vector3.Dot(v, planeNormal.normalized);
            return Mathf.Abs(dist);
        }

        private void LateUpdate()
        {
            //update plane visualizers i.a.w. visualization flag
            foreach (A_DetectedPlaneVisualizer vis in m_PlaneVisList)
            {
                if (vis != null && !m_isPlaneVisEnabled)
                {
                    vis.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        /// <summary>
        /// routine to process detected an augmented image
        /// </summary>
        private AugmentedImage GetFullTrackingImage()
        {
            //Debug.Log("image processing started");
            foreach (var image in m_updatedImageTarget)
            {
                if (image.TrackingState == TrackingState.Tracking && image.TrackingMethod == AugmentedImageTrackingMethod.FullTracking)
                {
                    string imageMsg = image.Name + " no index no " + image.DatabaseIndex + " is in " + image.TrackingState + " with the method:" + image.TrackingMethod;
                    Debug.Log(imageMsg);
                    return image;
                }
            }

            return null;
        }




        void VisualizeAnchor()
        {
            if (m_isAnchorVisEnabled && m_AnchorVisualizer != null && PlaneTracker.m_Anchor != null)
            {
                NcHelpers.ShowObject(m_AnchorVisualizer);
                m_AnchorVisualizer.transform.position = PlaneTracker.m_Anchor.transform.position;
                m_AnchorVisualizer.transform.rotation = PlaneTracker.m_Anchor.transform.rotation;
            }

            else
            {
                NcHelpers.HideObject(m_AnchorVisualizer);
            }

        }

        /// <summary>
        /// Place a game object on a anchor
        /// </summary>
        /// <param name="modelTransfom">transform of a game object</param>
        /// <param name="anchor">an anchor to which an object is moving</param>

        public void PlaceModelOnAnchor(Transform modelTransfom, Anchor anchor, AugmentedImage image = null)
        {
            if ((PlaneTracker.m_refPlane != null && PlaneTracker.m_Anchor != null) && m_ModelContents.parent != PlaneTracker.m_Anchor)
            {
                if (image == null)
                {
                    //modelTransfom.position = anchor.transform.position;
                    //modelTransfom.rotation = anchor.transform.rotation;
                    //modelTransfom.SetParent(anchor.transform);
                }

                else
                {


                    NcTransform model_trData = modelTransfom.GetComponent<NcGameObjectInfo>().OriginalTransformData;
                    NcTransform target_trData = null;

                    // check if the found image info is in the augmented image dictionary
                    NcAugmentedImageInfo imageInfo = null;
                    if (m_augementedImageDict.TryGetValue(image.DatabaseIndex, out imageInfo))
                    {
                        target_trData = imageInfo.OriginalTransformData;
                    }
                    else
                    {
                        showDebugToast("ERR!!>> there is no Image Info object for the augmented image no:: " + image.DatabaseIndex +
                            ". \nCheck if all the target visualizing info objects are properly created with right indices.");
                        return;
                    }


                    //Debug.Log(" >>> setting models on image no " + m_augementedImageDict[image.DatabaseIndex].m_augmentedImageIndex + " :: " + image.Name);
                    //Debug.Log(" >>> anchor rot :: in euler" + anchor.transform.rotation.ToString("F2"));

                    // Here, the moved target is an anchor, so we make a moved transfrom from the anchor. this is equivalent to 
                    // NcTransform movedTarget_trData = new NcTransform(PlaneTracker.m_Anchor.transform.position, PlaneTracker.m_Anchor.transform.rotation, PlaneTracker.m_Anchor.transform.lossyScale);
                    // movedTarget_trData.localScale = Vector3.one;
                    // Note that localscale is needed to cacluate the initial local position
                    NcTransform movedTarget_trData = new NcTransform(PlaneTracker.m_Anchor.transform);

                    // calculate the new global transfrom data. 
                    NcTransform newGlobalTransfrom = GetNewGlobalTransformData(model_trData, target_trData, movedTarget_trData);


                    // change the transfrom of the contents
                    modelTransfom.position = newGlobalTransfrom.position;
                    modelTransfom.rotation = newGlobalTransfrom.rotation;
                    modelTransfom.SetParent(anchor.transform);
                }
            }
        }


        #region MOVE_MODELS_ON_TARGETS


        //this function translate the global position of a model to a local position in the target space. Technically equivalent to InverseTransfromPoint.
        static public Vector3 TranslateGlobalPosToLocal(NcTransform model, NcTransform target)
        {
            //this part is inverse of local to global (inverse function of TransformPoint)
            Quaternion target_rot = target.rotation;
            Vector3 target_scale = target.lossyScale;
            Vector3 target_pos = target.position;
            Vector3 model_pos = model.position;

            var diference = (model_pos - target_pos);
            var FinalPos = Quaternion.Inverse(target_rot) * new Vector3(diference.x / target_scale.x, diference.y / target_scale.y, diference.z / target_scale.z);

            return FinalPos;
        }

        //based on the function above, this function cacluates parameteres of a new global transfrom from a moved target
        static public NcTransform GetNewGlobalTransformData(NcTransform trModel, NcTransform trTarget, NcTransform trMovedTarget)
        {
            ////// position calculation
            //the local positions of the model in both original and moved target transfrom are the same. 
            Vector3 localPos_in_originalTarget = TranslateGlobalPosToLocal(trModel, trTarget);
            //print("translatedLocal : " + localPos_in_originalTarget.ToString("F6"));

            // now we translate  the calculated local position in the moved target space to global
            // This is equivalent to " trMovedTarget.transform.TransformPoint ( localPosInTarget )"
            // ?? operator is needed because local scale of the moved target may not have a local scale if the NcTransform instance was not initiated from Transfrom class. 
            // However, it will have a value because the we will make the instance from a transform of the moved target.
            Vector3 newGlobalPos = trMovedTarget.rotation * Vector3.Scale(localPos_in_originalTarget, trMovedTarget.localScale ?? default) + trMovedTarget.position;
            //print("newGlobalPos : " + newGlobalPos.ToString("F6"));


            ////// rotation calculation
            //this translates the global rotation of the model to the local rotation in the target space.
            Quaternion localRot_in_originalTarget = Quaternion.Inverse(trTarget.rotation) * trModel.rotation;

            //this translates the local rotation of model in the *MOVED* target space to a global rotation.
            Quaternion newGlobalRot = trMovedTarget.rotation * localRot_in_originalTarget;

            ////// scale calculation
            //This calcualtes the local scale of the model in the original target space
            Vector3 newLocalScale = new Vector3(trTarget.lossyScale.x / trModel.lossyScale.x, trTarget.lossyScale.y / trModel.lossyScale.y, trTarget.lossyScale.z / trModel.lossyScale.z);
            //this caclulates the global scale of the model in the moved target space
            Vector3 newGlobalScale = new Vector3(trMovedTarget.lossyScale.x / newLocalScale.x, trMovedTarget.lossyScale.y / newLocalScale.y, trMovedTarget.lossyScale.z / newLocalScale.z);

            return new NcTransform(newGlobalPos, newGlobalRot, newGlobalScale);
        }

        #endregion //functions to move models on anchors

        public void RegesterAugmentedImageInfo(NcAugmentedImageInfo item)
        {
            // check if augmented image infos are duplicated
            NcAugmentedImageInfo imageInfo;
            if (m_augementedImageDict.TryGetValue(item.m_augmentedImageIndex, out imageInfo))
            {
                throw new System.InvalidOperationException("ERR!>> the info of augmented image no:: " + item.m_augmentedImageIndex + " is already in the info dictionary. " +
                   "\n This image info will not be added to the info dictionary");
            }

            m_augementedImageDict.Add(item.m_augmentedImageIndex, item);
        }
        /// <summary>
        /// Quit the application if there is a connection error in the ARCore session.
        /// </summary>
        void QuitOnConnectionErrors()
        {
            //if the app is quiting, skip the check
            if (isQuitting) return;

            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                isQuitting = true;
                string msg = "Allow Camera Permission to run this application";
                StartCoroutine(AndyHelper.ToastAndQuit(msg, 5));
            }

            else if (Session.Status == SessionStatus.FatalError)
            {
                string msg = "Something is fatally wrong. Please restart the app";
                isQuitting = true;
                StartCoroutine(AndyHelper.ToastAndQuit(msg, 5));
            }
        }

        ///////////////////////////////////////////////////// hinders to start from ini because of the checking routine
        public bool SetReferencePlane(DetectedPlane plane)
        {
            if (m_isInManualInitMode || m_isImgRealignEnabled)
            {
                return PlaneTracker.SetReferncePlane(plane);
            }
            else
            {
                /////////////////////////////////////change the touchlistnerAR onTouch function. now it directly sends. 
                //AndyHelper.ShowToastMsg("you are not in init mode");
                return false;
            }
        }


        public void CreateAnchorAtGaze()
        {
            bool res = false;
            if (GazeContoller.Instance.mTrackableHit.Trackable != null)
            {
                res = PlaneTracker.UpdateMainAnchor(GazeContoller.Instance.mTrackableHit.Pose);
            }

            // If anchor update was successful
            if (res)
            {
                m_ModelContents.gameObject.SetActive(true);
                m_ModelContents.SetParent(PlaneTracker.m_Anchor.transform);

                string msg = "The main anchor has been updated";
                AndyHelper.ShowToastMsg(msg);
                Debug.Log(msg);
            }

            //if anchor update was not suceessful
            else
            {
                ///////////////////////////////////////////////////////////////////////////////////////////////////
                // Here. the order matters 
                /////////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////////////////

                string msg = "The main anchor at gaze has not been updated";
                AndyHelper.ShowToastMsg(msg);
                Debug.Log(msg);
            }

        }


        public void SetDetectedPlaneAtGaze()
        {
            if (GazeContoller.Instance.m_PlaneOfInterest == null)
            {
                return;
            }

            if (SetReferencePlane(GazeContoller.Instance.m_PlaneOfInterest))
            {
                string msg = string.Format("Main Plain at Gaze is set." + "\n{0} plane centered at {1}", GazeContoller.Instance.m_PlaneOfInterest.PlaneType, GazeContoller.Instance.m_PlaneOfInterest.CenterPose.position);
                Debug.Log(msg);
                AndyHelper.ShowToastMsg(msg);
            }
            else
            {
                string msg = string.Format("There is no detected plane at your gaze." + "\n Please try again.");
                Debug.Log(msg);
                AndyHelper.ShowToastMsg(msg);
            }
        }

        public void showDebugToast(string msg)
        {
            if (!m_isDebugScreenEnabled) return;
            AndyHelper.ShowToastMsg(msg);
        }

        public void OnGUI()
        {
            if (!m_isDebugScreenEnabled) return;

            _ShowDebugMsg("Auto Init : " + m_isImgRealignEnabled.ToString());
            _ShowDebugMsg("Plane Vis : " + m_isPlaneVisEnabled.ToString());
            _ShowDebugMsg("Anchor Vis : " + m_isAnchorVisEnabled.ToString());
            _ShowDebugMsg("Manual Init : " + m_isInManualInitMode.ToString());

            NcDebugScreen.Instance.ChangeTextColor(Color.green);
            _ShowDebugMsg(string.Format("contents active: {0}", m_ModelContents.gameObject.activeInHierarchy));
            _ShowDebugMsg(string.Format("contents renderer: {0}", m_ModelContents.GetComponentInChildren<Renderer>().enabled));
            _ShowDebugMsg(string.Format("contents parents: {0}", m_ModelContents.parent.name));

            NcDebugScreen.Instance.ChangeTextColor(Color.magenta);
            if ((m_ModelContents.parent.GetComponent<Anchor>()) != null)
            {
                _ShowDebugMsg(string.Format("contents anchor: {0}", m_ModelContents.parent.GetComponent<Anchor>().GetHashCode()));
            }


            NcDebugScreen.Instance.ChangeTextColor(Color.red);
            _ShowDebugMsg(string.Format("Session Status: {0} ", Session.Status));
            _ShowDebugMsg(string.Format("{0} Planes are detected in all", m_AllDetectedPlane.Count));
            _ShowDebugMsg(string.Format("{0} new Planes were found", m_newPlanes.Count));
            _ShowDebugMsg(string.Format("{0} PlaneVisualizer were saved", m_PlaneVisList.Count));

            int count = 0;
            for (int i = 0; i < m_PlaneVisList.Count; i++)
            {
                if (m_PlaneVisList.ElementAt(i))
                {
                    count++;
                }
            }
            _ShowDebugMsg(string.Format("{0} PlaneVisualizers actaully exists", count));

            NcDebugScreen.Instance.ChangeTextColor(Color.blue);
            if (PlaneTracker.m_refPlane != null)
            {
                _ShowDebugMsg(string.Format("Mplane ID: {0} as {1} with {2} dist", PlaneTracker.m_refPlane.GetHashCode(), PlaneTracker.m_refPlane.TrackingState, m_distPlaneToImage), Color.blue);
                _ShowDebugMsg(string.Format("Mplane type: {0}", PlaneTracker.m_refPlane.PlaneType), Color.blue);
                _ShowDebugMsg(string.Format("Mplane pos at: {0}", PlaneTracker.m_refPlane.CenterPose), Color.blue);
                _ShowDebugMsg(string.Format("Mplane State: {0}", PlaneTracker.m_refPlane.TrackingState), Color.blue);
            }

            NcDebugScreen.Instance.ChangeTextColor(Color.magenta);
            if (PlaneTracker.m_Anchor != null)
            {
                _ShowDebugMsg(string.Format("main anchor ID : {0}", PlaneTracker.m_Anchor.GetHashCode()));
                _ShowDebugMsg(string.Format("main anchor Pos : {0}", PlaneTracker.m_Anchor.transform.position));


            }


            NcDebugScreen.Instance.ChangeTextColor(Color.cyan);
            if (GazeContoller.Instance.mObjOfInterest != null) _ShowDebugMsg(string.Format("Looking At Object : {0}", GazeContoller.Instance.mObjOfInterest.name));
            if (GazeContoller.Instance.m_PlaneOfInterest != null) _ShowDebugMsg(string.Format("Looking At trackable : {0}", GazeContoller.Instance.m_PlaneOfInterest.GetHashCode()));

            NcDebugScreen.Instance.ResetDebugScreen();
        }

        // if the AR trackable is just touched
        public void OnTrackableTouched(TrackableHit hit)
        {
            if ((hit.Trackable is DetectedPlane) &&
                       Vector3.Dot(Camera.main.transform.position - hit.Pose.position,
                           hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                // Choose the Andy model for the Trackable that got hit.
                GameObject prefab;
                if (hit.Trackable is FeaturePoint)
                {
                    // do something with the featurepoint
                }
                else if (hit.Trackable is DetectedPlane)
                {
                    DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                    if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                    {
                        // do something with the vertical
                    }
                    else
                    {
                        // do something with the horizontal
                    }
                }
                else
                {
                    // do or the othertyples
                }

            }

        }

        public void _ShowDebugMsg(string msg, Color? color = null, GUIStyle style = null)
        {
            if (msg != "") NcDebugScreen.Instance.ShowDebugMsg(msg, style, color);
        }

        public void SetDebugGUIStyle()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.5f));
            texture.Apply();

            mdebugTextStyle.normal.background = texture;
            mdebugTextStyle.normal.textColor = Color.red;
            mdebugTextStyle.fontSize = (int)((Screen.height / 45f) * 0.75f);
        }
    }
}