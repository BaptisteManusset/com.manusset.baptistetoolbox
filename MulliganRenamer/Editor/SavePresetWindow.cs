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
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// SavePresetWindow allows users to type in a value for a name and executes a callback when
    /// the name is saved.
    /// </summary>
    public class SavePresetWindow : EditorWindow
    {
        public event Action<string> PresetSaved;

        private bool textFieldNeedsFocus;
        private string enteredName;
        private List<string> existingPresetNames;

        /// <summary>
        /// Set the value for the name to edit.
        /// </summary>
        /// <param name="name">Name to edit</param>
        public void SetName(string name)
        {
            enteredName = name;
        }

        /// <summary>
        /// Set the existing presets to check against
        /// </summary>
        /// <param name="existingPresetNames">Preset names to test against.</param>
        public void SetExistingPresetNames(IEnumerable<string> existingPresetNames)
        {
            this.existingPresetNames.Clear();
            this.existingPresetNames.AddRange(existingPresetNames);
        }

        private static bool IsNameValid(string name)
        {
            return !string.IsNullOrEmpty(name);
        }

        private void OnEnable()
        {
            existingPresetNames = new List<string>();
            textFieldNeedsFocus = true;
        }

        private void OnGUI()
        {
            // This window is based on Unity's Save Layout window:
            // https://github.com/Unity-Technologies/UnityCsReference/blob/73f36bfe71b68d241a6802e0396dc6d6822cb520/Editor/Mono/GUI/WindowLayout.cs#L868

            GUILayout.Space(5.0f);

            // Detect the enter key. Note this must come before the Text Field is shown.
            Event currentEvent = Event.current;
            bool hitEnter = currentEvent.type == EventType.KeyDown &&
                (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter);

            var presetFieldName = "SavePresetField";
            GUI.SetNextControlName(presetFieldName);
            enteredName = EditorGUILayout.TextField(enteredName);

            if (textFieldNeedsFocus)
            {
                EditorGUI.FocusTextInControl(presetFieldName);
                textFieldNeedsFocus = false;
            }

            EditorGUI.BeginDisabledGroup(!IsNameValid(enteredName));
            if (GUILayout.Button("Save") || (hitEnter && IsNameValid(enteredName)))
            {
                var saveAndClose = false;
                if (existingPresetNames.Contains(enteredName))
                {
                    var popupMessage = string.Format(
                        "A preset named \"{0}\" already exists. Do you want to replace it?",
                        enteredName
                    );

                    saveAndClose = EditorUtility.DisplayDialog("Warning", popupMessage, "Replace", "No");
                }
                else
                {
                    saveAndClose = true;
                }

                if (saveAndClose)
                {
                    InvokePresetSaved();
                    Close();
                }
            }
            else
            {
                // Keep focus on the text field
                textFieldNeedsFocus = true;
            }

            EditorGUI.EndDisabledGroup();
        }

        private void InvokePresetSaved()
        {
            if (PresetSaved != null)
            {
                PresetSaved.Invoke(enteredName);
            }
        }
    }
}