using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Sons provisórios gerados por código (AudioClip.Create funciona em WebGL).
    /// TODO(Q-12): trocar pelos sons finais do grupo; basta apontar os clipes no AudioJogo.
    /// </summary>
    public static class SomProcedural
    {
        public const int Taxa = 22050;

        static AudioClip Criar(string nome, float[] dados)
        {
            var c = AudioClip.Create(nome, dados.Length, 1, Taxa, false);
            c.SetData(dados, 0);
            return c;
        }

        static float[] Buffer(float seg) => new float[Mathf.Max(1, (int)(seg * Taxa))];

        static float Env(float t, float ataque, float dur) => t < ataque ? t / ataque : Mathf.Exp(-(t - ataque) / dur);

        /// <summary>Vento: ruído marrom com rajadas lentas. Faz laço sem emenda (fade cruzado nas pontas).</summary>
        public static AudioClip Vento(float seg)
        {
            var d = Buffer(seg);
            var rnd = new System.Random(11);
            float b = 0f, lento = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                b = (b + ((float)rnd.NextDouble() * 2f - 1f) * 0.02f) * 0.997f;
                lento = 0.55f + 0.45f * Mathf.Sin(t * 0.9f) * Mathf.Sin(t * 0.37f + 1f);
                d[i] = b * 6f * lento;
            }
            Laco(d, 0.5f);
            return Criar("vento", d);
        }

        /// <summary>Batimento: "tum-tum" grave, 1 segundo (o pitch muda a frequência).</summary>
        public static AudioClip Batimento()
        {
            var d = Buffer(1f);
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                float a = Batida(t, 0f) + 0.7f * Batida(t, 0.22f);
                d[i] = a;
            }
            return Criar("batimento", d);
        }

        static float Batida(float t, float ini)
        {
            float x = t - ini;
            if (x < 0f || x > 0.18f) return 0f;
            float f = Mathf.Lerp(70f, 45f, x / 0.18f);
            return Mathf.Sin(2f * Mathf.PI * f * x) * Env(x, 0.005f, 0.05f) * 0.9f;
        }

        /// <summary>Sussurro: ruído filtrado com modulação parecida com sílabas.</summary>
        public static AudioClip Sussurro(float seg, int semente)
        {
            var d = Buffer(seg);
            var rnd = new System.Random(semente);
            float lp = 0f, hp = 0f, prev = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                float n = (float)rnd.NextDouble() * 2f - 1f;
                lp += (n - lp) * 0.35f;
                hp = lp - prev; prev = lp;
                float silabas = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 9f + Mathf.Sin(t * 2.3f) * 3f)), 2f);
                d[i] = hp * 1.8f * silabas;
            }
            Laco(d, 0.2f);
            return Criar("sussurro", d);
        }

        public static AudioClip Passo()
        {
            var d = Buffer(0.12f);
            var rnd = new System.Random(3);
            float lp = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                lp += (((float)rnd.NextDouble() * 2f - 1f) - lp) * 0.25f;
                d[i] = lp * Env(t, 0.003f, 0.03f) * 0.5f;
            }
            return Criar("passo", d);
        }

        public static AudioClip Clique()
        {
            var d = Buffer(0.06f);
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                d[i] = Mathf.Sin(2f * Mathf.PI * 2200f * t) * Env(t, 0.001f, 0.008f) * 0.4f;
            }
            return Criar("clique", d);
        }

        public static AudioClip Pancada()
        {
            var d = Buffer(0.9f);
            var rnd = new System.Random(5);
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                float n = (float)rnd.NextDouble() * 2f - 1f;
                float corpo = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 50f, t) * t);
                float metal = Mathf.Sin(2f * Mathf.PI * 840f * t) * 0.3f + Mathf.Sin(2f * Mathf.PI * 1270f * t) * 0.2f;
                d[i] = (n * Env(t, 0.001f, 0.05f) + corpo * Env(t, 0.002f, 0.15f) + metal * Env(t, 0.002f, 0.3f)) * 0.7f;
            }
            return Criar("pancada", d);
        }

        public static AudioClip Tom(float freq, float seg, float vol, string nome)
        {
            var d = Buffer(seg);
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Env(t, 0.01f, seg * 0.4f) * vol;
            }
            return Criar(nome, d);
        }

        /// <summary>Susto: acorde dissonante agudo + ruído, crescendo rápido.</summary>
        public static AudioClip Stinger()
        {
            var d = Buffer(2.4f);
            var rnd = new System.Random(9);
            float[] f = { 311f, 329.6f, 466f, 493.9f, 698f };
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                float s = 0f;
                foreach (var x in f) s += Mathf.Sin(2f * Mathf.PI * x * t * (1f + 0.004f * Mathf.Sin(t * 30f)));
                float n = (float)rnd.NextDouble() * 2f - 1f;
                float env = Mathf.Min(1f, t / 0.04f) * Mathf.Exp(-t * 1.1f);
                d[i] = Mathf.Clamp((s * 0.16f + n * 0.35f) * env, -1f, 1f);
            }
            return Criar("stinger", d);
        }

        /// <summary>Rabisco de caneta no papel.</summary>
        public static AudioClip Escrita()
        {
            var d = Buffer(1.6f);
            var rnd = new System.Random(21);
            float hp = 0f, prev = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                float n = (float)rnd.NextDouble() * 2f - 1f;
                hp = n - prev; prev = n;
                float traço = Mathf.Max(0f, Mathf.Sin(t * 23f + Mathf.Sin(t * 5f) * 4f));
                d[i] = hp * 0.12f * traço;
            }
            return Criar("escrita", d);
        }

        /// <summary>Ronco grave da caldeira.</summary>
        public static AudioClip Caldeira()
        {
            var d = Buffer(1.8f);
            var rnd = new System.Random(4);
            float lp = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = (float)i / Taxa;
                lp += (((float)rnd.NextDouble() * 2f - 1f) - lp) * 0.03f;
                d[i] = (lp * 3f + Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.3f) * Mathf.Min(1f, t * 3f) * Mathf.Min(1f, (1.8f - t) * 2f);
            }
            return Criar("caldeira", d);
        }

        static void Laco(float[] d, float segFade)
        {
            int n = Mathf.Min(d.Length / 2, (int)(segFade * Taxa));
            for (int i = 0; i < n; i++)
            {
                float a = (float)i / n;
                int j = d.Length - n + i;
                d[i] = d[i] * a + d[j] * (1f - a);
            }
        }
    }
}
