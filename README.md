# Unity tool for AR.js
This tool is in it's early stages, but is meant to help the user to build an AR.js project using Unity to help make the 3D object placement a breeze. This project is by no means complete, and currently only supports changing the image target, adding basic shapes to the scene, changing the shapes' colors or textures, adding .obj models, simple linear animations, plane videos, and the ability to click an object to trigger an animation or open a webpage. 

## Setup
Download the zip file from GitHub, extract the folder, then drag and drop it into the Assets folder within your Unity project, and make sure the name is "ARjs_Unity". If the name of the folder is "ARjs_Unity-master", change it to "ARjs_Unity". Currently, the name and placement of the folder is important, or else it won't work. After this has been added to your project, download the [AR.js project from GitHub](https://github.com/jeromeetienne/AR.js). Take the "AR.js-master" folder and drop that into the Assets folder as well. Again, name and placement of the folder matters. And this folder should be named "AR.js-master".

After that, you can get started. You can seperate the different ARjs projects your're working on by making different scenes. The project will compile to Assets>AR.js-master>aframe>{Active Scene Name}. This means if you have two different scenes in different folders, but with the same name, they will overwrite each other when compiling the ARjs project. It will also (likely) overwrite the animations on your game objects when you export animations if the scenes are the same name.

## Usage


### Changing the Image Target
There are two menu items for this. "AR.js/Image Target/1. Generate Image Target" and "AR.js/Image Target/2. Apply Generated Image". The first one will open up the website where you can go to generate both the image and the .patt file that you will need. The second one, should only be used after you already have an ImageTarget in scene. It will open another window where you will need drag and drop both the .patt file and the target image you created. Then hit update. If you don't alter anything before hitting update, it will reset the target to the default HIRO image target. 

### Adding Normal Objects
Right click the Hierarchy to add an image target, then right click the image target to add a shape.

### Texturing Normal Objects
You can right click in your assets folder to create a new material, with the new material selected, drag and drop an image under the material's "albedo" option, or simply change the color of the material. Then you can drag and drop this material onto your object.

### Adding Custom Model Objects
If you right click the image target, you can add a custom model to the scene. Custom models require .mtl files to be textured. And sometimes .mtl files require textures in order to work. After clicking on "AR.js>Custom Model" a window will appear. Drag and drop the .obj, .mtl, and texture file into the appropriate places in the window. Then click the button and the files will both get copied from wherever it was to the aframe/{scene name}/models folder and appear in the scene that you're working in. If you want to learn how to make simple objects in Unity click [here](https://www.procore3d.com/probuilder/)

### Button Objects
Select the object in question that you want to make into a button. Then click the "AR.js" menu item, and select "Make Button." You can then change the URL that the button links to in the inspector when the object is selected. NOTE: buttons don't work the best on mobile for some reason. You kinda need to tap the object many time in quick succession, but on the computer using the mouse it always gets it on just one click.

### Videos
Videos are very similar to Planes. You can add a video by right clicking the image target, selecting AR.js>Video. The Video object has a child also called "Video". The child called Video has a video player component. Drag and drop your video clip into the Video Players empty spot called "Video Clip". Select the parent Video in order to change the position, scale and rotation of the video. If you're changing the transform of the child, it won't export that information correctly. NOTE: Adding a video will also add a Mute/Unmute button to the webpage. This is mostly for iOS as it is required to have a mute button in order for there to be sound on the video.

### Animating Objects (Linear)
Select the object that you want to animate, click the "AR.js" menu item, and select "Make Animation" then select "Linear." This will add two scripts to the object. The one you will need to use for adding keyframes to your animation is the Custom List script [(that I modified from this original post)](https://forum.unity.com/threads/display-a-list-class-with-a-custom-editor-script.227847/). 

You can move your object anywhere and record that as a point (change the FrameTime to change when the object is supposed to get to that position). It will then be added to a list of keyframes. Note: you need to have at least one keyframe at time 0. After adding it to the list, each keyframe can be expanded and edited. If you want to check what that keyframe actually looks like, you can click "Set Transform from Frame," and it will set the transform of your object to what was recorded to that frame. If you move your object around a bit, and click "Update Transform" it will change the position, rotation, and scale for that keyframe to the objects current transform. You can change the time of the keyframe and it will be reordered in the list of keyframes to be chronological. **After you have all your keyframes, Click "Export Animation."** This will create a JSON file of all the keyframes you have made for the animation. If you then click the play button, you can see what your animation looks like.

There is a loop option, and a click option that you can enable. If checked, the "Loop Animation" option will cause the animation to loop. The "On Click" option will make the animation trigger upon your click (though only when you finally compile to HTML, and not when you click play in the Unity Editor).

### Animating Objects (Bezier Curve)
There is a lot I need to add here for it to be fully functional. Particularly a way of also rotating and scaling the objects. Right now, it only works with positional animation. And I need to add in several options like execute on click. Right now though, you can use it and get a feel for how this feature works.

On an AR.js object, click AR.js > Make Animation > Nonlinear Positional. This adds the Bezier Manager as a child to the object with two points. When the "BezierManager(Clone)" is selected in the inspector, you can create new points, loop/unloop the path (connect or disconnect the last point to the start point), or show/hide all the points and lines. When one of the points is selected, you can delete it. When a control point is selected, you can select "locked" and it will make the rotation of the opposite side control point mirror the control point you're moving. This locked feature allows you to more easily keep your animation curve smooth.

### Compiling to HTML
For Testing locally on your computer, click the "AR.js" menu item, then click "Compile Files>Testing" (this will prevent an error that causes textures not to render). For publishing to a website, click "CompileFiles>Final" (this will improve the clickablity of objects on mobile devices. Both of these options will create a file located in Assets>AR.js-master>aframe>{Active Scene Name}>index.html

This is the final file and can be opened in a browser to view the AR experience using either the default "Hiro" marker provided from Jerome's AR.js GitHub, or the custom marker that you've created and applied yourself. If you open it in FireFox, you don't have to worry about running it on a localhost server for it to work properly.

## How it Works
The CompileFile.cs script is where the majority of the work happens. It's just a lot of loops and conditional statements that goes through ever object attached to the ImageTarget in scene, and adds HTML text to a StringBuilder accordingly then saves the giant string as a file called index.html

## Acknowledgments
A huge shout out to Jerome Etienne for enabling Augmented Reality through any web browser. [GitHub again](https://github.com/jeromeetienne/AR.js)

Thank you to ForceX for the [Custom List template](https://forum.unity.com/threads/display-a-list-class-with-a-custom-editor-script.227847/) I'm using to make the KeyFrame list easily editable.

Thank you to Amit Kapdi from The App Guruz for his fantastic [article and code](http://www.theappguruz.com/blog/bezier-curve-in-games) for making Bezier Curves in Unity.

Thank you to Lee Stemkoski for his [fantastic examples](https://stemkoski.github.io/AR-Examples/) that helped me figure out how to implement animating curved paths in JavaScript.

Also thank you to Or-Aviram for his [Draw Field on condition](https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/). Which I'm not actively using, but the code is added to my project and I anticipate that I will be using it eventually.

## Demonstration

[![Using AR.js Unity Converter](http://i.imgur.com/DiMMC4R.jpg)](https://youtu.be/PYs_Y1U2_DI)
