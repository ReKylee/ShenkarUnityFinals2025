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
        // Find all scenes in the Scenes folder that match the pattern "Level{number}.unity"
        var sceneFolder = "Assets/Scenes";
        var sceneFiles = System.IO.Directory.GetFiles(sceneFolder, "Level*.unity");
        int maxLevel = 0;
        foreach (var file in sceneFiles)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            if (fileName.StartsWith("Level"))
            {
                if (int.TryParse(fileName.Substring(5), out int num))
                {
                    if (num > maxLevel) maxLevel = num;
                }
            }
        }
        // Set the new scene name to Level{maxLevel+1}
        string newLevelName = $"Level{maxLevel + 1}";
        scene.name = newLevelName;
        SceneManager.SetActiveScene(scene);
        Debug.Log($"[LevelsTemplatePipeline] Instantiated new level: {newLevelName}");
    }
}
