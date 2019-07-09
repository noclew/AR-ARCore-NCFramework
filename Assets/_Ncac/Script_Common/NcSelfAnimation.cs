using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NcSelfAnimation : MonoBehaviour
{
    public float PointerHeightInMeter = 0.2f;
    public float RotationSpeed = 10f;
    public bool RatateContinuous;

    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).localScale = new Vector3(PointerHeightInMeter, PointerHeightInMeter, PointerHeightInMeter);
        if (RatateContinuous)
        {          
            StartCoroutine(rotateCont());
        }
    }

    private void OnValidate()
    {
        transform.GetChild(0).localScale = new Vector3(PointerHeightInMeter, PointerHeightInMeter, PointerHeightInMeter);
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator rotateCont()
    {
        while (true)
        {
            Transform childTransform = transform.GetChild(0);
            childTransform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
          
            yield return null;
        }
    }
}