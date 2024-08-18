using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.UI;
using MG_BlocksEngine2.EditorScript;
using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.DragDrop
{
    // v2.12 - added new Drag class for the Function Blocks
    public class BE2_DragSelectionFunction : MonoBehaviour, I_BE2_Drag
    {
        BE2_DragDropManager _dragDropManager => BE2_DragDropManager.Instance;
        RectTransform _rectTransform;
        BE2_UI_SelectionBlock _uiSelectionBlock;
        ScrollRect _scrollRect;
        I_BE2_BlockLayout _blockLayout;

        Transform _transform;
        public Transform Transform => _transform ? _transform : transform;
        public Vector2 RayPoint => _rectTransform.position;
        public I_BE2_Block Block => null;
        [HideInInspector] public BE2_Ins_FunctionBlock functionBlockInstruction;
        public BE2_Ins_DefineFunction defineFunctionInstruction;

        void Awake()
        {
            _transform = transform;
            _rectTransform = GetComponent<RectTransform>();
            _uiSelectionBlock = GetComponent<BE2_UI_SelectionBlock>();
            _scrollRect = GetComponentInParent<ScrollRect>();
            _blockLayout = GetComponent<I_BE2_BlockLayout>();
        }

        void OnEnable()
        {
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnFunctionDefinitionRemoved, Remove);
        }

        void OnDisable()
        {
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypesBlock.OnFunctionDefinitionRemoved, Remove);
        }

        void Remove(I_BE2_Block block)
        {
            if (defineFunctionInstruction.Block == block)
            {
                Destroy(gameObject);
            }
        }

        Vector3 _envScale = Vector3.one;

        public void OnPointerDown()
        {
            _envScale = BE2_ExecutionManager.Instance.ProgrammingEnvsList.Find(x => x.Visible == true).Transform.localScale;
        }

        public void OnRightPointerDownOrHold()
        {

        }

        public void OnDrag()
        {
            _scrollRect.StopMovement();
            _scrollRect.enabled = false;

            GameObject instantiatedBlockGO = Instantiate(_uiSelectionBlock.prefabBlock);
            instantiatedBlockGO.name = _uiSelectionBlock.prefabBlock.name;
            I_BE2_Block instantiatedBlock = instantiatedBlockGO.GetComponent<I_BE2_Block>();

            functionBlockInstruction = instantiatedBlockGO.GetComponent<BE2_Ins_FunctionBlock>();
            functionBlockInstruction.Initialize(defineFunctionInstruction);

            I_BE2_BlockLayout instantiatedLayout = instantiatedBlockGO.GetComponent<I_BE2_BlockLayout>();

            int i = 0;
            foreach (I_BE2_BlockSectionHeaderItem item in defineFunctionInstruction.Block.Layout.SectionsArray[0].Header.ItemsArray)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if (item is BE2_BlockSectionHeader_Label)
                {
                    GameObject label = Instantiate(BE2_Inspector.Instance.LabelTextTemplate, Vector3.zero, Quaternion.identity,
                                                    instantiatedLayout.SectionsArray[0].Header.RectTransform);
                    label.GetComponent<TMP_Text>().text = item.Transform.GetComponent<TMP_Text>().text;
                }
                else if (item is BE2_BlockSectionHeader_LocalVariable)
                {
                    GameObject input = Instantiate(BE2_Inspector.Instance.InputFieldTemplate, Vector3.zero, Quaternion.identity,
                                                    instantiatedLayout.SectionsArray[0].Header.RectTransform);
                    input.GetComponent<TMP_InputField>().text = "";//item.Transform.GetComponent<TMP_InputField>().text;
                }

                i++;
            }


            instantiatedBlock.Drag.Transform.SetParent(_dragDropManager.DraggedObjectsTransform, true);

            I_BE2_BlocksStack blocksStack = instantiatedBlockGO.GetComponent<I_BE2_BlocksStack>();

            instantiatedBlockGO.transform.localScale = _envScale;

            instantiatedBlockGO.transform.position = transform.position;
            _dragDropManager.CurrentDrag = instantiatedBlock.Drag;

            instantiatedBlock.Drag.OnPointerDown();
            instantiatedBlock.Drag.OnDrag();

            instantiatedBlock.Transform.localEulerAngles = Vector3.zero;
        }

        public void OnPointerUp()
        {

        }
    }
}
