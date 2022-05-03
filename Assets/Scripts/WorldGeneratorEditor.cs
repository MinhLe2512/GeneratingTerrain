using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (WorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WorldGenerator world = (WorldGenerator)target;

        //If any value is change
        if (DrawDefaultInspector()) { 
            if(world.autoUpdate) {
                world.GenerateWorld();
            }       
        }

        //Add button to inspector
        if (GUILayout.Button("Generate")) {
            world.GenerateWorld();
        }
    }
}
