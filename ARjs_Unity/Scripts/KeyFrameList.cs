using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KeyFrameList
{
    public List<WeldonKeyFrame> frameList;

    public KeyFrameList()
    {
        frameList = new List<WeldonKeyFrame>();
    }
}

[Serializable]
public class WeldonKeyFrame
{
    public float time;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float scalX;
    public float scalY;
    public float scalZ;
    public Transform transform;

    public WeldonKeyFrame(float t, Transform trans)
    {
        time = t;
        posX = trans.localPosition.x;
        posY = trans.localPosition.y;
        posZ = trans.localPosition.z;
        rotX = trans.localEulerAngles.x;
        rotY = trans.localEulerAngles.y;
        rotZ = trans.localEulerAngles.z;
        scalX = trans.localScale.x;
        scalY = trans.localScale.y;
        scalZ = trans.localScale.z;
    }

    public WeldonKeyFrame()
    {
        time = -1;
        posX = 0;
        posY = 0;
        posZ = 0;
        rotX = 0;
        rotY = 0;
        rotZ = 0;
        scalX = 0;
        scalY = 0;
        scalZ = 0;
    }

    public string InformationString()
    {
        return "Time: " + time + " posX: " + posX + " posY: " + posY + " posZ: " + posZ + " rotX: " + rotX + " rotY: " + rotY + " rotZ: " + rotZ + " scalX: " + scalX + " scalY: " + scalY + " scalZ: " + scalZ;
    }

    public bool IsDifferentPosition(WeldonKeyFrame frame)
    {
        if(!posX.Equals(frame.posX) || !posY.Equals(frame.posY) || !posZ.Equals(frame.posZ))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDifferentRotation(WeldonKeyFrame frame)
    {
        if (!rotX.Equals(frame.rotX) || !rotY.Equals(frame.rotY) || !rotZ.Equals(frame.rotZ))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDifferentWidth(WeldonKeyFrame frame)
    {
        if (!scalX.Equals(frame.scalX))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDifferentHeight(WeldonKeyFrame frame)
    {
        if (!scalY.Equals(frame.scalY))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDifferentDepth(WeldonKeyFrame frame)
    {
        if (!scalZ.Equals(frame.scalZ))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
