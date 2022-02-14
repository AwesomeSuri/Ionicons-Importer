using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Taken from com.unity.2d.animation package
    /// </summary>
    public class SpriteSelectionWidget
    {
        private class Styles
        {
            public readonly GUIStyle GridListStyle;

            public Styles()
            {
                GridListStyle = new GUIStyle("GridList")
                {
                    alignment = GUI.skin.button.alignment
                };
            }
        }

        private Sprite[] _spriteList;
        private Texture[] _spritePreviews;
        private readonly List<int> _spritePreviewNeedFetching = new List<int>();
        private Vector2 _scrollPos;
        private Styles _style;
        private const int KTargetPreviewSize = 64;

        public void UpdateContents(Sprite[] sprites)
        {
            _spriteList = sprites;
            _spritePreviews = new Texture[sprites.Length];
            for (int i = 0; i < _spritePreviews.Length; ++i)
                _spritePreviewNeedFetching.Add(i);
            UpdateSpritePreviews();
        }

        public int ShowGUI(int selectedIndex)
        {
            _style ??= new Styles();

            UpdateSpritePreviews();

            if (_spriteList == null || _spriteList.Length == 0)
                return selectedIndex;

            selectedIndex = (selectedIndex > _spriteList.Length) ? 0 : selectedIndex;

            using var topRect = new EditorGUILayout.HorizontalScope();
            //GUILayout.Label(Styles.spriteList, EditorStyles.label, new [] {GUILayout.Width(EditorGUIUtility.labelWidth - 5)});
            using var selectionGridRect =
                new EditorGUILayout.HorizontalScope("box", new[] { GUILayout.ExpandWidth(true) });
            {
                GetRowColumnCount(EditorGUIUtility.currentViewWidth, KTargetPreviewSize, _spriteList.Length,
                    out var columnCount, out var rowCount, out var columnF);
                if (columnCount > 0 && rowCount > 0)
                {
                    float contentSize = (columnF * KTargetPreviewSize) / columnCount;

                    if (rowCount >= 2)
                        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUIStyle.none,
                            GUI.skin.verticalScrollbar, GUILayout.Height(contentSize * 1.2f));

                    _style.GridListStyle.fixedWidth = contentSize;
                    _style.GridListStyle.fixedHeight = contentSize;
                    selectedIndex = ContentSelectionGrid(selectedIndex, _spriteList, _style.GridListStyle,
                        columnCount - 1);
                    if (rowCount >= 2)
                        EditorGUILayout.EndScrollView();
                }
            }

            return selectedIndex;
        }

        private static void GetRowColumnCount(float drawWidth, int size, int contentCount, out int column, out int row,
            out float columnFloat)
        {
            columnFloat = (drawWidth) / size;
            column = (int)columnFloat;
            if (column == 0)
                row = 0;
            else
                row = (int)Mathf.Ceil((contentCount + column - 1f) / column);
        }

        private int ContentSelectionGrid(int selected, Sprite[] contents, GUIStyle style, int columnCount)
        {
            if (contents != null && contents.Length != 0)
            {
                selected = GUILayout.SelectionGrid(selected, _spritePreviews, columnCount, style);
            }

            return selected;
        }

        public bool NeedUpdatePreview()
        {
            return _spritePreviewNeedFetching.Count > 0;
        }

        void UpdateSpritePreviews()
        {
            for (int i = 0; i < _spritePreviewNeedFetching.Count; ++i)
            {
                var index = _spritePreviewNeedFetching[i];
                if (_spriteList[index] == null)
                    _spritePreviews[index] = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
                else
                    _spritePreviews[index] = AssetPreview.GetAssetPreview(_spriteList[index]);
                if (_spritePreviews[index] != null)
                {
                    _spritePreviewNeedFetching.RemoveAt(i);
                    --i;
                }
            }
        }
    }
}