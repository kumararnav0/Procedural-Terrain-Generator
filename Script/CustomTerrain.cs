using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using JetBrains.Annotations;
using Unity.VisualScripting.FullSerializer;
using System;
using UnityEngine.Playables;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.Diagnostics;
// using System.Numerics;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public UnityEngine.Vector3 heightMapScale = new UnityEngine.Vector3(1,1,1);

    public bool resetTerrain = false;

    //Perlin Noise ------------------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;

    //Multiple Perlin Noise ---------------------------
    [System.Serializable]
    public class PerlinParameters{
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }
    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>(){
        new PerlinParameters()
    };

    //Voronoi ------------------------------------------
    public float fallof = 0.2f; 
    public float dropoff = 0.5f; 
    public int peakCount = 3;
    public float minHeight = 0.2f;
    public float maxHeight = 1f;
    public enum VoronoiType {Linear = 0 ,Power = 1 , Combined = 2,Mountain = 3,Perlin = 4}
    public VoronoiType voronoiType = VoronoiType.Linear;

    //MPD -------------------------------------------------
    public float MPDheight = 0.5f;
    public float MPDroughness = 2.0f;
    public float MPDpow = 2f;

    //Smooth ----------------------------------------------
    public int smoothAmount = 2;

    //SplatMap --------------------------------------------
    [System.Serializable]
    public class SplatHeights{
        public Texture2D texture = null;
        public Texture2D textureNormalMap = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;

        public float splatOffset = 0.1f;
        public float splatNoiseXScale =0.01f;
        public float splatNoiseYScale =0.01f;
        public float splatNoiseZScale = 0.1f;

        public float minSlope = 0;
        public float maxSlope = 90;

        public Vector2 tileOffset = Vector2.zero;
        public Vector2 tileSize = new Vector2(50.0f,50.0f);
        
        public bool remove = false;
    }
    public List<SplatHeights> splatHeights = new List<SplatHeights>(){
        new SplatHeights()
    };

    //Vegetation -------------------------------------------
    [System.Serializable]
    public class Vegetation{
        public GameObject trees ;
        public float minHeight = 0f;
        public  float maxHeight = 0.3f;
        public float minSlope = 0f;
        public float maxSlope = 90;
        public float minScale = 0.5f;
        public float maxScale = 1.0f;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightColour = Color.white;
        public float minRotation = 0.0f;
        public float maxRotation = 360.0f;
        public float density = 0.5f;
        public bool remove = false;
    }
    public List<Vegetation> vegetations = new List<Vegetation>(){
        new Vegetation()
    };
    public int treeCount = 5000;
    public int spacing = 5;
    
    //DETAILS ------------------------------------------------
    [System.Serializable]
    public class Detail{
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1;
        public float noiseSpread = 0.5f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public Color dryColour = Color.white;
        public Color healthyColor = Color.white;
        public Vector2 heightRange = new Vector2(1,1);
        public Vector2 widthRange = new Vector2(1,1);
        public bool remove = false;
    }

    public List<Detail> details = new List<Detail>(){
        new Detail()
    };
    public int maxDetail = 5000;
    public int detailSpacing = 5;

    public Terrain terrain;
    public TerrainData terrainData;

    float[,] GetHeightMap(){
        if(!resetTerrain){
            return (terrainData.GetHeights(0,0,terrainData.heightmapResolution,terrainData.heightmapResolution));
        }
        else{
            return new float[terrainData.heightmapResolution,terrainData.heightmapResolution];
        }
    }

    void OnEnable(){
        Debug.Log("Initialisng Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    public void addDetail(){
        DetailPrototype[] newDetailPrototypes;
        newDetailPrototypes = new DetailPrototype[details.Count];
        int dindex = 0;
        foreach(Detail d in details){

            newDetailPrototypes[dindex] = new DetailPrototype(){
            prototype = d.prototype,
            prototypeTexture = d.prototypeTexture,
            healthyColor = Color.white,
            dryColor = Color.white,
            minHeight = d.heightRange.x,
            maxHeight = d.heightRange.y,
            minWidth = d.widthRange.x,
            maxWidth = d.widthRange.y,
            noiseSpread = d.noiseSpread
            };

            if(newDetailPrototypes[dindex].prototype){
                newDetailPrototypes[dindex].usePrototypeMesh = true;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.VertexLit;
            }
            else{
                newDetailPrototypes[dindex].usePrototypeMesh = false;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.GrassBillboard;
            }
            dindex++;
            
        }
        terrainData.detailPrototypes = newDetailPrototypes;

        float minDetailMapValue = 0;
        float maxDetailMapValue = 16;

        if(terrainData.detailScatterMode == DetailScatterMode.CoverageMode){
            maxDetailMapValue = 255;
        }
        float[,] heightMap = terrainData.GetHeights(0,0,terrainData.heightmapResolution,terrainData.heightmapResolution);

        for (int i = 0; i < terrainData.detailPrototypes.Length; i++) {

            int[,] detailMap = new int[terrainData.detailHeight, terrainData.detailWidth];

            for (int y = 0; y < terrainData.detailHeight; y += detailSpacing) {

                for (int x = 0; x < terrainData.detailWidth; x += detailSpacing) {

                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density) continue;
                    int yHM = (int)(x/(float)terrainData.detailHeight*terrainData.heightmapResolution);
                    int xHM = (int)(x/(float)terrainData.detailWidth*terrainData.heightmapResolution);

                    float thisNoise = Util.Map(Mathf.PerlinNoise(x*details[i].feather,y*details[i].feather),0,1,0.5f,1);
                    float thisHeightStart = details[i].minHeight*thisNoise - details[i].overlap*thisNoise;
                    float nextHeightStart = details[i].maxHeight*thisNoise + details[i].overlap*thisNoise;
                    float thisHeight = heightMap[yHM,xHM];
                    float steepness = terrainData.GetSteepness(xHM/(float)terrainData.size.x,y/(float)terrainData.size.z);
                    if((thisHeight >= thisHeightStart && thisHeight <= nextHeightStart) &&(steepness>=details[i].minSlope && steepness <= details[i].maxSlope)){
                        detailMap[y,x] =(int) UnityEngine.Random.Range(minDetailMapValue,maxDetailMapValue);
                    }
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
        
    }
    public void AddNewDetail(){
        details.Add(new Detail());
    }
    public void RemoveDetail(){
        List<Detail> keptDetail = new List<Detail>();
        for(int i=0;i<details.Count;i++){
            if(!details[i].remove){
                keptDetail.Add(details[i]);
            }
        }
        if(keptDetail.Count ==0){
            keptDetail.Add(details[0]);
        }
        details = keptDetail;
    }

    public void vegetation(){
        TreePrototype[] newTreePrototype;
        newTreePrototype = new TreePrototype[vegetations.Count];
        int tindex = 0;
        foreach(Vegetation t in vegetations){
            newTreePrototype[tindex] = new TreePrototype();
            newTreePrototype[tindex].prefab = t.trees;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototype;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        int tah = terrainData.alphamapHeight;
        int taw = terrainData.alphamapWidth;

        for(int z=0;z<tah;z += spacing){
            for(int x=0;x<taw;x+=spacing){
                for(int tp=0;tp<terrainData.treePrototypes.Length;++tp){
                    if(UnityEngine.Random.Range(0f,1f)>vegetations[tp].density) break;
                    float thisHeight = terrainData.GetHeight(x,z)/terrainData.size.y;
                    float thisHeightStart = vegetations[tp].minHeight;
                    float thisHeightEnd = vegetations[tp].maxHeight;

                    float normX = x* 1f/(taw-1);
                    float normY = z*1f/(tah-1);
                    float steepness = terrainData.GetSteepness(x,z);

                    if((thisHeight>= thisHeightStart && thisHeight<=thisHeightEnd) && (steepness>= vegetations[tp].minSlope && steepness <= vegetations[tp].maxSlope)){
                    
                        TreeInstance instance = new TreeInstance();
                        thisHeight = terrainData.GetHeight(x,z)/terrainData.size.y;

                        instance.position = new UnityEngine.Vector3((x+UnityEngine.Random.Range(-5.0f,5.0f))/taw,thisHeight,(z+UnityEngine.Random.Range(-5.0f,5.0f))/tah);
                        UnityEngine.Vector3 treeWorldPos = new UnityEngine.Vector3(instance.position.x*terrainData.size.x,instance.position.y*terrainData.size.y,instance.position.z * terrainData.size.z);
                        
                        RaycastHit hit;
                        int LayerMask = 1 <<terrainLayer;
                        if(Physics.Raycast(treeWorldPos + new UnityEngine.Vector3(0,10,0),-Vector3.up,out hit,100,LayerMask )||Physics.Raycast(treeWorldPos - new UnityEngine.Vector3(0,10,0),Vector3.up,out hit,100,LayerMask ) ){
                            float treeHeight = (hit.point.y - this.transform.position.y)/terrainData.size.y;
                            instance.position = new Vector3(instance.position.x,treeHeight,instance.position.z);
                        }

                        instance.rotation = UnityEngine.Random.Range(vegetations[tp].minRotation,vegetations[tp].maxRotation);
                        instance.prototypeIndex = tp;
                        instance.color = Color.Lerp(vegetations[tp].colour1,vegetations[tp].colour2,UnityEngine.Random.Range(0f,1f));
                        instance.lightmapColor = vegetations[tp].lightColour;
                        float s = UnityEngine.Random.Range(vegetations[tp].minScale,vegetations[tp].maxScale);
                        instance.heightScale = s;
                        instance.widthScale = s;
                        allVegetation.Add(instance);
                        if(allVegetation.Count>treeCount) goto TREESDONE;
                    }
                }
            }

        }
        TREESDONE:
            terrainData.treeInstances = allVegetation.ToArray();
    }
    public void AddNewVegetation(){
        vegetations.Add(new Vegetation());
    }
    public void RemoveVegetation(){
        List<Vegetation> keptVegetation = new List<Vegetation>();
        for(int i=0;i<vegetations.Count;i++){
            if(!vegetations[i].remove){
                keptVegetation.Add(vegetations[i]);
            }
        }
        if(keptVegetation.Count ==0){
            keptVegetation.Add(vegetations[0]);
        }
        vegetations = keptVegetation;
    }

    public void SplatMap(){
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spIndex = 0;

        foreach(SplatHeights sh in splatHeights){
            newSplatPrototypes[spIndex] = new TerrainLayer();
            newSplatPrototypes[spIndex].diffuseTexture = sh.texture;
            newSplatPrototypes[spIndex].normalMapTexture = sh.textureNormalMap;
            newSplatPrototypes[spIndex].diffuseTexture.Apply(true);
            newSplatPrototypes[spIndex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spIndex].tileSize = sh.tileSize;
            string path = "Assets/New TerrainLayer "+ spIndex+".terrainlayer";
            AssetDatabase.CreateAsset(newSplatPrototypes[spIndex],path);
            spIndex++;
            Selection.activeObject = this.gameObject; 
        }
        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = GetHeightMap();
        float[,,] splatMapData = new float[terrainData.alphamapWidth,terrainData.alphamapHeight,terrainData.alphamapLayers];

        for(int y =0;y<terrainData.alphamapHeight;y++){
            for(int x=0;x<terrainData.alphamapWidth;x++){
                float[] splat = new float[terrainData.alphamapWidth];
                bool emptySplat = true;
                for(int i=0;i<splatHeights.Count;i++){

                    float noise = Mathf.PerlinNoise(x*splatHeights[i].splatNoiseXScale,y*splatHeights[i].splatNoiseYScale)*splatHeights[i].splatNoiseZScale;
                    float offset = splatHeights[i].splatOffset+noise;

                    float thisHeightStart = splatHeights[i].minHeight-offset;
                    float thisHeightStop = splatHeights[i].maxHeight+offset;

                    int hmX = x*((terrainData.heightmapResolution-1)/terrainData.alphamapWidth);
                    int hmY = y*((terrainData.heightmapResolution-1)/terrainData.alphamapHeight);

                    float normX = x*1f/(terrainData.alphamapWidth-1);
                    float normY = y*1f/(terrainData.alphamapHeight-1);

                    var steepness = terrainData.GetSteepness(normX,normY);

                    if((heightMap[hmX,hmY]>= thisHeightStart && heightMap[hmX,hmY] <= thisHeightStop)&&
                        (steepness>=splatHeights[i].minSlope && steepness<= splatHeights[i].maxSlope)){

                        if(heightMap[hmX,hmY]<= splatHeights[i].minHeight){                                  //without the (1-X)the mixing is not clean 
                            splat[i] = 1-Mathf.Abs(heightMap[hmX,hmY]-splatHeights[i].minHeight)/offset;
                        }
                        else if(heightMap[hmX,hmY]>= splatHeights[i].maxHeight){
                            splat[i] = 1-Mathf.Abs(heightMap[hmX,hmY]-splatHeights[i].maxHeight)/offset;
                        }
                        else{
                           splat[i]=1;
                        }
                        emptySplat = false;
                    }

                }
                NormalizeVector(ref splat);
                if(emptySplat){
                    splatMapData[x,y,0] = 1;
                }
                else{
                    for(int j=0;j<splatHeights.Count;j++){
                        splatMapData[x,y,j] = splat[j];
                    }
                }
            }
            terrainData.SetAlphamaps(0,0,splatMapData);
        }

    }
    void NormalizeVector(ref float[] splat){
        float total = 0.0f;
        for(int i=0;i<splat.Length;i++){
            total+=splat[i];
        }
        if(total==0) return;
        for(int i=0;i<splat.Length;i++){
            splat[i]/= total;
        }
    }

    public void AddnewSplatHeight(){
        splatHeights.Add(new SplatHeights());
    }
    public void RemoveSplatHeight(){
        List<SplatHeights> keepSplatHeights = new List<SplatHeights>();
        for(int i =0;i<splatHeights.Count;++i){
            if(!splatHeights[i].remove){
                keepSplatHeights.Add(splatHeights[i]);
            }
        }
        if(keepSplatHeights.Count ==0){
            keepSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keepSplatHeights;
    }

    public void Perlin(){
        float[,] heightMap = terrainData.GetHeights(0,0,terrainData.heightmapResolution,terrainData.heightmapResolution);
        for(int y = 0;y<terrainData.heightmapResolution;y++){
            for(int x = 0;x<terrainData.heightmapResolution;x++){
                heightMap[x,y] += Util.fBM((x+perlinOffsetX)*perlinXScale,(y+perlinOffsetY)*perlinYScale,perlinOctaves,perlinPersistance)*perlinHeightScale;
            }
        }
        terrainData.SetHeights(0,0,heightMap);
    }
    public void MultiplePerlinTerrain(){
        float[,] heightMap = GetHeightMap();
        for(int y = 0;y<terrainData.heightmapResolution;y++){
            for(int x = 0;x<terrainData.heightmapResolution;x++){
                foreach(PerlinParameters p in perlinParameters){
                    heightMap[x,y] += Util.fBM((x+p.mPerlinOffsetX)*p.mPerlinXScale,(y+p.mPerlinOffsetY)*p.mPerlinYScale,p.mPerlinOctaves,p.mPerlinPersistance)*p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0,0,heightMap);
    }
    public void RidgeNoise(){
        float[,] heightMap = GetHeightMap();
        for(int y = 0; y<terrainData.heightmapResolution;y++){
            for(int x = 0;x<terrainData.heightmapResolution;x++){
                foreach(PerlinParameters p in perlinParameters){
                    heightMap[x,y] += Util.fBM((x+p.mPerlinOffsetX)*p.mPerlinXScale,(y+p.mPerlinOffsetY)*p.mPerlinYScale,p.mPerlinOctaves,p.mPerlinPersistance)*p.mPerlinHeightScale;
                }
                heightMap[x,y] = 1 -  Mathf.Abs(heightMap[x,y]-0.5f);
            }
        }
        terrainData.SetHeights(0,0,heightMap);

    }
    public void AddNewPerlin(){
        perlinParameters.Add(new PerlinParameters());
    }
    public void RemovePerlin(){
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for(int i = 0;i<perlinParameters.Count;i++){
            if(!perlinParameters[i].remove){
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if(keptPerlinParameters.Count== 0){
            keptPerlinParameters.Add(perlinParameters[0]);
        }
        perlinParameters = keptPerlinParameters;
    }
    public void RandomTerrain(){
        float[,] heightMap = GetHeightMap();
        for(int x = 0;x<terrainData.heightmapResolution;++x){
            for(int z = 0;z<terrainData.heightmapResolution;++z){
                heightMap[x,z] += UnityEngine.Random.Range(randomHeightRange.x,randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0,0,heightMap);
    }
    public void LoadTexture(){
        float[,] heightMap = GetHeightMap();
        for(int x = 0;x<terrainData.heightmapResolution;++x){
            for(int z = 0;z<terrainData.heightmapResolution;++z){
                heightMap[x,z] += heightMapImage.GetPixel((int)(x*heightMapScale.x),(int)(z*heightMapScale.z)).grayscale*heightMapScale.y;
                
            }
        }
        terrainData.SetHeights(0,0,heightMap);
    }
    public void Voronoi(){
        float[,] heightMap = GetHeightMap();
        //UnityEngine.Vector3 peak = new UnityEngine.Vector3(256,0.2f,256);   
        //UnityEngine.Vector3 peak = new UnityEngine.Vector3(Random.Range(1,terrainData.heightmapResolution),Random.Range(0,1f),Random.Range(0,terrainData.heightmapResolution));
        //heightMap[(int)peak.x,(int)peak.z] = peak.y;
        // Vector2 peakLocation = new Vector2(peak.x,peak.z);
        // float maxDistance = Vector2.Distance(new Vector2(0,0),new Vector2(terrainData.heightmapResolution,terrainData.heightmapResolution));
        // for(int x = 0;x<terrainData.heightmapResolution;x++){
        //     for(int z = 0;z<terrainData.heightmapResolution;z++){
        //         float distanceToPeak = Vector2.Distance(peakLocation,new Vector2(x,z))/maxDistance;
        //         float h  = peak.y - (distanceToPeak)*fallof-Mathf.Pow((distanceToPeak),dropoff);
        //         heightMap[x,z] = h;
        //     }
        // }
        for(int i = 0;i<peakCount;i++){
            UnityEngine.Vector3 peak = new UnityEngine.Vector3(UnityEngine.Random.Range(1,terrainData.heightmapResolution),UnityEngine.Random.Range(minHeight,maxHeight),UnityEngine.Random.Range(1,terrainData.heightmapResolution));
            if(heightMap[(int)peak.x,(int)peak.z]<peak.y){
                heightMap[(int)peak.x,(int)peak.z]=peak.y;
            }
            else{
                continue;
            }
            heightMap[(int) peak.x,(int) peak.z] = peak.y;
            Vector2 peakLocation = new Vector2(peak.x,peak.z);
            float maxDistance = Vector2.Distance (new Vector2(0,0), new Vector2(terrainData.heightmapResolution,terrainData.heightmapResolution));
            for(int j = 0 ; j<terrainData.heightmapResolution;j++){
                for(int k = 0;k<terrainData.heightmapResolution;k++){
                    if(!(peak.x == j && peak.z == j)){
                        float distanceToPeak = Vector2.Distance(peakLocation,new Vector2(j,k))/maxDistance;
                        float h;
                        if(voronoiType == VoronoiType.Combined){
                            h = peak.y - distanceToPeak* fallof - Mathf.Pow(distanceToPeak,dropoff);
                        }
                        else if(voronoiType == VoronoiType.Power){
                            h= peak.y - Mathf.Pow(distanceToPeak,dropoff)*fallof;
                        }
                        else if(voronoiType == VoronoiType.Mountain){
                            h = peak.y - Mathf.Pow(distanceToPeak*3,fallof)- Mathf.Sin(distanceToPeak*2*Mathf.PI)/dropoff;
                        }
                        else if(voronoiType == VoronoiType.Perlin){
                            h = (peak.y -distanceToPeak*fallof)+ Util.fBM((j+perlinOffsetX)*perlinXScale,(k+perlinOffsetY)*perlinYScale,perlinOctaves,perlinPersistance)*perlinHeightScale;
                        }
                        else{
                            h = peak.y  - distanceToPeak* fallof;
                        }
                        
                        if(heightMap[j,k]< h){
                            heightMap[j,k] = h;
                        }
                    }
                }
            }
        }
        terrainData.SetHeights(0,0,heightMap); 
    
    }
    public void MPD(){
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution-1;
        int squareSize = width;
        float height = MPDheight;
        float dampener = (float)Mathf.Pow(MPDpow,-1*MPDroughness);

        int cornerX, cornerY;
        int midX,midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        // heightMap[0,0] = UnityEngine.Random.Range(0f,0.2f);
        // heightMap[0,terrainData.heightmapResolution-2] = UnityEngine.Random.Range(0f,0.2f);
        // heightMap[terrainData.heightmapResolution-2,0] = UnityEngine.Random.Range(0f,0.2f);
        // heightMap[terrainData.heightmapResolution-2,terrainData.heightmapResolution-2] = UnityEngine.Random.Range(0f,0.2f);

        while(squareSize>0){
            for(int x=0;x<width;x+=squareSize){
                for(int y=0;y<width;y+=squareSize){
                    cornerX = x+squareSize;
                    cornerY = y+squareSize;

                    midX = (int)((x+cornerX)/2.0f);
                    midY = (int)((y+cornerY)/2.0f);
                    heightMap[midX,midY]= (float)(heightMap[x,y]+heightMap[x,cornerY]+heightMap[cornerX,y]+heightMap[cornerX,cornerY])/4f +UnityEngine.Random.Range(-height,height);


                }
            }
            for(int x=0;x<width;x+=squareSize){
                for(int y=0;y<width;y+=squareSize){
                    cornerX = x+squareSize;
                    cornerY = y+squareSize;                             

                    midX = (int)((x+cornerX)/2.0f);
                    midY = (int)((y+cornerY)/2.0f);
                    
                    pmidXR = (int)(midX+squareSize);
                    pmidYU = (int)(midY+squareSize);
                    pmidXL = (int)(midX-squareSize);
                    pmidYD = (int)(midY-squareSize);

                    if(pmidXL<=0 || pmidYD <+0 || pmidXR>= width -1 || pmidYU >= width-1){
                        continue;
                    }

                    heightMap[midX,y]= (float)(heightMap[x,y]+heightMap[midX,midY]+heightMap[midX,pmidYD]+heightMap[cornerX,y])/4f +UnityEngine.Random.Range(-height,height);
                    heightMap[cornerX,midY]= (float)(heightMap[cornerX,cornerY]+heightMap[midX,midY]+heightMap[pmidXR,midY]+heightMap[cornerX,y])/4f +UnityEngine.Random.Range(-height,height);
                    heightMap[midX,cornerY]= (float)(heightMap[cornerX,cornerY]+heightMap[midX,midY]+heightMap[x,cornerY]+heightMap[midX,pmidYU])/4f +UnityEngine.Random.Range(-height,height);
                    heightMap[x,midY]= (float)(heightMap[x,y]+heightMap[midX,midY]+heightMap[pmidXL,midY]+heightMap[x,cornerY])/4f +UnityEngine.Random.Range(-height,height);

                }
            }
            squareSize = (int)(squareSize/2.0f);
            height *= dampener;
        }
        terrainData.SetHeights(0,0,heightMap);

    }
    public void Smooth(){
        float[,] heightMap =terrainData.GetHeights(0,0,terrainData.heightmapResolution,terrainData.heightmapResolution);
        int smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing", "Progress", smoothProgress);
        for(int i = 0;i<smoothAmount;i++){
            for(int x = 0;x<terrainData.heightmapResolution;x++){
                for(int y= 0;y<terrainData.heightmapResolution;y++){
                    if(x-1<0 ||y-1<0 ||x+1>terrainData.heightmapResolution-1 ||y+1>terrainData.heightmapResolution-1){
                        continue;                                   //For now the corner cases are neglected
                    }
                    heightMap[x,y] = (float)(heightMap[x,y]+heightMap[x-1,y]+heightMap[x-1,y+1]+heightMap[x-1,y-1]+heightMap[x,y-1]+heightMap[x,y+1]+heightMap[x+1,y-1]+heightMap[x+1,y]+heightMap[x+1,y+1])/9f;
                }
            
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing","Progress",smoothProgress/smoothAmount);
        }
        terrainData.SetHeights(0,0,heightMap);
        EditorUtility.ClearProgressBar();
    }

    public void DefaultTerrain(){
        float[,] heightMap = new float[terrainData.heightmapResolution,terrainData.heightmapResolution];
        // float[,] heightMap = GetHeightMap();
        // for(int x = 0;x<terrainData.heightmapResolution;++x){
        //     for(int z = 0;z<terrainData.heightmapResolution;++z){
        //         heightMap[x,z] = 0;
        //     }
        // }
        terrainData.SetHeights(0,0,heightMap);
    }

    public void ResetTerrain(){
    
    DefaultTerrain(); // Resets the terrain to a default state
    Perlin(); // Apply Perlin noise or any other procedural generation
    }
    

    public enum TagType {Tag =0,Layer=1}
    [SerializeField]
    int terrainLayer =-1;
    void Start()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags"); 

        AddTag(tagsProp,"Terrain",TagType.Tag);
        AddTag(tagsProp,"Cloud",TagType.Tag);
        AddTag(tagsProp,"Shore",TagType.Tag);
        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp,"Terrain",TagType.Layer);
        tagManager.ApplyModifiedProperties();
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;


    }

    int AddTag(SerializedProperty tagsProp,string newTag,TagType tType){
        bool found = false;
        for(int i = 0; i< tagsProp.arraySize;i++){
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);  
            if(t.stringValue.Equals(newTag)){
                found = true;
                return i;
                }
        }
        if(!found && tType ==TagType.Tag){
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newtagProp = tagsProp.GetArrayElementAtIndex(0);
            newtagProp.stringValue = newTag;

        }
        else if(!found &&tType == TagType.Layer){
            for(int j=8;j<tagsProp.arraySize;j++){
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                if(newLayer.stringValue ==""){
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
