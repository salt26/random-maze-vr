using UnityEngine;

public class RandomDecorSpawner : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("Probability that decorations will appear on this wall (0 = never, 1 = always)")]
    public float decorationSpawnProbability = 0.1f;

    [Tooltip("Name patterns to search for (e.g., 'Torch', 'Rock'). Leave empty to disable all children.")]
    public string[] decorationNamePatterns = new string[] { "Torch", "Rock" };

    private void Awake()
    {
        // Check if we should show decorations based on probability
        bool shouldShowDecorations = Random.Range(0f, 1f) < decorationSpawnProbability;

        if (!shouldShowDecorations)
        {
            // Find and disable decoration objects
            foreach (Transform child in transform)
            {
                if (ShouldDisable(child.name))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private bool ShouldDisable(string objectName)
    {
        // If no patterns specified, don't disable anything
        if (decorationNamePatterns == null || decorationNamePatterns.Length == 0)
            return false;

        // Check if object name contains any of the patterns
        foreach (string pattern in decorationNamePatterns)
        {
            if (!string.IsNullOrEmpty(pattern) &&
                objectName.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
