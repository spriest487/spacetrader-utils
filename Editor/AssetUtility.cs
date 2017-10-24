using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util
{
    public static class AssetUtility
    {
        public static void SaveOrUpdate<TAsset>(this TAsset asset, string path)
            where TAsset : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            if (existing)
            {
                var srcObj = new SerializedObject(asset);
                var dstObj = new SerializedObject(existing);

                var propIt = srcObj.GetIterator();
                if (propIt.Next(true))
                {
                    while (propIt.Next(false))
                    {
                        dstObj.CopyFromSerializedProperty(propIt);
                    }
                }
                dstObj.ApplyModifiedProperties();
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}
