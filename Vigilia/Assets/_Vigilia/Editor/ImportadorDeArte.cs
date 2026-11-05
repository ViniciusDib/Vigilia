using UnityEditor;
using UnityEngine;

namespace Vigilia.EditorTools
{
    /// <summary>
    /// Configura sozinho todo PNG que entra em Assets/_Vigilia/Resources/Arte:
    /// sprite, filtro Point (pixel nítido), sem compressão, e 1 unidade do mundo = largura da imagem.
    /// Assim a arte final entra no tamanho certo sem ninguém mexer no Inspector.
    /// </summary>
    public class ImportadorDeArte : AssetPostprocessor
    {
        public const string Pasta = "Assets/_Vigilia/Resources/Arte/";

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(Pasta)) return;
            var imp = (TextureImporter)assetImporter;
            imp.GetSourceTextureWidthAndHeight(out int w, out int h);
            string nome = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            bool ladrilho = nome == "piso" || nome == "parede" || nome.StartsWith("piso_");

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            // Tamanho no mundo: ladrilho cobre 4×4 unidades; personagem tem 0,9 unidade de largura (ombros);
            // o resto ocupa 1 unidade (o construtor ainda ajusta móveis e itens ao espaço deles).
            bool personagem = nome.StartsWith("jogador_") || nome.StartsWith("npc_");
            bool topo = nome.StartsWith("topo_") || nome == "olhos_topo";
            float ppu = ladrilho ? w / 4f : topo ? w / 1.7f : personagem ? w / 0.9f : w;
            imp.spritePixelsPerUnit = Mathf.Max(1f, ppu);
            imp.spritePivot = new Vector2(0.5f, 0.5f);
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            var ts = new TextureImporterSettings();
            imp.ReadTextureSettings(ts);
            ts.spriteMeshType = ladrilho ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            ts.wrapMode = ladrilho ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            imp.SetTextureSettings(ts);
        }
    }
}
