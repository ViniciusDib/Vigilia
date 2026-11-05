using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Vigilia
{
    /// <summary>
    /// Cena FIXA do encontro com o monstro (jumpscare). Port para C# do design handoff
    /// "Asylum Monster Room" (Refs/Horror asylum monster encounter.zip, arquivo Asylum Room.dc.html).
    ///
    /// Tudo é gerado por código em 320×180: Build() pinta a sala e o monstro uma vez (albedo, profundidade,
    /// brilho molhado, olhos emissivos); Quadro() acende cada pixel com a lanterna que segue o mouse
    /// e encaixa a cor na paleta de 30 cores. Os números e as fórmulas são os do handoff (README do zip).
    ///
    /// Uso: yield return Jogo.I.encontro.Tocar(segundos, legenda).
    /// </summary>
    public class EncontroMonstro : MonoBehaviour
    {
        [Header("Ajustes do handoff")]
        [Range(0.6f, 1.6f)] public float tamanhoFeixe = 1.25f;   // o usuário do handoff preferiu 1.25
        public bool piscar = true;
        public bool olhosSeguem = true;
        public bool dither = false;                              // o usuário do handoff preferiu desligado

        const int W = 320, H = 180, N = W * H;
        const float VX = 160f, VY = 80f, T0 = 0.38f;

        float[] R, G, B, Z, S, EM;
        byte[] SF;
        byte[] LUT;
        Color32[] PAL;
        Color32[] px;
        Texture2D tex;
        Olho[] olhos;
        Particula[] motes;
        bool construido;

        Canvas canvas;
        RawImage imagem;
        Text legenda;
        Vector2 mira = new Vector2(160, 85);
        bool temMouse;
        float fAte, t0;

        class Olho
        {
            public float ex, ey, erx, ery;
            public int[] boxI; public float[] boxR, boxG, boxB, boxE;
            public float nb, bs, sn, se, sx, sy;
        }

        struct Particula { public float x, y, vx, vy, p; }

        delegate void PixFn(int x, int y, int i);

        // ================================================================== uso

        public bool Tocando { get; private set; }

        public IEnumerator Tocar(float duracao, string textoLegenda)
        {
            if (!construido) Construir();
            MontarUI();
            Tocando = true;
            canvas.gameObject.SetActive(true);
            legenda.text = textoLegenda ?? "";
            Jogo.I?.audioJogo?.Tocar(SomId.Stinger);
            t0 = Time.unscaledTime;
            float fim = t0 + duracao;
            while (Time.unscaledTime < fim)
            {
                float k = Mathf.InverseLerp(fim - duracao * 0.3f, fim, Time.unscaledTime);
                float esc = 1f + k * k * 0.45f;                    // investida final
                imagem.rectTransform.localScale = new Vector3(esc, esc, 1);
                Quadro(Time.unscaledTime - t0);
                yield return null;
            }
            canvas.gameObject.SetActive(false);
            Tocando = false;
        }

        void MontarUI()
        {
            if (canvas != null) return;
            canvas = UIFabrica.Canvas("CanvasEncontro", 50);
            canvas.transform.SetParent(transform, false);
            var fundo = UIFabrica.Imagem("Fundo", canvas.transform, new Color32(5, 3, 4, 255));
            UIFabrica.Cheio(fundo.rectTransform);
            var r = UIFabrica.Rect("Quadro", canvas.transform);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(1920, 1080);
            var fit = r.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio = 16f / 9f;
            imagem = r.gameObject.AddComponent<RawImage>();
            imagem.texture = tex;
            imagem.raycastTarget = false;
            legenda = UIFabrica.Texto("Legenda", canvas.transform, "", 30, TextAnchor.LowerCenter, new Color(0.42f, 0.07f, 0.08f));
            UIFabrica.Ancorar(legenda.rectTransform, Vector2.zero, new Vector2(1, 0), new Vector2(0, 20), new Vector2(0, 80));
            canvas.gameObject.SetActive(false);
        }

        void Start()
        {
            // Pinta a cena logo no início para o susto não engasgar na primeira vez.
            if (!construido) Construir();
            MontarUI();
        }

        // ================================================================== utilidades do handoff

        uint seed = 7331;

        /// <summary>mulberry32, mesmo gerador e semente do protótipo.</summary>
        float Rnd()
        {
            unchecked
            {
                seed += 0x6D2B79F5u;
                uint t = (seed ^ (seed >> 15)) * (1u | seed);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                return (float)((t ^ (t >> 14)) / 4294967296.0);
            }
        }

        static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (float)(h / 4294967296.0);
            }
        }

        static float Noise(float x, float y)
        {
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
            float fx = x - ix, fy = y - iy;
            float sx = fx * fx * (3 - 2 * fx), sy = fy * fy * (3 - 2 * fy);
            float a = Hash(ix, iy), b = Hash(ix + 1, iy), c = Hash(ix, iy + 1), d = Hash(ix + 1, iy + 1);
            return a + (b - a) * sx + (c - a) * sy + (a - b - c + d) * sx * sy;
        }

        static float Fbm(float x, float y, int o = 4)
        {
            float v = 0, a = 0.5f, f = 1, n = 0;
            for (int i = 0; i < o; i++) { v += Noise(x * f + i * 17.3f, y * f - i * 9.1f) * a; n += a; a *= 0.5f; f *= 2; }
            return v / n;
        }

        static float Cl(float v, float a = 0, float b = 1) => v < a ? a : v > b ? b : v;
        static float Ss(float a, float b, float v) { float t = Cl((v - a) / (b - a)); return t * t * (3 - 2 * t); }
        static float Mix(float a, float b, float t) => a + (b - a) * t;
        static int Rd(float v) => Mathf.FloorToInt(v + 0.5f);          // Math.round do JS
        static Vector2 P(float X, float Y, float Zz) => new Vector2(VX + X / Zz, VY + Y / Zz);
        static bool Inb(int x, int y) => x >= 0 && y >= 0 && x < W && y < H;
        static Vector3 C(float r, float g, float b) => new Vector3(r, g, b);
        static Vector3 Blood(float t) => C(Mix(0.62f, 0.24f, t), Mix(0.035f, 0.015f, t), Mix(0.04f, 0.025f, t));

        void Set(int i, Vector3 c) { R[i] = c.x; G[i] = c.y; B[i] = c.z; }

        void Put(float xf, float yf, Vector3 c, float s = float.NaN, float z = float.NaN)
        {
            int x = Rd(xf), y = Rd(yf);
            if (!Inb(x, y)) return;
            int i = y * W + x;
            Set(i, c);
            if (!float.IsNaN(s)) S[i] = s;
            if (!float.IsNaN(z)) Z[i] = z;
        }

        void BlendPx(float xf, float yf, Vector3 c, float a, float s = float.NaN)
        {
            int x = Rd(xf), y = Rd(yf);
            if (!Inb(x, y)) return;
            int i = y * W + x;
            R[i] += (c.x - R[i]) * a; G[i] += (c.y - G[i]) * a; B[i] += (c.z - B[i]) * a;
            if (!float.IsNaN(s)) S[i] = Mathf.Max(S[i], s);
        }

        PixFn Paint(Vector3 c, float s, float z, int sf) => (x, y, i) =>
        {
            if (sf >= 0 && SF[i] != sf) return;
            Set(i, c);
            if (!float.IsNaN(s)) S[i] = s;
            if (!float.IsNaN(z)) Z[i] = z;
        };

        void Dot(float x, float y, float w, PixFn fn)
        {
            int s = Mathf.Max(1, Rd(w));
            int x0 = Rd(x - s / 2f), y0 = Rd(y - s / 2f);
            for (int yy = y0; yy < y0 + s; yy++)
                for (int xx = x0; xx < x0 + s; xx++)
                    if (Inb(xx, yy)) fn(xx, yy, yy * W + xx);
        }

        void Line(Vector2 a, Vector2 b, float w, PixFn fn) => Line(a, b, _ => w, fn);

        void Line(Vector2 a, Vector2 b, Func<float, float> w, PixFn fn)
        {
            int n = Mathf.CeilToInt(Vector2.Distance(a, b) * 2) + 1;
            for (int k = 0; k <= n; k++)
            {
                float t = (float)k / n;
                Dot(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, w(t), fn);
            }
        }

        void Poly(Vector2[] pts, PixFn fn)
        {
            float x0 = 1e9f, y0 = 1e9f, x1 = -1e9f, y1 = -1e9f;
            foreach (var p in pts) { x0 = Mathf.Min(x0, p.x); y0 = Mathf.Min(y0, p.y); x1 = Mathf.Max(x1, p.x); y1 = Mathf.Max(y1, p.y); }
            for (int y = Mathf.Max(0, Mathf.FloorToInt(y0)); y <= Mathf.Min(H - 1, Mathf.CeilToInt(y1)); y++)
                for (int x = Mathf.Max(0, Mathf.FloorToInt(x0)); x <= Mathf.Min(W - 1, Mathf.CeilToInt(x1)); x++)
                {
                    float px_ = x + 0.5f, py = y + 0.5f;
                    bool ins = false;
                    for (int a = 0, b = pts.Length - 1; a < pts.Length; b = a++)
                    {
                        float xa = pts[a].x, ya = pts[a].y, xb = pts[b].x, yb = pts[b].y;
                        if ((ya > py) != (yb > py) && px_ < (xb - xa) * (py - ya) / (yb - ya) + xa) ins = !ins;
                    }
                    if (ins) fn(x, y, y * W + x);
                }
        }

        void Ell(float cx, float cy, float rx, float ry, int sf, Vector3 c, float s = float.NaN)
        {
            rx = Mathf.Max(rx, 0.6f); ry = Mathf.Max(ry, 0.6f);
            bool any = false;
            for (int y = Mathf.FloorToInt(cy - ry); y <= Mathf.CeilToInt(cy + ry); y++)
                for (int x = Mathf.FloorToInt(cx - rx); x <= Mathf.CeilToInt(cx + rx); x++)
                {
                    if (!Inb(x, y)) continue;
                    int i = y * W + x;
                    if (sf >= 0 && SF[i] != sf) continue;
                    float qx = (x + 0.5f - cx) / rx, qy = (y + 0.5f - cy) / ry;
                    if (qx * qx + qy * qy <= 1) { Set(i, c); if (!float.IsNaN(s)) S[i] = s; any = true; }
                }
            if (!any)
            {
                int x = Mathf.FloorToInt(cx), y = Mathf.FloorToInt(cy);
                if (Inb(x, y) && (sf < 0 || SF[y * W + x] == sf)) Put(x, y, c, s);
            }
        }

        // ================================================================== superfícies da sala

        static Vector3 Wall(float u, float v)
        {
            float r, g, b;
            if (v > 92) { float k = 0.6f + 0.4f * Noise(u * 0.2f, v * 0.3f); r = 0.22f * k; g = 0.12f * k; b = 0.07f * k; }
            else if (v > 28)
            {
                float tu = u / 14, tv = (v - 28) / 11, gu = tu - Mathf.Floor(tu), gv = tv - Mathf.Floor(tv);
                float th = Hash(Mathf.FloorToInt(tu), Mathf.FloorToInt(tv) + 99);
                if (gu < 0.1f || gv < 0.12f) { r = 0.16f; g = 0.15f; b = 0.12f; }
                else { r = 0.42f + th * 0.08f; g = 0.47f + th * 0.06f; b = 0.38f; if (th > 0.88f) { r = 0.2f; g = 0.18f; b = 0.15f; } }
                if (v < 31) { r = 0.3f; g = 0.3f; b = 0.26f; }
            }
            else
            {
                r = 0.58f; g = 0.52f; b = 0.41f;
                if (Fbm(u * 0.06f + 3, v * 0.06f) > 0.6f) { r = 0.33f; g = 0.28f; b = 0.24f; }
                if (v < -74) { r = 0.24f; g = 0.21f; b = 0.19f; }
            }
            float gm = 0.5f + 0.7f * Fbm(u * 0.03f, v * 0.03f); r *= gm; g *= gm; b *= gm;
            float streak = Fbm(u * 0.035f, v * 0.012f, 3) * 0.55f + Noise(u * 0.35f, v * 0.02f) * 0.45f;
            float bm = Ss(0.48f, 0.6f, streak + Ss(60, -80, v) * 0.12f);
            float dk = Fbm(u * 0.08f + 50, v * 0.05f);
            r = Mix(r, Mix(0.64f, 0.28f, dk), bm * 0.95f); g = Mix(g, Mix(0.04f, 0.02f, dk), bm * 0.95f); b = Mix(b, Mix(0.045f, 0.03f, dk), bm * 0.95f);
            return C(r * 1.08f + 0.04f, g * 0.75f, b * 0.75f);
        }

        static Vector3 FloorC(float u, float z)
        {
            const float pw = 22;
            int pi = Mathf.FloorToInt(u / pw); float fu = u / pw - pi;
            float off = Hash(pi, 7); int sg = Mathf.FloorToInt(z * 1.6f + off); float fz = z * 1.6f + off - sg;
            float k = (0.75f + Hash(pi, sg) * 0.35f) * (0.85f + Noise(u * 0.6f, z * 40) * 0.25f + Noise(u * 2, z * 8) * 0.1f);
            float r = 0.36f * k, g = 0.22f * k, b = 0.12f * k;
            if (fu < 0.07f || fu > 0.96f || fz < 0.03f) { r *= 0.35f; g *= 0.35f; b *= 0.35f; }
            float gr = Fbm(u * 0.02f, z * 2); r *= 0.6f + gr * 0.7f; g *= 0.6f + gr * 0.6f; b *= 0.6f + gr * 0.6f;
            float edge = Ss(115, 158, Mathf.Abs(u)); r *= 1 - edge * 0.55f; g *= 1 - edge * 0.55f; b *= 1 - edge * 0.55f;
            float m = Ss(0.6f, 0.7f, Fbm(u * 0.03f + 20, z * 1.5f + 5)) * 0.8f;
            return C(Mix(r, 0.36f, m), Mix(g, 0.03f, m), Mix(b, 0.03f, m));
        }

        static Vector3 CeilC(float u, float z)
        {
            float pu = u / 40, pz = z * 2.2f, fu = pu - Mathf.Floor(pu), fz = pz - Mathf.Floor(pz);
            if (Hash(Mathf.FloorToInt(pu) + 50, Mathf.FloorToInt(pz)) > 0.86f) return C(0.03f, 0.02f, 0.02f);
            float k = 0.6f + 0.5f * Fbm(u * 0.03f, z * 3); float r = 0.36f * k, g = 0.33f * k, b = 0.29f * k;
            if (fu < 0.05f || fz < 0.06f) { r *= 0.4f; g *= 0.4f; b *= 0.4f; }
            if (Fbm(u * 0.02f + 40, z * 1.2f + 3) > 0.58f) { r = Mix(r, 0.3f, 0.6f); g = Mix(g, 0.2f, 0.6f); b = Mix(b, 0.1f, 0.6f); }
            float bl = Ss(0.6f, 0.7f, Fbm(u * 0.05f + 9, z * 4)); r = Mix(r, 0.5f, bl); g = Mix(g, 0.03f, bl); b = Mix(b, 0.03f, bl);
            return C(r * 1.08f + 0.02f, g * 0.8f, b * 0.8f);
        }

        // ================================================================== build (uma vez)

        void Construir()
        {
            R = new float[N]; G = new float[N]; B = new float[N]; Z = new float[N]; S = new float[N]; EM = new float[N]; SF = new byte[N];
            seed = 7331;

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x; float dx = x + 0.5f - VX, dy = y + 0.5f - VY;
                    float tF = dy > 0 ? dy / 100 : 0, tC = dy < 0 ? -dy / 80 : 0, tW = Mathf.Abs(dx) / 160, tm = Mathf.Max(tF, Mathf.Max(tC, tW));
                    Vector3 c; float z; byte sf;
                    if (tm < T0) { sf = 0; z = 1 / T0; c = Wall(dx / T0 + 400, dy / T0); }
                    else if (tm == tF) { sf = 1; z = 1 / tF; c = FloorC(dx / tF, z); }
                    else if (tm == tC) { sf = 2; z = 1 / tC; c = CeilC(dx / tC, z); }
                    else { sf = (byte)(dx < 0 ? 3 : 4); z = 1 / tW; c = Wall((dx < 0 ? 0 : 900) + z * 160, dy / tW); }
                    Set(i, c); Z[i] = z; SF[i] = sf; S[i] = sf == 1 ? 0.12f : 0.05f;
                }

            ConstruirPorta();
            ConstruirSangue();
            ConstruirCama();
            // lâmpada pendurada
            Line(P(70, -80, 1.4f), P(70, -50, 1.4f), 1, (x, y, i) => Set(i, C(0.08f, 0.07f, 0.06f)));
            for (int r = 0; r < 4; r++) for (int x = -r - 1; x <= r; x++) Put(210 + x, 44 + r, C(0.24f, 0.28f, 0.22f), r == 0 ? 0.7f : 0.4f, 1.4f);
            Put(209, 48, C(0.75f, 0.72f, 0.6f), 1); Put(211, 48, C(0.55f, 0.52f, 0.45f), 0.8f);
            ConstruirMonstro();
            ConstruirPaleta();

            motes = new Particula[70];
            for (int k = 0; k < motes.Length; k++)
            {
                var m = new Particula();
                m.x = Rnd() * W; m.y = Rnd() * H; m.vx = (Rnd() - 0.5f) * 0.06f; m.vy = (Rnd() - 0.3f) * 0.04f; m.p = Rnd() * 6;
                motes[k] = m;
            }

            px = new Color32[N];
            tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            construido = true;
            Quadro(0f);
        }

        void ConstruirPorta()
        {
            Poly(new[] { P(160, -16, 1.46f), P(160, -16, 2.09f), P(160, 100, 2.09f), P(160, 100, 1.46f) }, (x, y, i) => Set(i, C(0.13f, 0.1f, 0.08f)));
            Poly(new[] { P(160, -12, 1.5f), P(160, -12, 2.05f), P(160, 100, 2.05f), P(160, 100, 1.5f) }, (x, y, i) =>
            {
                float n = Fbm(x * 0.2f, y * 0.2f), k = 0.6f + n * 0.6f; float r = 0.36f * k, g = 0.4f * k, b = 0.34f * k;
                if (Noise(x * 0.5f + 9, y * 0.35f) > 0.66f) { r = 0.4f * k; g = 0.19f * k; b = 0.08f * k; }
                if (y > 120) { r *= 0.7f; g *= 0.7f; b *= 0.7f; }
                float bm = Ss(0.55f, 0.65f, Fbm(x * 0.12f + 30, y * 0.05f)); r = Mix(r, 0.5f, bm); g = Mix(g, 0.03f, bm); b = Mix(b, 0.03f, bm);
                Set(i, C(r, g, b)); S[i] = 0.35f; SF[i] = 5;
            });
            Poly(new[] { P(160, 8, 1.64f), P(160, 8, 1.86f), P(160, 36, 1.86f), P(160, 36, 1.64f) }, (x, y, i) => { Set(i, C(0.02f, 0.015f, 0.015f)); S[i] = 0.2f; });
            Put(250, 90, C(0.6f, 0.56f, 0.42f)); Put(252, 90, C(0.6f, 0.56f, 0.42f));
            EM[90 * W + 250] = 1; EM[90 * W + 252] = 1;
            foreach (var zb in new[] { 1.69f, 1.75f, 1.81f }) Line(P(160, 8, zb), P(160, 36, zb), 1, Metal);
            Line(P(160, 46, 1.95f), P(160, 46, 1.87f), 1.6f, (x, y, i) => { Set(i, C(0.5f, 0.48f, 0.42f)); S[i] = 0.9f; });
            for (int k = 0; k < 4; k++)
                Line(new Vector2(252 + k * 2.4f, 104), new Vector2(258 + k * 2.4f, 126), 1, (x, y, i) =>
                {
                    if (SF[i] != 5) return;
                    Set(i, C(0.66f, 0.6f, 0.5f)); S[i] = 0.2f;
                    if (x + 1 < W && SF[i + 1] == 5) { R[i + 1] *= 0.3f; G[i + 1] *= 0.3f; B[i + 1] *= 0.3f; }
                });
        }

        void Metal(int x, int y, int i)
        {
            bool rust = Noise(x * 0.7f, y * 0.7f) > 0.58f;
            Set(i, rust ? C(0.42f, 0.2f, 0.1f) : C(0.36f, 0.34f, 0.31f));
            S[i] = rust ? 0.1f : 0.55f;
        }

        void Drip(float xf, float y0, float len, int sf, float dry = 0.2f)
        {
            int x = Rd(xf); int y = Rd(y0); float end = y0 + len;
            for (; y < end && y < H; y++)
            {
                if (y < 0 || x < 0 || x >= W) break;
                int i = y * W + x; if (SF[i] != sf) break;
                Set(i, Blood(dry + (y - y0) / len * 0.3f)); S[i] = 0.45f;
            }
            Ell(x + 0.5f, y - 0.5f, 0.9f, 1.2f, sf, Blood(dry), 0.6f);
        }

        void Splat(float cx, float cy, float rad, float sx, float sy, int drips = 0, int drops = 10, float gloss = 0.3f)
        {
            int sf = SF[(int)cy * W + (int)cx];
            float dry = Rnd() * 0.5f, sd = Rnd() * 100, gl = gloss;
            int ex = Mathf.CeilToInt(rad * 1.4f * sx) + 1, ey = Mathf.CeilToInt(rad * 1.4f * sy) + 1;
            for (int y = -ey; y <= ey; y++)
                for (int x = -ex; x <= ex; x++)
                {
                    int pxx = (int)cx + x, pyy = (int)cy + y; if (!Inb(pxx, pyy)) continue;
                    int i = pyy * W + pxx; if (SF[i] != sf) continue;
                    float ux = x / sx, uy = y / sy, d = Mathf.Sqrt(ux * ux + uy * uy), a = Mathf.Atan2(uy, ux);
                    if (d < rad * (0.6f + 0.7f * Noise(Mathf.Cos(a) * 1.8f + sd, Mathf.Sin(a) * 1.8f + sd)))
                    {
                        Set(i, Blood(dry + 0.35f * Noise(pxx * 0.5f, pyy * 0.5f))); S[i] = Mathf.Max(S[i], gl);
                    }
                }
            for (int k = 0; k < drops; k++)
            {
                float a = Rnd() * 6.283f, dd = rad * (1.05f + Rnd() * 1.7f), r = 0.5f + Rnd() * rad * 0.16f;
                Ell(cx + Mathf.Cos(a) * dd * sx, cy + Mathf.Sin(a) * dd * sy, r * sx, r * sy, sf, Blood(dry + Rnd() * 0.3f), gl);
            }
            for (int k = 0; k < drips; k++)
            {
                float dx = cx + (Rnd() - 0.5f) * rad * 1.3f * sx;
                float len = rad * sy * (1 + Rnd() * Rnd() * 7);
                Drip(dx, cy + rad * 0.3f * sy, len, sf, dry);
            }
        }

        void Hand(float cx, float cy, float s, int sf, float flip = 1)
        {
            var c = Blood(0.15f); var fn = Paint(c, 0.4f, float.NaN, sf);
            for (float y = 0; y < s * 10; y++)
                for (float x = -s * 2.2f; x <= s * 2.2f; x++)
                {
                    int pxx = Rd(cx + x), pyy = Rd(cy + y); if (!Inb(pxx, pyy)) continue;
                    if (Noise(pxx * 0.9f, pyy * 0.12f) > 0.42f + y / (s * 10) * 0.45f) fn(pxx, pyy, pyy * W + pxx);
                }
            Ell(cx, cy, 2.3f * s, 2.5f * s, sf, c, 0.4f);
            float[,] dedos = { { -0.5f, 4.4f }, { -0.17f, 5.3f }, { 0.15f, 5.1f }, { 0.45f, 4.2f }, { -1.25f * flip, 3.2f } };
            for (int k = 0; k < 5; k++)
            {
                float ang = dedos[k, 0], len = dedos[k, 1];
                float a = flip < 0 ? -ang : ang;
                Line(new Vector2(cx + Mathf.Sin(a) * 2 * s, cy - Mathf.Cos(a) * 2 * s),
                     new Vector2(cx + Mathf.Sin(a) * (2 + len) * s, cy - Mathf.Cos(a) * (2 + len) * s), Mathf.Max(1, s * 1.05f), fn);
            }
        }

        void ConstruirSangue()
        {
            Splat(122, 70, 5, 1, 1, drips: 6, drops: 14);
            Splat(206, 100, 4, 1, 1, drips: 4);
            Splat(108, 106, 3, 1, 1, drips: 2, drops: 6);
            Splat(70, 38, 7, 0.6f, 1, drips: 8, drops: 16);
            Splat(296, 55, 9, 0.55f, 1, drips: 10, drops: 18);
            Splat(302, 112, 6, 0.55f, 1, drips: 5);
            Splat(140, 20, 6, 1, 0.5f, drops: 22);
            Splat(250, 14, 5, 1, 0.5f, drops: 12);
            Hand(30, 66, 1.6f, 3); Hand(54, 80, 1.2f, 3, -1); Hand(14, 98, 1.9f, 3);
            Hand(283, 86, 1.7f, 4, -1);

            // "HELP" em fonte 3×5, escala 2
            string[][] fonte =
            {
                new[] { "101", "101", "111", "101", "101" }, new[] { "111", "100", "110", "100", "111" },
                new[] { "100", "100", "100", "100", "111" }, new[] { "110", "101", "110", "100", "100" },
            };
            int tx = 189;
            foreach (var ch in fonte)
            {
                for (int ry = 0; ry < 5; ry++)
                    for (int rx = 0; rx < 3; rx++)
                    {
                        if (ch[ry][rx] != '1') continue;
                        for (int a = 0; a < 2; a++) for (int b = 0; b < 2; b++) if (Rnd() > 0.08f) Put(tx + rx * 2 + a, 58 + ry * 2 + b, Blood(0.1f + Rnd() * 0.2f), 0.45f);
                        if (ry == 4 && Rnd() > 0.35f) Drip(tx + rx * 2 + (Rnd() > 0.5f ? 1 : 0), 67, 3 + Rnd() * 14, 0, 0.15f);
                    }
                tx += 8;
            }

            Splat(100, 162, 15, 1, 0.3f, gloss: 0.7f, drops: 8);
            Splat(232, 152, 11, 1, 0.3f, gloss: 0.7f, drops: 6);
            Splat(198, 128, 6, 1, 0.3f, gloss: 0.6f, drops: 4);
            Splat(286, 172, 8, 1, 0.3f, gloss: 0.6f, drops: 6);
            // rastro de arrasto até os pés do monstro
            for (int y = 143; y < H; y++)
            {
                float t = (y + 0.5f - VY) / 100, cx = Mix(164, 180, Mathf.Pow((y - 143) / 37f, 1.5f)), w = 20 * t;
                for (int x = Mathf.FloorToInt(cx - w / 2); x <= Mathf.CeilToInt(cx + w / 2); x++)
                {
                    if (!Inb(x, y)) continue; int i = y * W + x; if (SF[i] != 1) continue;
                    float u = (x + 0.5f - cx) / t, st = Noise(u * 0.28f, 3.3f), ed = 1 - Ss(0.25f, 0.5f, Mathf.Abs(x + 0.5f - cx) / w);
                    float a = ed * (0.45f + 0.5f * st); var c = Blood(0.35f + st * 0.3f);
                    R[i] = Mix(R[i], c.x, a); G[i] = Mix(G[i], c.y, a); B[i] = Mix(B[i], c.z, a); S[i] = Mathf.Max(S[i], 0.5f * a);
                }
            }
            // papéis no chão
            float[,] papeis = { { 250, 164, 9, 3, 2 }, { 56, 170, 10, 3, -2 }, { 214, 136, 5, 2, 1 }, { 126, 150, 6, 2, -1 } };
            for (int k = 0; k < 4; k++)
            {
                float x0 = papeis[k, 0], y0 = papeis[k, 1], w = papeis[k, 2], h = papeis[k, 3], sk = papeis[k, 4];
                Poly(new[] { new Vector2(x0, y0), new Vector2(x0 + w, y0), new Vector2(x0 + w + sk, y0 + h), new Vector2(x0 + sk, y0 + h) }, (x, y, i) =>
                {
                    float kk = 0.8f + 0.2f * Noise(x, y); Set(i, C(0.7f * kk, 0.66f * kk, 0.55f * kk)); S[i] = 0.1f;
                });
            }
            Splat(254, 165, 1.6f, 1, 0.6f, drops: 2);
            Poly(new[] { P(-160, 100, 1.15f), P(-96, 100, 1.15f), P(-96, 100, 1.85f), P(-160, 100, 1.85f) }, (x, y, i) =>
            {
                if (SF[i] == 1) { R[i] *= 0.4f; G[i] *= 0.4f; B[i] *= 0.4f; }
            });
            Sombra(160, 143, 54, 7, 0.75f);
        }

        void Sombra(float cx, float cy, float rx, float ry, float amt)
        {
            for (int y = Mathf.FloorToInt(cy - ry); y <= cy + ry; y++)
                for (int x = Mathf.FloorToInt(cx - rx); x <= cx + rx; x++)
                {
                    if (!Inb(x, y)) continue;
                    float qx = (x + 0.5f - cx) / rx, qy = (y + 0.5f - cy) / ry;
                    float k = 1 - (qx * qx + qy * qy); if (k <= 0) continue;
                    int i = y * W + x; float f = 1 - amt * Mathf.Min(1, k * 1.6f);
                    R[i] *= f; G[i] *= f; B[i] *= f;
                }
        }

        void ConstruirCama()
        {
            const float BZ = 1.5f;
            PixFn mbz = (x, y, i) => { Metal(x, y, i); Z[i] = BZ; };
            foreach (var X in new[] { -158f, -100f }) Line(P(X, 8, 1.8f), P(X, 100, 1.8f), 1.3f, mbz);
            foreach (var Y in new[] { 8f, 30f }) Line(P(-158, Y, 1.8f), P(-100, Y, 1.8f), 1, mbz);
            for (float X = -147; X < -100; X += 11) Line(P(X, 8, 1.8f), P(X, 30, 1.8f), 1, mbz);
            var stainC = P(-125, 50, 1.45f);
            Poly(new[] { P(-158, 50, 1.2f), P(-100, 50, 1.2f), P(-100, 50, 1.8f), P(-158, 50, 1.8f) }, (x, y, i) =>
            {
                float k = 0.7f + 0.4f * Fbm(x * 0.15f, y * 0.15f); float r = 0.58f * k, g = 0.53f * k, b = 0.42f * k;
                if (((int)(x * 0.6f + y * 1.4f)) % 4 == 0) { r *= 0.85f; g *= 0.85f; b *= 0.9f; }
                float ddx = (x + 0.5f - stainC.x) / 1.6f, ddy = y + 0.5f - stainC.y;
                float d = Mathf.Sqrt(ddx * ddx + ddy * ddy); float lim = 7 + 4 * Noise(x * 0.3f, y * 0.3f);
                if (d < lim) { var c = Blood(0.2f + 0.4f * d / lim); r = c.x; g = c.y; b = c.z; }
                Set(i, C(r, g, b)); Z[i] = BZ; S[i] = 0.15f;
            });
            void Lado(Vector2[] pts, float f) => Poly(pts, (x, y, i) =>
            {
                float k = (0.7f + 0.4f * Fbm(x * 0.2f, y * 0.2f)) * f; Set(i, C(0.55f * k, 0.5f * k, 0.4f * k)); Z[i] = BZ; S[i] = 0.1f;
                if (Noise(x * 0.8f, y * 0.15f) > 0.7f) Set(i, Blood(0.3f));
            });
            Lado(new[] { P(-100, 50, 1.2f), P(-100, 50, 1.8f), P(-100, 62, 1.8f), P(-100, 62, 1.2f) }, 0.7f);
            Lado(new[] { P(-158, 50, 1.2f), P(-100, 50, 1.2f), P(-100, 62, 1.2f), P(-158, 62, 1.2f) }, 0.85f);
            PixFn couro = (x, y, i) => { float k = 0.8f + 0.3f * Noise(x, y); Set(i, C(0.22f * k, 0.12f * k, 0.07f * k)); Z[i] = BZ; S[i] = 0.4f; };
            foreach (var zz in new[] { new Vector2(1.36f, 1.43f), new Vector2(1.6f, 1.67f) })
            {
                Poly(new[] { P(-158, 49.5f, zz.x), P(-100, 49.5f, zz.x), P(-100, 49.5f, zz.y), P(-158, 49.5f, zz.y) }, couro);
                Poly(new[] { P(-100, 49.5f, zz.x), P(-100, 49.5f, zz.y), P(-100, 62, zz.y), P(-100, 62, zz.x) }, couro);
            }
            Line(P(-100, 62, 1.4f), P(-97, 90, 1.37f), 2, couro);
            var fivela = P(-97, 90, 1.37f);
            Dot(fivela.x, fivela.y, 1, (x, y, i) => { Set(i, C(0.6f, 0.58f, 0.5f)); S[i] = 0.9f; });
            Line(P(-100, 64, 1.2f), P(-100, 64, 1.8f), 1.5f, mbz);
            Line(P(-100, 64, 1.8f), P(-100, 100, 1.8f), 1.5f, mbz);
            foreach (var X in new[] { -158f, -100f }) Line(P(X, 26, 1.2f), P(X, 100, 1.2f), 2, mbz);
            foreach (var Y in new[] { 26f, 40f }) Line(P(-158, Y, 1.2f), P(-100, Y, 1.2f), 1.4f, mbz);
            for (float X = -147; X < -100; X += 11) Line(P(X, 26, 1.2f), P(X, 40, 1.2f), 1, mbz);
        }

        // ================================================================== o monstro

        const float MX = 160, MZ = 1.6f, MY = 93, MRY = 27;
        static readonly float[][] EYES = { new[] { 150f, 50, 4.4f, 3.5f }, new[] { 169f, 51, 4.9f, 3.9f }, new[] { 160f, 39, 1.8f, 3.2f } };

        static float Hwf(float y)
        {
            if (y < 32) return -1;
            if (y < 54) { float k = (y - 54) / 22; return 21 * Mathf.Sqrt(Mathf.Max(0, 1 - k * k)); }
            if (y < 98) return 21 + (y - 54) * 0.06f;
            if (y < 114) return Mix(23.6f, 45, Ss(98, 114, y));
            return 45 - (y - 114) * 0.22f;
        }

        static float Mrx(float y) => 13 * (0.62f + 0.38f * Ss(66, 86, y)) * (1 - 0.28f * Ss(106, 121, y));
        static float MouthE(float x, float y) { float a = (x - MX) / Mrx(y), b = (y - MY) / MRY; return a * a + b * b; }

        static bool PertoDeOlho(float x, float y, float r, float off)
        {
            foreach (var e in EYES) { float dx = x - e[0] - off, dy = y - e[1] - off; if (Mathf.Sqrt(dx * dx + dy * dy) < r) return true; }
            return false;
        }

        void ConstruirMonstro()
        {
            // silhueta e carne
            for (int y = 31; y < 150; y++)
            {
                float w0 = Hwf(y + 0.5f); if (w0 <= 0) continue;
                for (int x = 100; x < 220; x++)
                {
                    float dx = x + 0.5f - MX;
                    float rag = y > 100 ? (Noise(y * 0.25f, dx > 0 ? 3 : 9) - 0.5f) * 5 : (Noise(y * 0.4f, dx > 0 ? 1 : 2) - 0.5f) * 1.5f;
                    float w = w0 + rag; if (Mathf.Abs(dx) > w) continue;
                    if (y > 136 + Noise(x * 0.45f, 7) * 8 - (Mathf.Abs(dx) > 30 ? 3 : 0)) continue;
                    int i = y * W + x; float nx = dx / w;
                    float shade = 0.35f + 0.65f * Mathf.Pow(Mathf.Max(0, 1 - nx * nx), 0.6f);
                    float lump = Fbm(x * 0.16f, y * 0.16f, 3), fine = Noise(x * 0.8f + 3, y * 0.8f);
                    float r = Mix(0.34f, 0.8f, lump), g = Mix(0.03f, 0.12f, lump * lump), b = Mix(0.03f, 0.09f, lump);
                    float fa = dx / 18.5f, fb = (y + 0.5f - 78) / 46; float fe = fa * fa + fb * fb;
                    if (fe > 1) { float kk = 0.42f + 0.4f * Noise(x * 0.4f, y * 0.05f); r *= kk; g *= kk; b *= kk; }
                    else if (fe > 0.85f) { r *= 0.78f; g *= 0.78f; b *= 0.78f; }
                    float k = shade * (0.82f + fine * 0.3f); r *= k; g *= k; b *= k;
                    Set(i, C(r, g, b)); Z[i] = MZ; SF[i] = 9; EM[i] = 0;
                    S[i] = fine > 0.74f && lump > 0.42f ? 0.95f : 0.12f;
                }
            }
            // nódulos
            for (int k = 0; k < 46; k++)
            {
                float x = MX + (Rnd() - 0.5f) * 34, y = 36 + Rnd() * 86;
                float qa = (x - MX) / 18.5f, qb = (y - 78) / 46; if (qa * qa + qb * qb > 0.95f) continue;
                if (PertoDeOlho(x, y, 6.5f, 0.5f)) continue;
                if (MouthE(x, y) < 1.7f) continue;
                Put(x, y, C(0.86f, 0.55f, 0.42f), 0.8f); Put(x, y + 1, C(0.22f, 0.02f, 0.02f));
            }
            // veias
            for (int k = 0; k < 12; k++)
            {
                float x = MX + (Rnd() - 0.5f) * 36, y = 34 + Rnd() * 20, a = Mathf.PI / 2 + (Rnd() - 0.5f) * 1.6f;
                float len = 18 + Rnd() * 30;
                for (int s = 0; s < len; s++)
                {
                    a += (Rnd() - 0.5f) * 0.7f; x += Mathf.Cos(a) * 0.9f; y += Mathf.Sin(a) * 0.9f;
                    int xi = Rd(x), yi = Rd(y); if (!Inb(xi, yi)) break; int i = yi * W + xi;
                    if (SF[i] != 9) break;
                    if (MouthE(x, y) < 1.6f || PertoDeOlho(x, y, 7.5f, 0f)) continue;
                    Set(i, C(0.2f, 0.01f, 0.04f)); S[i] = 0.3f;
                    if (Inb(xi + 1, yi) && SF[i + 1] == 9) { R[i + 1] = Mathf.Min(1, R[i + 1] * 1.25f + 0.05f); S[i + 1] = 0.8f; }
                }
            }
            // olhos
            foreach (var e in EYES)
            {
                float ex = e[0], ey = e[1], erx = e[2], ery = e[3];
                bool big = erx > 3;
                if (big) for (int k = -3; k <= 3; k++) { Put(ex + k * 2, ey - ery - 3.5f + k * k * 0.25f, C(0.8f, 0.5f, 0.38f), 0.7f); Put(ex + k * 2, ey - ery - 2.5f + k * k * 0.25f, C(0.2f, 0.01f, 0.02f)); }
                Ell(ex + 0.5f, ey + 0.5f, erx + 2.4f, ery + 2, -1, C(0.03f, 0, 0.01f), 0.1f);
                Ell(ex + 0.5f, ey + 0.5f, erx + 1.1f, ery + 0.9f, -1, C(0.45f, 0.05f, 0.05f), 0.8f);
                for (int y = (int)ey - Mathf.CeilToInt(ery); y <= ey + Mathf.CeilToInt(ery); y++)
                    for (int x = (int)ex - Mathf.CeilToInt(erx); x <= ex + Mathf.CeilToInt(erx); x++)
                    {
                        float qa = (x - ex) / erx, qb = (y - ey) / ery; float q = qa * qa + qb * qb; if (q >= 1) continue;
                        int i = y * W + x; float sh = 1 - q * 0.25f;
                        Set(i, big ? C(0.88f * sh, 0.8f * sh, 0.46f * sh) : C(0.95f * sh, 0.55f * sh, 0.12f * sh)); EM[i] = 1; S[i] = 0.9f;
                    }
                if (big)
                    for (int k = 0; k < 9; k++)
                    {
                        float a = Rnd() * 6.283f; float x = ex + Mathf.Cos(a) * erx * 0.95f, y = ey + Mathf.Sin(a) * ery * 0.95f;
                        float lim = 2 + Rnd() * 3;
                        for (int s = 0; s < lim; s++)
                        {
                            int i = Rd(y) * W + Rd(x);
                            if (i >= 0 && i < N && EM[i] == 1) Set(i, C(0.72f, 0.12f, 0.08f));
                            x += (ex - x) * 0.3f + (Rnd() - 0.5f); y += (ey - y) * 0.3f + (Rnd() - 0.5f) * 0.6f;
                        }
                    }
                float tl = big ? 16 + Rnd() * 14 : 5;
                float x0 = ex + (big ? (ex < MX ? -1 : 1) : 0); int y0 = Rd(ey + ery + 1);
                for (int y = y0; y < y0 + tl; y++)
                {
                    int x = (int)x0 + Rd(Mathf.Sin(y * 0.4f) * 0.6f); if (!Inb(x, y)) break; int i = y * W + x; if (SF[i] != 9) break;
                    Set(i, Blood(0.05f + (y - y0) / tl * 0.3f)); S[i] = 0.95f;
                }
            }
            // nariz
            var dark = C(0.05f, 0, 0.01f);
            int[] nariz = { 158, 56, 159, 56, 160, 56, 161, 56, 157, 57, 158, 57, 161, 57, 162, 57, 157, 58, 158, 58, 161, 58, 162, 58, 157, 59, 158, 59, 161, 59, 162, 59, 158, 60, 161, 60 };
            for (int k = 0; k < nariz.Length; k += 2) Put(nariz[k], nariz[k + 1], dark, 0.3f);
            Put(159, 57, C(0.3f, 0.03f, 0.03f)); Put(160, 57, C(0.3f, 0.03f, 0.03f));
            // boca
            for (int y = 60; y < 128; y++)
                for (int x = 138; x < 182; x++)
                {
                    float e = MouthE(x + 0.5f, y + 0.5f); int i = y * W + x; if (!Inb(x, y) || SF[i] != 9) continue;
                    if (e < 1)
                    {
                        float gm = Ss(0.55f, 0.92f, e); float r = Mix(0.07f, 0.5f, gm), g = Mix(0.005f, 0.06f, gm), b = Mix(0.01f, 0.07f, gm);
                        float ta = (x + 0.5f - MX) / 6, tb = (y + 0.5f - 84) / 11;
                        if (ta * ta + tb * tb < 1) { r *= 0.25f; g *= 0.25f; b *= 0.25f; }
                        float la = (x + 0.5f - MX) / 9, lb = (y + 0.5f - 113) / 6; float tg = la * la + lb * lb;
                        if (tg < 1) { float k = 0.55f + 0.45f * (1 - tg); r = 0.66f * k; g = 0.2f * k; b = 0.21f * k; if (x == 159 || x == 160) { r *= 0.7f; g *= 0.7f; b *= 0.7f; } }
                        Set(i, C(r, g, b)); S[i] = 0.55f;
                    }
                    else if (e < 1.1f) { Set(i, C(0.18f, 0.01f, 0.02f)); S[i] = 0.2f; }
                    else if (e < 1.42f) { float k = 0.85f + 0.25f * Noise(x * 0.6f, y * 0.6f); Set(i, C(0.68f * k, 0.1f * k, 0.09f * k)); S[i] = 0.75f; }
                }
            Anel(0.9f, 3.1f, true); Anel(0.74f, 4.2f, false);
            float hp = Mathf.PI / 2;
            float[,] presas = { { -hp - 0.5f, 7 }, { -hp + 0.45f, 8 }, { -hp - 1.0f, 5 }, { -hp + 1.05f, 6 }, { hp - 0.4f, 6 }, { hp + 0.5f, 7 }, { 0.1f, 5 }, { Mathf.PI - 0.15f, 5 } };
            for (int k = 0; k < 8; k++) Presa(presas[k, 0], (int)presas[k, 1]);
            int[,] babas = { { 152, 12 }, { 163, 18 }, { 168, 8 } };
            for (int k = 0; k < 3; k++)
            {
                int x = babas[k, 0], len = babas[k, 1];
                for (int y = 121; y < 121 + len; y++) { int i = y * W + x; if (SF[i] != 9) break; Set(i, Blood(0.05f + (y - 121) / (float)len * 0.3f)); S[i] = 0.9f; }
            }
            foreach (int s in new[] { -1, 1 })
                for (int y = 100; y < 138; y++)
                {
                    float x = MX + s * (20 + (y - 100) * 0.22f + Mathf.Sin(y * 0.35f) * 0.8f);
                    if (SF[y * W + Rd(x)] != 9) continue;
                    Put(x, y, C(0.22f, 0.02f, 0.03f)); Put(x + s, y, C(0.3f, 0.03f, 0.03f));
                    if (y % 3 == 0) Dente(x - (s < 0 ? 1 : 0), y, -s, 0, true);
                }
            for (int x = 147; x <= 172; x += 3) Dente(x, 123 + ((x - MX) * (x - MX)) / 45, 0, -1, true);
            var saliva = C(0.92f, 0.8f, 0.76f);
            int[,] fios = { { 153, 0 }, { 156, 1 }, { 163, 2 }, { 166, 3 } };
            for (int f = 0; f < 4; f++)
            {
                int x = fios[f, 0], k = fios[f, 1];
                int y0 = 62; while (y0 < 100 && MouthE(x + 0.5f, y0 + 0.5f) >= 1) y0++;
                int y1 = 124; while (y1 > y0 && MouthE(x + 0.5f, y1 + 0.5f) >= 1) y1--;
                y0 += 2; if (k == 2) y1 = y0 + 12;
                for (int y = y0; y < y1; y++) BlendPx(x + Rd(Mathf.Sin(y * 0.25f + k) * 0.7f), y, saliva, 0.55f, 1);
                if (k == 2) Put(x, y1, saliva, 1);
            }
            for (int y = 121; y < 140; y++) BlendPx(157 + Rd(Mathf.Sin(y * 0.3f) * 0.5f), y, C(0.82f, 0.48f, 0.42f), 0.7f, 1);
            Put(157, 140, C(0.85f, 0.55f, 0.5f), 1);
            // braços
            foreach (int s in new[] { -1, 1 })
            {
                var p0 = new Vector2(MX + s * 39, 108); var p1 = new Vector2(MX + s * 46, 121); var p2 = new Vector2(MX + s * 45, 131);
                PixFn contorno = (x, y, i) => { Set(i, C(0.1f, 0.01f, 0.01f)); Z[i] = MZ; SF[i] = 9; };
                PixFn carne = (x, y, i) => { float k = 0.75f + 0.35f * Noise(x * 0.7f, y * 0.7f); Set(i, C(0.46f * k, 0.05f * k, 0.05f * k)); Z[i] = MZ; S[i] = 0.4f; SF[i] = 9; };
                Line(p0, p1, 6, contorno); Line(p1, p2, 5, contorno);
                Line(p0, p1, 4, carne); Line(p1, p2, 3, carne);
                for (int f = 0; f < 4; f++)
                {
                    var tip = new Vector2(MX + s * (41 + f * 2.4f), 141 + (f == 1 || f == 2 ? 2 : 0));
                    Line(new Vector2(MX + s * 45, 132), tip, 1, (x, y, i) => { Set(i, C(0.42f, 0.07f, 0.06f)); Z[i] = MZ; S[i] = 0.3f; });
                    Put(tip.x, tip.y + 1, C(0.88f, 0.82f, 0.62f), 0.8f, MZ); Put(tip.x + s * 0.6f, tip.y + 2, C(0.5f, 0.4f, 0.28f), 0.5f, MZ);
                }
            }

            // caixas dos olhos (para restaurar a cada quadro antes de desenhar pupila e piscada)
            olhos = new Olho[EYES.Length];
            for (int k = 0; k < EYES.Length; k++)
            {
                var e = EYES[k];
                int bx = Mathf.CeilToInt(e[2]) + 3, by = Mathf.CeilToInt(e[3]) + 3;
                int cnt = (2 * by + 1) * (2 * bx + 1);
                var o = new Olho
                {
                    ex = e[0], ey = e[1], erx = e[2], ery = e[3],
                    boxI = new int[cnt], boxR = new float[cnt], boxG = new float[cnt], boxB = new float[cnt], boxE = new float[cnt],
                    nb = 2 + UnityEngine.Random.value * 5, bs = -9, sn = 1 + UnityEngine.Random.value * 3,
                };
                int n = 0;
                for (int y = (int)e[1] - by; y <= e[1] + by; y++)
                    for (int x = (int)e[0] - bx; x <= e[0] + bx; x++)
                    {
                        int i = y * W + x;
                        o.boxI[n] = i; o.boxR[n] = R[i]; o.boxG[n] = G[i]; o.boxB[n] = B[i]; o.boxE[n] = EM[i]; n++;
                    }
                olhos[k] = o;
            }
        }

        void Dente(float x, float y, float ix, float iy, bool big)
        {
            if (Rnd() < 0.08f) return;
            bool rot = Rnd() < 0.12f; Vector3 bse = rot ? C(0.45f, 0.3f, 0.14f) : C(0.9f, 0.82f, 0.58f); float v = 0.8f + Rnd() * 0.25f;
            var c = bse * v; var cd = C(c.x * 0.62f, c.y * 0.58f, c.z * 0.5f);
            int x0 = Rd(x), y0 = Rd(y);
            if (big) { Put(x0, y0, c, 0.85f); Put(x0 + 1, y0, c, 0.6f); Put(x0, y0 + 1, cd); Put(x0 + 1, y0 + 1, cd); Put(x0 + 0.5f + ix * 1.7f, y0 + 0.5f + iy * 1.7f, cd); }
            else { Put(x0, y0, c, 0.7f); Put(x0 + ix * 1.2f, y0 + iy * 1.2f, cd); }
        }

        void Anel(float escala, float espaco, bool big)
        {
            var pts = new Vector2[601];
            for (int k = 0; k <= 600; k++)
            {
                float th = k / 600f * Mathf.PI * 2, y = MY + MRY * escala * Mathf.Sin(th);
                pts[k] = new Vector2(MX + Mrx(y) * escala * Mathf.Cos(th), y);
            }
            float acc = espaco;
            for (int k = 1; k < pts.Length; k++)
            {
                acc += Vector2.Distance(pts[k], pts[k - 1]);
                if (acc >= espaco)
                {
                    acc = 0; var p = pts[k]; float l = Mathf.Sqrt((MX - p.x) * (MX - p.x) + (MY - p.y) * (MY - p.y)); if (l == 0) l = 1;
                    Dente(p.x - 0.5f, p.y - 0.5f, (MX - p.x) / l, (MY - p.y) / l, big);
                }
            }
        }

        void Presa(float th, int len)
        {
            float y = MY + MRY * 0.92f * Mathf.Sin(th), x = MX + Mrx(y) * 0.92f * Mathf.Cos(th);
            float l = Mathf.Sqrt((MX - x) * (MX - x) + (MY - y) * (MY - y)); if (l == 0) l = 1;
            float ix = (MX - x) / l, iy = (MY - y) / l; bool horiz = Mathf.Abs(ix) > Mathf.Abs(iy);
            for (int k = 0; k < len; k++)
            {
                var c = k >= len - 2 ? C(0.55f, 0.08f, 0.06f) : C(0.92f, 0.86f, 0.64f) * (1 - (float)k / len * 0.25f);
                Put(x + ix * k, y + iy * k, c, 0.9f);
                if (k < len * 0.45f) Put(x + ix * k + (horiz ? 0 : 1), y + iy * k + (horiz ? 1 : 0), C(c.x * 0.7f, c.y * 0.65f, c.z * 0.55f));
            }
        }

        void ConstruirPaleta()
        {
            string[] hex =
            {
                "050304", "0c0607", "150909", "1f0c0c", "2a0a0b", "3d0d0e", "521012", "6b1214", "851417", "a3191b", "c02a23", "d9473a",
                "e0705f", "f09a86", "2b1a10", "3f2616", "573520", "70472a", "8c5c38", "a87748", "c9b27a", "e3d29a", "f4ead0",
                "262626", "3a3836", "55524d", "7a756b", "2f3328", "4a5040", "fff6e0",
            };
            PAL = new Color32[hex.Length];
            for (int p = 0; p < hex.Length; p++)
                PAL[p] = new Color32(Convert.ToByte(hex[p].Substring(0, 2), 16), Convert.ToByte(hex[p].Substring(2, 2), 16), Convert.ToByte(hex[p].Substring(4, 2), 16), 255);
            LUT = new byte[32768];
            for (int r = 0; r < 32; r++)
                for (int g = 0; g < 32; g++)
                    for (int b = 0; b < 32; b++)
                    {
                        float cr = (r + 0.5f) * 8, cg = (g + 0.5f) * 8, cb = (b + 0.5f) * 8; int best = 0; float bd = 1e12f;
                        for (int p = 0; p < PAL.Length; p++)
                        {
                            var q = PAL[p];
                            float d = 3 * (q.r - cr) * (q.r - cr) + 4 * (q.g - cg) * (q.g - cg) + 2 * (q.b - cb) * (q.b - cb);
                            if (d < bd) { bd = d; best = p; }
                        }
                        LUT[(r << 10) | (g << 5) | b] = (byte)best;
                    }
        }

        // ================================================================== quadro (todo frame)

        static readonly int[] Bayer = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
        readonly System.Random aleatorio = new System.Random();
        float Rand() => (float)aleatorio.NextDouble();

        static int Q(float v) => v < 0 ? 0 : v > 1 ? 31 : (int)(v * 31.99f);

        void Quadro(float t)
        {
            // mira: mouse sobre a imagem, ou passeio automático
            float tx, ty;
            if (LerMouse(out var m)) { tx = m.x; ty = m.y; }
            else { tx = 160 + Mathf.Sin(t * 0.31f) * 55 + Mathf.Sin(t * 0.83f) * 10; ty = 86 + Mathf.Sin(t * 0.47f) * 26; }
            mira.x += (tx - mira.x) * 0.1f; mira.y += (ty - mira.y) * 0.1f;
            float lx = mira.x + Mathf.Sin(t * 1.7f) * 1.2f + Mathf.Sin(t * 0.53f) * 2.2f, ly = mira.y + Mathf.Cos(t * 1.3f) * 1 + Mathf.Sin(t * 0.41f) * 1.6f;

            float I = 1;
            if (piscar)
            {
                if (fAte > t) I = Rand() < 0.3f ? 0.05f : 0.35f + Rand() * 0.65f;
                else { I = 0.97f + 0.03f * Mathf.Sin(t * 23) * Mathf.Sin(t * 7.1f); if (Rand() < 0.0025f) fAte = t + 0.15f + Rand() * 0.35f; }
            }
            float R0e = 30 * tamanhoFeixe;

            foreach (var eb in olhos)
            {
                float ex = eb.ex, ey = eb.ey, erx = eb.erx, ery = eb.ery; bool big = erx > 3;
                for (int n = 0; n < eb.boxI.Length; n++) { int i = eb.boxI[n]; R[i] = eb.boxR[n]; G[i] = eb.boxG[n]; B[i] = eb.boxB[n]; EM[i] = eb.boxE[n]; }
                if (t > eb.nb) { eb.bs = t; eb.nb = t + (big ? 2.5f : 1.2f) + Rand() * 6; }
                if (t > eb.sn) { eb.sx = Rd((Rand() - 0.5f) * 4); eb.sy = Rd((Rand() - 0.5f) * 2); eb.se = t + 0.1f + Rand() * 0.3f; eb.sn = t + 0.8f + Rand() * 3.5f; }
                int ox = olhosSeguem ? Rd((lx - ex - 0.5f) / 18) : 0, oy = olhosSeguem ? Rd((ly - ey - 0.5f) / 26) : 0;
                if (t < eb.se) { ox += (int)eb.sx; oy += (int)eb.sy; }
                float ddx0 = lx - ex - 0.5f, ddy0 = ly - ey - 0.5f;
                bool lit = I > 0.3f && Mathf.Sqrt(ddx0 * ddx0 + ddy0 * ddy0) < R0e;
                var blk = C(0.02f, 0.01f, 0.01f);
                if (!big) { ox = Mathf.Clamp(ox, -1, 1); for (int dy = -2; dy <= 2; dy++) Pupila((int)ex + ox, (int)ey + dy, blk, 0.05f); }
                else
                {
                    ox = Mathf.Clamp(ox, -2, 2); oy = Mathf.Clamp(oy, -1, 1);
                    int cx = (int)ex + ox, cy = (int)ey + oy;
                    for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) Pupila(cx + dx, cy + dy, C(0.4f, 0.04f, 0.04f), 0.35f);
                    Pupila(cx, cy, blk, 0.05f);
                    if (!lit) { Pupila(cx - 1, cy, blk, 0.05f); Pupila(cx + 1, cy, blk, 0.05f); Pupila(cx, cy - 1, blk, 0.05f); Pupila(cx, cy + 1, blk, 0.05f); }
                }
                float bp = (t - eb.bs) / 0.22f, blink = bp >= 0 && bp <= 1 ? 1 - Mathf.Abs(bp * 2 - 1) : 0;
                if (blink > 0)
                    foreach (int i in eb.boxI)
                    {
                        int y = i / W;
                        if (EM[i] > 0 && y < ey - ery + blink * (ery * 2 + 1.5f)) { Set(i, C(0.5f, 0.06f, 0.05f)); EM[i] = 0; }
                    }
            }

            float R0 = 30 * tamanhoFeixe, inv = 1 / R0;
            for (int y = 0; y < H; y++)
            {
                int linhaTex = (H - 1 - y) * W;      // a textura do Unity começa embaixo
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    float ddx = (x + 0.5f - lx) * inv, ddy = (y + 0.5f - ly) * inv, d2 = ddx * ddx + ddy * ddy, dd = Mathf.Sqrt(d2);
                    float bl = 0;
                    if (dd < 2.2f)
                    {
                        bl = 0.24f * (1 - Ss(0.95f, 2.1f, dd));
                        if (dd < 1.05f) bl += 0.85f * (1 - Ss(0.82f, 1.03f, dd)) * (1 - 0.22f * d2) + 0.12f * Mathf.Exp(-(dd - 0.86f) * (dd - 0.86f) * 300) - 0.06f * Mathf.Exp(-d2 * 40);
                        float a = 1.35f / Z[i]; if (a > 1.25f) a = 1.25f; bl *= a * I;
                    }
                    float L = 0.06f + bl;
                    float Lq = dither ? Mathf.Floor(L * 9 + Bayer[(y & 3) * 4 + (x & 3)] / 16f) / 9 : L;
                    float r = R[i] * Lq * 1.55f, g = G[i] * Lq * 1.4f, b = B[i] * Lq * 1.2f;
                    float sp = S[i] * Mathf.Max(0, bl - 0.55f) * 1.6f; r += sp; g += sp * 0.9f; b += sp * 0.75f;
                    if (EM[i] > 0) { float e = EM[i] * (I < 0.3f ? 0.7f : 0.42f) * (1 - Mathf.Min(1, bl * 1.3f)) * (0.8f + 0.2f * Mathf.Sin(t * 1.3f)); r += e; g += e * 0.62f; b += e * 0.2f; }
                    float nz = (Rand() - 0.5f) * 0.035f; r += nz; g += nz; b += nz;
                    px[linhaTex + x] = PAL[LUT[(Q(r) << 10) | (Q(g) << 5) | Q(b)]];
                }
            }

            for (int k = 0; k < motes.Length; k++)
            {
                var m2 = motes[k];
                m2.x += m2.vx + Mathf.Sin(t * 0.5f + m2.p) * 0.03f; m2.y += m2.vy + Mathf.Cos(t * 0.4f + m2.p) * 0.02f;
                if (m2.x < 0) m2.x += W; if (m2.x >= W) m2.x -= W; if (m2.y < 0) m2.y += H; if (m2.y >= H) m2.y -= H;
                motes[k] = m2;
                float mdx = m2.x - lx, mdy = m2.y - ly; float md = Mathf.Sqrt(mdx * mdx + mdy * mdy) * inv; if (md >= 1) continue;
                int o = (H - 1 - (int)m2.y) * W + (int)m2.x; float add = 0.3f * (1 - md) * I;
                var c0 = px[o];
                float rr = Mathf.Min(1, c0.r / 255f + add), gg = Mathf.Min(1, c0.g / 255f + add * 0.9f), bb = Mathf.Min(1, c0.b / 255f + add * 0.7f);
                px[o] = PAL[LUT[(Q(rr) << 10) | (Q(gg) << 5) | Q(bb)]];
            }

            tex.SetPixels32(px);
            tex.Apply(false);
        }

        void Pupila(int x, int y, Vector3 c, float e)
        {
            int i = y * W + x;
            if (i < 0 || i >= N || EM[i] < 0.5f) return;
            Set(i, c); EM[i] = e;
        }

        bool LerMouse(out Vector2 p)
        {
            p = default;
            var mouse = Mouse.current;
            if (mouse == null || imagem == null) return false;
            var rt = imagem.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mouse.position.ReadValue(), null, out var local)) return false;
            var rect = rt.rect;
            float u = (local.x - rect.xMin) / rect.width, v = (local.y - rect.yMin) / rect.height;
            if (u < 0 || u > 1 || v < 0 || v > 1) return false;
            p = new Vector2(u * W, (1 - v) * H);
            return true;
        }
    }
}
