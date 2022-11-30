using System.Collections.Generic;
using DobreKody.Basic;
using Unity.VisualScripting;
using UnityEngine;

public static class StaticMemory
{
    public static ObjectDetectionScript DetectionScript;
    
    public static bool YoloRender(this Camera camera,int classIndex )
    {
        //camera.rect = new Rect(0, 0, 416, 416);
        camera.Render();
        DetectionScript.Process(camera.targetTexture);  
        
        return false;
    }
    
    public static Texture2D RTImage(this Camera camera)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image;
    }
}
