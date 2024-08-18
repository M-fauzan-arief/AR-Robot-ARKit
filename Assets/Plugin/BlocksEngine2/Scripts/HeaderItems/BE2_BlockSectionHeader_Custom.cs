using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block
{
    // v2.12 - new header item BE2_BlockSectionHeader_Custom added to enable custom blocks to use different types of items as headers.
    // By default it is used for Horizontal Function Blocks    
    [ExecuteInEditMode]
    public class BE2_BlockSectionHeader_Custom : MonoBehaviour, I_BE2_BlockSectionHeaderItem
    {
        RectTransform _rectTransform;

        public Transform Transform => transform;
        public Vector2 Size => _rectTransform ? _rectTransform.sizeDelta : GetComponent<RectTransform>().sizeDelta;

        public string serializableValue;

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}
