using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// TODOS os números de balanceamento do jogo (GDD 14.2). Regra do projeto: nenhum número mágico no código.
    /// Os valores abaixo são os iniciais de TESTE (Q-08) e devem ser calibrados jogando, uma alavanca por vez.
    /// Campos marcados [EXTRA] não estão na tabela 14.2: foram necessários para implementar e também são TESTE.
    /// Campos marcados P-xx são PROPOSTAS (GDD 16): ficam atrás de um bool para o grupo poder desligar.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "Vigilia/BalanceConfig")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("Banheira (possessao) · GDD 6")]
        [Tooltip("Limite da banheira. 100 = possuído.")]
        public float maxPossessao = 100f;
        [Tooltip("Quanto possessao sobe por segundo acordado à noite.")]
        public float taxaBase = 0.10f;
        public float multFria = 2.0f;
        public float multMorna = 1.0f;
        public float multQuente = 0.5f;
        [Tooltip("Multiplicador da vazão depois de escrever no caderno.")]
        public float multLucido = 0.5f;
        [Tooltip("Segundos de estado lúcido depois de escrever.")]
        public float duracaoLucido = 60f;
        [Tooltip("Soma de um toque de possuído.")]
        public float pulsoToque = 25f;
        [Tooltip("Soma de arrombar a porta com o pé de cabra (P-03).")]
        public float pulsoBarulho = 10f;
        [Tooltip("Ao dormir: possessao = possessao × fatorSono (0.4 esvazia 60%).")]
        public float fatorSono = 0.4f;

        [Header("Inimigos · GDD 9.3 e 14.2")]
        public float tempoRecuoToque = 2.5f;
        public float raioDeteccao = 6f;
        public float raioPerseguicao = 8f;
        public float velocidadePatrulha = 1.5f;
        public float velocidadePerseguicao = 2.2f;
        [Tooltip("Multiplicador de velocidade por noite (1, 2, 3).")]
        public float[] multVelocidadeNoite = { 0.8f, 1.0f, 1.2f };
        [Tooltip("[EXTRA] Paciente velho é mais rápido (glossário).")]
        public float multVelocidadePacienteVelho = 1.25f;
        [Tooltip("[EXTRA] Paciente velho ouve barulho de mais longe.")]
        public float multRaioBarulhoPacienteVelho = 1.5f;
        [Tooltip("[EXTRA] Distância em que o possuído encosta em Elias.")]
        public float distanciaToque = 0.6f;
        public float tempoAtordoado = 8f;

        [Header("Lanterna e bateria · GDD 7")]
        public float alcanceLanterna = 8f;
        public float meiaAberturaGraus = 28f;
        [Tooltip("Pontos de bateria por segundo com a lanterna ligada.")]
        public float consumoBateria = 0.4f;
        public float recargaPilha = 40f;
        [Tooltip("[EXTRA] Abaixo disso a lanterna pisca.")]
        public float bateriaFraca = 15f;

        [Header("Alavancas por noite · GDD 10 (índice 0 = noite 1)")]
        [Tooltip("Possuídos por noite. Decisão do grupo (04/11): noite 2 com mais inimigos (GDD: 2/4/6).")]
        public int[] inimigosPorNoite = { 2, 6, 8 };
        public int[] esconderijosPorNoite = { 5, 4, 3 };
        public int[] pilhasPorNoite = { 4, 3, 2 };
        [Tooltip("Informativo: as salas quentes de cada noite são definidas em cada Sala (2, 2, 1).")]
        public int[] salasQuentesPorNoite = { 2, 2, 1 };

        [Header("Jogador [EXTRA]")]
        public float velocidadeJogador = 3.2f;
        public float velocidadeEscondidoSaida = 1.2f;
        [Tooltip("Tempo travado depois de um toque (estado Tocado).")]
        public float tempoTocado = 0.6f;
        [Tooltip("Segundos escrevendo no caderno.")]
        public float tempoEscrita = 2f;
        [Tooltip("Segundos segurando E para abrir a porta com a chave (a chave 'gasta tempo', GDD 3.4).")]
        public float tempoAbrirComChave = 4f;
        [Tooltip("Raio em que o barulho do pé de cabra atrai inimigos.")]
        public float raioBarulhoPeDeCabra = 14f;
        [Tooltip("Distância máxima para interagir com objetos.")]
        public float alcanceInteracao = 1.3f;

        [Header("Esconderijos · Q-07 (ABERTO, valores provisórios)")]
        [Tooltip("TODO(Q-07): segundos escondido antes de um inimigo próximo encontrar Elias.")]
        public float tempoMaxEscondido = 12f;

        [Header("Ataque · Q-04 (ABERTO, sugestão do GDD 7.3)")]
        [Tooltip("TODO(Q-04): alcance do sedativo e do golpe letal.")]
        public float alcanceAtaque = 1.8f;

        [Header("Caldeira da noite 3 · Q-17 (sugestão do GDD 6.7)")]
        [Tooltip("TODO(Q-17): se ligado, na noite 3 a caldeira falha e o quarto fica frio até Elias religá-la.")]
        public bool noite3CaldeiraFalha = false;   // substituído pela fuga da noite 3 (decisão de 04/11)
        public float tempoReligarCaldeira = 3f;

        [Header("Matéria · GDD 11.1 (faixas TESTE)")]
        public FaixasMateria faixasMateria = new FaixasMateria();

        [Header("Propostas (GDD 16) · desligue para testar alternativas")]
        [Tooltip("P-02: transbordar na noite 3 leva ao Final C (não repete).")]
        public bool p02TransbordoNoite3VaiAoFinalC = true;
        [Tooltip("P-03: arrombar soma pulsoBarulho e atrai inimigos.")]
        public bool p03PeDeCabraFazBarulho = true;
        [Tooltip("P-04: possessao não sobe de dia.")]
        public bool p04PossessaoNaoSobeDeDia = true;
        [Tooltip("P-05: dormir exige quarto quente.")]
        public bool p05DormirExigeQuartoQuente = true;
        [Tooltip("P-05: dormir exige a porta do quarto aberta.")]
        public bool p05DormirExigePortaAberta = true;
        [Tooltip("P-06: inimigos não entram no quarto.")]
        public bool p06InimigosNaoEntramNoQuarto = true;
        [Tooltip("P-07: evidências só contam para a matéria depois de anotadas.")]
        public bool p07EvidenciaSoContaAnotada = true;
        [Tooltip("P-10 / Q-13: barra de possessão no HUD (por padrão escondida; F1 mostra o painel de debug).")]
        public bool mostrarBarraPossessao = false;
        [Tooltip("P-13: guardar o checkpoint em PlayerPrefs para 'Continuar'.")]
        public bool p13SalvarCheckpoint = true;

        [Header("Cena fixa do monstro (encontro / jumpscare)")]
        [Tooltip("Decisão do grupo (04/11): o monstro aparece só na sala secreta.")]
        public bool encontroAoTransbordar = false;
        public bool encontroNaSalaSecreta = true;
        public float duracaoEncontro = 5.5f;

        [Header("Noite 3: sala secreta e fuga (decisão do grupo, 04/11)")]
        [Tooltip("Na noite 3 a porta da sala secreta se abre; ao entrar, o monstro aparece e começa a fuga para fora do sanatório.")]
        public bool noite3Fuga = true;
        [Tooltip("Possuídos extras que surgem quando a fuga começa.")]
        public int fugaInimigosExtras = 4;
        [Tooltip("Multiplicador de velocidade dos possuídos durante a fuga.")]
        public float fugaMultVelocidade = 1.1f;
        [Tooltip("Multiplicador dos raios de detecção e perseguição durante a fuga.")]
        public float fugaMultRaio = 1.5f;
        [Tooltip("Multiplicador da vazão da banheira durante a fuga.")]
        public float fugaMultPossessao = 1.2f;
        [Tooltip("Os possuídos extras só surgem a pelo menos esta distância de Elias (dá tempo de reagir).")]
        public float fugaDistanciaMinimaExtras = 12f;
        public Color corLuzFuga = new Color(0.9f, 0.15f, 0.12f);

        [Header("Música")]
        [Range(0f, 1f)] public float volumeMusica = 0.85f;

        [Header("Luz e visual [EXTRA]")]
        public float intensidadeLuzGlobalDia = 0.65f;
        public float intensidadeLuzGlobalNoite = 0.035f;
        public Color corLuzGlobalDia = new Color(0.78f, 0.86f, 1f);
        public Color corLuzGlobalNoite = new Color(0.35f, 0.45f, 0.8f);
        public float intensidadeLanterna = 1.25f;
        public Color corLanterna = new Color(1f, 0.93f, 0.78f);
        [Tooltip("Graus de tremor da lanterna com possessao = 100 (P-10).")]
        public float tremorLanternaMax = 6f;

        /// <summary>Lê um valor por noite (noite 1..3) sem estourar o vetor.</summary>
        public static T PorNoite<T>(T[] valores, int noite)
        {
            if (valores == null || valores.Length == 0) return default;
            int i = Mathf.Clamp(noite - 1, 0, valores.Length - 1);
            return valores[i];
        }

        public float MultiplicadorSala(TemperaturaSala t)
        {
            switch (t)
            {
                case TemperaturaSala.Fria: return multFria;
                case TemperaturaSala.Quente: return multQuente;
                default: return multMorna;
            }
        }
    }

    /// <summary>Faixas dos eixos da matéria (GDD 11.1). Valores TESTE.</summary>
    [System.Serializable]
    public class FaixasMateria
    {
        [Tooltip("evidenciasAnotadas a partir de quanto vira V1 (1 a 2).")]
        public int verdadeV1 = 1;
        [Tooltip("evidenciasAnotadas a partir de quanto vira V2 (3 ou mais).")]
        public int verdadeV2 = 3;
        [Tooltip("vezesTocado a partir de quanto vira E1 (1 a 3).")]
        public int exposicaoE1 = 1;
        [Tooltip("vezesTocado a partir de quanto vira E2 (4 ou mais).")]
        public int exposicaoE2 = 4;
    }
}
