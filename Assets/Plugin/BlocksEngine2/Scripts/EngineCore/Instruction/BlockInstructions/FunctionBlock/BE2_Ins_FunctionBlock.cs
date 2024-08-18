using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.Utils;
using TMPro;

// v2.12 - new FunctionBlock instruction
public class BE2_Ins_FunctionBlock : BE2_InstructionBase, I_BE2_Instruction
{
    public BE2_Ins_FunctionBlock(BE2_Ins_DefineFunction defineInstruction)
    {
        this.defineInstruction = defineInstruction;
    }

    public string defineID;
    public BE2_Ins_DefineFunction defineInstruction;

    protected override void OnStart()
    {
        if (defineInstruction)
        {
            defineInstruction.InstructionBase.Block.Layout.SectionsArray[0].Body.UpdateLayout();
            defineID = defineInstruction.defineID;
        }

        // v2.12.1 - added localVariables to Function Blocks to hold the input values and enable recursive functions
        localValues = new List<string>();
        foreach (I_BE2_BlockSectionHeaderInput input in Block.Layout.SectionsArray[0].Header.InputsArray)
        {
            localValues.Add("");
        }
    }

    protected override void OnEnableInstruction()
    {
        if (_initialized)
            return;

        Initialize(defineInstruction);
    }

    protected override void OnDisableInstruction()
    {
        defineInstruction.onDefineChange.RemoveListener(RebuildFunctionInstance);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypesBlock.OnFunctionDefinitionRemoved, Remove);
        _initialized = false;
    }

    bool _initialized = false;
    public void Initialize(BE2_Ins_DefineFunction defineInstruction)
    {
        if (!defineInstruction)
            return;

        RebuildFunctionInstance();

        this.defineInstruction = defineInstruction;
        defineInstruction.onDefineChange.AddListener(RebuildFunctionInstance);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnFunctionDefinitionRemoved, Remove);

        defineID = defineInstruction.defineID;

        _initialized = true;
    }

    void Remove(I_BE2_Block block)
    {
        if (defineInstruction.Block == block)
        {
            Block.Transform.SetParent(null);
            I_BE2_BlocksStack stack = Block.Instruction.InstructionBase.BlocksStack;
            if (stack != null)
                stack.PopulateStack();

            BE2_BlockUtils.RemoveBlock(Block);
        }
    }

    public void RebuildFunctionInstance()
    {
        localVariables = new List<BE2_Op_FunctionLocalVariable>();

        StartCoroutine(C_RebuildFunctionInstance());
    }

    IEnumerator C_RebuildFunctionInstance()
    {
        yield return new WaitForEndOfFrame();

        I_BE2_BlockSectionBody body = Block.Layout.SectionsArray[0].Body;
        for (int i = body.ChildBlocksCount - 1; i >= 0; i--)
        {
            if (body.ChildBlocksArray[i] as Object)
                Destroy(body.ChildBlocksArray[i].Transform.gameObject);
        }

        foreach (I_BE2_Block childBlock in defineInstruction.Block.Layout.SectionsArray[0].Body.ChildBlocksArray)
        {
            InstantiateNoViewBlockRecursive(childBlock, Block.Layout.SectionsArray[0].Body.RectTransform);
        }
    }

    public BE2_Ins_FunctionBlock mirrorFunction;

    public List<BE2_Op_FunctionLocalVariable> localVariables;

    public List<string> localValues = new List<string>();

    void InstantiateNoViewBlockRecursive(I_BE2_Block mirrorBlock, Transform parent)
    {
        if (mirrorBlock is BE2_GhostBlock)
            return;

        I_BE2_Block noViewBlock = default;
        BE2_Ins_ReferenceFunctionBlock refFunction = default;

        if (mirrorBlock.Instruction.GetType() == typeof(BE2_Ins_FunctionBlock) && (mirrorBlock.Instruction as BE2_Ins_FunctionBlock).defineInstruction == defineInstruction)
        {
            mirrorFunction = mirrorBlock.Instruction as BE2_Ins_FunctionBlock;
            noViewBlock = mirrorBlock.InstantiateNoViewBlock<BE2_Ins_ReferenceFunctionBlock>();
            refFunction = noViewBlock.Instruction as BE2_Ins_ReferenceFunctionBlock;
            refFunction.Initialize(Block.Instruction as BE2_Ins_FunctionBlock);
        }
        else
        {
            noViewBlock = mirrorBlock.InstantiateNoViewBlock();
        }

        if (noViewBlock == null)
            return;

        int sectionIndex = 0;
        foreach (I_BE2_BlockSection section in mirrorBlock.Layout.SectionsArray)
        {
            I_BE2_BlockSectionHeader header = section.Header;
            header.UpdateInputsArray();
            I_BE2_BlockSection noViewSection = noViewBlock.Layout.SectionsArray[sectionIndex];
            int inputIndex = 0;
            foreach (I_BE2_BlockSectionHeaderInput input in header.InputsArray)
            {
                if (input is BE2_BlockSectionHeader_Operation)
                {
                    I_BE2_Block inputBlock = input.Transform.GetComponent<I_BE2_Block>();
                    InstantiateNoViewBlockRecursive(inputBlock, noViewSection.Header.RectTransform);
                }
                else if (input is BE2_BlockSectionHeader_LocalVariable)
                {
                    I_BE2_Block inputBlock = input.Transform.GetComponent<I_BE2_Block>();
                    InstantiateNoViewBlockRecursive(inputBlock, noViewSection.Header.RectTransform);
                }
                else
                {
                    GameObject nvInputGO = new GameObject("input", typeof(RectTransform));
                    nvInputGO.transform.SetParent(noViewSection.Header.RectTransform);
                    nvInputGO.transform.SetAsLastSibling();

                    BE2_BlockSectionHeader_ReferenceInput nvInput = nvInputGO.AddComponent<BE2_BlockSectionHeader_ReferenceInput>();
                    nvInput.referenceInput = input;
                }

                inputIndex++;
            }
            noViewSection.Header.UpdateInputsArray();

            if (!refFunction)
            {
                I_BE2_BlockSectionBody body = section.Body;
                if (body != null)
                {
                    body.UpdateChildBlocksList();
                    int bodyIndex = 0;
                    foreach (I_BE2_Block childBlock in body.ChildBlocksArray)
                    {
                        InstantiateNoViewBlockRecursive(childBlock, noViewSection.Body.RectTransform);

                        bodyIndex++;
                    }
                    noViewSection.Body.UpdateChildBlocksList();
                }
            }

            sectionIndex++;
        }

        noViewBlock.Transform.SetParent(parent);
        noViewBlock.Transform.SetAsLastSibling();

        if (noViewBlock.Instruction.GetType() == typeof(BE2_Op_FunctionLocalVariable))
        {
            // BE2_Op_FunctionLocalVariable localVariable = noViewBlock.Instruction as BE2_Op_FunctionLocalVariable;
            // localVariable.varName = mirrorBlock.Transform.GetComponentInChildren<TMP_Text>().text;
            // localVariables.Add(localVariable);

            StartCoroutine(C_SetLocalVarName(noViewBlock, mirrorBlock));
        }

        (noViewBlock.Instruction.InstructionBase as BE2_InstructionBase).Initialize();
    }

    // v2.12.1 - bugfix: tmp text not being found when Function Block A was set with local variable inside Define Block B 
    IEnumerator C_SetLocalVarName(I_BE2_Block noViewBlock, I_BE2_Block mirrorBlock)
    {
        yield return new WaitForEndOfFrame();

        BE2_Op_FunctionLocalVariable localVariable = noViewBlock.Instruction as BE2_Op_FunctionLocalVariable;
        TMP_Text tmpText = mirrorBlock.Transform.GetComponentInChildren<TMP_Text>();
        if (tmpText)
        {
            localVariable.varName = tmpText.text;
            localVariables.Add(localVariable);
        }
    }

    public override void OnPrepareToPlay()
    {
        foreach (BE2_Op_FunctionLocalVariable localvar in localVariables)
        {
            localvar.defineInstruction = defineInstruction;
            localvar.blockToObserve = Block as BE2_Block;
        }
    }

    public new void Function()
    {
        for (int i = 0; i < localValues.Count; i++)
        {
            localValues[i] = Section0Inputs[i].StringValue;
        }

        ExecuteSection(0);
    }
}
