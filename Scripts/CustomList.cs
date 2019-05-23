//Script name : CustomList.cs
using UnityEngine;
using System;
using System.Collections.Generic; // Import the System.Collections.Generic class to give us access to List<>
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CustomList : MonoBehaviour
{

    //This is our custom class with our variables
    [System.Serializable]
    public class MyClass : IComparable<MyClass>
    {
        public GameObject AnGO;
        public int AnInt;
        public float AnFloat;
        public Vector3 AnVector3;
        public int[] AnIntArray = new int[0];

        public WeldonKeyFrame frame;
        public float time;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public MyClass(WeldonKeyFrame kFrame)
        {
            frame = kFrame;
            time = kFrame.time;
            position = new Vector3(kFrame.posX, kFrame.posY, kFrame.posZ);
            rotation = new Vector3(kFrame.rotX, kFrame.rotY, kFrame.rotZ);
            scale = new Vector3(kFrame.scalX, kFrame.scalY, kFrame.scalZ);
        }

        public int CompareTo(MyClass other)
        {
            if (time > other.time) return 1;
            if (time < other.time) return -1;
            return 0;
        }
    }

    //This is our list we want to use to represent our class as an array.
    public List<MyClass> MyList = new List<MyClass>(0);
    public KeyFrameList keyList = new KeyFrameList();

    public void ExportKeyFrames()
    {
        keyList.frameList = new List<WeldonKeyFrame>();

        foreach(MyClass thing in MyList)
        {
            WeldonKeyFrame frame = new WeldonKeyFrame();
            frame.time = thing.time;
            frame.posX = thing.position.x;
            frame.posY = thing.position.y;
            frame.posZ = thing.position.z;
            frame.rotX = thing.rotation.x;
            frame.rotY = thing.rotation.y;
            frame.rotZ = thing.rotation.z;
            frame.scalX = thing.scale.x;
            frame.scalY = thing.scale.y;
            frame.scalZ = thing.scale.z;
            keyList.frameList.Add(frame);
        }

        string filePath = Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/";
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

        string stringToWrite = JsonUtility.ToJson(keyList);
        File.WriteAllText(filePath + gameObject.name + "_" + gameObject.transform.GetSiblingIndex() + ".txt", stringToWrite);
        //fileExported = true;
        keyList = new KeyFrameList();
        AssetDatabase.Refresh();
    }

    public void RecordPoint(float time)
    {
        WeldonKeyFrame frame = new WeldonKeyFrame(time, transform);
        MyList.Add(new MyClass(frame));

        Debug.Log("Frame Added: " + frame.InformationString());
    }

    public void ClearPoints()
    {
        MyList = new List<MyClass>(0);
    }

    public void SortByTime()
    {
        //MyList.Sort((x, y) => x.time.CompareTo(y.time));
        MyList.Sort();
    }

    void AddNew()
    {
        //Add a new index position to the end of our list
        MyList.Add(new MyClass(new WeldonKeyFrame()));
    }

    void Remove(int index)
    {
        //Remove an index position from our list at a point in our list array
        MyList.RemoveAt(index);
    }
}