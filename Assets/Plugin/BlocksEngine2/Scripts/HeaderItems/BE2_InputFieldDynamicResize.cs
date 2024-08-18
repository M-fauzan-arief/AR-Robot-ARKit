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
    public class BE2_InputFieldDynamicResize : MonoBehaviour
    {
        RectTransform _rectTransform;
        // v2.12 - use of BE2_InputField replaced by TMP_InputField
        TMP_InputField _inputField;
        public float minWidth = 70;
        public float widthOffset = 35;

        // v2.2 - added optional max width to the text input
        public float maxWidth = 0;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _inputField = GetComponent<TMP_InputField>();
        }

        void OnEnable()
        {
            if (_inputField == null)
                Awake();

            if (_inputField != null)
                _inputField.onValueChanged.AddListener(Resize);
        }

        void OnDisable()
        {
            if (_inputField == null)
                Awake();

            if (_inputField != null)
                _inputField.onValueChanged.RemoveAllListeners();
        }

        // v2.12 - BE2_InputFieldDynamicResize.Resize method refactored to resize based on the actual current value
        public void Resize(string value)
        {
            float width = widthOffset + _inputField.textComponent.GetPreferredValues(value).x;
            if (width < minWidth)
                width = minWidth;

            if (maxWidth > 0 && width > maxWidth)
                width = maxWidth;

            _rectTransform.sizeDelta = new Vector2(width, _rectTransform.sizeDelta.y);

            _inputField.textComponent.transform.localPosition = Vector3.zero;
        }
    }
}
