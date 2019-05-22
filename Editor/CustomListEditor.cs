using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CustomList))]
public class CustomListEditor : Editor
{

    //enum displayFieldType { DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields }
    //displayFieldType DisplayFieldType;

    CustomList t;
    SerializedObject GetTarget;
    SerializedProperty ThisList;
    int ListSize;
    float recordTime;
    List<bool> toggled = new List<bool>(); // Folded or not
    void OnEnable()
    {
        t = (CustomList)target;
        GetTarget = new SerializedObject(t);
        ThisList = GetTarget.FindProperty("MyList"); // Find the List in our script and create a refrence of it
    }

    public override void OnInspectorGUI()
    {
        //Update our list

        GetTarget.Update();

        //Choose how to display the list<> Example purposes only
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //DisplayFieldType = (displayFieldType)EditorGUILayout.EnumPopup("", DisplayFieldType);

        //Resize our list
        //EditorGUILayout.Space();
        //EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Define the list size with a number");
        ListSize = ThisList.arraySize;
        //ListSize = EditorGUILayout.IntField("List Size", ListSize);

        if (ListSize != ThisList.arraySize)
        {
            while (ListSize > ThisList.arraySize)
            {
                ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
            }
            while (ListSize < ThisList.arraySize)
            {
                ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
            }
        }

        //EditorGUILayout.Space();
        //EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Or");
        //EditorGUILayout.Space();
        //EditorGUILayout.Space();

        //Or add a new item to the List<> with a button
        EditorGUILayout.LabelField("Edit the list of Key Frames for the Animation");

        recordTime = EditorGUILayout.FloatField("FrameTime", recordTime);

        t.gameObject.transform.GetComponent<AnimationHelper>().loop = EditorGUILayout.Toggle("Loop Animation", t.gameObject.transform.GetComponent<AnimationHelper>().loop);
        t.gameObject.transform.GetComponent<AnimationHelper>().onClick = EditorGUILayout.Toggle("On Click", t.gameObject.transform.GetComponent<AnimationHelper>().onClick);

        if (GUILayout.Button("Record Point", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        {
            //MyArray.InsertArrayElementAtIndex(MyArray.arraySize);
            //MyArray.GetArrayElementAtIndex(MyArray.arraySize - 1).intValue = 0;
            t.RecordPoint(recordTime);
        }

        if (GUILayout.Button("Export Animation", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        {
            t.ExportKeyFrames();
        }

        if (GUILayout.Button("Clear List", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        {
            t.ClearPoints();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //Display our list to the inspector window

        for (int i = 0; i < ThisList.arraySize; i++)
        {
            toggled.Add(false); // Initialliazed as false
            SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
            //SerializedProperty MyInt = MyListRef.FindPropertyRelative("AnInt");
            SerializedProperty MyFrame = MyListRef.FindPropertyRelative("frame");
            SerializedProperty FrameTime = MyListRef.FindPropertyRelative("time");
            SerializedProperty FramePosition = MyListRef.FindPropertyRelative("position");
            SerializedProperty FrameRotation = MyListRef.FindPropertyRelative("rotation");
            SerializedProperty FrameScale = MyListRef.FindPropertyRelative("scale");
            //SerializedProperty MyFloat = MyListRef.FindPropertyRelative("AnFloat");
            //SerializedProperty MyVect3 = MyListRef.FindPropertyRelative("AnVector3");
            //SerializedProperty MyGO = MyListRef.FindPropertyRelative("AnGO");
            SerializedProperty MyArray = MyListRef.FindPropertyRelative("AnIntArray");


            WeldonKeyFrame frame = new WeldonKeyFrame();
            frame.time = FrameTime.floatValue;
            frame.posX = FramePosition.vector3Value.x;
            frame.posY = FramePosition.vector3Value.y;
            frame.posZ = FramePosition.vector3Value.z;
            frame.rotX = FrameRotation.vector3Value.x;
            frame.rotY = FrameRotation.vector3Value.y;
            frame.rotZ = FrameRotation.vector3Value.z;
            frame.scalX = FrameScale.vector3Value.x;
            frame.scalY = FrameScale.vector3Value.y;
            frame.scalZ = FrameScale.vector3Value.z;

            CustomList.MyClass classThing = new CustomList.MyClass(frame);
            t.MyList[i] = classThing;


            //t.MyList[i].time = FrameTime.floatValue;
            //t.MyList[i].position = FramePosition.vector3Value;
            //t.MyList[i].rotation = FrameRotation.vector3Value;
            //t.MyList[i].scale = FrameScale.vector3Value;

            toggled[i] = EditorGUILayout.Foldout(toggled[i], "[i=" + i +", t=" + (t.MyList.Count>i ? t.MyList[i].frame.time + "" : "?") + "]"); // Index Foldout
            // Display the property fields in two ways.
            if (toggled[i] == true)
            {


                if (true)
                {// Choose to display automatic or custom field types. This is only for example to help display automatic and custom fields.
                 //1. Automatic, No customization <-- Choose me I'm automatic and easy to setup
                    EditorGUILayout.LabelField("Automatic Field By Property Type");
                    //EditorGUILayout.PropertyField(MyGO);
                    EditorGUILayout.PropertyField(FrameTime);
                    EditorGUILayout.PropertyField(FramePosition);
                    EditorGUILayout.PropertyField(FrameRotation);
                    EditorGUILayout.PropertyField(FrameScale);
                    //EditorGUILayout.PropertyField(MyInt);
                    //EditorGUILayout.PropertyField(MyFloat);
                    //EditorGUILayout.PropertyField(MyVect3);

                    // Array fields with remove at index
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Array Fields");

                    if (GUILayout.Button("Update Transform", GUILayout.MaxWidth(150), GUILayout.MaxHeight(20)))
                    {
                        WeldonKeyFrame frame2 = new WeldonKeyFrame();
                        frame2.time = FrameTime.floatValue;
                        frame2.posX = t.transform.localPosition.x;
                        frame2.posY = t.transform.localPosition.y;
                        frame2.posZ = t.transform.localPosition.z;
                        frame2.rotX = t.transform.localEulerAngles.x;
                        frame2.rotY = t.transform.localEulerAngles.y;
                        frame2.rotZ = t.transform.localEulerAngles.z;
                        frame2.scalX = t.transform.localScale.x;
                        frame2.scalY = t.transform.localScale.y;
                        frame2.scalZ = t.transform.localScale.z;

                        CustomList.MyClass classThing2 = new CustomList.MyClass(frame2);
                        t.MyList[i] = classThing2;
                    }

                    if (GUILayout.Button("Set Transform from Frame", GUILayout.MaxWidth(250), GUILayout.MaxHeight(20)))
                    {
                        WeldonKeyFrame frame2 = new WeldonKeyFrame();
                        frame2.time = FrameTime.floatValue;
                        t.transform.localPosition = t.MyList[i].position;
                        t.transform.localEulerAngles = t.MyList[i].rotation;
                        t.transform.localScale = t.MyList[i].scale;
                    }


                    for (int a = 0; a < MyArray.arraySize; a++)
                    {
                        EditorGUILayout.PropertyField(MyArray.GetArrayElementAtIndex(a));
                        if (GUILayout.Button("Remove  (" + a.ToString() + ")", GUILayout.MaxWidth(100), GUILayout.MaxHeight(15)))
                        {
                            MyArray.DeleteArrayElementAtIndex(a);
                        }
                    }
                }
                else
                {
                    //Or

                    //2 : Full custom GUI Layout <-- Choose me I can be fully customized with GUI options.
                    EditorGUILayout.LabelField("Customizable Field With GUI");
                    FrameTime.floatValue = EditorGUILayout.FloatField("KeyFrame Time", FrameTime.floatValue);
                    FramePosition.vector3Value = EditorGUILayout.Vector3Field("Position", FramePosition.vector3Value);
                    FrameRotation.vector3Value = EditorGUILayout.Vector3Field("Rotation", FrameRotation.vector3Value);
                    FrameScale.vector3Value = EditorGUILayout.Vector3Field("Scale", FrameScale.vector3Value);
                    //MyGO.objectReferenceValue = EditorGUILayout.ObjectField("My Custom Go", MyGO.objectReferenceValue, typeof(GameObject), true);
                    //MyInt.intValue = EditorGUILayout.IntField("My Custom Int", MyInt.intValue);
                    //MyFloat.floatValue = EditorGUILayout.FloatField("My Custom Float", MyFloat.floatValue);
                    //MyVect3.vector3Value = EditorGUILayout.Vector3Field("My Custom Vector 3", MyVect3.vector3Value);


                    // Array fields with remove at index
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Array Fields");

                    if (GUILayout.Button("Add New Index", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
                    {
                        MyArray.InsertArrayElementAtIndex(MyArray.arraySize);
                        MyArray.GetArrayElementAtIndex(MyArray.arraySize - 1).intValue = 0;
                    }

                    for (int a = 0; a < MyArray.arraySize; a++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("My Custom Int (" + a.ToString() + ")", GUILayout.MaxWidth(120));
                        MyArray.GetArrayElementAtIndex(a).intValue = EditorGUILayout.IntField("", MyArray.GetArrayElementAtIndex(a).intValue, GUILayout.MaxWidth(100));
                        if (GUILayout.Button("-", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
                        {
                            MyArray.DeleteArrayElementAtIndex(a);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space();

                //Remove this index from the List
                EditorGUILayout.LabelField("Remove an index from the List<> with a button");
                if (GUILayout.Button("Remove This Index (" + i.ToString() + ")"))
                {
                    ThisList.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }
        t.SortByTime();
        //Apply the changes to our list
        GetTarget.ApplyModifiedProperties();
    }
}