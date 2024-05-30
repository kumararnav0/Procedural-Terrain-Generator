using UnityEditor;
using UnityEngine;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;
    
    SerializedProperty dropOff;
    SerializedProperty fallOff;
    SerializedProperty minHeight;
    SerializedProperty maxHeight;
    SerializedProperty peakCount;
    SerializedProperty voronoiType;
    SerializedProperty MPDheight;
    SerializedProperty MPDroughness;
    SerializedProperty MPDpow;
    SerializedProperty smoothAmount;
    SerializedProperty treeCount;
    SerializedProperty spacing;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    GUITableState vegetationsTable;
    SerializedProperty vegetations;

    GUITableState detailTable;
    SerializedProperty details;


    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMPD = false;
    bool showSmooth = false;
    bool showSplatMaps = false;
    bool showvegetation = false;
    bool showDetail = false;
    private void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinHeightScale= serializedObject.FindProperty("perlinHeightScale");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        dropOff = serializedObject.FindProperty("dropoff");
        fallOff = serializedObject.FindProperty("fallof");
        peakCount = serializedObject.FindProperty("peakCount");
        minHeight = serializedObject.FindProperty("minHeight");
        maxHeight = serializedObject.FindProperty("maxHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");
        MPDheight = serializedObject.FindProperty("MPDheight");
        MPDroughness = serializedObject.FindProperty("MPDroughness");
        MPDpow = serializedObject.FindProperty("MPDpow");
        smoothAmount = serializedObject.FindProperty("smoothAmount");
        splatHeights = serializedObject.FindProperty("splatHeights");
        splatMapTable = new GUITableState("splatMapTable");
        treeCount = serializedObject.FindProperty("treeCount");
        spacing = serializedObject.FindProperty("spacing");
        vegetationsTable = new GUITableState("vegetationTable");
        vegetations = serializedObject.FindProperty("vegetations");
        detailTable = new GUITableState("detailTable");
        details = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetail");
        detailSpacing = serializedObject.FindProperty("detailSpacing");

    }
    public override void OnInspectorGUI(){
        serializedObject.Update();
        CustomTerrain terrain = (CustomTerrain) target;

        EditorGUILayout.PropertyField(resetTerrain);
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if(showRandom){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values",EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if(GUILayout.Button("Random Heights")){
                terrain.RandomTerrain();
            }   
        }
        
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights,"Load Heights");
        if(showLoadHeights){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights Between Random Values",EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if(GUILayout.Button("Load Textures")){
                terrain.LoadTexture();
            }
        }
        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Perlin Noise");
        if(showPerlinNoise){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin Noise",EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale,0,1,new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale,0,1,new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX,0,10000,new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinOffsetY,0,10000,new GUIContent("Offset Y"));
            EditorGUILayout.IntSlider(perlinOctaves,0,10,new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance,0.1f,10,new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale,0,1,new GUIContent("Height Scale"));

            if(GUILayout.Button("Generate")){
                terrain.Perlin();
            }
        }
        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin,"Multiple Perlin Noise");
        if(showMultiplePerlin){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise",EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable,serializedObject.FindProperty("perlinParameters"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+")){
                terrain.AddNewPerlin();
            }
            if(GUILayout.Button("-")){
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Multiple Perlin")){
                terrain.MultiplePerlinTerrain();
            }
            if(GUILayout.Button("Apply Ridge Noise")){
                terrain.RidgeNoise();
            }
        }
        showVoronoi = EditorGUILayout.Foldout(showVoronoi,"Voronoi");
        if(showVoronoi){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(dropOff,0,10,new GUIContent("Drop Off"));
            EditorGUILayout.Slider(fallOff,0,10,new GUIContent("Fall Off"));
            EditorGUILayout.IntSlider(peakCount,1,10,new GUIContent("Peak Count")) ;
            EditorGUILayout.Slider(minHeight,0,1,new GUIContent("Minumum Height"));
            EditorGUILayout.Slider(maxHeight,0,1,new GUIContent("Maximum Height"));
            EditorGUILayout.PropertyField(voronoiType);
            if(GUILayout.Button("Voronoi")){
                terrain.Voronoi();
            }
        }
        showMPD = EditorGUILayout.Foldout(showMPD,"Midpoint Displacement");
        if(showMPD){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(MPDpow,0,10,new GUIContent("MPD Dampener Power"));
            EditorGUILayout.Slider(MPDheight,0,2,new GUIContent("Height Range"));
            EditorGUILayout.Slider(MPDroughness,0,10,new GUIContent("Roughness"));
            if(GUILayout.Button("MPD")){
                terrain.MPD();
            }
        }
        showSmooth = EditorGUILayout.Foldout(showSmooth,"Smooth");
        if(showSmooth){
            EditorGUILayout.IntSlider(smoothAmount,1,10, new GUIContent("Smooth Amount"));
            if(GUILayout.Button("Smooth")){
                terrain.Smooth();
            }
        }

        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps,"Splat Maps");
        if(showSplatMaps){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Splat Maps",EditorStyles.boldLabel);
            splatMapTable = GUITableLayout.DrawTable(splatMapTable,serializedObject.FindProperty("splatHeights"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+")){
                terrain.AddnewSplatHeight();
            }
            if(GUILayout.Button("-")){
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply SplatMaps")){
                terrain.SplatMap();
            }
        }

        showvegetation = EditorGUILayout.Foldout(showvegetation,"Vegetation");
        if(showvegetation){
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(treeCount,0,1000, new GUIContent("Tree Count"));
            EditorGUILayout.IntSlider(spacing,0,10,new GUIContent("Tree Spacing"));
            vegetationsTable = GUITableLayout.DrawTable(vegetationsTable,serializedObject.FindProperty("vegetations"));
            
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+")){
                terrain.AddNewVegetation();
            }
            if(GUILayout.Button("-")){
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Vegetation")){
                terrain.vegetation();
            }
        }
        showDetail = EditorGUILayout.Foldout(showDetail,"Detail");
        if(showDetail){
            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(maxDetails,0,10000,new GUIContent("Maximum Details"));
            EditorGUILayout.IntSlider(detailSpacing,1,10,new GUIContent("Details Sapcing"));
            detailTable = GUITableLayout.DrawTable(detailTable,serializedObject.FindProperty("details"));
            GUILayout.Space(20);
            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+")){
                terrain.AddNewDetail();
            }
            if(GUILayout.Button("-")){
                terrain.RemoveDetail();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Details")){
                terrain.addDetail();
            }
        }

        if(GUILayout.Button("Reset Default")){
            terrain.DefaultTerrain();
        }

        

        serializedObject.ApplyModifiedProperties();
    }
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
