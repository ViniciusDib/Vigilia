using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Vigilia
{
    /// <summary>
    /// Elias: movimento (visão de cima), mira com o mouse, interação e a máquina de estados do jogador
    /// dentro de uma noite (GDD 9.2, transições J-01 a J-14).
    ///
    /// Controles: WASD/setas andam · mouse mira a lanterna · F liga/desliga · E interage (segure quando pedir)
    /// · G arromba · Q sedativo · R golpe letal · Esc pausa.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public SpriteRenderer corpo;
        [Tooltip("Filho que gira para a direção da mira (a lanterna fica nele).")]
        public Transform pivoMira;
        public Camera cam;
        [Tooltip("Arte de corpo inteiro por direção. Se preenchido, o corpo não gira com a mira.")]
        public SpriteDirecional direcional;

        public EstadoJogador Estado { get; private set; } = EstadoJogador.Explorando;
        public event Action<EstadoJogador> OnEstadoMudou;

        /// <summary>Direção da mira (unitária).</summary>
        public Vector2 Direcao { get; private set; } = Vector2.up;
        public Vector2 Posicao => transform.position;
        public bool Escondido => Estado == EstadoJogador.Escondido;
        public bool NoQuarto => Jogo.I.temperatura != null && Jogo.I.temperatura.NoQuarto;
        /// <summary>Inimigos não alcançam Elias escondido, dormindo, recém tocado ou no quarto.</summary>
        public bool Alcancavel => Estado != EstadoJogador.Escondido && Estado != EstadoJogador.Dormindo
                                  && Estado != EstadoJogador.Transbordou && Estado != EstadoJogador.Tocado
                                  && !(NoQuarto && Jogo.Cfg.p06InimigosNaoEntramNoQuarto);

        Rigidbody2D rb;
        float timer;
        Action aoTerminarEscrita;
        HidingSpot esconderijo;
        Vector2 posAntesDeEsconder;
        Interativo alvoInteracao;
        float segurando;
        float confirmarGolpeAte;
        float passoTimer;

        Jogo J => Jogo.I;
        BalanceConfig Cfg => Jogo.Cfg;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (cam == null) cam = Camera.main;
        }

        // ------------------------------------------------------------------ máquina de estados

        void MudarEstado(EstadoJogador novo)
        {
            if (novo == Estado) return;
            // saída
            if (Estado == EstadoJogador.Escondido && corpo != null) corpo.enabled = true;
            Estado = novo;
            // entrada
            switch (novo)
            {
                case EstadoJogador.Tocado: timer = Cfg.tempoTocado; break;
                case EstadoJogador.Escondido: if (corpo != null) corpo.enabled = false; timer = 0f; break;
            }
            OnEstadoMudou?.Invoke(novo);
        }

        void AvaliarTransicoes()
        {
            bool noite = J.EhNoite;
            switch (Estado)
            {
                case EstadoJogador.Explorando:
                    if (NoQuarto) MudarEstado(EstadoJogador.NoQuarto);                                   // J-08
                    else if (noite && EnemyAI.AlguemPerseguindo(Posicao, Cfg.raioPerseguicao))
                        MudarEstado(EstadoJogador.Perseguido);                                           // J-01
                    break;
                case EstadoJogador.Perseguido:
                    // [EXTRA] fugir para dentro do quarto também vale: inimigos não entram (P-06).
                    if (NoQuarto) MudarEstado(EstadoJogador.NoQuarto);
                    else if (!EnemyAI.AlguemPerseguindo(Posicao, Cfg.raioPerseguicao))
                        MudarEstado(EstadoJogador.Explorando);                                           // J-02
                    break;
                case EstadoJogador.Escondido:
                    timer += Time.deltaTime;
                    // J-04 · TODO(Q-07): depois de tempoMaxEscondido, um possuído perto encontra Elias.
                    if (timer > Cfg.tempoMaxEscondido && EnemyAI.MaisProximoAtivo(Posicao, Cfg.raioDeteccao) is EnemyAI achou)
                    {
                        SairDoEsconderijo();
                        achou.Encontrar();
                        J.ui.Mensagem("Encontraram você!");
                        MudarEstado(EstadoJogador.Perseguido);
                    }
                    break;
                case EstadoJogador.Tocado:
                    timer -= Time.deltaTime;
                    if (timer <= 0f) MudarEstado(EstadoJogador.Explorando);                              // J-07
                    break;
                case EstadoJogador.NoQuarto:
                    if (!NoQuarto) MudarEstado(EstadoJogador.Explorando);                                // J-09
                    break;
                case EstadoJogador.Escrevendo:
                    timer -= Time.deltaTime;
                    J.ui.Progresso(1f - timer / Mathf.Max(0.01f, Cfg.tempoEscrita), "escrevendo...");
                    if (timer <= 0f)
                    {
                        J.ui.Progresso(-1f, null);
                        MudarEstado(EstadoJogador.NoQuarto);                                             // J-11
                        var cb = aoTerminarEscrita; aoTerminarEscrita = null; cb?.Invoke();
                    }
                    break;
            }
        }

        // ------------------------------------------------------------------ chamadas de outros sistemas

        public void PrepararDia()
        {
            if (esconderijo != null) esconderijo.Ocupado = false;
            esconderijo = null;
            if (corpo != null) corpo.enabled = true;
            Estado = EstadoJogador.Explorando;
            OnEstadoMudou?.Invoke(Estado);
        }

        public void PrepararNoite() => PrepararDia();

        public void Teleportar(Vector2 p)
        {
            rb.position = p;
            transform.position = p;
            rb.linearVelocity = Vector2.zero;
        }

        /// <summary>J-06: um possuído encostou. Os efeitos na banheira são aplicados pelo EnemyAI.</summary>
        public void ReceberToque(Vector2 de)
        {
            MudarEstado(EstadoJogador.Tocado);
            rb.linearVelocity = (Posicao - de).normalized * 4f;
            J.ui.Tremer(0.4f, 0.3f);
            J.ui.FlashVermelho();
            J.audioJogo?.Tocar(SomId.Toque);
        }

        /// <summary>J-03: Elias entra num esconderijo livre.</summary>
        public void Esconder(HidingSpot spot)
        {
            if (spot.Ocupado) return;
            esconderijo = spot;
            spot.Ocupado = true;
            posAntesDeEsconder = Posicao;
            Teleportar(spot.transform.position);
            MudarEstado(EstadoJogador.Escondido);
            J.audioJogo?.Tocar(SomId.Esconder);
        }

        /// <summary>J-05: Elias sai do esconderijo.</summary>
        public void SairDoEsconderijo()
        {
            if (esconderijo != null) esconderijo.Ocupado = false;
            esconderijo = null;
            Teleportar(posAntesDeEsconder);
            MudarEstado(EstadoJogador.Explorando);
        }

        /// <summary>J-10: começa a escrever (só no quarto).</summary>
        public void Escrever(float duracao, Action aoTerminar)
        {
            timer = duracao;
            aoTerminarEscrita = aoTerminar;
            MudarEstado(EstadoJogador.Escrevendo);
            J.audioJogo?.Tocar(SomId.Escrever);
        }

        /// <summary>J-12: dormir.</summary>
        public void Dormir() => MudarEstado(EstadoJogador.Dormindo);

        /// <summary>J-14: possessao chegou a 100.</summary>
        public void MarcarTransbordou() => MudarEstado(EstadoJogador.Transbordou);

        // ------------------------------------------------------------------ quadro a quadro

        void Update()
        {
            if (J == null) return;
            var kb = Keyboard.current;

            if (J.Travado || Estado == EstadoJogador.Dormindo || Estado == EstadoJogador.Transbordou)
            {
                rb.linearVelocity = Vector2.zero;
                J.ui?.Prompt(null);
                return;
            }

            AtualizarMira();
            AvaliarTransicoes();

            bool podeAndar = Estado != EstadoJogador.Escondido && Estado != EstadoJogador.Escrevendo;
            Vector2 mov = Vector2.zero;
            if (kb != null && podeAndar && Estado != EstadoJogador.Tocado)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) mov.y += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) mov.y -= 1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) mov.x += 1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) mov.x -= 1;
            }
            if (Estado != EstadoJogador.Tocado)
                rb.linearVelocity = mov.sqrMagnitude > 0 ? mov.normalized * Cfg.velocidadeJogador : Vector2.zero;

            if (mov.sqrMagnitude > 0)
            {
                passoTimer -= Time.deltaTime;
                if (passoTimer <= 0f) { passoTimer = 0.42f; J.audioJogo?.Tocar(SomId.Passo); }
            }

            AtualizarInteracao(kb);
            AtualizarAtaque(kb);
        }

        void AtualizarMira()
        {
            var mouse = Mouse.current;
            if (mouse == null || cam == null) return;
            Vector3 m = cam.ScreenToWorldPoint(mouse.position.ReadValue());
            Vector2 d = (Vector2)m - Posicao;
            if (d.sqrMagnitude > 0.01f) Direcao = d.normalized;
            if (pivoMira != null)
                pivoMira.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Direcao.y, Direcao.x) * Mathf.Rad2Deg - 90f);
            if (direcional != null) direcional.Aplicar(Direcao, false);
        }

        void AtualizarInteracao(Keyboard kb)
        {
            // Escondido: o único interativo é o próprio esconderijo
            alvoInteracao = Estado == EstadoJogador.Escondido ? esconderijo : MaisProximo();
            if (alvoInteracao == null)
            {
                segurando = 0f;
                J.ui.Prompt(null);
                return;
            }

            string alt = alvoInteracao.PromptAlternativo(this);
            J.ui.Prompt(alt == null ? alvoInteracao.Prompt(this) : alvoInteracao.Prompt(this) + "    " + alt);
            if (kb == null) return;

            float tempo = alvoInteracao.TempoSegurar(this);
            bool podeUsar = alvoInteracao.PodeUsar(this);
            if (tempo > 0f && podeUsar)
            {
                if (kb.eKey.isPressed)
                {
                    segurando += Time.deltaTime;
                    J.ui.Progresso(segurando / tempo, null);
                    if (segurando >= tempo)
                    {
                        segurando = 0f;
                        J.ui.Progresso(-1f, null);
                        alvoInteracao.Interagir(this);
                    }
                }
                else if (segurando > 0f)
                {
                    segurando = 0f;
                    J.ui.Progresso(-1f, null);
                }
            }
            else if (kb.eKey.wasPressedThisFrame && podeUsar)
            {
                alvoInteracao.Interagir(this);
            }

            if (alt != null && kb.gKey.wasPressedThisFrame) alvoInteracao.InteragirAlternativo(this);
        }

        Interativo MaisProximo()
        {
            Interativo melhor = null;
            float melhorD = Cfg.alcanceInteracao;
            foreach (var it in Interativo.Todos)
            {
                if (it == null || !it.Disponivel(this)) continue;
                float d = Vector2.Distance(Posicao, it.Posicao);
                if (d <= melhorD) { melhorD = d; melhor = it; }
            }
            return melhor;
        }

        /// <summary>TODO(Q-04): sugestão do GDD 7.3. Q = sedativo num congelado (E-09); R = golpe letal num atordoado (E-11).</summary>
        void AtualizarAtaque(Keyboard kb)
        {
            if (kb == null || !J.EhNoite || Estado == EstadoJogador.Escondido) return;
            var w = J.Mundo;

            if (kb.qKey.wasPressedThisFrame)
            {
                if (w.sedativos <= 0) { J.ui.Mensagem("Sem sedativos."); return; }
                var alvo = EnemyAI.AlvoDeAtaque(Posicao, Direcao, Cfg.alcanceAtaque, EstadoInimigo.Congelado);
                if (alvo == null) { J.ui.Mensagem("O sedativo só funciona num possuído parado na luz, bem perto."); return; }
                w.sedativos--;
                alvo.Atordoar();
                J.ui.Mensagem($"{alvo.nome} cambaleia e para.");
            }

            if (kb.rKey.wasPressedThisFrame)
            {
                if (w.objetosPesados <= 0) { J.ui.Mensagem("Você não tem nada pesado nas mãos."); return; }
                var alvo = EnemyAI.AlvoDeAtaque(Posicao, Direcao, Cfg.alcanceAtaque, EstadoInimigo.Atordoado);
                if (alvo == null) { J.ui.Mensagem("Só dá para golpear um possuído atordoado, bem perto."); return; }
                if (Time.time > confirmarGolpeAte)
                {
                    confirmarGolpeAte = Time.time + 2.5f;
                    J.ui.Mensagem($"É {alvo.nome}. De dia, é uma pessoa. [R] de novo para golpear. Não tem volta.");
                    return;
                }
                confirmarGolpeAte = 0f;
                w.objetosPesados--;
                alvo.Matar();
            }
        }
    }
}
