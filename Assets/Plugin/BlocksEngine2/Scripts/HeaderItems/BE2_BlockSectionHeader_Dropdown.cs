using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block
{

    // v2.10 - Dropdown and InputField references in the block header inputs replaced by BE2_Dropdown and BE2_InputField to enable the use of legacy or TMP components
    public class BE2_BlockSectionHeader_Dropdown : MonoBehaviour, I_BE2_BlockSectionHeaderItem, I_BE2_BlockSectionHeaderInput
    {
        BE2_Dropdown _dropdown;
        RectTransform _rectTransform;

        public Transform Transform => transform;
        public Vector2 Size => _rectTransform ? _rectTransform.sizeDelta : GetComponent<RectTransform>().sizeDelta;
        public I_BE2_Spot Spot { get; set; }
        public float FloatValue { get; set; }
        public string StringValue { get; set; }
        public BE2_InputValues InputValues { get; set; }

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _dropdown = BE2_Dropdown.GetBE2Component(transform);
            Spot = GetComponent<I_BE2_Spot>();
        }

        void OnEnable()
        {
            UpdateValues();
            if (_dropdown != null)
                _dropdown.onValueChanged.AddListener(delegate { UpdateValues(); });
        }

        void OnDisable()
        {
            if (_dropdown != null)
                _dropdown.onValueChanged.RemoveAllListeners();
        }

        void Start()
        {
	    GetComponent<BE2_DropdownDynamicResize>().Resize(0);
            UpdateValues();
        }

        //void Update()
        //{
        //
        //}

        public void UpdateValues()
        {
            bool isText = false;
            if (_dropdown.GetOptionsCount() > 0)
            {
                StringValue = _dropdown.GetSelectedOptionText();
            }
            else
            {
                StringValue = "";
            }

	    // v2.12 - dropdown input now returns the index of the selected item as FloatValue
            FloatValue = _dropdown.value;

            InputValues = new BE2_InputValues(StringValue, FloatValue, isText);
        }
    }
}
