using QuizSystem.SO;
using UnityEngine;

namespace QuizSystem.Custom
{
    public class ConditionalPropertyAttribute : PropertyAttribute
    {

        public string conditionPath;

        int enumValueIndex;

        public ConditionalPropertyAttribute(string condition)
        {
            this.conditionPath = condition;
        }

        public ConditionalPropertyAttribute(string conditionPath, int condition)
        {
            this.conditionPath = conditionPath;

            enumValueIndex = condition;
        }

        public int GetConditionEnumIndex()
        {
            return enumValueIndex;
        }
    } 
}