using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Efficient level discovery service that caches results to disk via GameDataService
    ///     Follows Single Responsibility Principle by focusing only on level discovery and caching
    /// </summary>
    public class LevelDiscoveryService : ILevelDiscoveryService
    {


        public async Task<List<LevelData>> DiscoverLevelsFromSceneAsync()
        {
            // This can be an expensive operation, so keep it async
            await Task.Yield();

            var levelPoints = Object.FindObjectsByType<LevelPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            // Sort by level name naturally (Level_01, Level_02, etc.)
            // Then by hierarchy order as secondary sort
            var sortedLevelPoints = levelPoints
                .OrderBy(lp => ExtractLevelNumber(lp.LevelName))
                .ThenBy(lp => lp.transform.GetSiblingIndex())
                .ToList();

            // Assign automatic indexes
            for (int i = 0; i < sortedLevelPoints.Count; i++)
            {
                sortedLevelPoints[i].SetCalculatedIndex(i);
            }

            // Convert to LevelData
            return sortedLevelPoints
                .Select(lp => lp.ToLevelData())
                .ToList();
        }

        /// <summary>
        ///     Extract the numeric part from level names like "Level_01" -> 1, "Level_02" -> 2
        ///     Returns a high number for non-standard names so they appear last
        /// </summary>
        private int ExtractLevelNumber(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
                return 9999;

            // Try to extract number from patterns like "Level_01", "Level1", "Lv01", etc.
            string[] parts = levelName.Split('_');

            // Check last part first (for "Level_01" format)
            if (parts.Length > 1 && int.TryParse(parts[^1], out int levelNum))
            {
                return levelNum;
            }

            // Check if there's a number at the end (for "Level1" format)
            string numberPart = "";
            for (int i = levelName.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(levelName[i]))
                {
                    numberPart = levelName[i] + numberPart;
                }
                else
                {
                    break;
                }
            }

            if (!string.IsNullOrEmpty(numberPart) && int.TryParse(numberPart, out int extractedNum))
            {
                return extractedNum;
            }

            // If no number found, return a high value so it appears last
            return 9999;
        }
    }
}
