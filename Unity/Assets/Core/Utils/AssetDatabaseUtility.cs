using UnityEngine;
using UnityEditor;

public class AssetDatabaseUtility
{
    public static T GetAssetAtPath<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

        public static T GetAssetByName<T>(string assetName, string rootPath ="") where T:Object
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(rootPath);

        foreach (Object asset in assets)
        {
            if (asset.name == assetName)
            {
                return (T) asset;
            }
        }

        return null; 
    }
}