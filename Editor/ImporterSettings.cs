using System.Collections.Generic;
using UnityEngine;

namespace RGP.Ionicons.Editor
{
    public enum GraphicType
    {
        Outline,
        Filled,
        Sharp
    }
    
    [CreateAssetMenu(fileName = "ImporterSettings", menuName = "Ionicons Importer/ImporterSettings")]
    public class ImporterSettings : ScriptableObject
    {
        public string searchFilter;
        public GraphicType graphicType;
        public int imageAmount;
        public int currentPage;
        
        public List<Sprite> selectedSprites = new List<Sprite>();
        public int currentDeselectionPage;
        public string targetPath;
        public int resolution;
        public int antiAliasing;

        public bool deleteSvgFolder;
        public bool unloadVectorGraphicPackage;

        public ImporterSettings()
        {
            Reset();
        }

        public void Reset()
        {
            searchFilter = string.Empty;
            graphicType = GraphicType.Filled;
            imageAmount = 8;
            currentPage = 0;
            
            targetPath = string.Empty;
            currentDeselectionPage = 0;
            resolution = 64;
            antiAliasing = 4;
            
            deleteSvgFolder = true;
            unloadVectorGraphicPackage = true;
        }
    }
}
