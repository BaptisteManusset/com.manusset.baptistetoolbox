﻿using System;
using NaughtyAttributes;
using TMPro;
using Toolbox.ValueAsset;
using UnityEngine;

namespace Toolbox.UIScript {
#pragma warning disable 0649

    public class UiVariableToText : MonoBehaviour {
        private TMP_Text _text;

        [SerializeField] private FloatValue valueAsset;


        [SerializeField] private string format = "{0}";


        private void Awake() {
            _text = GetComponent<TMP_Text>();
            OnValueChange();
        }

        private void OnEnable() {
            valueAsset.OnValueChange += OnValueChange;
        }

        private void OnDisable() {
            valueAsset.OnValueChange -= OnValueChange;
        }


        private void OnValueChange() {
            Debug.Log("onvaluechange");
            _text.text = String.Format(format, valueAsset.Value);
        }
    }
#pragma warning restore 0649
}