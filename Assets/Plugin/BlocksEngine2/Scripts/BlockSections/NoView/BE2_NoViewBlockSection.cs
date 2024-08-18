using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_BlocksEngine2.Block
{
    // v2.12 - added layout classes for blocks without visual components to enable Function Blocks
    [ExecuteInEditMode]
    public class BE2_NoViewBlockSection : MonoBehaviour, I_BE2_BlockSection
    {
        RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;
        public I_BE2_BlockLayout blockLayout;
        I_BE2_BlockSectionHeader _header;
        public I_BE2_BlockSectionHeader Header { get => _header; set => _header = value; }
        I_BE2_BlockSectionBody _body;
        public I_BE2_BlockSectionBody Body { get => _body; set => _body = value; }
        public I_BE2_Block Block { get; set; }
        public Vector2 Size => Vector2.zero;
        public int index;

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (transform.childCount > 0)
                _header = transform.GetChild(0).GetComponent<I_BE2_BlockSectionHeader>();
            if (transform.childCount > 1)
                _body = transform.GetChild(1).GetComponent<I_BE2_BlockSectionBody>();

            if (transform.parent)
                blockLayout = transform.parent.GetComponent<I_BE2_BlockLayout>();

            index = transform.GetSiblingIndex();

            Block = GetComponentInParent<I_BE2_Block>();
        }

        public void UpdateLayout()
        {
            if (Header != null)
                Header.UpdateLayout();
            if (Body != null)
                Body.UpdateLayout();
        }
    }
}
