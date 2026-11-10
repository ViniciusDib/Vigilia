using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Vigilia
{
    /// <summary>
    /// Lanterna: liga/desliga (F), bateria (D-20) e o cone visual (Light 2D).
    /// IMPORTANTE (GDD 7.1): a luz é só aparência. Quem decide se um inimigo congela é o LightFreezeDetector,
    /// usando os mesmos números (alcanceLanterna e meiaAberturaGraus) que este script aplica na luz.
    /// Escreve WorldState.bateria (dono da bateria, tabela 14.1).
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        public Light2D luz;
        public PlayerController jogador;

        bool ligada = true;
        bool avisouApagou;
        float piscar;

        BalanceConfig Cfg => Jogo.Cfg;
        WorldState Mundo => Jogo.I.Mundo;

        /// <summary>Condição 4 do teste de iluminação: ligada e bateria maior que zero.</summary>
        public bool Acesa => ligada && Mundo.bateria > 0f;
        public Vector2 Origem => jogador != null ? jogador.Posicao : (Vector2)transform.position;
        public Vector2 Direcao { get; private set; } = Vector2.up;
        public float Alcance => Cfg.alcanceLanterna;
        public float MeiaAbertura => Cfg.meiaAberturaGraus;

        public void PrepararNoite()
        {
            ligada = Mundo.bateria > 0f;
            avisouApagou = false;
        }

        /// <summary>Uma pilha encontrada no mapa (GDD 7.2).</summary>
        public void Recarregar()
        {
            Mundo.bateria = Mathf.Min(100f, Mundo.bateria + Cfg.recargaPilha);
            avisouApagou = false;
        }

        void Update()
        {
            var j = Jogo.I;
            if (j == null || jogador == null) return;

            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame && !j.Travado)
            {
                ligada = !ligada;
                j.audioJogo?.Tocar(SomId.Lanterna);
            }

            // Bateria só gasta à noite (de dia a luz não importa para o jogo).
            if (Acesa && j.EhNoite && !j.Travado)
            {
                Mundo.bateria = Mathf.Max(0f, Mundo.bateria - Cfg.consumoBateria * Time.deltaTime);
                if (Mundo.bateria <= 0f && !avisouApagou)
                {
                    avisouApagou = true;
                    j.ui.Mensagem(Idioma.T("A lanterna apagou. Procure uma pilha.", "The flashlight died. Find a battery."));
                }
            }

            // Tremor da mão cresce com a possessão (P-10).
            float nivel = j.possessao != null ? j.possessao.Nivel01 : 0f;
            float tremor = (Mathf.PerlinNoise(Time.time * 7f, 0.3f) - 0.5f) * 2f * Cfg.tremorLanternaMax * nivel * nivel;
            Vector2 d = jogador.Direcao;
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + tremor;
            Direcao = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));

            AtualizarLuz(j);
        }

        void AtualizarLuz(Jogo j)
        {
            if (luz == null) return;
            luz.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Direcao.y, Direcao.x) * Mathf.Rad2Deg - 90f);
            luz.pointLightOuterRadius = Cfg.alcanceLanterna;
            luz.pointLightInnerRadius = Cfg.alcanceLanterna * 0.15f;
            luz.pointLightOuterAngle = Cfg.meiaAberturaGraus * 2f;
            luz.pointLightInnerAngle = Cfg.meiaAberturaGraus * 1.3f;
            luz.color = Cfg.corLanterna;

            float intensidade = Cfg.intensidadeLanterna;
            bool fraca = Mundo.bateria < Cfg.bateriaFraca;
            if (fraca)
            {
                piscar -= Time.deltaTime;
                if (piscar <= 0f) piscar = Random.Range(0.05f, 0.4f);
                intensidade *= piscar < 0.08f ? 0.15f : Mathf.Lerp(0.55f, 1f, Mundo.bateria / Cfg.bateriaFraca);
            }
            luz.intensity = intensidade;
            luz.enabled = Acesa;
        }
    }
}
