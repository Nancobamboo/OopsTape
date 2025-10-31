using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

public class AnimatorTransitionModifier
{
    [MenuItem("EditorTools/Modify All Animator Transitions")]
    public static void ModifyAllAnimatorTransitions()
    {
        // 查找所有Animator Controller资源
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController");

        if (guids.Length == 0)
        {
            Debug.LogWarning("No Animator Controllers found in the project.");
            return;
        }

        int modifiedCount = 0;
        int transitionCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            if (controller == null)
            {
                continue;
            }

            bool hasChanges = false;

            // 遍历所有Layer
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = controller.layers[layerIndex];
                AnimatorStateMachine stateMachine = layer.stateMachine;

                // 处理状态机的转换
                ProcessStateMachineTransitions(stateMachine, ref hasChanges, ref transitionCount);
            }

            if (hasChanges)
            {
                EditorUtility.SetDirty(controller);
                modifiedCount++;
                Debug.Log($"Modified: {path}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"========================================");
        Debug.Log($"Modification Complete!");
        Debug.Log($"Animator Controllers modified: {modifiedCount}");
        Debug.Log($"Total transitions processed: {transitionCount}");
        Debug.Log($"========================================");
    }

    private static void ProcessStateMachineTransitions(AnimatorStateMachine stateMachine, ref bool hasChanges, ref int transitionCount)
    {
        // 处理状态机中状态的转换
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            AnimatorState state = childState.state;

            // 处理状态的转换
            if (state.transitions != null && state.transitions.Length > 0)
            {
                foreach (AnimatorStateTransition transition in state.transitions)
                {
                    if (ProcessTransition(transition))
                    {
                        hasChanges = true;
                        transitionCount++;
                    }
                }
            }
        }

        // 处理状态机自身的转换（AnyState转换和Entry转换）
        if (stateMachine.anyStateTransitions != null && stateMachine.anyStateTransitions.Length > 0)
        {
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                if (ProcessTransition(transition))
                {
                    hasChanges = true;
                    transitionCount++;
                }
            }
        }

        // Entry Transitions 通常不使用这些属性，跳过

        // 递归处理子状态机
        foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
        {
            ProcessStateMachineTransitions(childStateMachine.stateMachine, ref hasChanges, ref transitionCount);
        }
    }

    private static bool ProcessTransition(AnimatorStateTransition transition)
    {
        bool changed = false;

        // 设置Fixed Duration为false（未选中）
        if (transition.hasFixedDuration != false)
        {
            transition.hasFixedDuration = false;
            changed = true;
        }

        // 设置Transition Duration为0
        if (transition.duration != 0f)
        {
            transition.duration = 0f;
            changed = true;
        }

        return changed;
    }
}

