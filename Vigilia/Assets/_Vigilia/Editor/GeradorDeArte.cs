using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vigilia.EditorTools
{
    /// <summary>
    /// Gera a arte PROVISÓRIA do jogo como PNGs em Assets/_Vigilia/Resources/Gerado.
    /// Tudo pode (e deve) ser trocado pela arte do grupo: basta substituir o PNG mantendo o nome.
    /// TODO(Q-11): estilo dos retratos, paleta e filtro comuns.
    /// </summary>
    public static class GeradorDeArte
    {
        public const string Pasta = "Assets/_Vigilia/Resources/Gerado";

        [MenuItem("Vigilia/Gerar arte provisória")]
        public static void GerarTudo()
        {
            Directory.CreateDirectory(Pasta);
            Salvar("quadrado", Quadrado(16), 16, true);
            Salvar("circulo", Circulo(64, 0.48f, 0.02f), 64);
            Salvar("piso", Piso(32), 32, true);
            Salvar("parede", Parede(32), 32, true);
            Salvar("personagem", Personagem(64), 64);
            Salvar("olhos", Olhos(64), 64);
            Salvar("mira", Mira(32), 32);
            Salvar("cama", Cama(32, 64), 32);
            Salvar("mesa", Mesa(64, 32), 32);
            Salvar("armario", Armario(48, 32), 32);
            Salvar("caldeira", Caldeira(64), 32);
            Salvar("porta", Quadrado(16), 16, true);
            Salvar("sombra_borda", SombraBorda(4, 32), 32);
            foreach (var item in new[] { "pilha", "cobertor", "chave", "pedecabra", "papel", "pulseira", "sedativo", "pesado" })
                Salvar("item_" + item, Item(item, 32), 32);

            string[] ids = { "elias", "marta", "otis", "almeida", "wren", "holt", "paciente1", "paciente2" };
            foreach (var id in ids)
            {
                Salvar($"retrato_{id}_dia", Retrato(id, false, 128), 128);
                Salvar($"retrato_{id}_noite", Retrato(id, true, 128), 128);
            }
            Salvar("menu_sanatorio", Sanatorio(512, 384), 100);
            AssetDatabase.Refresh();
        }

        /// <summary>Arte final (Resources/Arte) se existir; senão a provisória.</summary>
        public static Sprite Sprite(string nome)
            => AssetDatabase.LoadAssetAtPath<Sprite>($"{ImportadorDeArte.Pasta}{nome}.png")
               ?? AssetDatabase.LoadAssetAtPath<Sprite>($"{Pasta}/{nome}.png");

        public static bool TemFinal(string nome) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ImportadorDeArte.Pasta}{nome}.png") != null;

        static void Salvar(string nome, Texture2D tex, float ppu, bool fullRect = false)
        {
            string caminho = $"{Pasta}/{nome}.png";
            File.WriteAllBytes(caminho, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(caminho);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            var ts = new TextureImporterSettings();
            imp.ReadTextureSettings(ts);
            ts.spriteMeshType = fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            ts.wrapMode = fullRect ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            imp.SetTextureSettings(ts);
            imp.SaveAndReimport();
        }

        // ---------------------------------------------------------------- utilidades

        static Texture2D Nova(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            t.SetPixels32(px);
            return t;
        }

        static void Px(Texture2D t, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= t.width || y >= t.height) return;
            if (c.a < 1f)
            {
                var b = t.GetPixel(x, y);
                c = new Color(Mathf.Lerp(b.r, c.r, c.a), Mathf.Lerp(b.g, c.g, c.a), Mathf.Lerp(b.b, c.b, c.a), Mathf.Max(b.a, c.a));
            }
            t.SetPixel(x, y, c);
        }

        static void Ret(Texture2D t, int x0, int y0, int w, int h, Color c)
        {
            for (int y = y0; y < y0 + h; y++) for (int x = x0; x < x0 + w; x++) Px(t, x, y, c);
        }

        static void Elipse(Texture2D t, float cx, float cy, float rx, float ry, Color c)
        {
            for (int y = Mathf.FloorToInt(cy - ry); y <= cy + ry; y++)
                for (int x = Mathf.FloorToInt(cx - rx); x <= cx + rx; x++)
                {
                    float a = (x + 0.5f - cx) / rx, b = (y + 0.5f - cy) / ry;
                    if (a * a + b * b <= 1f) Px(t, x, y, c);
                }
        }

        static Texture2D Fim(Texture2D t) { t.Apply(); return t; }

        // ---------------------------------------------------------------- formas

        static Texture2D Quadrado(int s) { var t = Nova(s, s); Ret(t, 0, 0, s, s, Color.white); return Fim(t); }

        /// <summary>Gradiente vertical: escuro no topo, some embaixo (sombra no chão junto à parede).</summary>
        static Texture2D SombraBorda(int w, int h)
        {
            var t = Nova(w, h);
            for (int y = 0; y < h; y++)
            {
                float a = Mathf.Pow(y / (float)(h - 1), 1.6f) * 0.85f;
                for (int x = 0; x < w; x++) t.SetPixel(x, y, new Color(0, 0, 0, a));
            }
            return Fim(t);
        }

        static Texture2D Circulo(int s, float r, float borda)
        {
            var t = Nova(s, s);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f) / s, new Vector2(0.5f, 0.5f));
                    float a = Mathf.InverseLerp(r, r - borda, d);
                    if (a > 0) t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            return Fim(t);
        }

        static Texture2D Piso(int s)
        {
            var t = Nova(s, s);
            var rnd = new System.Random(2);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    bool junta = x % 16 == 0 || y % 16 == 0;
                    float v = junta ? 0.55f : 0.78f + (float)rnd.NextDouble() * 0.08f;
                    t.SetPixel(x, y, new Color(v, v, v, 1));
                }
            return Fim(t);
        }

        static Texture2D Parede(int s)
        {
            var t = Nova(s, s);
            var rnd = new System.Random(4);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float v = 0.22f + (float)rnd.NextDouble() * 0.05f;
                    if (y % 8 == 0 || (x + (y / 8 % 2) * 8) % 16 == 0) v = 0.12f;
                    t.SetPixel(x, y, new Color(v * 0.9f, v * 0.95f, v * 1.1f, 1));
                }
            return Fim(t);
        }

        /// <summary>Pessoa vista de cima: ombros, cabeça e um nariz apontando para cima (frente).</summary>
        static Texture2D Personagem(int s)
        {
            var t = Nova(s, s);
            Elipse(t, s * 0.5f, s * 0.46f, s * 0.38f, s * 0.24f, new Color(0.75f, 0.75f, 0.75f));   // ombros
            Elipse(t, s * 0.5f, s * 0.5f, s * 0.2f, s * 0.2f, Color.white);                            // cabeça
            Elipse(t, s * 0.5f, s * 0.47f, s * 0.2f, s * 0.14f, new Color(0.85f, 0.85f, 0.85f));      // cabelo
            Elipse(t, s * 0.5f, s * 0.7f, s * 0.05f, s * 0.05f, new Color(0.9f, 0.9f, 0.9f));        // nariz (frente)
            return Fim(t);
        }

        static Texture2D Olhos(int s)
        {
            var t = Nova(s, s);
            var c = new Color(1f, 0.15f, 0.1f);
            Elipse(t, s * 0.42f, s * 0.6f, 2.2f, 2.2f, c);
            Elipse(t, s * 0.58f, s * 0.6f, 2.2f, 2.2f, c);
            return Fim(t);
        }

        static Texture2D Mira(int s)
        {
            var t = Nova(s, s);
            for (int y = s / 2; y < s; y++) Px(t, s / 2, y, new Color(1, 1, 1, Mathf.InverseLerp(s, s / 2, y)));
            return Fim(t);
        }

        static Texture2D Cama(int w, int h)
        {
            var t = Nova(w, h);
            Ret(t, 0, 0, w, h, new Color(0.35f, 0.33f, 0.32f));
            Ret(t, 2, 2, w - 4, h - 4, new Color(0.75f, 0.74f, 0.7f));
            Ret(t, 4, h - 16, w - 8, 12, new Color(0.92f, 0.92f, 0.9f));
            Ret(t, 2, 2, w - 4, h / 2, new Color(0.55f, 0.6f, 0.7f));
            return Fim(t);
        }

        static Texture2D Mesa(int w, int h)
        {
            var t = Nova(w, h);
            Ret(t, 0, 0, w, h, new Color(0.35f, 0.24f, 0.15f));
            Ret(t, 2, 2, w - 4, h - 4, new Color(0.5f, 0.35f, 0.22f));
            Ret(t, w / 2 - 8, h / 2 - 6, 16, 12, new Color(0.9f, 0.88f, 0.8f));     // caderno
            return Fim(t);
        }

        static Texture2D Armario(int w, int h)
        {
            var t = Nova(w, h);
            Ret(t, 0, 0, w, h, new Color(0.25f, 0.27f, 0.25f));
            Ret(t, 2, 2, w / 2 - 3, h - 4, new Color(0.42f, 0.45f, 0.42f));
            Ret(t, w / 2 + 1, 2, w / 2 - 3, h - 4, new Color(0.42f, 0.45f, 0.42f));
            return Fim(t);
        }

        static Texture2D Caldeira(int s)
        {
            var t = Nova(s, s);
            Elipse(t, s / 2f, s / 2f, s * 0.46f, s * 0.46f, new Color(0.3f, 0.3f, 0.32f));
            Elipse(t, s / 2f, s / 2f, s * 0.38f, s * 0.38f, new Color(0.55f, 0.52f, 0.5f));
            Elipse(t, s / 2f, s / 2f, s * 0.12f, s * 0.12f, new Color(0.95f, 0.55f, 0.2f));
            return Fim(t);
        }

        static Texture2D Item(string tipo, int s)
        {
            var t = Nova(s, s);
            switch (tipo)
            {
                case "pilha": Ret(t, 11, 6, 10, 20, new Color(0.2f, 0.6f, 0.3f)); Ret(t, 13, 26, 6, 3, new Color(0.8f, 0.8f, 0.8f)); break;
                case "cobertor": Ret(t, 4, 6, 24, 20, new Color(0.6f, 0.25f, 0.2f)); for (int x = 4; x < 28; x += 4) Ret(t, x, 6, 1, 20, new Color(0.4f, 0.15f, 0.12f)); break;
                case "chave": Elipse(t, 10, 16, 5, 5, new Color(0.85f, 0.7f, 0.3f)); Ret(t, 14, 15, 14, 3, new Color(0.85f, 0.7f, 0.3f)); Ret(t, 24, 11, 2, 4, new Color(0.85f, 0.7f, 0.3f)); break;
                case "pedecabra": for (int i = 0; i < 22; i++) Ret(t, 5 + i, 5 + i, 3, 3, new Color(0.75f, 0.15f, 0.12f)); Ret(t, 24, 26, 6, 3, new Color(0.75f, 0.15f, 0.12f)); break;
                case "papel": Ret(t, 7, 4, 18, 24, new Color(0.92f, 0.9f, 0.82f)); for (int y = 8; y < 26; y += 4) Ret(t, 9, y, 14, 1, new Color(0.3f, 0.3f, 0.35f)); break;
                case "pulseira": Elipse(t, 16, 16, 10, 7, new Color(0.9f, 0.9f, 0.95f)); Elipse(t, 16, 16, 7, 4, new Color(0, 0, 0, 0)); break;
                case "sedativo": Ret(t, 12, 4, 8, 20, new Color(0.7f, 0.85f, 0.95f)); Ret(t, 12, 24, 8, 4, new Color(0.9f, 0.9f, 0.9f)); break;
                case "pesado": Ret(t, 14, 4, 4, 22, new Color(0.55f, 0.55f, 0.6f)); Elipse(t, 16, 26, 6, 4, new Color(0.55f, 0.55f, 0.6f)); break;
            }
            // borda clara para ler no escuro
            var c = t.GetPixels32();
            var copia = (Color32[])c.Clone();
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    if (copia[y * s + x].a > 0) continue;
                    bool viz = false;
                    for (int dy = -1; dy <= 1 && !viz; dy++) for (int dx = -1; dx <= 1 && !viz; dx++)
                    {
                        int xx = x + dx, yy = y + dy;
                        if (xx >= 0 && yy >= 0 && xx < s && yy < s && copia[yy * s + xx].a > 0) viz = true;
                    }
                    if (viz) c[y * s + x] = new Color32(255, 245, 220, 180);
                }
            t.SetPixels32(c);
            return Fim(t);
        }

        /// <summary>Retrato estilo caixa de diálogo, com olheiras. À noite: pele pálida, olheiras fundas, olhos vermelhos.</summary>
        static Texture2D Retrato(string id, bool noite, int s)
        {
            var t = Nova(s, s);
            Color fundo, pele, cabelo, roupa;
            switch (id)
            {
                case "marta": pele = new Color(0.86f, 0.72f, 0.62f); cabelo = new Color(0.45f, 0.3f, 0.2f); roupa = new Color(0.9f, 0.9f, 0.95f); fundo = new Color(0.35f, 0.42f, 0.45f); break;
                case "otis": pele = new Color(0.7f, 0.55f, 0.42f); cabelo = new Color(0.55f, 0.55f, 0.55f); roupa = new Color(0.3f, 0.35f, 0.45f); fundo = new Color(0.35f, 0.32f, 0.28f); break;
                case "almeida": pele = new Color(0.82f, 0.7f, 0.6f); cabelo = new Color(0.15f, 0.12f, 0.1f); roupa = new Color(0.95f, 0.95f, 0.95f); fundo = new Color(0.3f, 0.38f, 0.35f); break;
                case "wren": pele = new Color(0.85f, 0.78f, 0.72f); cabelo = new Color(0.92f, 0.92f, 0.9f); roupa = new Color(0.65f, 0.72f, 0.75f); fundo = new Color(0.3f, 0.3f, 0.36f); break;
                case "holt": pele = new Color(0.84f, 0.73f, 0.66f); cabelo = new Color(0.75f, 0.75f, 0.78f); roupa = new Color(0.2f, 0.2f, 0.22f); fundo = new Color(0.4f, 0.28f, 0.22f); break;
                case "elias": pele = new Color(0.82f, 0.68f, 0.58f); cabelo = new Color(0.3f, 0.2f, 0.12f); roupa = new Color(0.45f, 0.38f, 0.3f); fundo = new Color(0.22f, 0.26f, 0.32f); break;
                default: pele = new Color(0.8f, 0.72f, 0.66f); cabelo = new Color(0.4f, 0.38f, 0.35f); roupa = new Color(0.65f, 0.72f, 0.75f); fundo = new Color(0.3f, 0.32f, 0.33f); break;
            }
            if (noite)
            {
                pele = Color.Lerp(pele, new Color(0.7f, 0.75f, 0.78f), 0.45f);
                fundo = Color.Lerp(fundo, new Color(0.05f, 0.03f, 0.06f), 0.75f);
            }
            Ret(t, 0, 0, s, s, fundo);
            Elipse(t, s * 0.5f, s * 0.05f, s * 0.42f, s * 0.28f, roupa);                       // ombros
            Ret(t, (int)(s * 0.42f), (int)(s * 0.2f), (int)(s * 0.16f), (int)(s * 0.15f), pele); // pescoço
            Elipse(t, s * 0.5f, s * 0.55f, s * 0.26f, s * 0.32f, pele);                          // rosto
            Elipse(t, s * 0.5f, s * 0.8f, s * 0.29f, s * 0.14f, cabelo);                         // cabelo
            if (id == "wren" || id == "holt") Elipse(t, s * 0.5f, s * 0.82f, s * 0.2f, s * 0.08f, pele); // calvície
            // olheiras (todos têm: o tema se lê no rosto, GDD 5.1)
            var olheira = noite ? new Color(0.25f, 0.12f, 0.2f, 0.9f) : new Color(0.45f, 0.3f, 0.35f, 0.55f);
            Elipse(t, s * 0.4f, s * 0.53f, s * 0.08f, s * (noite ? 0.06f : 0.04f), olheira);
            Elipse(t, s * 0.6f, s * 0.53f, s * 0.08f, s * (noite ? 0.06f : 0.04f), olheira);
            // olhos
            var branco = noite ? new Color(0.9f, 0.75f, 0.7f) : new Color(0.95f, 0.95f, 0.92f);
            Elipse(t, s * 0.4f, s * 0.58f, s * 0.055f, s * 0.03f, branco);
            Elipse(t, s * 0.6f, s * 0.58f, s * 0.055f, s * 0.03f, branco);
            var iris = noite ? new Color(0.75f, 0.05f, 0.05f) : new Color(0.15f, 0.12f, 0.1f);
            Elipse(t, s * 0.4f, s * 0.58f, s * 0.022f, s * 0.022f, iris);
            Elipse(t, s * 0.6f, s * 0.58f, s * 0.022f, s * 0.022f, iris);
            // boca
            Ret(t, (int)(s * 0.44f), (int)(s * 0.38f), (int)(s * 0.12f), 2, new Color(0.45f, 0.25f, 0.25f));
            if (id == "marta") Ret(t, (int)(s * 0.38f), (int)(s * 0.9f), (int)(s * 0.24f), (int)(s * 0.06f), Color.white); // touca
            if (id == "almeida" || id == "holt")
            {
                var aro = new Color(0.1f, 0.1f, 0.1f);
                for (int k = -1; k <= 1; k += 2)
                {
                    float cx = s * (0.5f + 0.1f * k);
                    for (int a = 0; a < 40; a++)
                    {
                        float ang = a / 40f * Mathf.PI * 2;
                        Px(t, (int)(cx + Mathf.Cos(ang) * s * 0.07f), (int)(s * 0.58f + Mathf.Sin(ang) * s * 0.05f), aro);
                    }
                }
            }
            return Fim(t);
        }

        /// <summary>Silhueta do sanatório na neve para o menu (inspirada em Refs/Ref_sanatorio_por_fora.jpg).</summary>
        static Texture2D Sanatorio(int w, int h)
        {
            var t = Nova(w, h);
            var rnd = new System.Random(8);
            // montanhas
            for (int x = 0; x < w; x++)
            {
                float m = h * (0.55f + 0.18f * Mathf.Sin(x * 0.012f) + 0.08f * Mathf.Sin(x * 0.037f + 1));
                for (int y = 0; y < m; y++)
                {
                    float neve = y > m - 18 - 10 * Mathf.Sin(x * 0.05f) ? 0.55f : 0.25f;
                    t.SetPixel(x, y, new Color(0.12f * neve * 3, 0.2f * neve * 3, 0.24f * neve * 3, 1));
                }
            }
            // prédio
            int bx0 = (int)(w * 0.12f), bx1 = (int)(w * 0.88f), by0 = 20, by1 = (int)(h * 0.62f);
            Ret(t, bx0, by0, bx1 - bx0, by1 - by0, new Color(0.62f, 0.66f, 0.58f));
            for (int y = by1; y < by1 + 26; y++)
            {
                int enc = (y - by1) * 2;
                Ret(t, bx0 - 6 + enc, y, bx1 - bx0 + 12 - enc * 2, 1, new Color(0.75f, 0.82f, 0.88f));
            }
            // janelas
            for (int fy = 0; fy < 5; fy++)
                for (int fx = 0; fx < 16; fx++)
                {
                    int x = bx0 + 12 + fx * ((bx1 - bx0 - 24) / 16), y = by0 + 14 + fy * ((by1 - by0 - 20) / 5);
                    bool acesa = rnd.NextDouble() < 0.18;
                    Ret(t, x, y, 10, 16, acesa ? new Color(0.95f, 0.8f, 0.4f) : new Color(0.12f, 0.16f, 0.18f));
                }
            // pinheiros
            foreach (int px in new[] { 30, 90, 220, 400, 470 })
                for (int y = 0; y < 110; y++)
                {
                    int larg = (int)((110 - y) * 0.3f);
                    Ret(t, px - larg, y + 6, larg * 2, 1, new Color(0.1f, 0.2f, 0.18f));
                }
            Ret(t, 0, 0, w, 12, new Color(0.8f, 0.86f, 0.92f));
            return Fim(t);
        }
    }
}
