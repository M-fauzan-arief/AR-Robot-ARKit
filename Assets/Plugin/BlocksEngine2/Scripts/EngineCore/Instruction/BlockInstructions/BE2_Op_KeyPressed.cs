using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Utils;
using MG_BlocksEngine2.Core;

// v2.10 - Dropdown and InputField references in the default instructions replaced by BE2_Dropdown and BE2_InputField to enable the use of legacy or TMP components as Block inputs
public class BE2_Op_KeyPressed : BE2_InstructionBase, I_BE2_Instruction
{
    BE2_Dropdown _dropdown;

    protected override void OnStart()
    {
        _dropdown = BE2_Dropdown.GetBE2Component(Section0Inputs[0].Transform);

	// v2.12 - null check added to instructions that use specific components as inputs so its input can be replaced by a ReferenceInput
        if (_dropdown == null)
            return;

        PopulateDropdown();
        _dropdown.value = _dropdown.GetIndexOf("A");
    }

    void PopulateDropdown()
    {
        _dropdown.ClearOptions();
        string[] keys = System.Enum.GetNames(typeof(KeyCode));
        foreach (string key in keys)
        {
            _dropdown.AddOption(key);
        }
        _dropdown.RefreshShownValue();
    }

    public new string Operation()
    {
	// v2.12 - replace the use of the dropdown directly by the header input FloatValue to enable the substituion of the block input by a ReferenceInput
        if (Input.GetKey(BE2_InputManager.keyCodeList[(int)Section0Inputs[0].FloatValue]))
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }
}
