using UnityEngine;
using UnityEditor;
using System.IO;
using Debug = UnityEngine.Debug;

namespace WhenImCleaningWindows.Editor
{
    /// <summary>
    /// Prefab Creator - Utility to generate test prefabs for window cleaning gameplay.
    /// Use this to quickly create prefabs for testing without manual setup.
    /// </summary>
    public class PrefabCreator
    {
        private const string PREFABS_PATH = "Assets/Prefabs";
        private const string MATERIALS_PATH = "Assets/Materials";
        
        [MenuItem("Tools/When I'm Cleaning Windows/Create Test Prefabs")]
        public static void CreateAllTestPrefabs()
        {
            // Ensure directories exist
            if (!Directory.Exists(PREFABS_PATH))
            {
                Directory.CreateDirectory(PREFABS_PATH);
                AssetDatabase.Refresh();
            }
            
            if (!Directory.Exists(MATERIALS_PATH))
            {
                Directory.CreateDirectory(MATERIALS_PATH);
                AssetDatabase.Refresh();
            }
            
            UnityEngine.Debug.Log("[PrefabCreator] Creating test prefabs...");
            
            CreateWindowQuadPrefab();
            CreateHazardQuadPrefab();
            CreateCleaningParticlePrefab();
            CreateWindowMaterials();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            UnityEngine.Debug.Log("[PrefabCreator] ✓ All test prefabs created successfully!");
            
            EditorUtility.DisplayDialog(
                "Prefabs Created!",
                "Test prefabs have been created in Assets/Prefabs/\n\n" +
                "Created:\n" +
                "- WindowQuad.prefab\n" +
                "- HazardQuad.prefab\n" +
                "- CleaningParticle.prefab\n" +
                "- Materials (Clean, Dirty, Hazard)",
                "OK"
            );
        }
        
        private static void CreateWindowQuadPrefab()
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "WindowQuad";
            quad.transform.localScale = new Vector3(10f, 8f, 1f);
            
            // Add mesh filter and renderer
            MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            
            // Create or assign material
            Material windowMat = CreateWindowMaterial();
            renderer.material = windowMat;
            
            // Remove collider (not needed for cleaning)
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            
            // Save as prefab
            string prefabPath = $"{PREFABS_PATH}/WindowQuad.prefab";
            PrefabUtility.SaveAsPrefabAsset(quad, prefabPath);
            
            Object.DestroyImmediate(quad);
            
            UnityEngine.Debug.Log($"[PrefabCreator] Created WindowQuad prefab at {prefabPath}");
        }
        
        private static void CreateHazardQuadPrefab()
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "HazardQuad";
            quad.transform.localScale = Vector3.one;
            
            // Add mesh renderer
            MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            
            // Create or assign material
            Material hazardMat = CreateHazardMaterial();
            renderer.material = hazardMat;
            
            // Remove collider
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            
            // Save as prefab
            string prefabPath = $"{PREFABS_PATH}/HazardQuad.prefab";
            PrefabUtility.SaveAsPrefabAsset(quad, prefabPath);
            
            Object.DestroyImmediate(quad);
            
            UnityEngine.Debug.Log($"[PrefabCreator] Created HazardQuad prefab at {prefabPath}");
        }
        
        private static void CreateCleaningParticlePrefab()
        {
            GameObject particleObj = new GameObject("CleaningParticle");
            
            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            
            // Main module
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = new Color(0.8f, 0.9f, 1f, 0.8f);
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Emission
            var emission = ps.emission;
            emission.rateOverTime = 50f;
            
            // Shape
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.2f;
            
            // Color over lifetime
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.cyan, 0.5f),
                    new GradientColorKey(Color.blue, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            
            // Size over lifetime
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateParticleMaterial();
            
            // Save as prefab
            string prefabPath = $"{PREFABS_PATH}/CleaningParticle.prefab";
            PrefabUtility.SaveAsPrefabAsset(particleObj, prefabPath);
            
            Object.DestroyImmediate(particleObj);
            
            UnityEngine.Debug.Log($"[PrefabCreator] Created CleaningParticle prefab at {prefabPath}");
        }
        
        private static Material CreateWindowMaterial()
        {
            string materialPath = $"{MATERIALS_PATH}/WindowClean.mat";
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat != null) return mat;
            
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "WindowClean";
            
            // Glass-like properties
            mat.SetColor("_BaseColor", new Color(0.9f, 0.95f, 1f, 0.3f));
            mat.SetFloat("_Smoothness", 0.9f);
            mat.SetFloat("_Metallic", 0.1f);
            
            // Enable transparency
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha
            mat.renderQueue = 3000;
            
            AssetDatabase.CreateAsset(mat, materialPath);
            UnityEngine.Debug.Log($"[PrefabCreator] Created WindowClean material at {materialPath}");
            
            return mat;
        }
        
        private static Material CreateHazardMaterial()
        {
            string materialPath = $"{MATERIALS_PATH}/Hazard.mat";
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat != null) return mat;
            
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "Hazard";
            
            // Dirty/stained appearance
            mat.SetColor("_BaseColor", new Color(0.4f, 0.35f, 0.3f, 0.8f));
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Metallic", 0f);
            
            // Enable transparency
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha
            mat.renderQueue = 3001;
            
            AssetDatabase.CreateAsset(mat, materialPath);
            UnityEngine.Debug.Log($"[PrefabCreator] Created Hazard material at {materialPath}");
            
            return mat;
        }
        
        private static Material CreateParticleMaterial()
        {
            string materialPath = $"{MATERIALS_PATH}/Particle.mat";
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat != null) return mat;
            
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.name = "Particle";
            
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Surface", 1); // Transparent
            mat.renderQueue = 3000;
            
            AssetDatabase.CreateAsset(mat, materialPath);
            UnityEngine.Debug.Log($"[PrefabCreator] Created Particle material at {materialPath}");
            
            return mat;
        }
        
        private static void CreateWindowMaterials()
        {
            // Create additional material variants
            CreateDirtyWindowMaterial();
            CreateFrameMaterial();
        }
        
        private static void CreateDirtyWindowMaterial()
        {
            string materialPath = $"{MATERIALS_PATH}/WindowDirty.mat";
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat != null) return;
            
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "WindowDirty";
            
            // Opaque dirty appearance
            mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 1f));
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Metallic", 0f);
            
            AssetDatabase.CreateAsset(mat, materialPath);
            UnityEngine.Debug.Log($"[PrefabCreator] Created WindowDirty material at {materialPath}");
        }
        
        private static void CreateFrameMaterial()
        {
            string materialPath = $"{MATERIALS_PATH}/WindowFrame.mat";
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat != null) return;
            
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "WindowFrame";
            
            // Wood frame appearance
            mat.SetColor("_BaseColor", new Color(0.4f, 0.3f, 0.2f, 1f));
            mat.SetFloat("_Smoothness", 0.4f);
            mat.SetFloat("_Metallic", 0f);
            
            AssetDatabase.CreateAsset(mat, materialPath);
            UnityEngine.Debug.Log($"[PrefabCreator] Created WindowFrame material at {materialPath}");
        }
        
        [MenuItem("Tools/When I'm Cleaning Windows/Clean Up Prefabs")]
        public static void CleanUpPrefabs()
        {
            if (EditorUtility.DisplayDialog(
                "Clean Up Prefabs?",
                "This will delete all test prefabs and materials in Assets/Prefabs/ and Assets/Materials/.\n\n" +
                "This action cannot be undone!",
                "Delete",
                "Cancel"))
            {
                if (Directory.Exists(PREFABS_PATH))
                {
                    Directory.Delete(PREFABS_PATH, true);
                    File.Delete(PREFABS_PATH + ".meta");
                }
                
                if (Directory.Exists(MATERIALS_PATH))
                {
                    Directory.Delete(MATERIALS_PATH, true);
                    File.Delete(MATERIALS_PATH + ".meta");
                }
                
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("[PrefabCreator] Cleaned up all test prefabs and materials");
            }
        }
    }
}








