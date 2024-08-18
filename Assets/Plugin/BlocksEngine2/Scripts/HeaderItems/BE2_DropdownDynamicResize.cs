using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;
using TMPro;

namespace MG_BlocksEngine2.Block
{
    // v2.10 - Dropdown and InputField references in the block header inputs replaced by BE2_Dropdown and BE2_InputField to enable the use of legacy or TMP components
    [ExecuteInEditMode]
    public class BE2_DropdownDynamicResize : MonoBehaviour
    {
        RectTransform _rectTransform;
        // v2.12 - use of BE2_Dropdown replaced by TMP_Dropdown
        TMP_Dropdown _dropdown;
        float _minWidth = 70;
        // v2.9 - bugfix: dropdown text being cropped on the Blocks Selection Panel
        float _offset = 45;

        // v2.2 - added optional max width to the dropdown input
        public float maxWidth = 0;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _dropdown = GetComponent<TMP_Dropdown>();
        }

        void Start()
        {
            Resize(_dropdown.value);
        }

        void OnEnable()
        {
            if (_dropdown != null)
                _dropdown.onValueChanged.AddListener(Resize);
        }

        void OnDisable()
        {
            if (_dropdown != null)
                _dropdown.onValueChanged.RemoveAllListeners();
        }

        // v2.12 - BE2_DropdownDynamicResize.Resize method refactored to resize based on the actual current value
        public void Resize(int value)
        {
            if (_dropdown != null)
            {
                float width = _offset + _dropdown.captionText.GetPreferredValues(_dropdown.options[value].text).x;
                if (width < _minWidth)
                    width = _minWidth;

                if (maxWidth > 0 && width > maxWidth)
                    width = maxWidth;

                _rectTransform.sizeDelta = new Vector2(width, _rectTransform.sizeDelta.y);
            }
        }
    }
}
