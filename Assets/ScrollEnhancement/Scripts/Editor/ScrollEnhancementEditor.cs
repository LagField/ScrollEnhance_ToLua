using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScrollEnhancement
{
    [CustomEditor(typeof(ScrollEnhancement))]
    public class ScrollEnhancementEditor : Editor
    {
        private ScrollEnhancement scrollEnhancement;

        private int testItemAmount;

        private void OnEnable()
        {
            scrollEnhancement = target as ScrollEnhancement;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            testItemAmount = EditorGUILayout.IntField("(编辑器)测试多个子物体的效果", testItemAmount);
            if (EditorGUI.EndChangeCheck())
            {
                testItemAmount = Mathf.Clamp(testItemAmount, 0, 100);
                scrollEnhancement.RefreshItemAmountInEditor(testItemAmount);
                scrollEnhancement.RefreshItemsLayoutInEditor();
            }

            if (GUILayout.Button("重置"))
            {
                testItemAmount = 0;
                scrollEnhancement.ClearAllChildItems();
            }
        }

    }
}