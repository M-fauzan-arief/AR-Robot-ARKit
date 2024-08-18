using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block;
using TMPro;
using MG_BlocksEngine2.Environment;

namespace MG_BlocksEngine2.UI.FunctionBlock
{
    // v2.12 - added class to amange the UI that directs the creation of Function Blocks
    public class BE2_UI_CreateFunctionBlockMenu : MonoBehaviour
    {
        public Transform editorBlockTransform;
        I_BE2_Block _editorBlock;

        public GameObject templateInput;
        public GameObject templateLabel;

        void Awake()
        {
            _editorBlock = editorBlockTransform.GetComponent<I_BE2_Block>();
        }

        void OnEnable()
        {
            bool createLabel = true;
            foreach (I_BE2_BlockSectionHeaderItem item in _editorBlock.Layout.SectionsArray[0].Header.ItemsArray)
            {
                Label isLabel = item.Transform.GetComponent<Label>();

                if (isLabel)
                {
                    item.Transform.GetComponent<TMP_InputField>().Select();
                    createLabel = false;
                    break;
                }
            }

            if (createLabel)
                AddLabel();
        }

        public void AddInput()
        {
            GameObject input = Instantiate(templateInput, Vector3.zero, Quaternion.identity, _editorBlock.Layout.SectionsArray[0].Header.RectTransform);
            Button removeButton = input.GetComponentInChildren<Button>(true);
            removeButton.onClick.AddListener(delegate { RemoveItem(input); });

            input.GetComponent<TMP_InputField>().Select();
        }

        public void AddLabel()
        {
            GameObject label = Instantiate(templateLabel, Vector3.zero, Quaternion.identity, _editorBlock.Layout.SectionsArray[0].Header.RectTransform);
            Button removeButton = label.GetComponentInChildren<Button>(true);
            removeButton.onClick.AddListener(delegate { RemoveItem(label); });

            label.GetComponent<TMP_InputField>().Select();
        }

        public void RemoveItem(GameObject item)
        {
            item.transform.SetParent(null);
            _editorBlock.Layout.SectionsArray[0].Header.UpdateItemsArray();
            Destroy(item);
        }

        public void OnButtonCreateFunctionBlock()
        {
            List<Serializer.DefineItem> items = new List<Serializer.DefineItem>();
            foreach (I_BE2_BlockSectionHeaderItem item in _editorBlock.Layout.SectionsArray[0].Header.ItemsArray)
            {
                Label isLabel = item.Transform.GetComponent<Label>();

                if (isLabel)
                {
                    items.Add(new Serializer.DefineItem("label", item.Transform.GetComponent<TMP_InputField>().text));
                }
                else
                {
                    items.Add(new Serializer.DefineItem("variable", item.Transform.GetComponent<TMP_InputField>().text));
                }
            }

            BE2_FunctionBlocksManager.Instance.CreateFunctionBlock(items);
        }

    }
}
