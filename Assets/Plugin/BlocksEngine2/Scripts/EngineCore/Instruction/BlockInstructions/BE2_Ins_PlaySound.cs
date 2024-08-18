using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Environment;
using MG_BlocksEngine2.Utils;

// v2.10 - Dropdown and InputField references in the default instructions replaced by BE2_Dropdown and BE2_InputField to enable the use of legacy or TMP components as Block inputs
public class BE2_Ins_PlaySound : BE2_InstructionBase, I_BE2_Instruction
{
    BE2_Dropdown _dropdown;

    void PopulateDropdown()
    {
        _dropdown.ClearOptions();
        foreach (AudioClip clip in BE2_AudioManager.instance.audiosArray)
        {
            _dropdown.AddOption(clip.name);
        }
        _dropdown.value = 1;
        _dropdown.RefreshShownValue();
        _dropdown.value = 0;
    }

    protected override void OnStart()
    {
        _dropdown = BE2_Dropdown.GetBE2Component(GetSectionInputs(0)[0].Transform);

	// v2.12 - null check added to instructions that use specific components as inputs so its input can be replaced by a ReferenceInput
        if (_dropdown == null)
            return;

        PopulateDropdown();
    }

    string _value;

    public new void Function()
    {
        _value = Section0Inputs[0].StringValue;

        int idx = _dropdown.GetIndexOf(_value);

        BE2_AudioManager.instance.PlaySound(idx);
        ExecuteNextInstruction();
    }
}
