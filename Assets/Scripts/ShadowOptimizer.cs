using UnityEngine;
using System.Collections.Generic;

public class ShadowOptimizer : MonoBehaviour
{
    [Header("Shadow Optimization Settings")]
    public bool optimizeOnStart = true;
    public float shadowCastDistance = 20f;
    public float updateInterval = 0.25f;
    
    [Header("Shadow Quality Settings")]
    public bool disableShadowsOnSmallObjects = true;
    public float minObjectSizeForShadows = 2f;
    public bool keepPlayerShadows = true;
    public bool keepImportantObjectShadows = true;
    
    [Header("Performance Settings")]
    public int maxShadowCasters = 15;
    public bool prioritizeNearObjects = true;
    
    private Camera playerCamera;
    private List<ShadowCasterInfo> shadowCasters = new List<ShadowCasterInfo>();
    private float lastUpdateTime;
    
    [System.Serializable]
    private class ShadowCasterInfo
    {
        public Renderer renderer;
        public UnityEngine.Rendering.ShadowCastingMode originalShadowMode;
        public bool isImportant;
        public float size;
        public float lastDistance;
    }
    
    private void Start()
    {
        playerCamera = Camera.main ?? FindObjectOfType<Camera>();
        
        if (optimizeOnStart)
        {
            OptimizeGlobalShadowSettings();
        }
        
        StartCoroutine(InitializeShadowCasters());
    }
    
    private void OptimizeGlobalShadowSettings()
    {
        // Optimize shadow quality settings
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowDistance = shadowCastDistance + 5f;
        QualitySettings.shadowCascades = 2;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        
        Debug.Log("ShadowOptimizer: Applied global shadow optimizations");
    }
    
    private System.Collections.IEnumerator InitializeShadowCasters()
    {
        yield return null; // Wait for maze generation
        
        CollectShadowCasters();
        OptimizeShadowCasters();
        
        Debug.Log($"ShadowOptimizer: Managing {shadowCasters.Count} shadow casters");
    }
    
    private void CollectShadowCasters()
    {
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null || renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off)
                continue;
            
            ShadowCasterInfo info = new ShadowCasterInfo
            {
                renderer = renderer,
                originalShadowMode = renderer.shadowCastingMode,
                isImportant = IsImportantObject(renderer.gameObject),
                size = GetObjectSize(renderer),
                lastDistance = 0f
            };
            
            shadowCasters.Add(info);
        }
    }
    
    private bool IsImportantObject(GameObject obj)
    {
        string name = obj.name.ToLower();
        
        // Keep shadows for important objects
        if (keepPlayerShadows && (name.Contains("player") || name.Contains("swat")))
            return true;
            
        if (keepImportantObjectShadows && (name.Contains("character") || name.Contains("npc")))
            return true;
            
        return false;
    }
    
    private float GetObjectSize(Renderer renderer)
    {
        return renderer.bounds.size.magnitude;
    }
    
    private void OptimizeShadowCasters()
    {
        foreach (ShadowCasterInfo info in shadowCasters)
        {
            if (info.renderer == null) continue;
            
            // Disable shadows on small objects immediately
            if (disableShadowsOnSmallObjects && info.size < minObjectSizeForShadows && !info.isImportant)
            {
                info.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                continue;
            }
            
            // Disable shadows on maze walls by default (they create too many shadows)
            if (IsMazeWall(info.renderer.gameObject))
            {
                info.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
    
    private bool IsMazeWall(GameObject obj)
    {
        string name = obj.name.ToLower();
        return name.Contains("wall") || 
               name.Contains("concrete") ||
               name.Contains("edge") ||
               name.Contains("corner");
    }
    
    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateShadowCasters();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateShadowCasters()
    {
        if (playerCamera == null) return;
        
        Vector3 cameraPos = playerCamera.transform.position;
        List<ShadowCasterInfo> nearCasters = new List<ShadowCasterInfo>();
        
        // First pass: disable all distant shadows and collect near objects
        foreach (ShadowCasterInfo info in shadowCasters)
        {
            if (info.renderer == null) continue;
            
            float distance = Vector3.Distance(cameraPos, info.renderer.transform.position);
            info.lastDistance = distance;
            
            if (distance > shadowCastDistance)
            {
                // Disable shadows for distant objects
                if (info.renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
                {
                    info.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            else if (!info.isImportant && IsMazeWall(info.renderer.gameObject))
            {
                // Keep maze walls with shadows disabled for performance
                info.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                nearCasters.Add(info);
            }
        }
        
        // Second pass: limit the number of active shadow casters
        if (prioritizeNearObjects)
        {
            nearCasters.Sort((a, b) => a.lastDistance.CompareTo(b.lastDistance));
        }
        
        int activeCasters = 0;
        foreach (ShadowCasterInfo info in nearCasters)
        {
            if (info.isImportant || activeCasters < maxShadowCasters)
            {
                if (info.renderer.shadowCastingMode != info.originalShadowMode)
                {
                    info.renderer.shadowCastingMode = info.originalShadowMode;
                }
                if (!info.isImportant) activeCasters++;
            }
            else
            {
                if (info.renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
                {
                    info.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }
    }
    
    public void SetMaxShadowCasters(int count)
    {
        maxShadowCasters = Mathf.Max(0, count);
    }
    
    public void SetShadowDistance(float distance)
    {
        shadowCastDistance = Mathf.Max(5f, distance);
        QualitySettings.shadowDistance = shadowCastDistance + 5f;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        
        Vector3 cameraPos = playerCamera.transform.position;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(cameraPos, shadowCastDistance);
        
        // Draw active shadow casters
        Gizmos.color = Color.red;
        foreach (ShadowCasterInfo info in shadowCasters)
        {
            if (info.renderer != null && info.renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
            {
                Gizmos.DrawWireCube(info.renderer.bounds.center, info.renderer.bounds.size * 0.1f);
            }
        }
    }
    
    private void OnDestroy()
    {
        // Restore original shadow settings
        foreach (ShadowCasterInfo info in shadowCasters)
        {
            if (info.renderer != null)
            {
                info.renderer.shadowCastingMode = info.originalShadowMode;
            }
        }
    }
}