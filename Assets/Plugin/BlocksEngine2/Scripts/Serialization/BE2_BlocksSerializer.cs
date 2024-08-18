using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.DragDrop;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.Environment;
using MG_BlocksEngine2.Utils;
using MG_BlocksEngine2.Attribute;
using System.Linq;
using TMPro;
using MG_BlocksEngine2.EditorScript;
using MG_BlocksEngine2.UI.FunctionBlock;
using MG_BlocksEngine2.UI;

namespace MG_BlocksEngine2.Serializer
{
    // v2.12 - BE2_BlocksSerializer refactored to enable Function Blocks
    public static class BE2_BlocksSerializer
    {
        // v2.11 - BE2_BlocksSerializer.SaveCode refactored to use the BlocksCodeToXML method
        // v2.3 - added method SaveCode to facilitate the save of code by script
        public static void SaveCode(string path, I_BE2_ProgrammingEnv targetProgrammingEnv)
        {
            StreamWriter sw = new StreamWriter(path, false);
            sw.WriteLine(BlocksCodeToXML(targetProgrammingEnv));
            sw.Close();

            // v2.10.2 - bugfix: WebGL saves data not persisting after page reload
            PlayerPrefs.SetString("forceSave", string.Empty);
            PlayerPrefs.Save();
        }

        // v2.11 - added method BE2_BlocksSerializer.BlocksCodeToXML to make it possible to save or send the code XML string without the need for generating a .BE2 file 
        public static string BlocksCodeToXML(I_BE2_ProgrammingEnv targetProgrammingEnv)
        {
            string xmlString = "";

            targetProgrammingEnv.UpdateBlocksList();
            // v2.12 - the serialized blocks are reordered by placing the Define Function blocks first on the file
            // to guarantee that related Function Blocks are deserialization correctly
            List<I_BE2_Block> orderedBlocks = new List<I_BE2_Block>();
            orderedBlocks.AddRange(targetProgrammingEnv.BlocksList.OrderBy(OrderOnType));
            foreach (I_BE2_Block block in orderedBlocks)
            {
                xmlString += SerializableToXML(BlockToSerializable(block));
                xmlString += "\n#\n";
            }

            return xmlString;
        }

        // v2.12 - function to define the order of the serialized blocks
        private static int OrderOnType(I_BE2_Block block)
        {
            if (block.Type == BlockTypeEnum.define)
                return 0;

            return 1;
        }


        // v2.9 - BlockToSerializable refactored to enable and facilitate the addition of custom variable types
        public static BE2_SerializableBlock BlockToSerializable(I_BE2_Block block)
        {
            BE2_SerializableBlock serializableBlock = new BE2_SerializableBlock();

            serializableBlock.blockName = block.Transform.name;
            // v2.4 - bugfix: fixed blocks load in wrong position if resolution changes
            serializableBlock.position = block.Transform.localPosition;

            System.Type instructionType = block.Instruction.GetType();
            SerializeAsVariableAttribute varAttribute = (SerializeAsVariableAttribute)System.Attribute.GetCustomAttribute(instructionType, typeof(SerializeAsVariableAttribute));

            if (varAttribute != null)
            {
                System.Type varManagerType = varAttribute.variablesManagerType;

                serializableBlock.varManagerName = varManagerType.ToString();

                // v2.1 - using BE2_Text to enable usage of Text or TMP components
                BE2_Text varName = BE2_Text.GetBE2Text(block.Transform.GetChild(0).GetChild(0).GetChild(0));
                serializableBlock.varName = varName.text;
            }
            else
            {
                serializableBlock.varManagerName = "";
            }

            // v2.12 - serializer Function Blocks
            if (instructionType == typeof(BE2_Op_FunctionLocalVariable))
            {
                BE2_Text varName = BE2_Text.GetBE2Text(block.Transform.GetChild(0).GetChild(0).GetChild(0));
                serializableBlock.varName = varName.text;
                serializableBlock.isLocalVar = "true";
            }

            if (instructionType == typeof(BE2_Ins_FunctionBlock) || block.Type == BlockTypeEnum.define)
            {
                BE2_Ins_FunctionBlock functionBlock = block.Instruction as BE2_Ins_FunctionBlock;
                if (functionBlock != null)
                    serializableBlock.defineID = functionBlock.defineID;

                BE2_Ins_DefineFunction defineBlock = block.Instruction as BE2_Ins_DefineFunction;
                if (defineBlock != null)
                {
                    serializableBlock.defineID = defineBlock.defineID;
                    serializableBlock.defineItems = new List<DefineItem>();
                    int i = 0;
                    foreach (I_BE2_BlockSectionHeaderItem item in block.Layout.SectionsArray[0].Header.ItemsArray)
                    {
                        if (item.Transform.name.Contains("[FixedLabel]"))
                        {
                            i++;
                            continue;
                        }

                        BE2_BlockSectionHeader_Label labelItem = item.Transform.GetComponent<BE2_BlockSectionHeader_Label>();
                        BE2_BlockSectionHeader_InputField inputFieldItem = item.Transform.GetComponent<BE2_BlockSectionHeader_InputField>();
                        BE2_BlockSectionHeader_LocalVariable localVariableItem = item.Transform.GetComponent<BE2_BlockSectionHeader_LocalVariable>();
                        BE2_BlockSectionHeader_Custom customItem = item.Transform.GetComponent<BE2_BlockSectionHeader_Custom>();

                        if (labelItem)
                        {
                            serializableBlock.defineItems.Add(new DefineItem("label", item.Transform.GetComponent<TMP_Text>().text));
                        }
                        else if (inputFieldItem || localVariableItem)
                        {
                            serializableBlock.defineItems.Add(new DefineItem("variable", item.Transform.GetComponentInChildren<TMP_Text>().text));
                        }
                        else if (customItem)
                        {
                            serializableBlock.defineItems.Add(new DefineItem("custom", customItem.serializableValue));
                        }

                        i++;
                    }
                }
            }

            foreach (I_BE2_BlockSection section in block.Layout.SectionsArray)
            {
                BE2_SerializableSection serializableSection = new BE2_SerializableSection();
                serializableBlock.sections.Add(serializableSection);

                foreach (I_BE2_BlockSectionHeaderInput input in section.Header.InputsArray)
                {
                    BE2_SerializableInput serializableInput = new BE2_SerializableInput();
                    serializableSection.inputs.Add(serializableInput);

                    I_BE2_Block inputBlock = input.Transform.GetComponent<I_BE2_Block>();
                    if (inputBlock != null)
                    {
                        serializableInput.isOperation = true;
                        serializableInput.operation = BlockToSerializable(inputBlock);

                        serializableInput.value = input.InputValues.stringValue;
                    }
                    else
                    {
                        serializableInput.isOperation = false;
                        serializableInput.value = input.InputValues.stringValue;
                    }
                }

                // v2.12 - condition to not serialize blocks child of Function Blocks (No View blocks), they are recreated automatically
                if (section.Body != null && block.Instruction.GetType() != typeof(BE2_Ins_FunctionBlock))
                {
                    foreach (I_BE2_Block childBlock in section.Body.ChildBlocksArray)
                    {
                        serializableSection.childBlocks.Add(BlockToSerializable(childBlock));
                    }
                }
            }

            return serializableBlock;
        }

        public static string SerializableToXML(BE2_SerializableBlock serializableBlock)
        {
            // JsonUtility has a depth limitation but you can use another Json alternative
            return BE2_BlockXML.SBlockToXElement(serializableBlock).ToString();
        }

        // v2.11 - BE2_BlocksSerializer.LoadCode refactored to use the XMLToBlocksCode method
        // v2.3 - added method LoadCode to facilitate the load of code by script
        public static bool LoadCode(string path, I_BE2_ProgrammingEnv targetProgrammingEnv)
        {
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                string xmlCode = sr.ReadToEnd();
                sr.Close();

                XMLToBlocksCode(xmlCode, targetProgrammingEnv);

                return true;
            }

            return false;
        }

        // v2.11 - added method BE2_BlocksSerializer.XMLToBlocksCode to make it possible to load code from a XML string without the need for a .BE2 file 
        public static void XMLToBlocksCode(string xmlString, I_BE2_ProgrammingEnv targetProgrammingEnv)
        {
            string[] xmlBlocks = xmlString.Split('#');

            foreach (string xmlBlock in xmlBlocks)
            {
                BE2_SerializableBlock serializableBlock = XMLToSerializable(xmlBlock);

                // v2.12 - check if the serialized Define Function Block already exists on scene
                bool serializeBlock = true;
                if (serializableBlock != null && serializableBlock.blockName == "Block Ins DefineFunction")
                {
                    targetProgrammingEnv.UpdateBlocksList();
                    BE2_Ins_DefineFunction define = default;
                    foreach (I_BE2_Block envBlock in targetProgrammingEnv.BlocksList)
                    {
                        define = envBlock.Instruction as BE2_Ins_DefineFunction;
                        if (define != null)
                        {
                            if (define.defineID == serializableBlock.defineID)
                            {
                                serializeBlock = false;
                                break;
                            }
                        }
                    }
                }

                if (serializeBlock)
                    SerializableToBlock(serializableBlock, targetProgrammingEnv);
            }
        }

        public static BE2_SerializableBlock XMLToSerializable(string blockString)
        {
            // v2.2 - bugfix: fixed empty blockString from XML file causing error on load
            blockString = blockString.Trim();
            if (blockString.Length > 1)
            {
                // JsonUtility has a depth limitation but you can use another Json alternative
                XElement xBlock = XElement.Parse(blockString);
                return BE2_BlockXML.XElementToSBlock(xBlock);
            }
            else
            {
                return null;
            }
        }

        // v2.12.1 - added counter variable in the serializer to chech end of serialization of all inputs
        static int counterForEndOfDeserialization = 0;
        static IEnumerator C_AddInputs(I_BE2_Block block, BE2_SerializableBlock serializableBlock, I_BE2_ProgrammingEnv programmingEnv)
        {
            yield return new WaitForEndOfFrame();

            I_BE2_BlockSection[] sections = block.Layout.SectionsArray;

            for (int s = 0; s < sections.Length; s++)
            {
                // v2.12 - deserialize local variables of Function Block definitions  
                if (block.Instruction.GetType() != typeof(BE2_Ins_DefineFunction))
                {
                    I_BE2_BlockSectionHeaderInput[] inputs = sections[s].Header.InputsArray;
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        BE2_SerializableInput serializableInput = serializableBlock.sections[s].inputs[i];
                        if (serializableInput.isOperation)
                        {
                            I_BE2_Block operation = SerializableToBlock(serializableInput.operation, programmingEnv);

                            if (operation.Instruction.GetType() == typeof(BE2_Op_FunctionLocalVariable))
                            {
                                operation.Transform.GetComponentInChildren<TMP_Text>().text = serializableInput.value;
                            }

                            BE2_DragDropManager.Instance.CurrentSpot = inputs[i].Transform.GetComponent<I_BE2_Spot>();
                            operation.Transform.GetComponent<I_BE2_Drag>().OnPointerDown();
                            operation.Transform.GetComponent<I_BE2_Drag>().OnPointerUp();
                        }
                        else
                        {

                            // v2.10 - Dropdown and InputField references replaced by BE2_Dropdown and BE2_InputField to enable the use of legacy or TMP components
                            BE2_InputField inputText = BE2_InputField.GetBE2Component(inputs[i].Transform);
                            BE2_Dropdown inputDropdown = BE2_Dropdown.GetBE2Component(inputs[i].Transform);
                            if (inputText != null && !inputText.isNull)
                            {
                                inputText.text = serializableInput.value;
                            }
                            else if (inputDropdown != null && !inputDropdown.isNull)
                            {
                                inputDropdown.value = inputDropdown.GetIndexOf(serializableInput.value);
                            }
                        }

                        if (serializableBlock.isLocalVar == "true")
                        {
                            //                                        | block        | section   | header    | text      |
                            // BE2_Text newVarName = BE2_Text.GetBE2Text(block.Transform.GetChild(0).GetChild(0).GetChild(0));
                            TMP_Text newVarName = block.Transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                            newVarName.text = serializableBlock.varName;
                        }

                        inputs[i].UpdateValues();
                    }
                }

                I_BE2_BlockSectionBody body = sections[s].Body;
                if (body != null)
                {
                    // add children
                    foreach (BE2_SerializableBlock serializableChildBlock in serializableBlock.sections[s].childBlocks)
                    {
                        I_BE2_Block childBlock = SerializableToBlock(serializableChildBlock, programmingEnv);
                        childBlock.Transform.SetParent(body.RectTransform);
                    }
                }

                sections[s].Header.UpdateItemsArray();
                sections[s].Header.UpdateInputsArray();
            }

            yield return null;

            counterForEndOfDeserialization--;
        }

        // v2.9 - SerializableToBlock refactored to enable and facilitate the addition of custom variable types
        public static I_BE2_Block SerializableToBlock(BE2_SerializableBlock serializableBlock, I_BE2_ProgrammingEnv programmingEnv)
        {
            I_BE2_Block block = null;

            if (serializableBlock != null)
            {
                string prefabName = serializableBlock.blockName;
                GameObject loadedPrefab = BE2_BlockUtils.LoadPrefabBlock(prefabName);

                if (loadedPrefab)
                {
                    GameObject blockGo = MonoBehaviour.Instantiate(
                        loadedPrefab,
                        serializableBlock.position,
                        Quaternion.identity,
                        programmingEnv.Transform) as GameObject;

                    blockGo.name = prefabName;

                    // v2.6 - adjustments on position and angle of blocks for supporting all canvas render modes              
                    // v2.4 - bugfix: fixed blocks load in wrong position if resolution changes
                    blockGo.transform.localPosition = new Vector3(serializableBlock.position.x, serializableBlock.position.y, 0);
                    blockGo.transform.localEulerAngles = Vector3.zero;

                    block = blockGo.GetComponent<I_BE2_Block>();

                    // v2.12 - deserializer Function Blocks
                    if (block.Instruction as BE2_Ins_DefineFunction)
                    {
                        (block.Instruction as BE2_Ins_DefineFunction).defineID = serializableBlock.defineID;

                        foreach (DefineItem item in serializableBlock.defineItems)
                        {
                            if (item.type == "label")
                            {
                                GameObject labelDefine = MonoBehaviour.Instantiate(BE2_Inspector.Instance.LabelTextTemplate, Vector3.zero, Quaternion.identity,
                                                                block.Layout.SectionsArray[0].Header.RectTransform);
                                labelDefine.GetComponentInChildren<TMP_Text>().text = item.value;
                            }
                            else if (item.type == "variable")
                            {
                                GameObject inputDefine = MonoBehaviour.Instantiate(BE2_FunctionBlocksManager.Instance.templateDefineLocalVariable, Vector3.zero, Quaternion.identity,
                                                                block.Layout.SectionsArray[0].Header.RectTransform);

                                inputDefine.GetComponentInChildren<TMP_Text>().text = item.value;
                            }
                            else if (item.type == "custom")
                            {
                                GameObject inputDefine = MonoBehaviour.Instantiate(BE2_FunctionBlocksManager.Instance.templateDefineCustomHeaderItem, Vector3.zero, Quaternion.identity,
                                                                block.Layout.SectionsArray[0].Header.RectTransform);

                                inputDefine.GetComponentInChildren<BE2_BlockSectionHeader_Custom>().serializableValue = item.value;
                            }
                        }

                        BE2_FunctionBlocksManager.Instance.CreateSelectionFunction(serializableBlock.defineItems, block.Instruction as BE2_Ins_DefineFunction);
                    }

                    if (block.Instruction is BE2_Ins_FunctionBlock)
                    {
                        programmingEnv.UpdateBlocksList();
                        BE2_Ins_DefineFunction define = default;
                        foreach (I_BE2_Block envBlock in programmingEnv.BlocksList)
                        {
                            define = envBlock.Instruction as BE2_Ins_DefineFunction;
                            if (define != null)
                            {
                                // v2.12.1 - bugfix: loading more than one Function Blocks not recognizing the correct Define Block  
                                if (define.defineID == serializableBlock.defineID)
                                {
                                    break;
                                }
                            }
                        }

                        int i = 0;
                        define.Block.Layout.SectionsArray[0].Header.UpdateItemsArray();
                        foreach (I_BE2_BlockSectionHeaderItem item in define.Block.Layout.SectionsArray[0].Header.ItemsArray)
                        {
                            if (item.Transform.name.Contains("[FixedLabel]"))
                            {
                                i++;
                                continue;
                            }

                            if (item is BE2_BlockSectionHeader_Label)
                            {
                                GameObject label = MonoBehaviour.Instantiate(BE2_Inspector.Instance.LabelTextTemplate, Vector3.zero, Quaternion.identity,
                                                                block.Layout.SectionsArray[0].Header.RectTransform);
                                label.GetComponent<TMP_Text>().text = item.Transform.GetComponent<TMP_Text>().text;
                            }
                            else if (item is BE2_BlockSectionHeader_LocalVariable)
                            {
                                GameObject input = MonoBehaviour.Instantiate(BE2_Inspector.Instance.InputFieldTemplate, Vector3.zero, Quaternion.identity,
                                                                block.Layout.SectionsArray[0].Header.RectTransform);
                            }
                            else if (item is BE2_BlockSectionHeader_Custom)
                            {
                                GameObject input = MonoBehaviour.Instantiate(BE2_FunctionBlocksManager.Instance.templateDefineCustomHeaderItem, Vector3.zero, Quaternion.identity,
                                                                block.Layout.SectionsArray[0].Header.RectTransform);
                                input.GetComponentInChildren<BE2_BlockSectionHeader_Custom>().serializableValue = item.Transform.GetComponentInChildren<BE2_BlockSectionHeader_Custom>().serializableValue;
                            }

                            i++;
                        }

                        BE2_ExecutionManager.Instance.StartCoroutine(C_InitializeFunctionInstruction(block.Instruction as BE2_Ins_FunctionBlock, define));
                    }

                    if (serializableBlock.varManagerName != null && serializableBlock.varManagerName != "")
                    {
                        //                                        | block        | section   | header    | text      |
                        BE2_Text newVarName = BE2_Text.GetBE2Text(block.Transform.GetChild(0).GetChild(0).GetChild(0));
                        newVarName.text = serializableBlock.varName;

                        System.Type varManagerType = System.Type.GetType(serializableBlock.varManagerName);
                        if (varManagerType != null)
                        {
                            I_BE2_VariablesManager varManager = MonoBehaviour.FindObjectOfType(varManagerType) as I_BE2_VariablesManager;
                            varManager.CreateAndAddVarToPanel(serializableBlock.varName);
                        }
                        else
                        {
                            Debug.Log("Variables manager of type *" + serializableBlock.varManagerName + "* was not found.");
                        }
                    }

                    if (serializableBlock.isLocalVar == "true")
                    {
                        //                                        | block        | section   | header    | text      |
                        // BE2_Text newVarName = BE2_Text.GetBE2Text(block.Transform.GetChild(0).GetChild(0).GetChild(0));
                        TMP_Text newVarName = block.Transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                        newVarName.text = serializableBlock.varName;
                    }

                    // add inputs
                    counterForEndOfDeserialization++;

                    BE2_ExecutionManager.Instance.StartCoroutine(C_AddInputs(block, serializableBlock, programmingEnv));

                    if (block.Type == BlockTypeEnum.trigger && block.Type != BlockTypeEnum.define)
                    {
                        BE2_ExecutionManager.Instance.AddToBlocksStackArray(block.Instruction.InstructionBase.BlocksStack, programmingEnv.TargetObject);

                        // v2.11 - bugfix: trigger blocks loaded from the save menu didn't execut correctly if no PrimaryKey click was done before 
                        block.Instruction.InstructionBase.BlocksStack.PopulateStack();
                    }
                }

                // v2.12 - added unload prefabs after use to liberate memory
                BE2_BlockUtils.UnloadPrefab();
            }

            return block;
        }

        // v2.12 - all Function Block Instructions are initialized after being loaded to make sure it has a definition set (DefineFunction Instruction)
        static IEnumerator C_InitializeFunctionInstruction(BE2_Ins_FunctionBlock functionInstruction, BE2_Ins_DefineFunction defineInstruction)
        {
            yield return new WaitForEndOfFrame();
            functionInstruction.Initialize(defineInstruction);

            // v2.12.1 - bugfix: Function Blocks not being rebuild right after being loaded
            yield return new WaitUntil(() => counterForEndOfDeserialization == 0);
            functionInstruction.RebuildFunctionInstance();
        }
    }
}
