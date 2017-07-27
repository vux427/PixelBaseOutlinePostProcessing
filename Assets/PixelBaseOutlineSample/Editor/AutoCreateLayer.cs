using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public class AutoCreateLayer : Editor {

    static AutoCreateLayer()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        float count = 0;
        while (layers.NextVisible(true))
        {
            if (layers.name == "data") {

                if (layers.stringValue == "Outline")
                    return;

                if (layers.stringValue == ""  && count > 8)
                {
                    //Debug.Log(layers.stringValue);
                    layers.stringValue = "Outline";
                    tagManager.ApplyModifiedProperties();

                    return;
                }
            }
            count++;
        }
    }
}
