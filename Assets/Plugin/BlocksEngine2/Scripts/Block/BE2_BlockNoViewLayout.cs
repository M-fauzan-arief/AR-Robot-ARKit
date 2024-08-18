using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;
using MG_BlocksEngine2.Core;

namespace MG_BlocksEngine2.Block
{
    // v2.12 - added layout classes for blocks without visual components to enable Function Blocks
    [ExecuteInEditMode]
    public class BE2_BlockNoViewLayout : MonoBehaviour, I_BE2_BlockLayout
    {
        RectTransform _rectTransform;
        public RectTransform RectTransform { get => _rectTransform; set => _rectTransform = value; }
        I_BE2_BlockSection[] _sectionsArray;
        public I_BE2_BlockSection[] SectionsArray => _sectionsArray;
        public Color Color { get; set; }
        public Vector2 Size => Vector2.zero;

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            _rectTransform = GetComponent<RectTransform>();
            _sectionsArray = new I_BE2_BlockSection[0];

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                I_BE2_BlockSection section = transform.GetChild(i).GetComponent<I_BE2_BlockSection>();
                if (section != null)
                    BE2_ArrayUtils.Add(ref _sectionsArray, section);
            }
        }

        // v2.9 - Block layout update refactored (executed as coroutine at the end of frame) to be executed by the execution manager 
        public void UpdateLayout()
        {
            int sectionsLength = SectionsArray.Length;
            for (int i = 0; i < sectionsLength; i++)
            {
                SectionsArray[i].UpdateLayout();
            }
        }
    }
}
