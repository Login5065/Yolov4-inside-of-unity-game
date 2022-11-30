using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restart : MonoBehaviour
{
    public Material shader;


    public List<Material> materials;

    // Start is called before the first frame update
    void Start()
    {

        foreach (var t in materials)
        {
            Texture textureData =  t.mainTexture;
            t.shader = shader.shader;
            t.mainTexture=textureData;
            
        }



    }
    
}
