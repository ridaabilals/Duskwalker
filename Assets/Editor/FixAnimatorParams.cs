using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class FixAnimatorParams
{
    [MenuItem("Tools/Fix Player Animator Parameters")]
    public static void Fix()
    {
        var selected = Selection.activeObject as AnimatorController;

        if (selected == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var animator = player.GetComponentInChildren<Animator>();
                if (animator != null)
                    selected = animator.runtimeAnimatorController as AnimatorController;
            }
        }

        if (selected == null)
        {
            EditorUtility.DisplayDialog(
                "Fix Player Animator Parameters",
                "No AnimatorController is selected and no Player object with an AnimatorController was found in the scene. Select the Animator Controller asset first or make sure the Player is tagged as 'Player'.",
                "OK");
            return;
        }

        //bool changed = false;

        AddParameterIfMissing(selected, "Speed", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(selected, "Grounded", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(selected, "YVelocity", AnimatorControllerParameterType.Float);
        AddParameterIfMissing(selected, "Jump", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(selected, "DoubleJump", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(selected, "Attack", AnimatorControllerParameterType.Bool);
        AddParameterIfMissing(selected, "Melee", AnimatorControllerParameterType.Trigger);
        AddParameterIfMissing(selected, "WallGrab", AnimatorControllerParameterType.Bool);

        EditorUtility.DisplayDialog(
            "Fix Player Animator Parameters",
            "Checked and ensured the required Animator parameters exist in: " + selected.name,
            "OK");
    }

    private static void AddParameterIfMissing(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (controller == null || string.IsNullOrEmpty(name))
            return;

        var existing = controller.parameters.FirstOrDefault(p => p.name == name && p.type == type);
        if (existing != null)
            return;

        var duplicateName = controller.parameters.FirstOrDefault(p => p.name == name);
        if (duplicateName != null)
        {
            Debug.LogWarning($"Animator parameter '{name}' already exists with a different type. Leaving it unchanged.");
            return;
        }

        controller.AddParameter(name, type);
        Debug.Log($"Added missing Animator parameter: {name} ({type}) to {controller.name}");
    }
}
