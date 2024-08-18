using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Utils;

// v2.10 - Dropdown and InputField references in the default instructions replaced by BE2_Dropdown and BE2_InputField to enable the use of legacy or TMP components as Block inputs
public class BE2_Op_JoystickKeyPressed : BE2_InstructionBase, I_BE2_Instruction
{
    BE2_VirtualJoystick _virtualJoystick;

    protected override void OnStart()
    {
        _virtualJoystick = BE2_VirtualJoystick.instance;
    }

    public new string Operation()
    {
	// v2.12 - replace the use of the dropdown directly by the header input FloatValue to enable the substituion of the block input by a ReferenceInput
        if (_virtualJoystick.keys[(int)Section0Inputs[0].FloatValue].isPressed)
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }
}
