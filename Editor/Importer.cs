using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RGP.Utils;
using RGP.Utils.Editor;
using RGP.Utils.Graphics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace RGP.Ionicons.Editor
{
    public class Importer : EditorWindow
    {
        [MenuItem("Window/Ionicons Importer")]
        public static void ShowWindow()
        {
            GetWindow<Importer>("Ionicons Importer");
        }

        #region Locations

        private const string IoniconsURL = "https://ionic.io/ionicons/ionicons.designerpack.zip";
        private const string IoniconsRoot = "Ionicons";
        private const string ZipName = "Ionicons.zip";
        private const string ZipPath = IoniconsRoot + "/" + ZipName;
        private const string SvgLocation = IoniconsRoot + "/SVG";
        private const string ImportSettingsName = "ImporterSettings";

        private static string ToLocalPath(string path)
        {
            return path.Substring(Application.dataPath.Length - "Assets".Length);
        }

        private static string ToGlobalPath(string resourcesPath)
        {
            return Application.dataPath + "/Resources/" + resourcesPath;
        }

        #endregion


        private static GUIStyle _headerStyle;
        private static ImporterSettings _importerSettings;
        private static readonly SpriteSelectionWidget SpriteSelectionWidget = new SpriteSelectionWidget();
        private static readonly SpriteSelectionWidget SpriteDeselectionWidget = new SpriteSelectionWidget();
        private static Vector2 _scrollPosition;


        //------------------------------------------------------------------------------------------------------------//
        private void OnGUI()
        {
            // general
            minSize = new Vector2(420, minSize.y);

            _headerStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(),
                fontStyle = FontStyle.Bold,
                fontSize = 20,
                normal = new GUIStyleState { textColor = GUI.contentColor }
            };

            if (_importerSettings == null)
            {
                _importerSettings = Resources.Load<ImporterSettings>(ImportSettingsName);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // settings reference
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Data", _importerSettings, typeof(ImporterSettings), false);
            GUI.enabled = true;

            // import svg files
            EditorGUILayout.Space();
            if (!ImportZip())
            {
                EndWindow();
                return;
            }

            // convert to png files
            EditorGUILayout.Space();
            SelectImages();

            EditorGUILayout.Space();
            CloseImporter();

            EndWindow();
        }

        private static void EndWindow()
        {
            EditorGUILayout.EndScrollView();
        }

        private static bool ImportZip()
        {
            EditorGUILayout.LabelField("1. Import SVGs", _headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Location: Assets/Resources/" + SvgLocation);

            if (GUILayout.Button("Import (~1MB, requires internet connection)"))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                var zipLocation = ToGlobalPath(IoniconsRoot);
                var zipPath = ToGlobalPath(ZipPath);
                if (!Directory.Exists(zipLocation)) Directory.CreateDirectory(zipLocation);
                EditorDownloadFromWeb
                    .DownloadFile(IoniconsURL, zipPath, DownloadCompleted);
            }

            EditorGUILayout.EndVertical();

            return Directory.Exists(ToGlobalPath(SvgLocation));
        }

        private static void DownloadCompleted(bool success)
        {
            if (success)
            {
                var zipPath = ToGlobalPath(ZipPath);
                var svgLocation = ToGlobalPath(SvgLocation);
                ExtractZip.ExtractFiles(zipPath, svgLocation, true);
            }
            else
            {
                Debug.LogError("Downloading from " + IoniconsURL + " has failed.");
            }

            AssetDatabase.Refresh();
        }

        private static void SelectImages()
        {
            EditorGUILayout.LabelField("2. Select images to convert to PNG (optional)", _headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // set filters
            var input = EditorGUILayout.TextField("Search", _importerSettings.searchFilter);
            if (!input.Equals(_importerSettings.searchFilter))
            {
                _importerSettings.searchFilter = input;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            foreach (GraphicType type in Enum.GetValues(typeof(GraphicType)))
            {
                GUI.enabled = _importerSettings.graphicType != type;
                if (GUILayout.Button(type.ToString()))
                {
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    _importerSettings.graphicType = type;
                }
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // get filtered files
            var result = SvgSearcher.GetFileNames(
                ToGlobalPath(SvgLocation),
                _importerSettings.searchFilter,
                _importerSettings.graphicType.ToString());

            // show results
            SetupSelection("Results",
                result.Length, _importerSettings.currentPage,
                p => _importerSettings.currentPage = p,
                i => Resources.Load<Sprite>(Path.Combine(SvgLocation, result[i])),
                SpriteSelectionWidget,
                s =>
                {
                    if (!_importerSettings.selectedSprites.Contains(s))
                    {
                        _importerSettings.selectedSprites.Add(s);
                        _importerSettings.currentDeselectionPage = int.MaxValue;
                    }
                });

            // show selected sprites
            SetupSelection("Selected",
                _importerSettings.selectedSprites.Count, _importerSettings.currentDeselectionPage,
                p => _importerSettings.currentDeselectionPage = p,
                i => _importerSettings.selectedSprites[i],
                SpriteDeselectionWidget,
                s => _importerSettings.selectedSprites.Remove(s));

            // setup target path
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Convert to Location");

            var defaultColor = GUI.backgroundColor;
            var invalidLocation = _importerSettings.targetPath.Length == 0 ||
                                  !Directory.Exists(_importerSettings.targetPath);
            if (invalidLocation)
                GUI.backgroundColor = Color.red;
            _importerSettings.targetPath = EditorGUILayout.TextField(GUIContent.none, _importerSettings.targetPath);
            GUI.backgroundColor = defaultColor;

            if (GUILayout.Button("Browse..."))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;

                _importerSettings.targetPath = _importerSettings.targetPath.Length == 0
                    ? Application.dataPath
                    : _importerSettings.targetPath;
                _importerSettings.targetPath = EditorUtility.OpenFolderPanel("Import Target",
                    _importerSettings.targetPath.Length == 0 ? Application.dataPath : _importerSettings.targetPath,
                    string.Empty);
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            // setup resolution
            var exp = Mathf.FloorToInt(Mathf.Log(_importerSettings.resolution, 2));
            exp = EditorGUILayout.IntSlider("Resolution Exponent", exp, 4, 12);
            _importerSettings.resolution = Mathf.FloorToInt(Mathf.Pow(2, exp));
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"=> Resolution: {_importerSettings.resolution}");
            EditorGUI.indentLevel--;

            // setup resolution
            exp = Mathf.FloorToInt(Mathf.Log(_importerSettings.antiAliasing, 2));
            exp = EditorGUILayout.IntSlider("AntiAliasing Exponent", exp, 0, 3);
            _importerSettings.antiAliasing = Mathf.FloorToInt(Mathf.Pow(2, exp));
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"=> AntiAliasing: {_importerSettings.antiAliasing}x");
            EditorGUI.indentLevel--;

            // button
            var buttonText = _importerSettings.selectedSprites.Count == 0 ? "Nothing selected" :
                invalidLocation ? "Invalid Location" : "Convert";
            var readyToConvert = !invalidLocation && _importerSettings.selectedSprites.Count > 0;
            GUI.enabled = readyToConvert;
            if (GUILayout.Button(buttonText))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;

                _importerSettings.selectedSprites = _importerSettings.selectedSprites.OrderBy(s => s.name).ToList();
                foreach (var sprite in _importerSettings.selectedSprites)
                {
                    if(sprite.name.Length > "Sprite".Length)
                    {
                        var last = sprite.name.Substring(sprite.name.Length - "Sprite".Length);
                        if (last.Equals("Sprite"))
                            sprite.name = sprite.name.Substring(0, sprite.name.Length - last.Length);
                    }
                    SvgConverter.ConvertToPNG(
                        sprite,
                        _importerSettings.resolution,
                        _importerSettings.antiAliasing,
                        _importerSettings.targetPath,
                        ColorTextureToWhite.ColorTexture);
                }

                AssetDatabase.Refresh();
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        private static void SetupSelection(string label,
            int totalAmount, int currentPage,
            UnityAction<int> applyPage, Func<int, Sprite> getSprite,
            SpriteSelectionWidget selectionWidget,
            UnityAction<Sprite> clickSprite)
        {
            // show count
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{label}: {totalAmount}");

            // setup pages
            var pages = Mathf.CeilToInt(totalAmount * 1f / _importerSettings.imageAmount);
            currentPage = Mathf.Clamp(currentPage, 1, pages);
            applyPage(currentPage);

            if (totalAmount > _importerSettings.imageAmount)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // left button
                GUI.enabled = currentPage > 1;
                if (GUILayout.Button("<"))
                {
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    currentPage--;
                    applyPage(currentPage);
                }

                // number input
                GUI.enabled = true;
                applyPage(EditorGUILayout.IntField(GUIContent.none, currentPage));
                GUILayout.Label("/ " + pages);

                // right button
                GUI.enabled = currentPage < pages;
                if (GUILayout.Button(">"))
                {
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    currentPage++;
                    applyPage(currentPage);
                }

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            // show page
            var startIndex = Mathf.Max(0, _importerSettings.imageAmount * (currentPage - 1));
            var shownResults = new List<Sprite>();
            for (var i = 0; i < _importerSettings.imageAmount; i++)
            {
                var index = startIndex + i;
                if (index > totalAmount - 1) break;

                var sprite = getSprite(index);
                // EditorGUILayout.LabelField(sprite.name);
                shownResults.Add(sprite);
            }

            // setup sprites
            selectionWidget.UpdateContents(shownResults.ToArray());
            var selection = selectionWidget.ShowGUI(-1);
            if (selection >= 0)
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                var selectedSprite = shownResults[selection];
                clickSprite(selectedSprite);
            }
        }

        private static void CloseImporter()
        {
            EditorGUILayout.LabelField("3. Close Importer (optional)", _headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _importerSettings.deleteSvgFolder =
                EditorGUILayout.ToggleLeft("Delete SVG Folder", _importerSettings.deleteSvgFolder);
            if (_importerSettings.deleteSvgFolder)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("The folder \"Assets/Resources/" + SvgLocation +
                                        "\" and all its content will be deleted." +
                                        "Make sure you don't need anything in there.", MessageType.Warning);
                EditorGUI.indentLevel--;

                _importerSettings.unloadVectorGraphicPackage =
                    EditorGUILayout.ToggleLeft("Unload Vector Graphics Package",
                        _importerSettings.unloadVectorGraphicPackage);
                if (_importerSettings.unloadVectorGraphicPackage)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox("The package \"" + SvgLocation + "\" will be removed from this project." +
                                            "Make sure you don't depend on it somewhere else.", MessageType.Warning);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}