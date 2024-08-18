using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.Environment;
using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block.Instruction
{
    public class BE2_InstructionBase : MonoBehaviour, I_BE2_InstructionBase
    {
        I_BE2_BlockLayout _blockLayout;
        I_BE2_BlockSection[] _sectionsList;
        I_BE2_BlockSectionHeader _section0header;
        int _lastLocation;

        public I_BE2_Instruction Instruction { get; set; }
        public I_BE2_Block Block { get; set; }
        public I_BE2_BlocksStack BlocksStack { get; set; }
        public I_BE2_TargetObject TargetObject { get; set; }

        public int[] LocationsArray { get; set; }
        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnButtonPlay() { }
        protected virtual void OnButtonStop() { }
        public virtual void OnPrepareToPlay() { }
        public virtual void OnStackActive() { }
        // v2.11 - OnEnableInstruction and OnDisableInstruction virtual methods added to the instruction Base. Available to be used as needed but it is
        // mainly used on trigger instructions with conditions that should be checked on Update
        protected virtual void OnEnableInstruction() { }
        protected virtual void OnDisableInstruction() { }

        void Awake()
        {
            InstructionBase = this;
            Instruction = GetComponent<I_BE2_Instruction>();
            Block = GetComponent<I_BE2_Block>();
            _blockLayout = GetComponent<I_BE2_BlockLayout>();

            if (Block.Type == BlockTypeEnum.trigger)
            {
                BlocksStack = GetComponent<I_BE2_BlocksStack>();
            }

            OnAwake();
        }

        void Start()
        {
            Initialize();
            OnStart();
        }

        // v2.12 - Initialize method added to the InstructionBase class to enable blocks with "noView" layout to
        // initialize needed variables after it is fully constructed by Function Blocks
        public void Initialize()
        {
            _section0header = Block.Layout.SectionsArray[0].Header;
            _section0header.UpdateInputsArray();

            I_BE2_BlockSection[] tempSectionsArr = _blockLayout.SectionsArray;
            LocationsArray = new int[
                BE2_ArrayUtils.FindAll(ref tempSectionsArr, (x => x.Body != null)).Length + 1];

            _sectionsList = _blockLayout.SectionsArray;
        }

        // v2.10 - bugfix: trigger blocks won't work if programmingEnv is disable and enabled again
        void OnEnable()
        {
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPlay, OnButtonPlay);
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnStop, OnButtonStop);
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyUpEnd, GetBlockStack);

            OnEnableInstruction();
        }

        void OnDisable()
        {
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPlay, OnButtonPlay);
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnStop, OnButtonStop);
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyUpEnd, GetBlockStack);

            OnDisableInstruction();
        }

        // v2.9 - GetTargetObject method of the instruction base changed to UpdateTargetObject
        // v2.7 - added method on the instruction to get the TargetObject (not null when block is placed in a ProgrammingEnv)
        public void UpdateTargetObject()
        {
            TargetObject = GetComponentInParent<I_BE2_ProgrammingEnv>()?.TargetObject;
        }

        void GetBlockStack()
        {
            BlocksStack = GetComponentInParent<I_BE2_BlocksStack>();
            if (BlocksStack == null)
            {
                Block.SetShadowActive(false);
            }
            else if (BlocksStack.IsActive)
            {
                Block.SetShadowActive(true);
            }
        }

        public I_BE2_BlockSectionHeaderInput[] Section0Inputs
        {
            get
            {
                return _section0header?.InputsArray;
            }
        }

        public I_BE2_BlockSectionHeaderInput[] GetSectionInputs(int sectionIndex)
        {
            return _sectionsList[sectionIndex].Header.InputsArray;
        }

        public void PrepareToPlay()
        {
            _lastLocation = LocationsArray[LocationsArray.Length - 1];

            OnPrepareToPlay();
        }

        int _overflowLimit = 100;

        // v2.9 - ExecuteSection and ExecuteNextInstruction refactored to enable StepPlay and Pause
        public void ExecuteSection(int sectionIndex)
        {
            if (BlocksStack.InstructionsArray.Length > LocationsArray[sectionIndex])
            {
                // v2.9 - renamed instruction to nextInstruction
                I_BE2_Instruction nextInstruction = BlocksStack.InstructionsArray[LocationsArray[sectionIndex]];
                // v2.1 - Loops are now executed "in frame" instead of mandatorily "in update". Faster loop execution and nested loops without delay
                if (!BlocksStack.IsStepPlay)
                {
                    // v2.11 - new way to detect end of stack execution
                    if (nextInstruction.InstructionBase.Block.Type == BlockTypeEnum.trigger)
                    {
                        BlocksStack.OnStackLastBlockExecuted.Invoke();
                    }

                    if (!nextInstruction.ExecuteInUpdate && BlocksStack.OverflowGuard < _overflowLimit)
                    {
                        BlocksStack.OverflowGuard++;
                        ExecuteInstruction(nextInstruction);
                    }
                    else
                    {
                        BlocksStack.Pointer = LocationsArray[sectionIndex];
                    }
                }
                else
                {
                    // v2.12.1 - bugfix: Function Block being counted during the step by step play 
                    if (Block.Type == BlockTypeEnum.trigger || Block.BlockIsFunction())
                    {
                        BlocksStack.OverflowGuard++;
                        ExecuteInstruction(nextInstruction);
                    }
                    else
                    {
                        BlocksStack.Pointer = LocationsArray[sectionIndex];
                        BlocksStack.IsActive = false;
                    }
                }
            }
            else
            {
                BlocksStack.Pointer = LocationsArray[sectionIndex];
            }
        }

        // v2.9 - ExecuteSection and ExecuteNextInstruction refactored to enable StepPlay and Pause
        public void ExecuteNextInstruction()
        {
            if (BlocksStack.InstructionsArray.Length > _lastLocation)
            {
                // v2.9 - renamed instruction to nextInstruction
                I_BE2_Instruction nextInstruction = BlocksStack.InstructionsArray[_lastLocation];
                // v2.1 - Loops are now executed "in frame" instead of mandatorily "in update". Faster loop execution and nested loops without delay
                if (!BlocksStack.IsStepPlay)
                {
                    // v2.11 - new way to detect end of stack execution
                    if (nextInstruction.InstructionBase.Block.Type == BlockTypeEnum.trigger)
                    {
                        BlocksStack.OnStackLastBlockExecuted.Invoke();
                    }

                    if (BlocksStack.IsActive && !nextInstruction.ExecuteInUpdate && BlocksStack.OverflowGuard < _overflowLimit)
                    {
                        BlocksStack.OverflowGuard++;
                        ExecuteInstruction(nextInstruction);
                    }
                    else
                    {
                        // v2.11.2 - bugfix: blocks stack not starting from the beginning when execution was finished and new blocks are added
                        BlocksStack.Pointer = nextInstruction.InstructionBase.Block.Type == BlockTypeEnum.trigger ? 0 : _lastLocation;
                    }
                }
                else
                {
                    // v2.12.1 - bugfix: Function Block being counted during the step by step play
                    if (Block.Type == BlockTypeEnum.condition || Block.Type == BlockTypeEnum.loop || Block.BlockIsFunction())
                    {
                        BlocksStack.OverflowGuard++;
                        ExecuteInstruction(nextInstruction);
                    }
                    else
                    {
                        BlocksStack.Pointer = _lastLocation;
                        BlocksStack.IsActive = false;
                    }
                }
            }
            else
            {
                BlocksStack.Pointer = _lastLocation;

                if (BlocksStack.IsStepPlay)
                {
                    if (Block.Type == BlockTypeEnum.condition || Block.Type == BlockTypeEnum.loop)
                    {
                        ExecuteInstruction(BlocksStack.InstructionsArray[0]);
                    }
                }
            }
        }

        void ExecuteInstruction(I_BE2_Instruction instruction)
        {
            BlocksStack.OnFunctionStart.Invoke(instruction);
            instruction.Function();
        }

        // ### Instruction ###
        public I_BE2_InstructionBase InstructionBase { get; set; }
        public bool ExecuteInUpdate { get; }

        public string Operation() { return ""; }
        public void Function() { }
        public void Reset() { }
    }

}
