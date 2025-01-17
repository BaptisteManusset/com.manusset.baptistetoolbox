﻿using System;
using System.Reflection;
using UnityEditor;

namespace ItsBaptiste.Toolbox.Editor {
    public static class MouseLock {
        [MenuItem("Tools/Editor/Toggle Inspector lock %#l")]
        public static void ToggleInspectorLock() {
            Type inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);

            var inspectorWindow = EditorWindow.GetWindow(inspectorType);
            var state = isLocked.GetGetMethod().Invoke(inspectorWindow, new object[] { });
            isLocked.GetSetMethod().Invoke(inspectorWindow, new object[] {!(bool) state});
        }
    }
}