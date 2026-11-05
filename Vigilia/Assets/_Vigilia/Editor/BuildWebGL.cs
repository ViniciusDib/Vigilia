using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vigilia.EditorTools
{
    /// <summary>
    /// Build WebGL para o itch.io (GDD 13.4). Menu: Vigilia > Build WebGL. Saída: Builds/WebGL (fora de Assets).
    /// Para publicar: zipe o CONTEÚDO de Builds/WebGL (index.html na raiz do zip) e suba no itch.io como HTML.
    /// Linha de comando: Unity.exe -batchmode -quit -projectPath . -executeMethod Vigilia.EditorTools.BuildWebGL.Construir
    /// </summary>
    public static class BuildWebGL
    {
        [MenuItem("Vigilia/Build WebGL")]
        public static void Construir()
        {
            var cenas = new[] { "Assets/_Vigilia/Cenas/Menu.unity", "Assets/_Vigilia/Cenas/Jogo.unity" };
            var opcoes = new BuildPlayerOptions
            {
                scenes = cenas,
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            var rel = BuildPipeline.BuildPlayer(opcoes);
            Debug.Log($"[Vigilia] Build WebGL: {rel.summary.result} · {rel.summary.totalSize / (1024 * 1024)} MB · {rel.summary.totalTime}");
            if (Application.isBatchMode && rel.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
