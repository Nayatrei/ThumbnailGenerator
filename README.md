# ThumbnailGenerator
Unity Tool: Generate Thumbnail from Prefab from editor

Thumbnail Generator Tool for Unity Editor
The Thumbnail Generator is a custom tool designed for Unity Editor that assists in creating thumbnail images for your prefabs. This document serves as a guide to understand and use the Thumbnail Generator effectively.

Overview
Thumbnails are small images that represent larger objects, commonly used to provide a visual preview. In the context of game development with Unity, thumbnails can help visualize prefabs (pre-fabricated objects) in your asset library. The Thumbnail Generator tool simplifies the process of creating these thumbnails, offering options for different sizes and background configurations.

Features
Customizable Thumbnail Sizes: Generate thumbnails in various resolutions (128x128, 256x256, 512x512, and 1024x1024 pixels) to suit different needs.
Background Options: Choose between a solid background color for your thumbnail or generate a clear (transparent) thumbnail.
Background Color Removal: Process an existing texture to remove a specified background color, useful for creating transparent textures.


How to Use
Opening the Tool
In Unity Editor, navigate to the menu bar.
Click on Tools, then Toolkit, and select Thumbnail Generator.
This opens the Thumbnail Generator window.


Generating Thumbnails
Select Background Color: If you opt for a solid background, you can select the color directly in the tool window.
Choose Prefab: Select the prefab in your project for which you want to generate a thumbnail.
Select Size: Click on one of the size options (128, 256, 512, 1024) under the "Generate Prefab Thumbnail with Background" section for a solid background thumbnail, or under the "Generate Prefab Clear Thumbnail" section for a transparent background thumbnail.
Save Location: Thumbnails are saved in the same directory as the selected prefab, in a subfolder named "Thumbnails".
Removing Background Color from Textures
Load Texture: In the Thumbnail Generator window, under "Remove background color from texture", load the source texture by clicking on the field next to "Source Texture".
Set Background Color: Set the color you want to remove from the texture.
Process and Save: Click the "Process and Save" button. A new texture, with the specified color made transparent, is saved alongside the original texture with "_processed" appended to its filename.

Troubleshooting and Tips
Dark Thumbnails: If generated thumbnails appear darker than expected, this could be due to the current lighting setup in your scene. Adjust lighting or camera settings before capturing thumbnails.
Read/Write Enabled: For texture processing (background color removal), ensure the texture's import settings have "Read/Write Enabled" set to true.
Camera Settings: Thumbnails are captured using the last active Scene view camera. Adjustments to this camera's settings (field of view, position, etc.) might be necessary for optimal thumbnail capture.

Conclusion
The Thumbnail Generator tool for Unity Editor provides a convenient way to create visual representations of your prefabs. By following the steps outlined in this manual, you can efficiently generate thumbnails to enhance your asset management and selection process within the Unity Editor.
