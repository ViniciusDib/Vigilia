using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Vigilia
{
    /// <summary>
    /// Uma sala do mapa. Guarda a área (retângulo), a temperatura de cada noite (GDD 6.3, atributo TemperaturaSala)
    /// e se é o quarto (porto seguro, P-06) ou a sala secreta.
    /// TODO(Q-10): salas e temperaturas finais são decisão do grupo; o construtor da cena usa a sugestão do GDD.
    /// </summary>
    public class Sala : MonoBehaviour
    {
        public string id;
        public string nome;
        [Tooltip("Área da sala em coordenadas do mundo (canto inferior esquerdo + tamanho).")]
        public Rect area;
        [Tooltip("Temperatura em cada noite (índice 0 = noite 1).")]
        public TemperaturaSala[] temperaturaPorNoite = { TemperaturaSala.Morna, TemperaturaSala.Morna, TemperaturaSala.Morna };

        [Header("Regras especiais")]
        [Tooltip("Quarto de Elias: quente só com cobertor (e caldeira funcionando, Q-17). Inimigos não entram (P-06).")]
        public bool ehQuarto;
        [Tooltip("Se a caldeira falhou (noite 3), esta sala cai de Quente para Morna.")]
        public bool dependeDaCaldeira;
        public bool ehSalaSecreta;
        [Tooltip("Lado de fora da porta principal: chegar aqui durante a fuga termina o jogo.")]
        public bool ehSaida;

        [Header("Visual")]
        [Tooltip("Luz alaranjada que acende quando a sala está quente à noite (rubrica 3).")]
        public Light2D luzQuente;
        public SpriteRenderer piso;
        public Color corPisoFria = new Color(0.55f, 0.62f, 0.72f);
        public Color corPisoMorna = new Color(0.62f, 0.6f, 0.55f);
        public Color corPisoQuente = new Color(0.72f, 0.58f, 0.45f);

        bool noite;
        int noiteAtual = 1;
        TemperaturaSala ultimaAplicada = (TemperaturaSala)(-1);

        public bool Contem(Vector2 p) => area.Contains(p);

        public bool ProibidaParaInimigos => ehQuarto && Jogo.Cfg.p06InimigosNaoEntramNoQuarto;

        /// <summary>Temperatura agora, já considerando cobertor e caldeira.</summary>
        public TemperaturaSala TemperaturaAtual
        {
            get
            {
                var w = Jogo.I != null ? Jogo.I.Mundo : null;
                if (ehQuarto && w != null)
                    return w.temCobertor && !w.caldeiraFalhou ? TemperaturaSala.Quente : TemperaturaSala.Fria;
                var t = BalanceConfig.PorNoite(temperaturaPorNoite, noiteAtual);
                if (dependeDaCaldeira && w != null && w.caldeiraFalhou && t == TemperaturaSala.Quente)
                    t = TemperaturaSala.Morna;
                return t;
            }
        }

        public void Aplicar(bool ehNoite, int n)
        {
            noite = ehNoite;
            noiteAtual = n;
            ultimaAplicada = (TemperaturaSala)(-1);
            AtualizarVisual();
        }

        void Update() => AtualizarVisual();

        void AtualizarVisual()
        {
            var t = TemperaturaAtual;
            if (t == ultimaAplicada) return;
            ultimaAplicada = t;
            if (luzQuente != null) luzQuente.enabled = noite && t == TemperaturaSala.Quente;
            if (piso != null)
                piso.color = t == TemperaturaSala.Fria ? corPisoFria : t == TemperaturaSala.Quente ? corPisoQuente : corPisoMorna;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = ehQuarto ? new Color(0.3f, 1f, 0.4f, 0.6f) : new Color(1f, 1f, 1f, 0.25f);
            Gizmos.DrawWireCube(area.center, area.size);
        }
    }
}
