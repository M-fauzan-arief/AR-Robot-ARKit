using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.Utils;
using TMPro;

// v2.12 - new DefineFunction instruction
public class BE2_Ins_DefineFunction : BE2_InstructionBase, I_BE2_Instruction
{
    protected override void OnAwake()
    {
        BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnFunctionDefinitionAdded, Block);
        defineID = System.Guid.NewGuid().ToString();
    }

    // v2.12.1 - make sure the inputs are updated on Define Block start
    protected override void OnStart()
    {
        Block.Layout.SectionsArray[0].Header.UpdateInputsArray();
    }

    public string defineID;

    public UnityEvent onDefineChange = new UnityEvent();

    protected override void OnEnableInstruction()
    {
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnDropAtStack, HandleDefineChange);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnDropAtInputSpot, HandleDefineChange);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnDragFromStack, HandleDefineChange);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnDragFromInputSpot, HandleDefineChange);
    }

    protected override void OnDisableInstruction()
    {
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypesBlock.OnDropAtStack, HandleDefineChange);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypesBlock.OnDropAtInputSpot, HandleDefineChange);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypesBlock.OnDragFromStack, HandleDefineChange);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypesBlock.OnDragFromInputSpot, HandleDefineChange);
    }

    public void HandleDefineChange(I_BE2_Block block)
    {
        BE2_Ins_DefineFunction defineInstruction = block.ParentSection.RectTransform.GetComponentInParent<BE2_Ins_DefineFunction>();
        if (defineInstruction == this)
        {
            onDefineChange.Invoke();
        }
    }

    public int GetLocalVariableIndex(string varName)
    {
        int index = -1;

        I_BE2_BlockSectionHeaderInput[] inputs = Block.Layout.SectionsArray[0].Header.InputsArray;
        int inputsLength = inputs.Length;
        for (int i = 0; i < inputsLength; i++)
        {
            if (inputs[i].Transform.GetComponentInChildren<TMP_Text>().text == varName)
                return i;
        }

        return index;
    }

    // public new void Function()
    // {

    // }

    void OnDestroy()
    {
        BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnFunctionDefinitionRemoved, Block);
        BE2_BlockUtils.RemoveBlock(Block);
    }
}
