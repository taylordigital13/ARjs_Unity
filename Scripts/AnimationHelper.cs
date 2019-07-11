using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationHelper : MonoBehaviour
{
    Transform transformToRecord;
    [HideInInspector]
    public float timeOfPosition;
    [HideInInspector]
    public bool loop;
    [HideInInspector]
    public bool onClick;

    float startTime;
    WeldonKeyFrame oFrame;
    KeyFrameList keyList;
    int currentKeyFrameNumber = 0;

    private void Start()
    {
        keyList = new KeyFrameList();
        string filePath = Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + gameObject.name + "_" + gameObject.transform.GetSiblingIndex() + ".txt";
        if (File.Exists(filePath))
        {
            string textFile = File.ReadAllText(filePath);
            keyList = JsonUtility.FromJson<KeyFrameList>(textFile);
        }
        else
        {
            keyList = null;
        }
        startTime = Time.time;
        oFrame = new WeldonKeyFrame(0, transform);
    }

    //This runs on each object when you hit the play button. It mimics the same animation that will get excecuted when you run the HTML in a browser. 
    private void Update()
    {
        float currentTime = Time.time-startTime;
        
        float percent = 0;

        if (keyList != null && currentKeyFrameNumber<keyList.frameList.Count)
        {
            WeldonKeyFrame frame = keyList.frameList[currentKeyFrameNumber];
            float timeToUse = frame.time;
            if (currentKeyFrameNumber>0) timeToUse = frame.time - keyList.frameList[currentKeyFrameNumber - 1].time;

            if (currentTime <= timeToUse && !timeToUse.Equals(0.0f) && !currentTime.Equals(0.0f))
            {

                percent = currentTime / timeToUse;
                //Debug.Log(currentTime + " percent: " + percent + " pos: " + transform.localPosition.x);


                Vector3 newPosition = new Vector3(oFrame.posX + (-oFrame.posX + frame.posX) * percent,
                    oFrame.posY + (-oFrame.posY + frame.posY) * percent,
                    oFrame.posZ + (-oFrame.posZ + frame.posZ) * percent);
                Vector3 newRotation = new Vector3(oFrame.rotX + (-oFrame.rotX + frame.rotX) * percent,
                    oFrame.rotY + (-oFrame.rotY + frame.rotY) * percent,
                    oFrame.rotZ + (-oFrame.rotZ + frame.rotZ) * percent);


                transform.localPosition = newPosition;
                transform.eulerAngles = newRotation;
                transform.localScale = new Vector3(oFrame.scalX + (-oFrame.scalX + frame.scalX) * percent,
                    oFrame.scalY + (-oFrame.scalY + frame.scalY) * percent,
                    oFrame.scalZ + (-oFrame.scalZ + frame.scalZ) * percent);
            }
            else
            {
                oFrame = keyList.frameList[currentKeyFrameNumber];
                currentKeyFrameNumber++;
                startTime = Time.time;
            }
        }
        else if (loop)
        {
            startTime = Time.time;
            currentKeyFrameNumber = 0;
            Debug.Log(currentTime + " percent: " + percent + " pos: " + transform.localPosition.x);
        }

    }

    public void RecordPoint()
    {
        if (keyList==null) keyList = new KeyFrameList();

        transformToRecord = gameObject.transform;
        WeldonKeyFrame frame = new WeldonKeyFrame(timeOfPosition, transformToRecord);
        List<CustomList.MyClass> myClass = gameObject.GetComponent<CustomList>().MyList;
        myClass.Add(new CustomList.MyClass(frame));
        keyList.frameList.Add(frame);
        Debug.Log("Frame Added: " + frame.InformationString());
    }

    public void ExportPoints()
    {
        string filePath = Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/";
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

        string stringToWrite = JsonUtility.ToJson(keyList);
        File.WriteAllText(filePath + gameObject.name + "_" + gameObject.transform.GetSiblingIndex() + ".txt", stringToWrite);
        //fileExported = true;
        keyList = new KeyFrameList();
        AssetDatabase.Refresh();

    }

    public void ClearCurrentPoints()
    {
        List<CustomList.MyClass> myClass = gameObject.GetComponent<CustomList>().MyList;
        myClass.Clear();
        keyList = new KeyFrameList();
    }

    public void KeyListInfo()
    {
        Debug.Log("The KeyList currently has " + keyList.frameList.Count + " key frames.");
    }

}
