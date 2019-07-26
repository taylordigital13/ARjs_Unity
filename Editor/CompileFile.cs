using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CompileFile : MonoBehaviour
{
    [MenuItem("AR.js/Compile Files", true)]
    static bool CompileFileHTMLingvalidation()
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
        if (!File.Exists("Assets/AR.js-master/aframe/fullscreen.png"))
        {
            File.Copy("Assets/ARjs_Unity/Icons/fullscreen.png", "Assets/AR.js-master/aframe/fullscreen.png");
        }
        if (!File.Exists("Assets/AR.js-master/aframe/exit_fullscreen.png"))
        {
            File.Copy("Assets/ARjs_Unity/Icons/exit_fullscreen.png", "Assets/AR.js-master/aframe/exit_fullscreen.png");
        }
        string fileName = "index.html";
        bool hasVideo = false;

        #region HTML Strings
        string aframeString = "<script src=\"https://aframe.io/releases/0.9.2/aframe.min.js\"></script>";
        string topHTML = $"<!DOCTYPE html>\n<!-- include aframe -->\n{aframeString}\n<!-- include ar.js -->\n<script src=\"https://cdn.rawgit.com/jeromeetienne/AR.js/1.7.2/aframe/build/aframe-ar.js\"></script>\n\n<!-- to load .ply model -->\n<script src=\"https://rawgit.com/donmccurdy/aframe-extras/v6.0.0/dist/aframe-extras.loaders.min.js\"></script>\n\n";
        string bodyHtml = @"<body style='margin : 0px; overflow: hidden; font-family: Monospace;'>";
        string middleHTML = @"<!-- <a-scene embedded arjs='debugUIEnabled: false; sourceType: video; sourceUrl:../../data/videos/headtracking.mp4;'> -->
    <a-scene embedded arjs='debugUIEnabled: false; sourceType: webcam' vr-mode-ui='enabled: false'>
    <a-entity id=""mouseCursor"" cursor=""rayOrigin: mouse"" raycaster=""objects: .intersectable; useWorldCoordinates: true;""></a-entity>";
        string buttonHTML = "<div style='position: absolute; bottom: 10px; right: 30px; width:100%; text-align: center; z-index: 1;'>\n      <button id=\"mutebutton\" style='position: absolute; bottom: 10px'>\n          Unmute\n      </button>\n  </div>";
        string fullscreenButtonHTML = "<div style='position: absolute; bottom: 5px; left: 30px; width:100%; text-align: right; z-index: 1;'>\n        <input type=\"image\" id=\"fullscreen\" src=\"../fullscreen.png\" style='position: absolute; bottom: 0px; right: 35px;'>\n        </input>\n    </div>";
        string fullscreenButtonActionHTML = "fullbutton.addEventListener(\"click\", function (evt) {\n                if (fullscreen == 0) {\n                    if (elem.requestFullscreen) {\n                        elem.requestFullscreen();\n                    } else if (elem.mozRequestFullScreen) {\n                        /* Firefox */\n                        elem.mozRequestFullScreen();\n                    } else if (elem.webkitRequestFullscreen) {\n                        /* Chrome, Safari and Opera */\n                        elem.webkitRequestFullscreen();\n                    } else if (elem.msRequestFullscreen) {\n                        /* IE/Edge */\n                        elem.msRequestFullscreen();\n                    }\n                    fullbutton.setAttribute(\"src\", \"../exit_fullscreen.png\");\n                    fullscreen = 1;\n                } else {\n                    if (document.exitFullscreen) {\n                        document.exitFullscreen();\n                    } else if (document.webkitExitFullscreen) {\n                        document.webkitExitFullscreen();\n                    } else if (document.mozCancelFullScreen) {\n                        document.mozCancelFullScreen();\n                    } else if (document.msExitFullscreen) {\n                        document.msExitFullscreen();\n                    }\n                    fullbutton.setAttribute(\"src\", \"../fullscreen.png\");\n                    fullscreen = 0;\n                }\n\n            });";
        string patternName = GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().patternName;
        string presetText = $"preset=\"hiro\" emitevents=\"true\" button";
        if (patternName != "default") presetText = $"type=\"pattern\" preset=\"custom\" src=\"{patternName}\" url=\"{patternName}\" emitevents=\"true\" button";
        string markerHTML = "<a-marker id=\"marker\" " + presetText + ">";
        string bottomHTML = @"</a-marker>
        <a-entity camera></a-entity>
        </a-scene>
</body>
</html>";
        #endregion

        StringBuilder sb = new StringBuilder();
        #region Top HTML
        sb.AppendLine("<!-- BEGIN: Top HTML -->");
        sb.Append(topHTML);
        sb.AppendLine("<!-- END: Top HTML -->");
        #endregion Top HTML

        Transform imageTarget = GameObject.FindGameObjectWithTag("ImageTarget").transform;
        if(imageTarget == null)
        {
            Debug.Log("AR.js error: There is no Image Target to Compile");
            return;
        }

        #region Unity Compiled Events
        //Adds in the actions of the children to javascript.
        sb.AppendLine("<!-- BEGIN: Unity Compiled Events -->");
        sb.AppendLine("<script>");

        //GLOBALS FOR ANIMATION UPDATE
        sb.AppendLine("var markerFound = 0;");
        bool hasBezier = false;
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            for(int j = 0; j<childToAdd.transform.childCount; j++)
            {
                if (childToAdd.transform.GetChild(j).name.ToLower().Contains("bezier"))
                {
                    hasBezier = true;
                }
            }
        }
        if (hasBezier)
        {
            //sb.AppendLine("var subtractTime = 0;");
            for (int i = 0; i < imageTarget.childCount; i++)
            {
                GameObject childToAdd = imageTarget.GetChild(i).gameObject;
                string childID = childToAdd.name + "_" + i;
                Bezier bez = childToAdd.GetComponentInChildren<Bezier>();
                if (bez != null)
                {
                    sb.AppendLine("var " + childID + "_CurrentPoint = 0;");
                    sb.AppendLine("var " + childID + "_SubtractTime = 0;");
                    sb.AppendLine("var " + childID + "_PointsArray = " + bez.PointsListString() + ";");
                }
            }
        }
        //END GLOBALS FOR ANIMATION UPDATE

        sb.AppendLine("AFRAME.registerComponent('button', {");
        sb.AppendLine("init: function () {");
        sb.AppendLine($"var elem = document.documentElement;");
        sb.AppendLine($"var marker = document.querySelector(\"#marker\");");
        sb.AppendLine($"var fullbutton = document.querySelector(\"#fullscreen\");");
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            if (childToAdd.tag == "Video")
            {

                hasVideo = true;
            }
        }
        if(hasVideo) sb.AppendLine($"var button = document.querySelector(\"#mutebutton\");");

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
        }
        //marker event listeners
        sb.AppendLine("marker.addEventListener(\"markerFound\", function (evt) {");
        sb.AppendLine("markerFound = 1;");
        for(int i = 0; i<imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;

            if(childToAdd.tag == "Video")
            {
                string lineToAppend = id + ".play();";
                sb.AppendLine(lineToAppend);
            }
        }
        sb.AppendLine("});");
        sb.AppendLine("marker.addEventListener(\"markerLost\", function (evt) {");
        sb.AppendLine("markerFound = 0;");
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;

            if (childToAdd.tag == "Video")
            {
                string lineToAppend = id + ".pause();";
                sb.AppendLine(lineToAppend);
            }
        }
        sb.AppendLine("});");
        //end marker event listeners
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;

            if (childToAdd.GetComponent<ButtonHelper>() != null)
            {
                sb.AppendLine(@"open(""" + childToAdd.GetComponent<ButtonHelper>().URL + @""");");
                sb.AppendLine(id + @".addEventListener(""mousedown"", function(evt){");
                sb.AppendLine("});");
            }
            if (childToAdd.tag == "Video")
            {
                //string lineToAppend = "marker.addEventListener(\"markerFound\", function (evt) {\n" +
                	//id + ".play();\n" +
                	//"});\n" +
                	//"marker.addEventListener(\"markerLost\", function (evt) {\n" +
                	//id + ".pause();\n" +
                	//"});";
                
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


                //sb.AppendLine(lineToAppend);
                sb.AppendLine(secondLine);
            }
        }
        //end marker event listeners

        sb.AppendLine(fullscreenButtonActionHTML);
        sb.AppendLine("},");
        //BEGIN: Tick function for bezier animations
        sb.AppendLine("tick: function (totalTime, deltaTime) {");
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;
            Bezier bez = childToAdd.GetComponentInChildren<Bezier>();
            if (bez != null)
            {
                sb.AppendLine($"var {id} = document.querySelector(\"#{id}\");");
                sb.AppendLine($"var {id}_Speed = {bez.speed/60};");
                sb.AppendLine($"var {id}_Time = (totalTime - {id}_SubtractTime) / 1000;");
            }
        }
        //sb.AppendLine("var time = (totalTime - subtractTime) / 1000;");
        sb.AppendLine("var dTime = deltaTime / 1000;");
        sb.AppendLine("");
        sb.AppendLine("if (markerFound == 1) {");
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;
            Bezier bez = childToAdd.GetComponentInChildren<Bezier>();
            if (bez != null)
            {
                sb.AppendLine($"{id}_Update();");
            }
        }
        sb.AppendLine("}");
        sb.AppendLine();
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            string id = childToAdd.name + "_" + i;
            Bezier bez = childToAdd.GetComponentInChildren<Bezier>();
            if (bez != null)
            {
                sb.AppendLine($"function {id}_Update()" + " {");
                sb.AppendLine($"var newPosition = bezierPath({id}_PointsArray[{id}_CurrentPoint], {id}_PointsArray[{id}_CurrentPoint + 1], { id}_PointsArray[{ id}_CurrentPoint + 2], { id}_PointsArray[{ id}_CurrentPoint + 3], {id}_Time *{ id}_Speed);");
                sb.AppendLine();
                sb.AppendLine($"if ({id}_Time*{id}_Speed>1) " + "{");
                sb.AppendLine($"{id}_CurrentPoint += 3;");
                sb.AppendLine($"{id}_SubtractTime = totalTime;");
                sb.AppendLine($"if ({id}_CurrentPoint >= {id}_PointsArray.length - 3) " + "{");
                sb.AppendLine($"{id}_CurrentPoint = 0;");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine($"{id}.setAttribute('position', " + "{");
                sb.AppendLine("x: newPosition.x,");
                sb.AppendLine("y: newPosition.y,");
                sb.AppendLine("z: newPosition.z,");
                sb.AppendLine("});");
                sb.AppendLine("}");
            }
        }

        sb.AppendLine("function bezierEvaluate(p0, p1, p2, p3, t) {\n                " +
        	"var u = (1 - t);\n                " +
        	"var uu = u * u;\n                " +
        	"var uuu = u * u * u;\n                " +
        	"var tt = t * t;\n                " +
        	"var ttt = t * t * t;\n                " +
        	"//B(t) = (1-t)^3*P0 + 3*(1-t)^2*t*P1 + 3*(1-t)*t^2*P2 + t^3*P3 , 0 < t < 1\n                " +
        	"return (uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3);\n\n            " +
        	"}\n           " +
        	"function bezierPath(p0, p1, p2, p3, t) {\n                " +
        	"return new THREE.Vector3(\n                    " +
        	"bezierEvaluate(p0.x, p1.x, p2.x, p3.x, t),\n                    " +
        	"bezierEvaluate(p0.y, p1.y, p2.y, p3.y, t),\n                    " +
        	"bezierEvaluate(p0.z, p1.z, p2.z, p3.z, t)\n                " +
        	");\n            " +
        	"}\n\n        " +
        	"}");
        //END: Tick function for bezier animations
        sb.AppendLine("});");
        sb.AppendLine("</script>");
        sb.AppendLine("<!-- END: Unity Compiled Events -->");
        sb.AppendLine("");
        #endregion Unity Compiled Events

        //MiddleHTML
        sb.AppendLine("<!-- BEGIN: Middle HTML -->");
        sb.AppendLine(bodyHtml);
        if (hasVideo) sb.AppendLine(buttonHTML);
        sb.AppendLine(fullscreenButtonHTML);
        sb.AppendLine(middleHTML);
        sb.AppendLine("<!-- END: Middle HTML -->");
        sb.AppendLine("");

        #region Unity Compiled Assets
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

            if(childToAdd.tag == "Model")
            {
                if (!Directory.Exists(folderPath + "models/")) Directory.CreateDirectory(folderPath + "models/");
                CustomModelHelper modelHelper = childToAdd.GetComponent<CustomModelHelper>();
                if (File.Exists(folderPath + "models/" + modelHelper.objName))
                {
                    sb.AppendLine($"<a-asset-item id=\"{childToAdd.name + "_Asset_obj_" + i}\" src=\"models/{modelHelper.objName}\"></a-asset-item>");
                }
                else
                {
                    Debug.LogError($"The model file {modelHelper.objName} doesn't seem to exist in the proper location.");
                }
                if (File.Exists(folderPath + "models/" + modelHelper.mtlName))
                {
                    sb.AppendLine($"<a-asset-item id=\"{childToAdd.name + "_Asset_mtl_" + i}\" src=\"models/{modelHelper.mtlName}\"></a-asset-item>");
                }
                else
                {
                    Debug.LogWarning("The object doesn't have a material.");
                }
            }
        }
        sb.AppendLine("</a-assets>");
        sb.AppendLine("<!-- END: Unity Compiled Assets -->");
        #endregion Unity Compiled Assets

        sb.AppendLine("<!-- BEGIN: Add Image Target (marker) -->");
        sb.AppendLine(markerHTML);
        sb.AppendLine("<!-- END: Add Image Target (marker) -->");
        sb.AppendLine("");

        #region Unity Compiled Objects
        sb.AppendLine("<!-- BEGIN: Unity Compiled Objects -->");
        //Adds in the physical object for each child of the ImageTarget
        for (int i = 0; i < imageTarget.childCount; i++)
        {
            GameObject childToAdd = imageTarget.GetChild(i).gameObject;
            Texture2D objectTexture = (Texture2D)childToAdd.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture;
            string textureName = null;
            bool transparency = false;
            if (objectTexture != null && childToAdd.tag!="Model")
            {
                textureName = objectTexture.name;
                byte[] bytes = objectTexture.EncodeToPNG();
                if (!Directory.Exists(folderPath + "textures/")) Directory.CreateDirectory(folderPath + "textures/");
                File.WriteAllBytes(folderPath + "textures/" + textureName + ".png", bytes);
                transparency = true;

            }
            switch (childToAdd.tag)
            {
                case "Plane":
                    var Plane = (width: childToAdd.transform.localScale.x, height: childToAdd.transform.localScale.y,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponentInChildren<MeshRenderer>().sharedMaterial.color),
                                src: textureName != null ? "textures/" + textureName + ".png" : "");
                    sb.AppendLine($"<a-plane src=\"{Plane.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" width=\"{Plane.width}\" height=\"{Plane.height}\" position=\"{Plane.position}\" rotation=\"{Plane.rotation}\" color=\"{Plane.color}\" transparent={transparency}");

                    string planeID = childToAdd.name.ToLower() + "_" + i;
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
                                posFrom = $"from: {-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10};";
                                rotFrom = $"from: {prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ};";
                                widthFrom = $"from: {prevFrame.scalX};";
                                heightFrom = $"from: {prevFrame.scalY};";

                                animTrigger = $"startEvents: animationcomplete__{planeID}_f{index-1}" + ((index==1 && childToAdd.GetComponent<AnimationHelper>().loop)? $", animationcomplete__{planeID}_f{keyList.frameList.Count-1};" : ";");
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"startEvents: mousedown;";
                            }

                            string posTo = $"to: {-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10};",
                                rotTo = $"to: {frame.rotX} {-frame.rotY} {-frame.rotZ};",
                                widthTo = $"to: {frame.scalX};",
                                heightTo = $"to: {frame.scalY};";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{planeID}_f{index}=\" property: position; {posFrom} {posTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{planeID}_f{index}=\" property: rotation; {rotFrom} {rotTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{planeID}_f{index}=\" property: width; {widthFrom} {widthTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{planeID}_f{index}=\" property: height; {heightFrom} {heightTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                        }
                    }
                    sb.AppendLine("></a-plane>");
                    break;

                case "Video":
                    string videoID = childToAdd.name.ToLower() + "_" + i;
                    var Video = (width: childToAdd.transform.localScale.x, height: childToAdd.transform.localScale.y,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponentInChildren<MeshRenderer>().sharedMaterial.color),
                                src: "#" + childToAdd.name + "_Asset_" + i);
                    sb.AppendLine($"<a-video src=\"{Video.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" width=\"{Video.width}\" height=\"{Video.height}\" position=\"{Video.position}\" rotation=\"{Video.rotation}\" color=\"{Video.color}\" transparent={transparency}");

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
                                posFrom = $"from: {-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10};";
                                rotFrom = $"from: {prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ};";
                                widthFrom = $"from: {prevFrame.scalX};";
                                heightFrom = $"from: {prevFrame.scalY};";

                                animTrigger = $"startEvents: animationcomplete__{videoID}_f{index-1}" + ((index == 1 && childToAdd.GetComponent<AnimationHelper>().loop) ? $", animationcomplete__{videoID}_f{keyList.frameList.Count - 1};" : ";");
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"startEvents: mousedown;";
                            }

                            string posTo = $"to: {-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10};",
                                rotTo = $"to: {frame.rotX} {-frame.rotY} {-frame.rotZ};",
                                widthTo = $"to: {frame.scalX};",
                                heightTo = $"to: {frame.scalY};";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{videoID}_f{index}=\" property: position; {posFrom} {posTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{videoID}_f{index}=\" property: rotation; {rotFrom} {rotTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{videoID}_f{index}=\" property: width; {widthFrom} {widthTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{videoID}_f{index}=\" property: height; {heightFrom} {heightTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                        }
                    }
                    sb.AppendLine("></a-video>");
                    break;

                case "Cube":
                    var Cube = (width: childToAdd.transform.localScale.x/10, height: childToAdd.transform.localScale.y / 10, depth: childToAdd.transform.localScale.z / 10,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponent<MeshRenderer>().sharedMaterial.color),
                                src: textureName!=null?"textures/" + textureName + ".png" : "");
                    sb.AppendLine($"<a-box src=\"{Cube.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" width=\"{Cube.width}\" height=\"{Cube.height}\" depth=\"{Cube.depth}\" position=\"{Cube.position}\" rotation=\"{Cube.rotation}\" color=\"{Cube.color}\" transparent={transparency}");

                    string cubeID = childToAdd.name.ToLower() + "_" + i;

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
                                posFrom = $"from: {-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10};";
                                rotFrom = $"from: {prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ};";
                                widthFrom = $"from: {prevFrame.scalX / 10};";
                                heightFrom = $"from: {prevFrame.scalY / 10};";
                                depthFrom = $"from: {prevFrame.scalZ / 10};";

                                animTrigger = $"startEvents: animationcomplete__{cubeID}_f{index-1}" + ((index == 1 && childToAdd.GetComponent<AnimationHelper>().loop) ? $", animationcomplete__{cubeID}_f{keyList.frameList.Count - 1};" : ";");
                            }
                            else
                            {
                                if(childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"startEvents: mousedown;";
                            }

                            string posTo = $"to: {-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10};", 
                                rotTo = $"to: {frame.rotX} {-frame.rotY} {-frame.rotZ};", 
                                widthTo = $"to: {frame.scalX / 10};", 
                                heightTo = $"to: {frame.scalY / 10};", 
                                depthTo = $"to: {frame.scalZ / 10};";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cubeID}_f{index}=\"property: position; {posFrom} {posTo} dur: {(frame.time-prevFrame.time)*1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cubeID}_f{index}=\"property: rotation; {rotFrom} {rotTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cubeID}_f{index}=\"property: width; {widthFrom} {widthTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cubeID}_f{index}=\"property: height; {heightFrom} {heightTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentDepth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cubeID}_f{index}=\"property: depth; {depthFrom} {depthTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                        }
                    }
                    sb.AppendLine("></a-box>");
                    break;

                case "Model":
                    string modelID = childToAdd.name.ToLower() + "_" + i;

                    var Model = (width: childToAdd.transform.localScale.x / 10, height: childToAdd.transform.localScale.y / 10, depth: childToAdd.transform.localScale.z / 10,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#ffffff",
                                src: textureName != null ? "textures/" + textureName + ".png" : "");
                    sb.AppendLine($"<a-entity obj-model=\"obj: #{childToAdd.name + "_Asset_obj_" + i}; mtl: #{childToAdd.name + "_Asset_mtl_" + i}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" scale=\"{Model.width} {Model.height} {Model.depth}\" position=\"{Model.position}\" rotation=\"{Model.rotation}\" color=\"{Model.color}\" transparent={transparency}");


                    if (childToAdd.GetComponent<AnimationHelper>() != null)
                    {
                        string animationFile = File.ReadAllText(Application.dataPath + "/Animations/JsonExports/" + SceneManager.GetActiveScene().name + "/" + modelID + ".txt");
                        KeyFrameList keyList = JsonUtility.FromJson<KeyFrameList>(animationFile);

                        foreach (WeldonKeyFrame frame in keyList.frameList)
                        {
                            int index = keyList.frameList.FindIndex(obj => obj == frame);
                            string loopTrueString = "";
                            string animTrigger = "";
                            string posFrom = "", rotFrom = "", scaleFrom = "";
                            WeldonKeyFrame prevFrame = new WeldonKeyFrame();
                            if (index > 0)
                            {
                                prevFrame = keyList.frameList[index - 1];
                                posFrom = $"from: {-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10};";
                                rotFrom = $"from: {prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ};";
                                scaleFrom = $"from: {prevFrame.scalX / 10} {prevFrame.scalY / 10} {prevFrame.scalZ / 10};";

                                animTrigger = $"startEvents: animationcomplete__{modelID}_f{index-1}" + ((index == 1 && childToAdd.GetComponent<AnimationHelper>().loop) ? $", animationcomplete__{modelID}_f{keyList.frameList.Count - 1};" : ";");
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"startEvents: mousedown;";
                            }

                            string posTo = $"to: {-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10};",
                                rotTo = $"to: {frame.rotX} {-frame.rotY} {-frame.rotZ};",
                                scaleTo = $"to: {frame.scalX / 10} {frame.scalY / 10} {frame.scalZ / 10};";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{modelID}_f{index}=\" property: position; {posFrom} {posTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{modelID}_f{index}=\" property: rotation; {rotFrom} {rotTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{modelID}_f{index}=\" property: scale; {scaleFrom} {scaleTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                        }
                    }
                    sb.AppendLine("></a-entity>");
                    break;

                case "Sphere":
                    var Sphere = (radius: childToAdd.transform.localScale.x / 20,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponent<MeshRenderer>().sharedMaterial.color),
                                src: textureName != null ? "textures/" + textureName + ".png" : "");
                    Debug.Log(Sphere.src);
                    sb.AppendLine($"<a-sphere src=\"{Sphere.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" radius=\"{Sphere.radius}\" position=\"{Sphere.position}\" rotation=\"{Sphere.rotation}\" color=\"{Sphere.color}\" transparent={transparency}");
                    string shpereID = childToAdd.name.ToLower() + "_" + i;
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
                                posFrom = $"from: {-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10};";
                                rotFrom = $"from: {prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ};";
                                radiusFrom = $"from: {prevFrame.scalX / 20};";

                                animTrigger = $"startEvents: animationcomplete__{shpereID}_f{index-1}" + ((index == 1 && childToAdd.GetComponent<AnimationHelper>().loop) ? $", animationcomplete__{shpereID}_f{keyList.frameList.Count - 1};" : ";");
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"startEvents: mousedown;";
                            }

                            string posTo = $"to: {-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10};",
                                rotTo = $"to: {frame.rotX} {-frame.rotY} {-frame.rotZ};",
                                radiusTo = $"to: {frame.scalX / 20};";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{shpereID}_f{index}= \"property: position; {posFrom} {posTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{shpereID}_f{index}= \"property: rotation; {rotFrom} {rotTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{shpereID}_f{index}= \"property: radius; {radiusFrom} {radiusTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                        }
                    }
                    sb.AppendLine("></a-sphere>");

                    break;

                case "Cylinder":
                    var Cylinder = (radius: (childToAdd.transform.localScale.x + childToAdd.transform.localScale.z) / 40, height: childToAdd.transform.localScale.y / 5,
                                position: -childToAdd.transform.localPosition.x / 10 + " " + childToAdd.transform.localPosition.y / 10 + " " + childToAdd.transform.localPosition.z / 10,
                                rotation: childToAdd.transform.localEulerAngles.x + " " + -childToAdd.transform.localEulerAngles.y + " " + -childToAdd.transform.localEulerAngles.z,
                                color: "#" + ColorUtility.ToHtmlStringRGB(childToAdd.GetComponent<MeshRenderer>().sharedMaterial.color),
                                src: textureName != null ? "textures/" + textureName + ".png": "");
                    Debug.Log(Cylinder.src);
                    sb.AppendLine($"<a-cylinder src=\"{Cylinder.src}\" id=\"{childToAdd.name + "_" + i}\" class=\"intersectable\" radius=\"{Cylinder.radius}\" height=\"{Cylinder.height}\" position=\"{Cylinder.position}\" rotation=\"{Cylinder.rotation}\" color=\"{Cylinder.color}\" transparent={transparency}");
                    string cylinderID = childToAdd.name.ToLower() + "_" + i;
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
                                posFrom = $"from: {-prevFrame.posX / 10} {prevFrame.posY / 10} {prevFrame.posZ / 10};";
                                rotFrom = $"from: {prevFrame.rotX} {-prevFrame.rotY} {-prevFrame.rotZ};";
                                radiusFrom = $"from: {(prevFrame.scalX + prevFrame.scalZ) / 40};";
                                heightFrom = $"from: {prevFrame.scalY / 5};";

                                animTrigger = $"startEvents: animationcomplete__{cylinderID}_f{index-1}" + ((index == 1 && childToAdd.GetComponent<AnimationHelper>().loop) ? $", animationcomplete__{cylinderID}_f{keyList.frameList.Count - 1};" : ";");
                            }
                            else
                            {
                                if (childToAdd.GetComponent<AnimationHelper>().onClick) animTrigger = $"startEvents: mousedown;";
                            }

                            string posTo = $"to: {-frame.posX / 10} {frame.posY / 10} {frame.posZ / 10};",
                                rotTo = $"to: {frame.rotX} {-frame.rotY} {-frame.rotZ};",
                                radiusTo = $"to: {(frame.scalX + frame.scalZ) / 40};",
                                heightTo = $"to: {frame.scalY / 5};";

                            //if (childToAdd.GetComponent<AnimationHelper>().loop) loopTrueString = $"repeat = \"indefinite\"";
                            bool isFirstFrame = prevFrame.time.Equals(-1) ? true : false;
                            if (isFirstFrame) prevFrame.time = 0;
                            if (frame.IsDifferentPosition(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cylinderID}_f{index}=\" property: position; {posFrom} {posTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentRotation(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cylinderID}_f{index}=\" property: rotation; {rotFrom} {rotTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentWidth(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cylinderID}_f{index}=\" property: radius; {radiusFrom} {radiusTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                            if (frame.IsDifferentHeight(prevFrame) || isFirstFrame) sb.AppendLine($"animation__{cylinderID}_f{index}=\" property: height; {heightFrom} {heightTo} dur: {(frame.time - prevFrame.time) * 1000}; easing: linear; {animTrigger}\"");
                        }
                    }
                    sb.AppendLine("></a-cylinder>");
                    break;

                default:

                    break;
            }
        }

        sb.AppendLine("<!-- END: Unity Compiled Objects -->");
        #endregion Unity Compiled Objects

        sb.AppendLine("");
        sb.AppendLine("<!-- BEGIN: Bottom HTML -->");
        sb.Append(bottomHTML);
        sb.AppendLine("<!-- END: Bottom HTML -->");

        File.WriteAllText(folderPath + fileName, sb.ToString());
        Debug.Log("index file successfully created");
        AssetDatabase.Refresh();
    }
}
