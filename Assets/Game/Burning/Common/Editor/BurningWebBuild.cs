using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FilmInspiredGames.Burning.Editor
{
    public static class BurningWebBuild
    {
        private const string ScenePath = "Assets/Game/Burning/Scenes/Burning_Act1_Playable.unity";
        private const string NextScenePath = "Assets/Game/Burning/C08/Scenes/Burning_C08_C12_Playable.unity";
        private const string OutputPath = "Builds/Web";

        [MenuItem("Tools/Burning/Build Web for GitHub Pages")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("재생을 멈춘 뒤 웹 빌드를 생성하세요.");
                return;
            }

            Directory.CreateDirectory(OutputPath);

            BuildPlayerOptions options = new()
            {
                scenes = new[] { ScenePath, NextScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"웹 빌드 실패: {report.summary.result}");
            }

            File.WriteAllText(Path.Combine(OutputPath, ".nojekyll"), string.Empty);
            Debug.Log($"웹 빌드 완료: {OutputPath} ({report.summary.totalSize / 1024f / 1024f:F1} MB)");
        }
    }
}
