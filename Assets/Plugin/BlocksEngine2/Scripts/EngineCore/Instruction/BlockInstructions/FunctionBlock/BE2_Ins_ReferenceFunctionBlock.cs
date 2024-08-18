using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block.Instruction;

// v2.12 - new ReferenceFunctionBlock instruction to enable recursive functions
public class BE2_Ins_ReferenceFunctionBlock : BE2_InstructionBase, I_BE2_Instruction
{
    public BE2_Ins_ReferenceFunctionBlock(BE2_Ins_FunctionBlock functionInstruction)
    {
        this.functionInstruction = functionInstruction;
    }

    public BE2_Ins_FunctionBlock functionInstruction;

    public void Initialize(BE2_Ins_FunctionBlock functionInstruction)
    {
        this.functionInstruction = functionInstruction;
    }

    List<I_BE2_Instruction> _instructionsToReset;

    public override void OnPrepareToPlay()
    {
        _instructionsToReset = new List<I_BE2_Instruction>();
        bool start = false;
        foreach (I_BE2_Instruction instruction in BlocksStack.InstructionsArray)
        {
            if (start)
            {
                _instructionsToReset.Add(instruction);
            }

            if (instruction == functionInstruction as I_BE2_Instruction)
                start = true;

            if (instruction == this as I_BE2_Instruction)
                break;
        }
    }

    // v2.12.1 - refactored to use Function Blocks localVariables
    public new void Function()
    {
        foreach (I_BE2_Instruction instruction in _instructionsToReset)
        {
            instruction.Reset();
        }

        for (int i = 0; i < functionInstruction.mirrorFunction.localValues.Count; i++)
        {
            functionInstruction.localValues[i] = Section0Inputs[i].StringValue;
        }

        functionInstruction.ExecuteSection(0);
    }
}
