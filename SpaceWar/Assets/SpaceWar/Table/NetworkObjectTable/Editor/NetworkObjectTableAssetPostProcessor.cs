using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityQuickSheet;

///
/// !!! Machine generated code !!!
///
public class NetworkObjectTableAssetPostprocessor : AssetPostprocessor 
{
    private static readonly string filePath = "Assets/SpaceWar/Table/GameInfoTable.xlsx";
    private static readonly string assetFilePath = "Assets/SpaceWar/Table/NetworkObjectTable.asset";
    private static readonly string sheetName = "NetworkObjectTable";
    
    static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets) 
        {
            if (!filePath.Equals (asset))
                continue;
                
            NetworkObjectTable data = (NetworkObjectTable)AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(NetworkObjectTable));
            if (data == null) {
                data = ScriptableObject.CreateInstance<NetworkObjectTable> ();
                data.SheetName = filePath;
                data.WorksheetName = sheetName;
                AssetDatabase.CreateAsset ((ScriptableObject)data, assetFilePath);
                //data.hideFlags = HideFlags.NotEditable;
            }
            
            //data.dataArray = new ExcelQuery(filePath, sheetName).Deserialize<NetworkObjectTableData>().ToArray();		

            //ScriptableObject obj = AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(ScriptableObject)) as ScriptableObject;
            //EditorUtility.SetDirty (obj);

            ExcelQuery query = new ExcelQuery(filePath, sheetName);
            if (query != null && query.IsValid())
            {
                data.dataArray = query.Deserialize<NetworkObjectTableData>().ToArray();
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(ScriptableObject)) as ScriptableObject;
                EditorUtility.SetDirty (obj);
            }
        }
    }
}
