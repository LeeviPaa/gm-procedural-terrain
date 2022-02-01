using System.Collections;
using System.Collections.Generic;
using Procedural.Terrain;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;
        
        if(DrawDefaultInspector() && mapGen.AutoUpdate)
            mapGen.DrawMapInEditor();

        if(GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
