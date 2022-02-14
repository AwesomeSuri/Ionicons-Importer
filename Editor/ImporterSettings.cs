using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    public enum GraphicType
    {
        Outline,
        Filled,
        Sharp
    }
    
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

        public bool showConvertAll;

        public ImporterSettings()
        {
            Reset();
        }

        public void Reset()
        {
            searchFilter = string.Empty;
            graphicType = GraphicType.Filled;
            imageAmount = 8;
            currentPage = 1;
            
            targetPath = string.Empty;
            currentDeselectionPage = 1;
            resolution = 64;
            antiAliasing = 4;

            showConvertAll = false;
        }
    }
}
