using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_BlocksEngine2.Environment
{
    // v2.12 - added Function Blocks manager component
    public abstract class BE2_FunctionBlocksManager : MonoBehaviour
    {
        static BE2_FunctionBlocksManager _instance;
        public static BE2_FunctionBlocksManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<BE2_FunctionBlocksManager>();
                }
                return _instance;
            }
            set => _instance = value;
        }

        public GameObject DefineFunctionBlockTemplate;
        public GameObject templateSelectionBlock;
        public GameObject templateDefineLocalVariable;
        public GameObject templateDefineCustomHeaderItem;
        public Transform functionBlocksPanel;
        public Vector2 positionToInstantiate = new Vector2(300, -100);

        public virtual void CreateFunctionBlock(List<MG_BlocksEngine2.Serializer.DefineItem> items) { }
        public virtual void CreateSelectionFunction(List<MG_BlocksEngine2.Serializer.DefineItem> items, BE2_Ins_DefineFunction defineBlockIns) { }
    }
}
