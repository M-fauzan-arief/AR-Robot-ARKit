using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block
{
    // v2.12 - new header input BE2_BlockSectionHeader_ReferenceInput added to enable Function Blocks
    public class BE2_BlockSectionHeader_ReferenceInput : MonoBehaviour, I_BE2_BlockSectionHeaderInput
    {
        public I_BE2_BlockSectionHeaderInput referenceInput;

        public Transform Transform => transform;
        public I_BE2_Spot Spot { get; }
        public float FloatValue => referenceInput.FloatValue;
        public string StringValue => referenceInput.StringValue;
        public BE2_InputValues InputValues => referenceInput.InputValues;

        public void UpdateValues() { }
    }
}
