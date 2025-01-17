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

    public class EnumerateOperationDrawer : RenameOperationDrawer<EnumerateOperation>
    {
        public EnumerateOperationDrawer()
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
                return "Add/Count";
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
                return "Count";
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
                return AddColor;
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
                return "Format";
            }
        }

        private List<EnumeratePresetGUI> GUIPresets { get; set; }

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
                        if (GUIPresets[i].Preset == RenameOperation.FormatPreset)
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
            var defaultHeight = CalculateGUIHeightForLines(5);
            var preferredHeight = defaultHeight;
            if (!RenameOperation.IsCountStringFormatValid)
            {
                preferredHeight += GetHeightForHelpBox();
            }

            return preferredHeight;
        }

        private float GetHeightForHelpBox()
        {
            return 56.0f;
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            var presetsContent = new GUIContent("Format", "Select a preset format or specify your own format.");
            var names = new List<GUIContent>(GUIPresets.Count);
            foreach (var preset in GUIPresets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }

            var currentLine = 0;
            float[] weights;
            bool countFormatWasValidBeforeDraw;
            if (!RenameOperation.IsCountStringFormatValid)
            {
                weights = new float[] { 1, 1, 3, 1, 1, 1 };
                countFormatWasValidBeforeDraw = false;
            }
            else
            {
                weights = new float[] { 1, 1, 1, 1, 1 };
                countFormatWasValidBeforeDraw = true;
            }

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, presetsContent.text));
            var newlySelectedIndex = EditorGUI.Popup(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                presetsContent,
                SelectedPresetIndex,
                names.ToArray());
            var selectedPreset = GUIPresets[newlySelectedIndex];

            EditorGUI.BeginDisabledGroup(selectedPreset.ReadOnly);
            var countFormatContent = new GUIContent("Count Format", "The string format to use when adding the Count to the name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFormatContent.text));
            if (selectedPreset.ReadOnly)
            {
                EditorGUI.TextField(operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                       countFormatContent,
                       RenameOperation.CountFormat);
                RenameOperation.SetCountFormatPreset(selectedPreset.Preset);
            }
            else
            {
                // Clear out the sequence when moving from a non-custom to Custom so that they don't
                // see it prepopulated with the previous preset's entries.
                if (RenameOperation.FormatPreset != EnumerateOperation.CountFormatPreset.Custom)
                {
                    RenameOperation.SetCountFormat("0");
                }

                RenameOperation.SetCountFormat(EditorGUI.TextField(
                    operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                    countFormatContent,
                    RenameOperation.CountFormat));
            }

            EditorGUI.EndDisabledGroup();

            if (!RenameOperation.IsCountStringFormatValid)
            {
                // On the first frame a user sets the count invalid, measurements will be broken because
                // the Height was calculated using the non-erroring size. So don't draw the error box until next frame
                if (countFormatWasValidBeforeDraw)
                {
                    GUIUtility.ExitGUI();
                    return;
                }

                var helpBoxMessage = "Invalid Count Format. Typical formats are D1 for one digit with no " +
                                     "leading zeros, D2, for two, etc." +
                                     "\nLookup the String.Format() method for more info on formatting options.";
                var helpRect = operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights);
                helpRect = helpRect.AddPadding(4, 4, 4, 4);
                EditorGUI.HelpBox(helpRect, helpBoxMessage, MessageType.Warning);
            }

            var countFromContent = new GUIContent("Count From", "The value to start counting from. The first object will have this number.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, countFromContent.text));
            RenameOperation.StartingCount = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                countFromContent,
                RenameOperation.StartingCount);

            var incrementContent = new GUIContent("Increment", "The value to add to each object when counting.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, incrementContent.text));
            RenameOperation.Increment = EditorGUI.IntField(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                incrementContent,
                RenameOperation.Increment);

            var prependContent = new GUIContent("Add as Prefix", "Add the count to the front of the object's name.");
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, prependContent.text));
            RenameOperation.Prepend = EditorGUI.Toggle(
                operationRect.GetSplitVerticalWeighted(++currentLine, LineSpacing, weights),
                prependContent,
                RenameOperation.Prepend);
        }

        private void Initialize()
        {
            var singleDigitPreset = new EnumeratePresetGUI()
            {
                DisplayName = "0, 1, 2...",
                Preset = EnumerateOperation.CountFormatPreset.SingleDigit,
                ReadOnly = true
            };

            var leadingZeroPreset = new EnumeratePresetGUI()
            {
                DisplayName = "00, 01, 02...",
                Preset = EnumerateOperation.CountFormatPreset.LeadingZero,
                ReadOnly = true
            };

            var underscorePreset = new EnumeratePresetGUI()
            {
                DisplayName = "_00, _01, _02...",
                Preset = EnumerateOperation.CountFormatPreset.Underscore,
                ReadOnly = true
            };

            var customPreset = new EnumeratePresetGUI()
            {
                DisplayName = "Custom",
                Preset = EnumerateOperation.CountFormatPreset.Custom,
                ReadOnly = false
            };

            GUIPresets = new List<EnumeratePresetGUI>
            {
                singleDigitPreset,
                leadingZeroPreset,
                underscorePreset,
                customPreset
            };
        }

        private class EnumeratePresetGUI
        {
            public string DisplayName { get; set; }

            public EnumerateOperation.CountFormatPreset Preset { get; set; }

            public bool ReadOnly { get; set; }
        }
    }
}