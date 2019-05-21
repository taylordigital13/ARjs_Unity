# Unity tool for AR.js
This tool is in it's early stages, but is meant to help the user to build an AR.js project using Unity to help make the 3D object placement a breeze. This project is by no means complete, and currently only supports adding basic shapes to the scene, changing the shapes' colors or textures, simple linear animations, and the ability to click an object to trigger an animation or open a webpage.

## Setup
Drop the folder titled "ARjs_Unity" into the Assets folder within your Unity project. Currently, the name and placement of the folder is important, or else it won't work. After this has been added to your project, download the [AR.js project from GitHub](https://github.com/jeromeetienne/AR.js). Take the "AR.js-master" folder and drop that into the Assets folder as well. Again, name and placement of the folder matters.

## Usage

### Adding Objects
Right click the Hierarchy to add an image target, then right click the image target to add a shape.

![](adding_image_target_and_cube.gif)

### Texturing Objects
Adding textures is as easy as dragging and dropping. You can right click in your assets to create a new material, and put a texture under its "albedo" option, or simply change the color of the material. Then you can drag and drop this material onto your object.

![](adding_textures.gif)

