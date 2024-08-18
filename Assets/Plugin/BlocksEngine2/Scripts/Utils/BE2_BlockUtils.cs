using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.DragDrop;
using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.UI;
using MG_BlocksEngine2.Environment;
using MG_BlocksEngine2.Serializer;

namespace MG_BlocksEngine2.Utils
{
    public static class BE2_BlockUtils
    {
        public static void RemoveEngineComponents(Transform blockTransform)
        {
            I_BE2_BlockSectionHeaderItem[] headerItems = blockTransform.GetComponentsInChildren<I_BE2_BlockSectionHeaderItem>();
            for (int i = headerItems.Length - 1; i >= 0; i--) MonoBehaviour.DestroyImmediate(headerItems[i] as MonoBehaviour);

            I_BE2_BlockSection[] sections = blockTransform.GetComponentsInChildren<I_BE2_BlockSection>();
            for (int i = sections.Length - 1; i >= 0; i--) MonoBehaviour.DestroyImmediate(sections[i] as MonoBehaviour);

            I_BE2_BlockSectionBody[] bodies = blockTransform.GetComponentsInChildren<I_BE2_BlockSectionBody>();
            for (int i = bodies.Length - 1; i >= 0; i--) MonoBehaviour.DestroyImmediate(bodies[i] as MonoBehaviour);

            I_BE2_BlockSectionHeader[] headers = blockTransform.GetComponentsInChildren<I_BE2_BlockSectionHeader>();
            for (int i = headers.Length - 1; i >= 0; i--) MonoBehaviour.DestroyImmediate(headers[i] as MonoBehaviour);

            BE2_SpotOuterArea outerArea = blockTransform.GetComponentInChildren<BE2_SpotOuterArea>();
            if (outerArea) MonoBehaviour.DestroyImmediate(outerArea.transform.gameObject);
            I_BE2_Spot[] spots = blockTransform.GetComponentsInChildren<I_BE2_Spot>();
            for (int i = spots.Length - 1; i >= 0; i--) MonoBehaviour.DestroyImmediate(spots[i] as MonoBehaviour);

            I_BE2_Drag drag = blockTransform.GetComponent<I_BE2_Drag>();
            if (drag != null) MonoBehaviour.DestroyImmediate(drag as MonoBehaviour);

            I_BE2_Instruction instruction = blockTransform.GetComponent<I_BE2_Instruction>();
            if (instruction != null) MonoBehaviour.DestroyImmediate(instruction as MonoBehaviour);

            I_BE2_BlockLayout layout = blockTransform.GetComponent<I_BE2_BlockLayout>();
            if (layout != null) MonoBehaviour.DestroyImmediate(layout as MonoBehaviour);

            I_BE2_Block block = blockTransform.GetComponent<I_BE2_Block>();
            if (block != null) MonoBehaviour.DestroyImmediate(block as MonoBehaviour);

            I_BE2_BlocksStack blockStack = blockTransform.GetComponent<I_BE2_BlocksStack>();
            if (blockStack != null) MonoBehaviour.DestroyImmediate(blockStack as MonoBehaviour);
        }

        public static void AddSelectionMenuComponents(Transform blockTransform)
        {
            GameObject blockGameObject = blockTransform.gameObject;
            blockGameObject.AddComponent<BE2_UI_SelectionBlock>();
            blockGameObject.AddComponent<BE2_DragSelectionBlock>();
        }

        public static void DuplicateBlock(I_BE2_Block block)
        {
            I_BE2_ProgrammingEnv programmingEnv = block.Transform.GetComponentInParent<I_BE2_ProgrammingEnv>();
            I_BE2_Block newBlock = BE2_BlocksSerializer.SerializableToBlock(BE2_BlocksSerializer.BlockToSerializable(block), programmingEnv);

            // v2.6 - bugfix: fixed block being instantiated at wrong position on "Duplicate"
            newBlock.Transform.position = block.Transform.position + new Vector3(10, 10, 0);

            if (newBlock.Type == BlockTypeEnum.trigger)
            {
                BE2_ExecutionManager.Instance.AddToBlocksStackArray(newBlock.Instruction.InstructionBase.BlocksStack, programmingEnv.TargetObject);
            }
        }

        // v2.6 - new method added to Block Utils to get root parent block
        public static I_BE2_Block GetRootBlock(I_BE2_Block block)
        {
            I_BE2_Block parentBlock = block.ParentSection?.Block;
            if (parentBlock != null)
            {
                return GetRootBlock(parentBlock);
            }
            else
            {
                return block;
            }
        }

        public static void RemoveBlock(I_BE2_Block block)
        {
            if (block.Type == BlockTypeEnum.trigger)
            {
                BE2_ExecutionManager.Instance.RemoveFromBlocksStackList(block.Instruction.InstructionBase.BlocksStack);
            }

            MonoBehaviour.Destroy(block.Transform.gameObject);
        }

        public static GameObject CreatePrefab(I_BE2_Block block)
        {
            GameObject prefabBlock = null;
#if UNITY_EDITOR
            // v2.3 - using settable paths
            string localPath = BE2_Paths.TranslateMarkupPath(BE2_Paths.NewBlockPrefabPath) + block.Transform.name + ".prefab";
            prefabBlock = PrefabUtility.SaveAsPrefabAssetAndConnect(block.Transform.gameObject, localPath, InteractionMode.UserAction);
            PrefabUtility.UnpackPrefabInstance(block.Transform.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
#endif
            return prefabBlock;
        }

        public static I_BE2_Instruction GetParentInstructionOfType(I_BE2_Instruction thisInstruction, BlockTypeEnum blockType)
        {
            I_BE2_Instruction foundInstruction = null;

            I_BE2_BlockSectionBody parentBody = thisInstruction.InstructionBase.Block.Transform.parent.GetComponent<BE2_BlockSectionBody>();
            if (parentBody != null)
            {
                I_BE2_Block parentBlock = parentBody.BlockSection.Block;
                if (parentBlock.Type == blockType)
                {
                    foundInstruction = parentBlock.Instruction;
                }
                else
                {
                    foundInstruction = GetParentInstructionOfType(parentBlock.Instruction, blockType);
                }
            }

            return foundInstruction;
        }

        // v2.12 - added method LoadPrefabBlock to the BlockUtils to better separate the serialization actions
        public static GameObject LoadPrefabBlock(string prefabName)
        {
            GameObject loadedPrefab = Resources.Load<GameObject>("Blocks/" + prefabName);
            if (!loadedPrefab)
                loadedPrefab = Resources.Load<GameObject>("Blocks/Custom/" + prefabName);
            if (!loadedPrefab)
                loadedPrefab = Resources.Load<GameObject>("Blocks/FunctionBlock/" + prefabName);
            // v2.3 - using settable paths
            if (!loadedPrefab)
                loadedPrefab = Resources.Load<GameObject>(BE2_Paths.PathToResources(BE2_Paths.TranslateMarkupPath(BE2_Paths.NewBlockPrefabPath)) + prefabName);

            return loadedPrefab;
        }
        public static void UnloadPrefab()
        {
            Resources.UnloadUnusedAssets();
        }

        // v2.4 - added new method GetParentInstructionOfTypeAll to BE2_BlockUtils
        public static List<I_BE2_Instruction> GetParentInstructionOfTypeAll(I_BE2_Instruction thisInstruction, BlockTypeEnum blockType)
        {
            List<I_BE2_Instruction> foundInstructions = new List<I_BE2_Instruction>();

            I_BE2_BlockSectionBody parentBody = thisInstruction.InstructionBase.Block.Transform.parent.GetComponent<BE2_BlockSectionBody>();
            if (parentBody != null)
            {
                I_BE2_Block parentBlock = parentBody.BlockSection.Block;
                if (parentBlock.Type == blockType)
                {
                    foundInstructions.Add(parentBlock.Instruction);
                }

                foundInstructions.AddRange(GetParentInstructionOfTypeAll(parentBlock.Instruction, blockType));
            }

            return foundInstructions;
        }

        // v2.10 - BlockIsVariable method added to BE2_BlockUtils class to identify Variable blocks 
        public static bool BlockIsVariable(this I_BE2_Block block)
        {
            System.Type instructionType = block.Instruction.GetType();
            Attribute.SerializeAsVariableAttribute varAttribute = (Attribute.SerializeAsVariableAttribute)System.Attribute.GetCustomAttribute(instructionType, typeof(Attribute.SerializeAsVariableAttribute));

            return varAttribute != null ? true : false;
        }

        // v2.12 - added method to the BlockUtils to check if a block's instruction is of type FunctionBlock
        public static bool BlockIsFunction(this I_BE2_Block block)
        {
            return block.Instruction.GetType() == typeof(BE2_Ins_FunctionBlock);
        }

        // v2.11 - added CallOnEndOfFrame(this MonoBehaviour, System.Action) method to the Utils class
        // derived from https://forum.unity.com/threads/tip-invoke-any-function-with-delay-also-with-parameters.978273/
        public static void CallOnEndOfFrame(this MonoBehaviour monoBehaviour, System.Action action)
        {
            monoBehaviour.StartCoroutine(InvokeRoutine(action));
        }
        private static IEnumerator InvokeRoutine(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }

        // v2.12 - added method to the BlockUtils to instantiate "noView" blocks adding only the needed components excluding the visual components
        public static I_BE2_Block InstantiateNoViewBlock(this I_BE2_Block block)
        {
            if (block is BE2_GhostBlock)
                return null;

            I_BE2_Block noViewBlock = null;
            I_BE2_Instruction noViewInstruction = null;

            GameObject noViewBlockGO = new GameObject(block.Transform.name + " noView", typeof(RectTransform));
            BE2_BlockNoViewLayout layout = noViewBlockGO.AddComponent<BE2_BlockNoViewLayout>();
            noViewBlock = noViewBlockGO.AddComponent<BE2_Block>() as I_BE2_Block;
            noViewBlock.Type = block.Type;

            for (int i = 0; i < block.Layout.SectionsArray.Length; i++)
            {
                I_BE2_BlockSection section = block.Layout.SectionsArray[i];

                GameObject nvSectionGO = new GameObject("section", typeof(RectTransform));
                nvSectionGO.transform.SetParent(noViewBlockGO.transform);
                nvSectionGO.transform.SetAsLastSibling();
                BE2_NoViewBlockSection nvSection = nvSectionGO.AddComponent<BE2_NoViewBlockSection>();
                nvSection.index = i;

                if (section.Header != null)
                {
                    GameObject nvSectionHeaderGO = new GameObject("header", typeof(RectTransform));
                    nvSectionHeaderGO.transform.SetParent(nvSectionGO.transform);
                    nvSectionHeaderGO.transform.SetAsLastSibling();
                    BE2_NoViewBlockSectionHeader nvSectionHeader = nvSectionHeaderGO.AddComponent<BE2_NoViewBlockSectionHeader>();

                    nvSection.Header = nvSectionHeader;
                }

                if (section.Body != null)
                {
                    GameObject nvSectionBodyGO = new GameObject("body", typeof(RectTransform));
                    nvSectionBodyGO.transform.SetParent(nvSectionGO.transform);
                    nvSectionBodyGO.transform.SetAsLastSibling();
                    BE2_NoViewBlockSectionBody nvSectionBody = nvSectionBodyGO.AddComponent<BE2_NoViewBlockSectionBody>();

                    nvSection.Body = nvSectionBody;
                }
            }

            layout.Initialize();

            noViewInstruction = noViewBlockGO.AddComponent(block.Instruction.GetType()) as I_BE2_Instruction;
            if (noViewInstruction is BE2_Ins_FunctionBlock)
            {
                (noViewInstruction as BE2_Ins_FunctionBlock).Initialize((block.Instruction as BE2_Ins_FunctionBlock).defineInstruction);
            }
            noViewBlock.Instruction = noViewInstruction;

            if (noViewBlock.Type == BlockTypeEnum.operation)
            {
                if (noViewBlockGO.GetComponent<BE2_Op_FunctionLocalVariable>())
                    noViewBlockGO.AddComponent<BE2_BlockSectionHeader_LocalVariable>();
                else
                    noViewBlockGO.AddComponent<BE2_BlockSectionHeader_Operation>();
            }

            return noViewBlock;
        }

        // v2.12 - added method to the BlockUtils to instantiate "noView" blocks adding only the needed components excluding the visual components
        public static I_BE2_Block InstantiateNoViewBlock<T>(this I_BE2_Block block) where T : I_BE2_Instruction
        {
            if (block is BE2_GhostBlock)
                return null;

            I_BE2_Block noViewBlock = null;
            I_BE2_Instruction noViewInstruction = null;

            GameObject noViewBlockGO = new GameObject(block.Transform.name + " noView", typeof(RectTransform));
            BE2_BlockNoViewLayout layout = noViewBlockGO.AddComponent<BE2_BlockNoViewLayout>();
            noViewBlock = noViewBlockGO.AddComponent<BE2_Block>() as I_BE2_Block;
            noViewBlock.Type = block.Type;

            for (int i = 0; i < block.Layout.SectionsArray.Length; i++)
            {
                I_BE2_BlockSection section = block.Layout.SectionsArray[i];

                GameObject nvSectionGO = new GameObject("section", typeof(RectTransform));
                nvSectionGO.transform.SetParent(noViewBlockGO.transform);
                nvSectionGO.transform.SetAsLastSibling();
                BE2_NoViewBlockSection nvSection = nvSectionGO.AddComponent<BE2_NoViewBlockSection>();
                nvSection.index = i;

                if (section.Header != null)
                {
                    GameObject nvSectionHeaderGO = new GameObject("header", typeof(RectTransform));
                    nvSectionHeaderGO.transform.SetParent(nvSectionGO.transform);
                    nvSectionHeaderGO.transform.SetAsLastSibling();
                    BE2_NoViewBlockSectionHeader nvSectionHeader = nvSectionHeaderGO.AddComponent<BE2_NoViewBlockSectionHeader>();

                    nvSection.Header = nvSectionHeader;
                }

                if (section.Body != null)
                {
                    GameObject nvSectionBodyGO = new GameObject("body", typeof(RectTransform));
                    nvSectionBodyGO.transform.SetParent(nvSectionGO.transform);
                    nvSectionBodyGO.transform.SetAsLastSibling();
                    BE2_NoViewBlockSectionBody nvSectionBody = nvSectionBodyGO.AddComponent<BE2_NoViewBlockSectionBody>();

                    nvSection.Body = nvSectionBody;
                }
            }

            layout.Initialize();

            noViewInstruction = noViewBlockGO.AddComponent(typeof(T)) as I_BE2_Instruction;
            if (noViewInstruction is BE2_Ins_FunctionBlock)
            {
                (noViewInstruction as BE2_Ins_FunctionBlock).Initialize((block.Instruction as BE2_Ins_FunctionBlock).defineInstruction);
            }
            noViewBlock.Instruction = noViewInstruction;

            if (noViewBlock.Type == BlockTypeEnum.operation)
                noViewBlockGO.AddComponent<BE2_BlockSectionHeader_Operation>();

            return noViewBlock;
        }
    }
}
