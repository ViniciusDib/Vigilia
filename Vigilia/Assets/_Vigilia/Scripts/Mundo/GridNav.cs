using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Navegação simples em grade para os inimigos (A*). Visão de cima: o mapa vira uma grade de células;
    /// célula com parede é bloqueada; células do quarto são proibidas para inimigos (P-06).
    /// Reconstruir() é chamado quando uma porta abre ou fecha.
    ///
    /// Por que não o NavMesh do Unity? O NavMesh é 3D; para 2D precisaria de pacote extra.
    /// Uma grade com A* é curta, fácil de entender e roda bem em WebGL.
    /// </summary>
    public class GridNav : MonoBehaviour
    {
        [Tooltip("Canto inferior esquerdo da área navegável.")]
        public Vector2 origem = new Vector2(-30, -22);
        [Tooltip("Tamanho da área navegável em unidades.")]
        public Vector2 tamanho = new Vector2(60, 38);
        public float celula = 0.5f;
        [Tooltip("Meia largura do inimigo: células mais perto que isso de uma parede ficam bloqueadas.")]
        public float raioAgente = 0.3f;
        public LayerMask paredes;

        int largura, altura;
        bool[] livre;
        bool[] proibida;
        bool pronto;

        void Start() => Reconstruir();

        public void Reconstruir()
        {
            largura = Mathf.CeilToInt(tamanho.x / celula);
            altura = Mathf.CeilToInt(tamanho.y / celula);
            livre = new bool[largura * altura];
            proibida = new bool[largura * altura];
            Physics2D.SyncTransforms();

            Sala[] salas = null;
            if (Jogo.I != null && Jogo.I.mapa != null) salas = Jogo.I.mapa.GetComponentsInChildren<Sala>(true);

            var caixa = Vector2.one * (celula + raioAgente * 2f) * 0.9f;
            for (int y = 0; y < altura; y++)
                for (int x = 0; x < largura; x++)
                {
                    var c = Centro(x, y);
                    int i = y * largura + x;
                    livre[i] = Physics2D.OverlapBox(c, caixa, 0f, paredes) == null;
                    if (salas != null)
                        foreach (var s in salas)
                            if (s.ProibidaParaInimigos && s.area.Contains(c)) proibida[i] = true;
                }
            pronto = true;
        }

        public Vector2 Centro(int x, int y) => origem + new Vector2((x + 0.5f) * celula, (y + 0.5f) * celula);

        bool Celula(Vector2 p, out int x, out int y)
        {
            x = Mathf.FloorToInt((p.x - origem.x) / celula);
            y = Mathf.FloorToInt((p.y - origem.y) / celula);
            return x >= 0 && y >= 0 && x < largura && y < altura;
        }

        bool Passavel(int x, int y, bool respeitarProibida)
        {
            if (x < 0 || y < 0 || x >= largura || y >= altura) return false;
            int i = y * largura + x;
            return livre[i] && !(respeitarProibida && proibida[i]);
        }

        /// <summary>Verdadeiro se o ponto está numa área onde inimigos não entram (quarto).</summary>
        public bool EmAreaProibida(Vector2 p) => pronto && Celula(p, out int x, out int y) && proibida[y * largura + x];

        /// <summary>Procura a célula passável mais próxima (para quando o alvo está dentro de uma parede ou do quarto).</summary>
        bool MaisProxima(Vector2 p, bool respeitarProibida, out int rx, out int ry)
        {
            Celula(p, out int cx, out int cy);
            for (int r = 0; r < 12; r++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                        if (Passavel(cx + dx, cy + dy, respeitarProibida)) { rx = cx + dx; ry = cy + dy; return true; }
                    }
            rx = ry = 0;
            return false;
        }

        // ---------- A* ----------
        float[] custo;
        int[] veioDe;
        int[] versao;
        int versaoAtual;
        readonly List<(float f, int i)> heap = new List<(float, int)>();

        static readonly int[] DX = { 1, -1, 0, 0, 1, 1, -1, -1 };
        static readonly int[] DY = { 0, 0, 1, -1, 1, -1, 1, -1 };

        /// <summary>Calcula um caminho de 'de' até 'para'. Devolve falso se não há caminho.</summary>
        public bool Caminho(Vector2 de, Vector2 para, List<Vector2> saida, bool respeitarProibida = true)
        {
            saida.Clear();
            if (!pronto) return false;
            if (!MaisProxima(de, respeitarProibida, out int sx, out int sy)) return false;
            if (!MaisProxima(para, respeitarProibida, out int tx, out int ty)) return false;

            int n = largura * altura;
            if (custo == null || custo.Length != n) { custo = new float[n]; veioDe = new int[n]; versao = new int[n]; }
            versaoAtual++;
            heap.Clear();

            int ini = sy * largura + sx, fim = ty * largura + tx;
            Visitar(ini, 0f, -1);
            Push(Heur(sx, sy, tx, ty), ini);

            int expandidos = 0;
            while (heap.Count > 0 && expandidos < 6000)
            {
                var (_, atual) = Pop();
                if (atual == fim) break;
                expandidos++;
                int ax = atual % largura, ay = atual / largura;
                for (int k = 0; k < 8; k++)
                {
                    int nx = ax + DX[k], ny = ay + DY[k];
                    if (!Passavel(nx, ny, respeitarProibida)) continue;
                    if (k >= 4 && (!Passavel(ax + DX[k], ay, respeitarProibida) || !Passavel(ax, ay + DY[k], respeitarProibida))) continue; // sem cortar quina
                    int ni = ny * largura + nx;
                    float c = custo[atual] + (k >= 4 ? 1.4142f : 1f);
                    if (versao[ni] == versaoAtual && custo[ni] <= c) continue;
                    Visitar(ni, c, atual);
                    Push(c + Heur(nx, ny, tx, ty), ni);
                }
            }
            if (versao[fim] != versaoAtual) return false;

            for (int i = fim; i != -1; i = veioDe[i]) saida.Add(Centro(i % largura, i / largura));
            saida.Reverse();
            if (saida.Count > 0) saida[saida.Count - 1] = para;
            return true;
        }

        void Visitar(int i, float c, int de) { versao[i] = versaoAtual; custo[i] = c; veioDe[i] = de; }

        static float Heur(int ax, int ay, int bx, int by)
        {
            int dx = Mathf.Abs(ax - bx), dy = Mathf.Abs(ay - by);
            return (dx + dy) + (1.4142f - 2f) * Mathf.Min(dx, dy);
        }

        void Push(float f, int i)
        {
            heap.Add((f, i));
            int c = heap.Count - 1;
            while (c > 0)
            {
                int p = (c - 1) / 2;
                if (heap[p].f <= heap[c].f) break;
                (heap[p], heap[c]) = (heap[c], heap[p]);
                c = p;
            }
        }

        (float f, int i) Pop()
        {
            var topo = heap[0];
            int ult = heap.Count - 1;
            heap[0] = heap[ult];
            heap.RemoveAt(ult);
            int c = 0;
            while (true)
            {
                int l = c * 2 + 1, r = l + 1, m = c;
                if (l < heap.Count && heap[l].f < heap[m].f) m = l;
                if (r < heap.Count && heap[r].f < heap[m].f) m = r;
                if (m == c) break;
                (heap[m], heap[c]) = (heap[c], heap[m]);
                c = m;
            }
            return topo;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(origem + tamanho / 2f, tamanho);
            if (!pronto) return;
            for (int y = 0; y < altura; y++)
                for (int x = 0; x < largura; x++)
                {
                    int i = y * largura + x;
                    if (livre[i] && !proibida[i]) continue;
                    Gizmos.color = proibida[i] ? new Color(0, 1, 0, 0.25f) : new Color(1, 0, 0, 0.25f);
                    Gizmos.DrawCube(Centro(x, y), Vector3.one * celula * 0.9f);
                }
        }
    }
}
