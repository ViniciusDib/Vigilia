using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Vigilia
{
    /// <summary>
    /// Ponto central da cena Jogo: guarda as referências para os sistemas e os dois "cadernos" (WorldState e NpcMemory).
    /// Qualquer script acessa com Jogo.I (ex.: Jogo.I.Mundo.temChave) e a configuração com Jogo.Cfg.
    ///
    /// Por que um ponto central? Para cada sistema não precisar procurar os outros na cena,
    /// e para ficar fácil de ver, num lugar só, tudo o que o jogo tem.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Jogo : MonoBehaviour
    {
        public static Jogo I { get; private set; }

        static BalanceConfig cfgPadrao;
        /// <summary>A configuração ativa. Fora da cena Jogo (menu, testes) usa uma cópia com os valores padrão.</summary>
        public static BalanceConfig Cfg
        {
            get
            {
                if (I != null && I.config != null) return I.config;
                if (cfgPadrao == null) cfgPadrao = ScriptableObject.CreateInstance<BalanceConfig>();
                return cfgPadrao;
            }
        }

        [Header("Configuração")]
        public BalanceConfig config;

        [Header("Sistemas (o construtor da cena preenche; se faltar, é procurado no Awake)")]
        public NightManager noites;
        public PossessionMeter possessao;
        public PlayerController jogador;
        public FlashlightController lanterna;
        public LightFreezeDetector detectorLuz;
        public RoomTemperature temperatura;
        public Notebook caderno;
        public SleepController cama;
        public DialogueSystem dialogo;
        public UIJogo ui;
        public AudioJogo audioJogo;
        public EncontroMonstro encontro;
        public GridNav nav;
        public Light2D luzGlobal;
        public Transform mapa;

        /// <summary>Fatos reais (GDD 8.2). Persistem entre dias e noites.</summary>
        public WorldState Mundo { get; private set; } = new WorldState();
        /// <summary>O que cada NPC lembra. Zerada às 06:00.</summary>
        public NpcMemory Memoria { get; private set; } = new NpcMemory();

        // Travas de fluxo: enquanto alguma estiver ligada, o jogador não age e a banheira não enche.
        public bool Pausado { get; set; }
        public bool EmDialogo { get; set; }
        public bool EmCena { get; set; }

        public bool EhNoite => noites != null && noites.EhNoite;
        public int Noite => Mundo.noiteAtual;
        public bool Travado => Pausado || EmDialogo || EmCena;

        /// <summary>A banheira só enche à noite (P-04), sem travas e com Elias acordado.</summary>
        public bool PossessaoAtiva
        {
            get
            {
                if (Travado || noites == null) return false;
                bool periodo = EhNoite || !Cfg.p04PossessaoNaoSobeDeDia;
                if (!periodo || noites.EmFinal) return false;
                if (jogador != null && (jogador.Estado == EstadoJogador.Dormindo || jogador.Estado == EstadoJogador.Transbordou)) return false;
                return true;
            }
        }

        void Awake()
        {
            I = this;
            if (noites == null) noites = FindAnyObjectByType<NightManager>();
            if (possessao == null) possessao = FindAnyObjectByType<PossessionMeter>();
            if (jogador == null) jogador = FindAnyObjectByType<PlayerController>();
            if (lanterna == null) lanterna = FindAnyObjectByType<FlashlightController>();
            if (detectorLuz == null) detectorLuz = FindAnyObjectByType<LightFreezeDetector>();
            if (temperatura == null) temperatura = FindAnyObjectByType<RoomTemperature>();
            if (caderno == null) caderno = FindAnyObjectByType<Notebook>();
            if (cama == null) cama = FindAnyObjectByType<SleepController>();
            if (dialogo == null) dialogo = FindAnyObjectByType<DialogueSystem>();
            if (ui == null) ui = FindAnyObjectByType<UIJogo>();
            if (audioJogo == null) audioJogo = FindAnyObjectByType<AudioJogo>();
            if (encontro == null) encontro = FindAnyObjectByType<EncontroMonstro>();
            if (nav == null) nav = FindAnyObjectByType<GridNav>();
            AudioListener.volume = SaveJogo.Volume;
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }
    }
}
