﻿/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class RemoveCharactersOperationDrawer : RenameOperationDrawer<RemoveCharactersOperation>
    {
        public RemoveCharactersOperationDrawer()
        {
            Initialize();
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Delete/Remove Characters";
            }
        }

        /// <summary> 
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return "Remove Characters";
            }
        }

        /// <summary>
        /// Gets the color to use for highlighting the operation.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public override Color32 HighlightColor
        {
            get
            {
                return DeleteColor;
            }
        }

        /// <summary>
        /// Gets the name of the control to focus when this operation is focused
        /// </summary>
        /// <value>The name of the control to focus.</value>
        public override string ControlToFocus
        {
            get
            {
                return "Preset";
            }
        }

        private List<CharacterPresetGUI> GUIPresets { get; set; }

        private int SelectedPresetIndex
        {
            get
            {
                if (RenameOperation == null)
                {
                    return 0;
                }
                else
                {
                    for (int i = 0; i < GUIPresets.Count; ++i)
                    {
                        if (GUIPresets[i].PresetID == RenameOperation.CurrentPresetID)
                        {
                            return i;
                        }
                    }

                    // Could not find a GUIPreset that uses the operation's preset.
                    // Just fallback to 0
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            var selectedPreset = GUIPresets[SelectedPresetIndex];
            int numGUILines;
            if (selectedPreset.IsReadOnly)
            {
                numGUILines = 2;
            }
            else
            {
                numGUILines = 3;
            }

            return CalculateGUIHeightForLines(numGUILines);
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            // Read and write into copies so that we don't resize the view while it's being worked on,
            // which is what is required when the user switches settings around and options (lines) are added into the GUI,
            // after it's already been measured based on it's PRE Update state.
            var originalPresetIndex = SelectedPresetIndex;

            var currentSplit = 0;
            int numSplits = 2;
            if (GUIPresets[originalPresetIndex].IsReadOnly)
            {
                numSplits = 2;
            }
            else
            {
                numSplits = 3;
            }

            var presetsContent = new GUIContent("Preset", "Select a preset or specify your own characters.");
            var names = new List<GUIContent>(GUIPresets.Count);
            foreach (var preset in GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, presetsContent.text));
            var selectedPresetIndex = EditorGUI.Popup(
                operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                presetsContent,
                originalPresetIndex,
                names.ToArray());

            var selectedPreset = GUIPresets[selectedPresetIndex];
            var workingOptions = new RemoveCharactersOperation.RenameOptions();

            // We can't resize the Rects mid-GUI loop (GetHeight already said how tall it would be),
            // so if we've changed presets we just apply the defaults for the new change. They can
            // modify it next frame.
            if (selectedPresetIndex != originalPresetIndex)
            {
                if (selectedPreset.IsReadOnly)
                {
                    RenameOperation.SetOptionPreset(selectedPreset.PresetID);
                }
                else
                {
                    RenameOperation.SetOptions(workingOptions);
                }
                return;
            }

            if (selectedPreset.IsReadOnly)
            {
                // The Readonly Label just looks better disabled.
                EditorGUI.BeginDisabledGroup(true);
                var readonlyLabelContent = new GUIContent(selectedPreset.ReadOnlyLabel);
                var labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.alignment = TextAnchor.MiddleRight;
                EditorGUI.LabelField(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    readonlyLabelContent,
                    labelStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                var charactersFieldContent = new GUIContent("Characters to Remove", "All characters that will be removed from the names.");
                GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, charactersFieldContent.text));
                workingOptions.CharactersToRemove = EditorGUI.TextField(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    charactersFieldContent,
                    RenameOperation.CharactersToRemove);

                var caseSensitiveToggleContent = new GUIContent("Case Sensitive", "Flag the search to match only the specified case");
                workingOptions.IsCaseSensitive = EditorGUI.Toggle(
                    operationRect.GetSplitVertical(++currentSplit, numSplits, LineSpacing),
                    caseSensitiveToggleContent,
                    RenameOperation.IsCaseSensitive);
            }

            if (selectedPreset.IsReadOnly)
            {
                RenameOperation.SetOptionPreset(selectedPreset.PresetID);
            }
            else
            {
                RenameOperation.SetOptions(workingOptions);
            }
        }

        private void Initialize()
        {
            var symbolsPreset = new CharacterPresetGUI()
            {
                DisplayName = "Symbols",
                ReadOnlyLabel = "Removes special characters (ie. !@#$%^&*)",
                PresetID = RemoveCharactersOperation.PresetID.Symbols,
                IsReadOnly = true
            };

            var numbersPreset = new CharacterPresetGUI()
            {
                DisplayName = "Numbers",
                ReadOnlyLabel = "Removes digits 0-9",
                PresetID = RemoveCharactersOperation.PresetID.Numbers,
                IsReadOnly = true
            };

            var whitespacePreset = new CharacterPresetGUI()
            {
                DisplayName = "Whitespace",
                ReadOnlyLabel = "Removes whitespace",
                PresetID = RemoveCharactersOperation.PresetID.Whitespace,
                IsReadOnly = true
            };

            var customPreset = new CharacterPresetGUI()
            {
                DisplayName = "Custom",
                PresetID = RemoveCharactersOperation.PresetID.Custom,
                IsReadOnly = false,
                ReadOnlyLabel = string.Empty
            };

            GUIPresets = new List<CharacterPresetGUI>
            {
                symbolsPreset,
                numbersPreset,
                whitespacePreset,
                customPreset
            };
        }

        private class CharacterPresetGUI
        {
            public string DisplayName { get; set; }

            public RemoveCharactersOperation.PresetID PresetID { get; set; }

            public string ReadOnlyLabel { get; set; }

            public bool IsReadOnly { get; set; }
        }
    }
}