using System.Linq;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsTemplatePipeline : ISceneTemplatePipeline
{
    public virtual bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        return true;
    }

    public virtual void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
    {
        
    }

    public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {
        var sceneFolder = System.IO.Path.Combine("Assets", "Scenes");
        var sceneFiles = System.IO.Directory.GetFiles(sceneFolder, "Level*.unity");
        int levelPrefixLength = "Level".Length;
        int maxLevel = sceneFiles
            .Select(System.IO.Path.GetFileNameWithoutExtension)
            .Where(fileName => fileName.StartsWith("Level"))
            .Select(fileName => int.TryParse(fileName[levelPrefixLength..], out int num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();
        string newLevelName = $"Level{maxLevel + 1}";
        scene.name = newLevelName;
        SceneManager.SetActiveScene(scene);
        Debug.Log($"[LevelsTemplatePipeline] Instantiated new level: {newLevelName}");
    }
}
