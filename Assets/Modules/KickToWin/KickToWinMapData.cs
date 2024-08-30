using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Playbux/KickToWin/KickToWinMap", fileName = "KickToWinMapInstaller")]
public class KickToWinMapData : ScriptableObject
{
    [SerializeField]
    private Texture2D kickToWinMap;

    [SerializeField]
    private Vector2[] kickPositionOnMap;
   
    [SerializeField]
    private Vector2[] kickPositionOnWorld;

    public Vector2[] KickPositionOnWorld { get => kickPositionOnWorld; set => kickPositionOnWorld = value; }
    public Texture2D KickToWinMap { get => kickToWinMap; set => kickToWinMap = value; }
    public Vector2[] KickPositionOnMap { get => kickPositionOnMap; set => kickPositionOnMap = value; }
}

#if UNITY_EDITOR
[CustomEditor(typeof(KickToWinMapData))]
public class KickToWinMapDataEditor : Editor
{
    

    public override void OnInspectorGUI()
    {
        KickToWinMapData map = (KickToWinMapData)target;
        if (GUILayout.Button("Generate Position"))
        {
            GetPosXY();
            
            AssetDatabase.Refresh();
        }

        /*if (GUILayout.Button("Reassign from Folder [Bux Parts]"))
        {

            
            EditorUtility.SetDirty(map);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }*/
        DrawDefaultInspector();
    }

    public void GetPosXY()
    {
        KickToWinMapData map = (KickToWinMapData)target;
        var data = map.KickToWinMap.GetPixels();
        List<Vector2> mapPos = new List<Vector2>();
        List<Vector2> worldPos = new List<Vector2>();
        //Debug.Log(data[data.Length-1].linear);
        for (int i = 0; i < data.Length; i+=1)
        {
            var x = i % map.KickToWinMap.width;
            var y = i / map.KickToWinMap.width;
            if (data[i] == Color.red)
            {
                mapPos.Add(new Vector2(x, y));
                var worldPosConvert = ConvertMinimapToWorld(new Vector2(-32.74f, 80.76f), new Vector2(76.25665f, -138.6033f), new Vector2(1264.5f, 2047f-645f), new Vector2(569f, 2047f-2014f), new Vector2(x, y));
                //var worldPosConvert = ConvertMinimapToWorld(new Vector2(-33.33719f, 83.64428f), new Vector2(76.47823f, -138.5925f), new Vector2(0.36537f, 0.4404099f), new Vector2(0.0249f, -0.2307776f), new Vector2(normalizedX, normalizedY));
                worldPos.Add(worldPosConvert);
            }
        }
        map.KickPositionOnMap =  mapPos.ToArray();   
        map.KickPositionOnWorld = worldPos.ToArray();
    }

    public Vector2 ConvertMinimapToWorld(Vector2 worldPos1, Vector2 worldPos2,
                                                Vector2 minimapPos1, Vector2 minimapPos2,
                                                Vector2 targetMinimapPos)
    {
        // Calculate transformation coefficients
        float a = (minimapPos1.x - minimapPos2.x) / (worldPos1.x - worldPos2.x);
        float b = minimapPos1.x - a * worldPos1.x;
        float c = (minimapPos1.y - minimapPos2.y) / (worldPos1.y - worldPos2.y);
        float d = minimapPos1.y - c * worldPos1.y;

        // Apply inverse transformation to target minimap position
        float targetWorldX = (targetMinimapPos.x - b) / a;
        float targetWorldY = (targetMinimapPos.y - d) / c;

        return new Vector2(targetWorldX, targetWorldY);
    }

}



#endif
