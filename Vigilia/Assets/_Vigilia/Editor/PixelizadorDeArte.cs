using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vigilia.EditorTools
{
    /// <summary>
    /// Transforma as imagens geradas por IA em arte do jogo, com a mesma cara do jumpscare:
    ///   1. lê os PNG/JPG de Vigilia/ArteBruta/ (fora de Assets, para o Unity não importar duas vezes);
    ///   2. troca o fundo magenta (#FF00FF) por transparente;
    ///   3. reduz para o tamanho do jogo (média por área, sem borrar);
    ///   4. (opcional) encaixa cada cor na paleta do jogo, como o jumpscare faz;
    ///   5. salva em Assets/_Vigilia/Resources/Arte/ com o mesmo nome.
    /// Depois rode "Vigilia > Construir tudo" para os sprites novos entrarem na cena.
    /// Tamanhos e nomes: docs/prompts-arte.md.
    /// </summary>
    public static class PixelizadorDeArte
    {
        static string PastaBruta => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ArteBruta"));
        const string PastaSaida = "Assets/_Vigilia/Resources/Arte";

        [MenuItem("Vigilia/Arte/Processar ArteBruta (paleta do jogo)")]
        public static void ProcessarComPaleta() => Processar(true);

        [MenuItem("Vigilia/Arte/Processar ArteBruta (cores livres)")]
        public static void ProcessarSemPaleta() => Processar(false);

        /// <summary>Um clique: processa a ArteBruta e reconstrói as cenas. Também usado na linha de comando.</summary>
        [MenuItem("Vigilia/Arte/Processar e construir tudo")]
        public static void ProcessarEConstruir()
        {
            Processar(true);
            ConstrutorDeCenas.ConstruirTudo();
        }

        [MenuItem("Vigilia/Arte/Abrir pasta ArteBruta")]
        public static void AbrirPasta()
        {
            Directory.CreateDirectory(PastaBruta);
            EditorUtility.RevealInFinder(PastaBruta);
        }

        /// <summary>Sprites recortados até a borda do desenho e reduzidos pelo MAIOR lado (mantém a proporção).</summary>
        public static bool EhRecortado(string nome)
            => !(nome.StartsWith("retrato_") || nome == "menu_fundo" || nome.StartsWith("intro_") || nome.StartsWith("cena_")
                 || nome.StartsWith("piso") || nome == "parede" || nome.StartsWith("topo_") || nome == "olhos_topo");

        /// <summary>Tamanho final (largura, altura) pelo prefixo do nome. Para recortados, x = maior lado.</summary>
        public static Vector2Int Tamanho(string nome)
        {
            if (nome.StartsWith("topo_") || nome == "olhos_topo") return new Vector2Int(96, 96);   // já vem em pixel art
            if (nome.StartsWith("jogador_") || nome.StartsWith("npc_")) return new Vector2Int(96, 96);
            if (nome.StartsWith("retrato_")) return new Vector2Int(160, 160);
            if (nome.StartsWith("item_")) return new Vector2Int(48, 48);
            if (nome == "piso" || nome == "parede" || nome.StartsWith("piso_")) return new Vector2Int(128, 128); // cobre 4×4 unidades
            if (nome == "menu_fundo" || nome.StartsWith("intro_") || nome.StartsWith("cena_")) return new Vector2Int(640, 360);
            if (nome == "logo") return new Vector2Int(480, 0);
            return new Vector2Int(128, 0);                       // móveis e o resto
        }

        public static void Processar(bool paleta)
        {
            Directory.CreateDirectory(PastaBruta);
            Directory.CreateDirectory(PastaSaida);
            var arquivos = new List<string>();
            arquivos.AddRange(Directory.GetFiles(PastaBruta, "*.png"));
            arquivos.AddRange(Directory.GetFiles(PastaBruta, "*.jpg"));
            arquivos.AddRange(Directory.GetFiles(PastaBruta, "*.jpeg"));
            if (arquivos.Count == 0)
            {
                Debug.LogWarning($"[Vigilia] Nenhuma imagem em {PastaBruta}. Coloque lá os PNGs da IA com os nomes de docs/prompts-arte.md.");
                return;
            }
            int n = 0;
            foreach (var caminho in arquivos)
            {
                string nome = Path.GetFileNameWithoutExtension(caminho).ToLowerInvariant();
                var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!src.LoadImage(File.ReadAllBytes(caminho))) { Debug.LogWarning($"[Vigilia] não li {caminho}"); continue; }
                var t = Tamanho(nome);
                bool fundoTransparente = !(nome == "menu_fundo" || nome.StartsWith("intro_") || nome.StartsWith("cena_") || nome.StartsWith("piso") || nome == "parede");
                var saida = Converter(src, t.x, t.y, fundoTransparente, paleta && UsaPaleta(nome), EhRecortado(nome));
                File.WriteAllBytes($"{PastaSaida}/{nome}.png", saida.EncodeToPNG());
                Object.DestroyImmediate(src);
                Object.DestroyImmediate(saida);
                n++;
            }
            AssetDatabase.Refresh();
            Debug.Log($"[Vigilia] {n} imagens processadas para {PastaSaida}. Agora rode Vigilia > Construir tudo.");
        }

        /// <summary>Retratos e telas ficam melhores com cores livres (a paleta achata os rostos pintados).</summary>
        public static bool UsaPaleta(string nome)
            => nome.StartsWith("npc_") || nome.StartsWith("jogador_") || nome == "logo";   // o resto já vem desenhado no estilo do jogo

        public static Texture2D Converter(Texture2D src, int largura, int altura, bool chroma, bool paleta, bool recortar = false)
        {
            var px = src.GetPixels32();
            int sw = src.width, sh = src.height;

            // 1) chroma key: magenta vira transparente; bordas meio-magenta perdem o tom rosa (despill)
            if (chroma)
                for (int i = 0; i < px.Length; i++)
                {
                    var c = px[i];
                    int m = Mathf.Min(c.r, c.b) - c.g;           // quanto "magenta" o pixel tem
                    if (m > 90 && Mathf.Abs(c.r - c.b) < 80) { px[i].a = 0; continue; }
                    if (m > 25)
                    {
                        int tira = m - 25;
                        px[i].r = (byte)Mathf.Max(0, c.r - tira);
                        px[i].b = (byte)Mathf.Max(0, c.b - tira);
                        if (m > 60) px[i].a = (byte)(c.a / 2);
                    }
                }

            // 2) recorte até a borda do desenho (sprites): sem margem transparente sobrando
            if (recortar)
            {
                int minX = sw, minY = sh, maxX = -1, maxY = -1;
                for (int y = 0; y < sh; y++)
                    for (int x = 0; x < sw; x++)
                        if (px[y * sw + x].a > 128) { if (x < minX) minX = x; if (x > maxX) maxX = x; if (y < minY) minY = y; if (y > maxY) maxY = y; }
                if (maxX >= minX)
                {
                    int nw = maxX - minX + 1, nh = maxY - minY + 1;
                    var cort = new Color32[nw * nh];
                    for (int y = 0; y < nh; y++)
                        System.Array.Copy(px, (y + minY) * sw + minX, cort, y * nw, nw);
                    px = cort; sw = nw; sh = nh;
                }
                // reduz pelo maior lado, mantendo a proporção
                float k = (float)largura / Mathf.Max(sw, sh);
                if (k > 1f) k = 1f;
                largura = Mathf.Max(1, Mathf.RoundToInt(sw * k));
                altura = Mathf.Max(1, Mathf.RoundToInt(sh * k));
            }
            if (altura <= 0) altura = Mathf.Max(1, Mathf.RoundToInt(largura * (float)sh / sw));

            // 2) redução por média de área, ponderada pelo alfa (sem halo magenta)
            var o = new Texture2D(largura, altura, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var outPx = new Color32[largura * altura];
            float fx = (float)sw / largura, fy = (float)sh / altura;
            for (int y = 0; y < altura; y++)
                for (int x = 0; x < largura; x++)
                {
                    int x0 = (int)(x * fx), x1 = Mathf.Max(x0 + 1, (int)((x + 1) * fx));
                    int y0 = (int)(y * fy), y1 = Mathf.Max(y0 + 1, (int)((y + 1) * fy));
                    float r = 0, g = 0, b = 0, a = 0; int cont = 0;
                    for (int yy = y0; yy < y1 && yy < sh; yy++)
                        for (int xx = x0; xx < x1 && xx < sw; xx++)
                        {
                            var c = px[yy * sw + xx];
                            float w = c.a / 255f;
                            r += c.r * w; g += c.g * w; b += c.b * w; a += w; cont++;
                        }
                    float cobertura = cont > 0 ? a / cont : 0;
                    if (cobertura < 0.5f || a <= 0) { outPx[y * largura + x] = new Color32(0, 0, 0, 0); continue; }
                    var cor = new Color32((byte)(r / a), (byte)(g / a), (byte)(b / a), 255);
                    outPx[y * largura + x] = paleta ? MaisProxima(cor) : cor;
                }
            o.SetPixels32(outPx);
            o.Apply();
            return o;
        }

        // Paleta do jumpscare (30 cores do handoff) + tons frios e de pele das referências noturnas.
        static readonly string[] Hex =
        {
            "050304", "0c0607", "150909", "1f0c0c", "2a0a0b", "3d0d0e", "521012", "6b1214", "851417", "a3191b", "c02a23", "d9473a",
            "e0705f", "f09a86", "2b1a10", "3f2616", "573520", "70472a", "8c5c38", "a87748", "c9b27a", "e3d29a", "f4ead0",
            "262626", "3a3836", "55524d", "7a756b", "2f3328", "4a5040", "fff6e0",
            // frios (Refs/Ref_ambiente_noite.jpg, Mais_ref_noite.jpg)
            "0b1418", "142228", "1f3540", "2e4c5a", "46697a", "6b8fa0", "9db8c4", "d6e4ea",
            // pele
            "7a5444", "a87a62", "d2a68a", "e8c9b0",
        };
        static Color32[] pal;

        static Color32 MaisProxima(Color32 c)
        {
            if (pal == null)
            {
                pal = new Color32[Hex.Length];
                for (int i = 0; i < Hex.Length; i++)
                    pal[i] = new Color32(System.Convert.ToByte(Hex[i].Substring(0, 2), 16), System.Convert.ToByte(Hex[i].Substring(2, 2), 16), System.Convert.ToByte(Hex[i].Substring(4, 2), 16), 255);
            }
            int melhor = 0; float md = float.MaxValue;
            for (int i = 0; i < pal.Length; i++)
            {
                float dr = pal[i].r - c.r, dg = pal[i].g - c.g, db = pal[i].b - c.b;
                float d = 3 * dr * dr + 4 * dg * dg + 2 * db * db;   // mesmos pesos do handoff
                if (d < md) { md = d; melhor = i; }
            }
            return pal[melhor];
        }
    }
}
