using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public CustomTerrain terrain;

    // Start is called before the first frame update
    void Start()
    {
        terrain.Perlin();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
