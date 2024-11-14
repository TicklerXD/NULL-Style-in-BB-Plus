using BaldiLevelEditor;
using MTM101BaldAPI;
using NULL.Manager;
using UnityEngine;

namespace DevTools;

public class ConditionalPatchNULL : ConditionalPatch
{
    public override bool ShouldPatch()
    {
        bool flag = true;
        if (NULL.Manager.CompatibilityModule.Plugins.IsEditor) SceneIsLevelEditor();
        void SceneIsLevelEditor()
        {
            flag = Object.FindObjectOfType<PlusLevelEditor>() is null;
        }
        return ModManager.NullStyle && flag;
    }
}
