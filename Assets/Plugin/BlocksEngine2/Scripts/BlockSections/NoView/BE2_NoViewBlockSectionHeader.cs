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
    public class BE2_NoViewBlockSectionHeader : MonoBehaviour, I_BE2_BlockSectionHeader
    {
        RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;
        I_BE2_BlockSection _section;
        I_BE2_BlockLayout _blockLayout;
        public Vector2 Size => Vector2.zero;
        I_BE2_BlockSectionHeaderItem[] _itemsArray;
        public I_BE2_BlockSectionHeaderItem[] ItemsArray => _itemsArray;
        I_BE2_BlockSectionHeaderInput[] _inputsArray;
        public I_BE2_BlockSectionHeaderInput[] InputsArray => _inputsArray;
        public Shadow Shadow { get; }

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            UpdateItemsArray();
            UpdateInputsArray();

            _rectTransform = GetComponent<RectTransform>();

            if (transform.parent)
            {
                _section = transform.parent.GetComponent<I_BE2_BlockSection>();
                _blockLayout = transform.parent.parent.GetComponent<I_BE2_BlockLayout>();
            }
        }

        void OnEnable()
        {
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnDrag, UpdateItemsArray);
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyUpEnd, UpdateItemsArray);
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyUpEnd, UpdateInputsArray);
        }

        void OnDisable()
        {
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnDrag, UpdateItemsArray);
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyUpEnd, UpdateItemsArray);
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyUpEnd, UpdateInputsArray);
        }

        public void UpdateItemsArray()
        {
            _itemsArray = new I_BE2_BlockSectionHeaderItem[0];
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                I_BE2_BlockSectionHeaderItem item = transform.GetChild(i).GetComponent<I_BE2_BlockSectionHeaderItem>();
                if (item != null && item.Transform.gameObject.activeSelf)
                {
                    BE2_ArrayUtils.Add(ref _itemsArray, item);
                }
            }
        }

        public void UpdateInputsArray()
        {
            _inputsArray = new I_BE2_BlockSectionHeaderInput[0];
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                I_BE2_BlockSectionHeaderInput input = transform.GetChild(i).GetComponent<I_BE2_BlockSectionHeaderInput>();
                if (input != null && input.Transform.gameObject.activeSelf)
                {
                    BE2_ArrayUtils.Add(ref _inputsArray, input);
                }
            }
        }

        public void UpdateLayout() { }
    }
}
