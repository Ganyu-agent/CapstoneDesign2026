using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CapstoneDesign.EditorTools
{
    public static class BuildTools
    {
        private const string ScenePath = "Assets/Scenes/MockupMain.unity";
        private const string PackageName = "com.capstonedesign2026.mockup";

        [MenuItem("Capstone Mockup/Validate Project")]
        public static void ValidateProject()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ScenePath)))
            {
                throw new InvalidOperationException("Mockup scene is missing. Run Capstone Mockup/Generate All first.");
            }

            Scene scene = EditorSceneManagerProxy.OpenScene(ScenePath);
            GameObject camera = GameObject.Find("MockupCamera");
            GameObject island = GameObject.Find("FloatingIsland");
            GameObject canvas = GameObject.Find("UiCanvas");
            if (camera == null || island == null || canvas == null)
            {
                throw new InvalidOperationException("Generated scene is missing camera, island, or UI canvas.");
            }

            if (GameObject.Find("MoonLight") == null)
            {
                throw new InvalidOperationException("Generated scene is missing the single moon light.");
            }

            int lightCount = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length;
            if (lightCount != 1)
            {
                throw new InvalidOperationException("Mockup expects exactly one Light, found " + lightCount + ".");
            }

            Debug.Log("Mockup validation passed: " + scene.path + " / package " + PackageName);
        }

        [MenuItem("Capstone Mockup/Build Android")]
        public static void BuildAndroid()
        {
            ValidateProject();
            string artifacts = Environment.GetEnvironmentVariable("CAPSTONE_ARTIFACTS");
            if (string.IsNullOrWhiteSpace(artifacts))
            {
                artifacts = "/artifacts";
            }

            string buildDirectory = Path.Combine(artifacts, "build");
            Directory.CreateDirectory(buildDirectory);
            string outputPath = Path.Combine(buildDirectory, "capstone-mockup.apk");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Android build failed: " + report.summary.result + "\n" + report.summary.totalErrors + " errors");
            }

            Debug.Log("Android APK written to " + outputPath + " (" + report.summary.totalSize + " bytes)");
        }

        [MenuItem("Capstone Mockup/Run Validation")]
        public static void RunValidation()
        {
            ValidateProject();
        }

        private static class EditorSceneManagerProxy
        {
            public static Scene OpenScene(string path)
            {
                return UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            }
        }
    }
}
