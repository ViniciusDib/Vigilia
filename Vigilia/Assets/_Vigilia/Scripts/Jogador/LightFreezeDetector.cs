using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Decide, pela LÓGICA e não pelos pixels, se cada inimigo está iluminado (GDD 7.1, Figura 4).
    /// As quatro condições precisam ser verdadeiras ao mesmo tempo:
    ///   1. distância ≤ alcance da lanterna
    ///   2. ângulo entre a direção da lanterna e a direção do inimigo ≤ meia abertura
    ///   3. raycast de Elias até o inimigo não encontra parede
    ///   4. lanterna ligada e bateria > 0
    /// Avisa cada inimigo com DefinirIluminado (que emite OnIluminado / OnSaiuDaLuz).
    /// </summary>
    public class LightFreezeDetector : MonoBehaviour
    {
        public FlashlightController lanterna;
        public LayerMask paredes;

        public int IluminadosAgora { get; private set; }

        public bool Iluminado(Vector2 alvo)
        {
            if (lanterna == null || !lanterna.Acesa) return false;                          // 4
            Vector2 origem = lanterna.Origem;
            Vector2 para = alvo - origem;
            float dist = para.magnitude;
            if (dist > lanterna.Alcance) return false;                                       // 1
            if (dist > 0.001f && Vector2.Angle(lanterna.Direcao, para) > lanterna.MeiaAbertura) return false; // 2
            return !Physics2D.Linecast(origem, alvo, paredes);                               // 3
        }

        void LateUpdate()
        {
            int n = 0;
            bool noite = Jogo.I != null && Jogo.I.EhNoite;
            foreach (var e in EnemyAI.Ativos)
            {
                bool lit = noite && Iluminado(e.Posicao);
                e.DefinirIluminado(lit);
                if (lit) n++;
            }
            IluminadosAgora = n;
        }
    }
}
