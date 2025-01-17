﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ItsBaptiste.Toolbox.Tools.ReplaceGamobjects {
    public class ReplaceGameObjects : ScriptableWizard {
        public GameObject useGameObject;

        private void OnWizardCreate() {
            foreach (Transform transform in Selection.transforms) {
                GameObject newObject = (GameObject) PrefabUtility.InstantiatePrefab(useGameObject);
                Transform newObj = newObject.transform;
                newObj.parent = transform.parent;
                newObj.position = transform.position;
                newObj.rotation = transform.rotation;
                newObj.localScale = transform.localScale;
                GameObjectUtility.SetStaticEditorFlags(newObject,
                    GameObjectUtility.GetStaticEditorFlags(newObj.parent.gameObject));
            }

            foreach (GameObject go in Selection.gameObjects) DestroyImmediate(go);
        }

        [MenuItem("Tools/Replace GameObjects")]
        public static void CreateWizard() {
            DisplayWizard("Replace GameObjects", typeof(ReplaceGameObjects), "Replace");
        }
    }
}

#endif