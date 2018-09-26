using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityQuickSheet;

///
/// !!! Machine generated code !!!
///
public class PlanetObstacleTableAssetPostprocessor : AssetPostprocessor 
{
    private static readonly string filePath = "Assets/SpaceWar/Table/GameInfoTable.xlsx";
    private static readonly string assetFilePath = "Assets/SpaceWar/Table/PlanetObstacleTable.asset";
    private static readonly string sheetName = "PlanetObstacleTable";
    
    static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets) 
        {
            if (!filePath.Equals (asset))
                continue;
                
            PlanetObstacleTable data = (PlanetObstacleTable)AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(PlanetObstacleTable));
            if (data == null) {
                data = ScriptableObject.CreateInstance<PlanetObstacleTable> ();
                data.SheetName = filePath;
                data.WorksheetName = sheetName;
                AssetDatabase.CreateAsset ((ScriptableObject)data, assetFilePath);
                //data.hideFlags = HideFlags.NotEditable;
            }
            
            //data.dataArray = new ExcelQuery(filePath, sheetName).Deserialize<PlanetObstacleTableData>().ToArray();		

            //ScriptableObject obj = AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(ScriptableObject)) as ScriptableObject;
            //EditorUtility.SetDirty (obj);

            ExcelQuery query = new ExcelQuery(filePath, sheetName);
            if (query != null && query.IsValid())
            {
                data.dataArray = query.Deserialize<PlanetObstacleTableData>().ToArray();
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(ScriptableObject)) as ScriptableObject;
                EditorUtility.SetDirty (obj);
            }
        }
    }
}
