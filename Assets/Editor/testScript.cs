using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class testScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [MenuItem("Example/test")]
    static void doshit()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            //Debug.Log(Selection.gameObjects.Length + ":" +  go.name);
        }

        Transform modeltr = GetModelByNameFromSelection("model");
        Transform targettr = GetModelByNameFromSelection("target");
        Transform movedTargettr = GetModelByNameFromSelection("movedTarget");


        NcTransform model = new NcTransform(modeltr);
        NcTransform target = new NcTransform(targettr);
        NcTransform movedTarget = new NcTransform(movedTargettr);
        NcTransform newGlobalTransform = GetNewGlobalTransformData(model, target, movedTarget);

        modeltr.position = newGlobalTransform.position;
        modeltr.rotation = newGlobalTransform.rotation;
        modeltr.localScale = newGlobalTransform.lossyScale;
        modeltr.SetParent(movedTargettr);

        //GetNewGlobalTransformData2(modeltr, targettr, movedTargettr);
    }

    [MenuItem("Example/test_working")]
    static void doshit2()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            //Debug.Log(Selection.gameObjects.Length + ":" + go.name);
        }

        Transform modeltr = GetModelByNameFromSelection("model");
        Transform targettr = GetModelByNameFromSelection("target");
        Transform movedTargettr = GetModelByNameFromSelection("movedTarget");


        //NcTransform model = new NcTransform(modeltr);
        //NcTransform target = new NcTransform(targettr);
        //NcTransform movedTarget = new NcTransform(movedTargettr);
        //NcTransform newGlobalTransform = GetNewGlobalTransformData(model, target, movedTarget, targettr);

        //modeltr.position = newGlobalTransform.position;
        //modeltr.rotation = newGlobalTransform.rotation;
        //modeltr.SetParent(movedTargettr);

        GetNewGlobalTransformData2(modeltr, targettr, movedTargettr);
    }



   
    //this function translate the global position of a model to a local position in the target space. Technically equivalent to InverseTransfromPoint.
    static public Vector3 CalcInitialLocalPosOfModelToTarget(Transform model, Transform target)
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
    //based on the function above, this function moves the model onto the target setting it as the model's parent
    static public void GetNewGlobalTransformData2(Transform model, Transform target, Transform movedTarget)
    {

        //the local positions of the model in both original and moved target transfrom are the same. 
        Vector3 localPosInMovedTarget = CalcInitialLocalPosOfModelToTarget(model, target);
        print("translatedLocal : " + localPosInMovedTarget.ToString("F6"));

        //this translates the global rotation of the model to the local rotation in the target space.
        Quaternion modelRotInitial = Quaternion.Inverse(target.rotation) * model.rotation;

        //this translates the local rotation of model in the *MOVED* target space to a global rotation.
        Quaternion movedRotation = movedTarget.transform.rotation * modelRotInitial;

        //model.rotation = (model.rotation * model.transform.rotation) * Quaternion.Inverse(model.rotation);

        //model.position = movedTarget.TransformPoint(localPosInMovedTarget);
        //model.rotation = movedRotation;
        print("newGlobalPos : " + movedTarget.TransformPoint(localPosInMovedTarget).ToString("F6"));
        
    }


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
        print("translatedLocal : " + localPos_in_originalTarget.ToString("F6"));

        // now we translate  the calculated local position in the moved target space to global
        // This is equivalent to " trMovedTarget.transform.TransformPoint ( localPosInTarget )"
        // ?? operator is needed because local scale of the moved target may not have a local scale if the NcTransform instance was not initiated from Transfrom class. 
        // However, it will have a value because the we will make the instance from a transform of the moved target.
        Vector3 newGlobalPos = trMovedTarget.rotation * Vector3.Scale(localPos_in_originalTarget, trMovedTarget.localScale ?? default) + trMovedTarget.position;
        print("newGlobalPos : " + newGlobalPos.ToString("F6"));


        ////// rotation calculation
        //this translates the global rotation of the model to the local rotation in the target space.
        Quaternion localRot_in_originalTarget = Quaternion.Inverse(trTarget.rotation) * trModel.rotation;

        //this translates the local rotation of model in the *MOVED* target space to a global rotation.
        Quaternion newGlobalRot = trMovedTarget.rotation * localRot_in_originalTarget;

        ////// scale calculation
        Vector3 newLocalScale = new Vector3(trTarget.lossyScale.x / trModel.lossyScale.x, trTarget.lossyScale.y / trModel.lossyScale.y, trTarget.lossyScale.z / trModel.lossyScale.z);
        Vector3 newGlobalScale = new Vector3(trMovedTarget.lossyScale.x / newLocalScale.x, trMovedTarget.lossyScale.y / newLocalScale.y, trMovedTarget.lossyScale.z / newLocalScale.z);

        return new NcTransform(newGlobalPos, newGlobalRot, newGlobalScale);
    }

    [MenuItem("Example/undo")]
    static public void undoshit()
    {
        Transform ori = GetModelByNameFromSelection("model_original");
        Transform model = GetModelByNameFromSelection("model");

        model.SetParent(null);
        model.localScale = ori.localScale;
        model.position = ori.position;
        model.rotation = ori.rotation;
        
        
    }

    static public Transform GetModelByNameFromSelection(string name)
    {
        foreach(GameObject go in Selection.gameObjects)
        {
            if (go.name == name) return go.transform;
        }
        return null;
    }
}
