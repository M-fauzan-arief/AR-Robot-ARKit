using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block
{
    // v2.12 - added layout classes for blocks without visual components to enable Function Blocks
    [ExecuteInEditMode]
    public class BE2_NoViewBlockSectionBody : MonoBehaviour, I_BE2_BlockSectionBody
    {
        RectTransform _rectTransform;
        I_BE2_BlockSection _section;
        I_BE2_BlockLayout _blockLayout;
        public RectTransform RectTransform => _rectTransform;
        public I_BE2_Block[] ChildBlocksArray { get; set; }
        public I_BE2_BlockSection BlockSection { get; set; }
        public Vector2 Size => Vector2.zero;
        public I_BE2_Spot Spot { get; set; }
        public int ChildBlocksCount { get; set; }
        public Shadow Shadow { get; }

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (transform.parent)
            {
                _section = transform.parent.GetComponent<I_BE2_BlockSection>();
                _blockLayout = transform.parent.parent.GetComponent<I_BE2_BlockLayout>();
                BlockSection = transform.parent.GetComponent<I_BE2_BlockSection>();
            }

            ChildBlocksArray = new I_BE2_Block[0];
        }

        public void UpdateChildBlocksList()
        {
            ChildBlocksArray = new I_BE2_Block[0];
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                I_BE2_Block childBlock = transform.GetChild(i).GetComponent<I_BE2_Block>();
                if (childBlock != null)
                {
                    ChildBlocksArray = BE2_ArrayUtils.AddReturn(ChildBlocksArray, childBlock);
                }
            }
            ChildBlocksCount = ChildBlocksArray.Length;
        }

        public void UpdateLayout()
        {
            UpdateChildBlocksList();
        }
    }
}
