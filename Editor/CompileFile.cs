using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CompileFile : MonoBehaviour
{
    [MenuItem("AR.js/Compile Files", true)]
    static bool CompileFileHTMLvalidation() 
    {
        if (GameObject.FindWithTag("ImageTarget") != null)
        {
            return true;
        }
        else return false;
    }


    //Creates a menu item for building out the objects under the ImageTarget in scene as HTML code and saves said code to a file.
    [MenuItem("AR.js/Compile Files", false, 17)]
    static void CompileFileHTML()
    {
        string folderPath = GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().destination;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string fileName = "index.html";

        #region HTML
        string topHTML = @"<!DOCTYPE html>
<!-- include aframe -->
<script src=""../demos/vendor/aframe/build/aframe.min.js""></script>
<!-- include ar.js -->
<script src=""../build/aframe-ar.js""></script>

<!-- to load .ply model -->
<script src=""https://rawgit.com/donmccurdy/aframe-extras/v3.13.1/dist/aframe-extras.loaders.min.js""></script>

";
        string bodyHtml = @"<body style='margin : 0px; overflow: hidden; font-family: Monospace;'>";
        string middleHTML = @"<!-- <a-scene embedded arjs='sourceType: video; sourceUrl:../../data/videos/headtracking.mp4;'> -->
    <a-scene embedded arjs='sourceType: webcam'>
    <a-entity id=""mouseCursor"" cursor=""rayOrigin: mouse"" raycaster=""objects: .intersectable""></a-entity>";
        bool hasVideo = false;
        string buttonHTML = "<div style='position: fixed; top: 10px; width:100%; text-align: center; z-index: 1;'>\n      <button id=\"mutebutton\">\n          Unmute\n      </button>\n  </div>";
        string patternName = GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().patternName;
        string presetText = $"preset=\"hiro\"";
        if (patternName != "default") presetText = $"type=\"pattern\" preset=\"custom\" src=\"{patternName}\" url=\"{patternName}\" emitevents=\"true\" button";
        string markerHTML = "<a-marker id=\"marker\" " + presetText + ">";
//        string bottomHTML = @"  <a-marker-camera " + presetText + @"></a-marker-camera>
//        </a-scene>
//</body>
//</html>";
        string bottomHTML = @"</a-marker>
        <a-entity camera></a-entity>
        </a-scene>
</body>
</html>";
        #endregion

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<!-- BEGIN: Top HTML -->");
        sb.Append(topHTML);
        sb.AppendLine("<!-- END: Top HTML -->");

        Transform imageTarget = GameObject.FindGameObjectWithTag("ImageTarget").transform;
        if(imageTarget == null)
        {
            Debug.Log("AR.js error: There is no Image Target to Compile");
            return;
        }

        //Adds in the actions of the children to javascript.
        sb.AppendLine("<!-- BEGIN: Unity Compiled Events -->");
        sb.AppendLine("<script>");
        sb.AppendLine("AFRAME.registerComponent('button', {");
        sb.AppendLine("init: function () {");
        sb.AppendLine($"var marker = document.querySelector(\"#marker\");");
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            if (childToAdd.tag == "Video")
            {

                hasVideo = true;
            }
        }
        if (hasVideo) sb.AppendLine($"var button = document.querySelector(\"#mutebutton\");");
            //TODO: Add in consecutive keyframe animations with loopability
            for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;
            if (childToAdd.GetComponent<ButtonHelper>() != null)
            {
                sb.AppendLine($"var {id} = document.querySelector(\"#{id}\");");
            }

            if (childToAdd.tag == "Video")
            {
                hasVideo = true;
                sb.AppendLine($"var {id} = document.querySelector(\"#{childToAdd.name + "_Asset_" + i}\");");
            }

            if (childToAdd.GetComponent<AnimationHelper>() != null)
            {
                string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + id + ".txt");
                KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                foreach (WeldonKeyFrame frame in keyList.frameList)
                {
                    int index = keyList.frameList.FindIndex((obj) => obj == frame);
                    WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                    if (index != 0) prevFrame = keyList.frameList[index - 1];

                    string name = $"{id}_F{index}";
                    sb.AppendLine($"var {name} = document.querySelector(\"#{name}\");");
                }
            }
        }

        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;

            if (childToAdd.GetComponent<ButtonHelper>() != null)
            {
                sb.AppendLine(id + @".addEventListener(""click"", function(evt){");
                sb.AppendLine(@"open(""" + childToAdd.GetComponent<ButtonHelper>().URL + @""");");
                sb.AppendLine("});");
            }

            if (childToAdd.tag == "Video")
            {
                string lineToAppend = "marker.addEventListener(\"markerFound\", function (evt) {\n       " +
                	id + ".play();\n        " +
                	"});\n      " +
                	"marker.addEventListener(\"markerLost\", function (evt) {\n       " +
                	id + ".pause();\n        " +
                	"});";
                string secondLine = "button.addEventListener(\"click\", function(evt){\n" +
                	"console.log(\"button clicked\")\n" +
                	"if(" + id + ".muted==true){\n" +
                	"button.innerHTML=\"Mute\";\n" +
                	id + ".muted=false;\n" +
                	"}else{\n" +
                	"button.innerHTML=\"Unmute\";\n" +
                	id + ".muted=true;\n" +
                	"}\n" +
                	"});";

                sb.AppendLine(lineToAppend);
                sb.AppendLine(secondLine);
            }

            if (childToAdd.GetComponent<AnimationHelper>() != null)
            {
                string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + id + ".txt");
                KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);


                foreach (WeldonKeyFrame frame in keyList.frameList)
                {
                    //TODO: Add in exceptions for the last frame and if the animation is set to repeat.
                    int index = keyList.frameList.FindIndex((obj) => obj == frame);
                    if (index != 0)
                    {

                        WeldonKeyFrame prevFrame = keyList.frameList[index - 1];

                        if (index < keyList.frameList.Count - 1)
                        {
                            string nameListener = $"{id}_F{index - 1}";
                            string nameNext = $"{id}_F{index}";

                            sb.AppendLine(nameListener + @".addEventListener(""animationend"", function(evt){");
                            sb.AppendLine(nameNext + $".emit(\"{nameNext}\")");
                            sb.AppendLine("});");
                        }
                        else if (childToAdd.GetComponent<AnimationHelper>().loop)
                        {

                            string nameListener = $"{id}_F{index - 1}";
                            string nameNext = $"{id}_F{index}";

                            sb.AppendLine(nameListener + @".addEventListener(""animationend"", function(evt){");
                            sb.AppendLine(nameNext + $".emit(\"{nameNext}\")");
                            sb.AppendLine("});");


                            nameListener = $"{id}_F{index}";
                            nameNext = $"{id}_F{1}";

                            sb.AppendLine(nameListener + @".addEventListener(""animationend"", function(evt){");
                            sb.AppendLine(nameNext + $".emit(\"{nameNext}\")");
                            sb.AppendLine("});");
                        }
                        else
                        {
                            string nameListener = $"{id}_F{index - 1}";
                            string nameNext = $"{id}_F{index}";

                            sb.AppendLine(nameListener + @".addEventListener(""animationend"", function(evt){");
                            sb.AppendLine(nameNext + $".emit(\"{nameNext}\")");
                            sb.AppendLine("});");
                        }
                    }

                }


            }
        }

        sb.AppendLine("}");
        sb.AppendLine("});");
        sb.AppendLine("</script>");
        sb.AppendLine("<!-- END: Unity Compiled Events -->");
        sb.AppendLine("");
        //MiddleHTML
        sb.AppendLine("<!-- BEGIN: Middle HTML -->");
        sb.AppendLine(bodyHtml);
        if (hasVideo) sb.AppendLine(buttonHTML);
        sb.AppendLine(middleHTML);
        sb.AppendLine("<!-- END: Middle HTML -->");
        sb.AppendLine("");



        sb.AppendLine("<!-- BEGIN: Unity Compiled Assets -->");
        sb.AppendLine("<a-assets>");
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            if(childToAdd.tag == "Video")
            {
                VideoPlayer player = childToAdd.GetComponentInChildren<VideoPlayer>();
                string location = player.clip.originalPath;
                string[] splitName = location.Split('/');
                string videoName = splitName[splitName.Length - 1];
                string destination = folderPath + "videos/" + videoName;
                if(!Directory.Exists(folderPath + "videos/")) Directory.CreateDirectory(folderPath + "videos/");
                if (File.Exists(destination)) File.Delete(destination);
                File.Copy(location, destination);
                sb.AppendLine($"<video id=\"{childToAdd.name + "_Asset_" + i}\" autoplay=\"false\" loop crossorigin=\"anonymous\" src=\"videos/{videoName}\" webkit-playsinline playsinline controls muted></video>");
            }
        }
        sb.AppendLine("</a-assets>");
        sb.AppendLine("<!-- END: Unity Compiled Assets -->");


        sb.AppendLine("<!-- BEGIN: Add Image Target (marker) -->");
        sb.AppendLine(markerHTML);
        sb.AppendLine("<!-- END: Add Image Target (marker) -->");
        sb.AppendLine("");

        sb.AppendLine("<!-- BEGIN: Unity Compiled Objects -->");
        //Adds in the physical object for each child of the ImageTarget
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            Texture2D objectTexture = (Texture2D)childToAdd.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture;
            string textureName = null;
            bool transparency = false;
            if (objectTexture != null)
            {
                textureName = objectTexture.name;
                byte[] bytes = objectTexture.EncodeToPNG();
                if (!Directory.Exists(folderPath + "textures/")) Directory.CreateDirectory(folderPath + "textures/");
                File.WriteAllBytes(folderPath + "textures/" + textureName + ".png", bytes);
                transparency = true;

            }
            //TODO: Add in animation capability for color as well
            switch (childToAdd.tag)
            {
                case "Plane":
                    var Plane = (width: childToAdd.transform.localScale.x, height: childToAdd.transform.localScale.y,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponentInChildren<MeshRenderer>().sharedMaterial.color),
                                src: textureName != null ? "textures/" + textureName + ".png" : "");
                    sb.AppendLine($"<a-plane src=\"{Plane.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" width=\"{Plane.width}\" height=\"{Plane.height}\" position=\"{Plane.position}\" rotation=\"{Plane.rotation}\" color=\"{Plane.color}\" transparent={transparency}>");

                    string planeID = childToAdd.name + "_" + i;
                    //TODO: Add in consecutive keyframe animations with loopability
                    if (childToAdd.GetComponent<AnimationHelper>() != null)
                    {
                        string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + planeID + ".txt");
                        KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                        foreach (WeldonKeyFrame frame in keyList.frameList)
                        {
                            int index = keyList.frameList.FindIndex(obj => obj == frame);
                            string loopTrueString = "";
                            string animTrigger = "";
                            string posFrom = "", rotFrom = "", widthFrom = "", heightFrom = "";
                            WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                            if (index > 0)
                            {
                                prevFrame = keyList.frameList[index - 1];
                                posFrom = $"from=\"{-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10}\"";
                                rotFrom = $"from=\"{prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ}\"";
                                widthFrom = $"from=\"{prevFrame.scalX}\"";
                                heightFrom = $"from=\"{prevFrame.scalY}\"";

                                animTrigger = $"begin= \"{planeID}_F{index}\"";
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"begin= \"click\"";
                            }

                            string posTo = $"to=\"{-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10}\"",
                                rotTo = $"to=\"{frame.rotX} {-frame.rotY} {-frame.rotZ}\"",
                                widthTo = $"to=\"{frame.scalX}\"",
                                heightTo = $"to=\"{frame.scalY}\"";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{planeID}_F{index}\" attribute=\"position\" {posFrom} {posTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{planeID}_F{index}\" attribute=\"rotation\" {rotFrom} {rotTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{planeID}_F{index}\" attribute=\"width\" {widthFrom} {widthTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{planeID}_F{index}\" attribute=\"height\" {heightFrom} {heightTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                        }
                    }
                    sb.AppendLine("</a-plane>");
                    break;

                case "Video":
                    string videoID = childToAdd.name + "_" + i;
                    var Video = (width: childToAdd.transform.localScale.x, height: childToAdd.transform.localScale.y,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponentInChildren<MeshRenderer>().sharedMaterial.color),
                                src: "#" + childToAdd.name + "_Asset_" + i);
                    sb.AppendLine($"<a-video src=\"{Video.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" width=\"{Video.width}\" height=\"{Video.height}\" position=\"{Video.position}\" rotation=\"{Video.rotation}\" color=\"{Video.color}\" transparent={transparency}>");

                    if (childToAdd.GetComponent<AnimationHelper>() != null)
                    {
                        string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + videoID + ".txt");
                        KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                        foreach (WeldonKeyFrame frame in keyList.frameList)
                        {
                            int index = keyList.frameList.FindIndex(obj => obj == frame);
                            string loopTrueString = "";
                            string animTrigger = "";
                            string posFrom = "", rotFrom = "", widthFrom = "", heightFrom = "";
                            WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                            if (index > 0)
                            {
                                prevFrame = keyList.frameList[index - 1];
                                posFrom = $"from=\"{-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10}\"";
                                rotFrom = $"from=\"{prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ}\"";
                                widthFrom = $"from=\"{prevFrame.scalX}\"";
                                heightFrom = $"from=\"{prevFrame.scalY}\"";

                                animTrigger = $"begin= \"{videoID}_F{index}\"";
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"begin= \"click\"";
                            }

                            string posTo = $"to=\"{-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10}\"",
                                rotTo = $"to=\"{frame.rotX} {-frame.rotY} {-frame.rotZ}\"",
                                widthTo = $"to=\"{frame.scalX}\"",
                                heightTo = $"to=\"{frame.scalY}\"";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{videoID}_F{index}\" attribute=\"position\" {posFrom} {posTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{videoID}_F{index}\" attribute=\"rotation\" {rotFrom} {rotTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{videoID}_F{index}\" attribute=\"width\" {widthFrom} {widthTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{videoID}_F{index}\" attribute=\"height\" {heightFrom} {heightTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                        }
                    }
                    sb.AppendLine("</a-video>");
                    break;

                case "Cube":
                    var Cube = (width: childToAdd.transform.localScale.x/10, height: childToAdd.transform.localScale.y / 10, depth: childToAdd.transform.localScale.z / 10,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponent<MeshRenderer>().sharedMaterial.color),
                                src: textureName!=null?"textures/" + textureName + ".png" : "");
                    sb.AppendLine($"<a-box src=\"{Cube.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" width=\"{Cube.width}\" height=\"{Cube.height}\" depth=\"{Cube.depth}\" position=\"{Cube.position}\" rotation=\"{Cube.rotation}\" color=\"{Cube.color}\" transparent={transparency}>");

                    string cubeID = childToAdd.name + "_" + i;

                    if (childToAdd.GetComponent<AnimationHelper>() != null)
                    {
                        string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + cubeID + ".txt");
                        KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                        foreach (WeldonKeyFrame frame in keyList.frameList)
                        {
                            int index = keyList.frameList.FindIndex(obj => obj == frame);
                            string loopTrueString = "";
                            string animTrigger = "";
                            string posFrom = "", rotFrom = "", widthFrom = "", heightFrom = "", depthFrom = "";
                            WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                            if (index > 0)
                            {
                                prevFrame = keyList.frameList[index - 1];
                                posFrom = $"from=\"{-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10}\"";
                                rotFrom = $"from=\"{prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ}\"";
                                widthFrom = $"from=\"{prevFrame.scalX / 10}\"";
                                heightFrom = $"from=\"{prevFrame.scalY / 10}\"";
                                depthFrom = $"from=\"{prevFrame.scalZ / 10}\"";

                                animTrigger = $"begin= \"{cubeID}_F{index}\"";
                            }
                            else
                            {
                                if(childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"begin= \"click\"";
                            }

                            string posTo = $"to=\"{-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10}\"", 
                                rotTo = $"to=\"{frame.rotX} {-frame.rotY} {-frame.rotZ}\"", 
                                widthTo = $"to=\"{frame.scalX / 10}\"", 
                                heightTo = $"to=\"{frame.scalY / 10}\"", 
                                depthTo = $"to=\"{frame.scalZ / 10}\"";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cubeID}_F{index}\" attribute=\"position\" {posFrom} {posTo} dur=\"{(frame.time-prevFrame.time)*1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cubeID}_F{index}\" attribute=\"rotation\" {rotFrom} {rotTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cubeID}_F{index}\" attribute=\"width\" {widthFrom} {widthTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cubeID}_F{index}\" attribute=\"height\" {heightFrom} {heightTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentDepth(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cubeID}_F{index}\" attribute=\"depth\" {depthFrom} {depthTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                        }
                    }
                    sb.AppendLine("</a-box>");
                    break;

                case "Sphere":
                    var Sphere = (radius: childToAdd.transform.localScale.x / 20,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponent<MeshRenderer>().sharedMaterial.color),
                                src: textureName != null ? "textures/" + textureName + ".png" : "");
                    Debug.Log(Sphere.src);
                    sb.AppendLine($"<a-sphere src=\"{Sphere.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" radius=\"{Sphere.radius}\" position=\"{Sphere.position}\" rotation=\"{Sphere.rotation}\" color=\"{Sphere.color}\" transparent={transparency}>");
                    string shpereID = childToAdd.name + "_" + i;
                    if (childToAdd.GetComponent<AnimationHelper>() != null)
                    {
                        string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + shpereID + ".txt");
                        KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                        foreach (WeldonKeyFrame frame in keyList.frameList)
                        {
                            int index = keyList.frameList.FindIndex(obj => obj == frame);
                            string loopTrueString = "";
                            string animTrigger = "";
                            string posFrom = "", rotFrom = "", radiusFrom = "";
                            WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                            if (index > 0)
                            {
                                prevFrame = keyList.frameList[index - 1];
                                posFrom = $"from=\"{-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10}\"";
                                rotFrom = $"from=\"{prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ}\"";
                                radiusFrom = $"from=\"{prevFrame.scalX / 20}\"";

                                animTrigger = $"begin= \"{shpereID}_F{index}\"";
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"begin= \"click\"";
                            }

                            string posTo = $"to=\"{-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10}\"",
                                rotTo = $"to=\"{frame.rotX} {-frame.rotY} {-frame.rotZ}\"",
                                radiusTo = $"to=\"{frame.scalX / 20}\"";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{shpereID}_F{index}\" attribute=\"position\" {posFrom} {posTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{shpereID}_F{index}\" attribute=\"rotation\" {rotFrom} {rotTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{shpereID}_F{index}\" attribute=\"radius\" {radiusFrom} {radiusTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                        }
                    }
                    sb.AppendLine("</a-sphere>");

                    break;

                //TODO: Add animation stuff to Cylinder once you've finished animation stuff for the other shape types.
                case "Cylinder":
                    var Cylinder = (radius: (childToAdd.transform.localScale.x + childToAdd.transform.localScale.z) / 40, height: childToAdd.transform.localScale.y / 5,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponent<MeshRenderer>().sharedMaterial.color),
                                src: textureName != null ? "textures/" + textureName + ".png": "");
                    Debug.Log(Cylinder.src);
                    sb.AppendLine($"<a-cylinder src=\"{Cylinder.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" radius=\"{Cylinder.radius}\" height=\"{Cylinder.height}\" position=\"{Cylinder.position}\" rotation=\"{Cylinder.rotation}\" color=\"{Cylinder.color}\" transparent={transparency}>");
                    string cylinderID = childToAdd.name + "_" + i;
                    if (childToAdd.GetComponent<AnimationHelper>() != null)
                    {
                        string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + cylinderID + ".txt");
                        KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                        foreach (WeldonKeyFrame frame in keyList.frameList)
                        {
                            int index = keyList.frameList.FindIndex(obj => obj == frame);
                            string loopTrueString = "";
                            string animTrigger = "";
                            string posFrom = "", rotFrom = "", radiusFrom = "", heightFrom = "";
                            WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                            if (index > 0)
                            {
                                prevFrame = keyList.frameList[index - 1];
                                posFrom = $"from=\"{-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10}\"";
                                rotFrom = $"from=\"{prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ}\"";
                                radiusFrom = $"from=\"{(prevFrame.scalX + prevFrame.scalZ) / 40}\"";
                                heightFrom = $"from=\"{prevFrame.scalY / 5}\"";

                                animTrigger = $"begin= \"{cylinderID}_F{index}\"";
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"begin= \"click\"";
                            }

                            string posTo = $"to=\"{-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10}\"",
                                rotTo = $"to=\"{frame.rotX} {-frame.rotY} {-frame.rotZ}\"",
                                radiusTo = $"to=\"{(frame.scalX + frame.scalZ) / 40}\"",
                                heightTo = $"to=\"{frame.scalY / 5}\"";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cylinderID}_F{index}\" attribute=\"position\" {posFrom} {posTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cylinderID}_F{index}\" attribute=\"rotation\" {rotFrom} {rotTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cylinderID}_F{index}\" attribute=\"radius\" {radiusFrom} {radiusTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"<a-animation id=\"{cylinderID}_F{index}\" attribute=\"height\" {heightFrom} {heightTo} dur=\"{(frame.time - prevFrame.time) * 1000}\" easing='linear' {animTrigger}></a-animation>");
                        }
                    }
                    sb.AppendLine("</a-cylinder>");
                    break;

                default:

                    break;
            }
        }

        sb.AppendLine("<!-- END: Unity Compiled Objects -->");
        sb.AppendLine("");
        sb.AppendLine("<!-- BEGIN: Bottome HTML -->");
        sb.Append(bottomHTML);
        sb.AppendLine("<!-- END: Bottome HTML -->");

        File.WriteAllText(folderPath + fileName, sb.ToString());
        Debug.Log("index file successfully created");
        AssetDatabase.Refresh();
    }
}
