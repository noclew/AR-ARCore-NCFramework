using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NcTransform
{
    public Vector3 position { get; set; }
    public Quaternion rotation { get; set; }
    public Vector3 lossyScale { get; set; }

    public Vector3? localPosition { get; set; }
    public Quaternion? localRotation { get; set; }
    public Vector3? localScale { get; set; }

    public NcTransform(Transform tr)
    {
        position = tr.position;
        rotation = tr.rotation;
        lossyScale = tr.lossyScale;

        localPosition = tr.localPosition;
        localRotation = tr.localRotation;
        localScale = tr.localScale;
    }

    public NcTransform(Vector3 pos_gl, Quaternion rot_gl, Vector3 sc_gl)
    {
        position = pos_gl;
        rotation = rot_gl;
        lossyScale = sc_gl;

        localPosition = null;
        localRotation = null;
        localScale = null;
    }



}
