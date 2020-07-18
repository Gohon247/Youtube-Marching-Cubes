using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGen))]
public class MapGeneratorEditor : Editor //adds generate map to editor and autoupdate
{
    public override void OnInspectorGUI()
    {
        MapGen mapGen = (MapGen)target;
        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.Run();
            }
        }
        if (GUILayout.Button("Generate")) //only will update when generate clicked
        {
            mapGen.Run();
        }
    }
}
