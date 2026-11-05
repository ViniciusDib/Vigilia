using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Um NPC do sanatório. De dia é inofensivo (Rotina, Seguindo); à noite é um possuído
    /// (Patrulha, Perseguindo, Congelado, Atordoado, Recuando, Morto). GDD 9.3, transições E-01 a E-13.
    ///
    /// Padrão: enum + switch + MudarEstado(novo). Cada transição tem o ID da tabela no comentário.
    /// Escreve no WorldState: vezesTocado, matouAlguem e npcMorto (tabela 14.1).
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        /// <summary>NPCs ativos agora (de dia ou de noite). O detector de luz percorre esta lista.</summary>
        public static readonly List<EnemyAI> Ativos = new List<EnemyAI>();

        [Header("Identidade")]
        public string npcId = "marta";
        public string nome = "Marta";
        public TipoInimigo tipo = TipoInimigo.Funcionario;
        [Tooltip("Prioridade para virar inimigo à noite (0 primeiro). Ver alavanca inimigosPorNoite.")]
        public int ordemNoite;
        [Tooltip("Falso para o diretor: nunca caça à noite.")]
        public bool podeSerInimigo = true;
        [Tooltip("Possuído extra: só aparece quando começa a fuga da noite 3 (decisão do grupo, 04/11).")]
        public bool soNaFuga;
        [Tooltip("Frase sussurrada a primeira vez que persegue à noite (ex.: Marta chama 'Elias...', pista 1).")]
        public string sussurro;
        [Tooltip("Noite em que o sussurro acontece (0 = todas).")]
        public int noiteDoSussurro = 1;

        [Header("Lugares")]
        public Vector2 postoDia;
        public Vector2 postoNoite;
        [Tooltip("Pontos de patrulha à noite (centros de sala). Vazio = anda em volta do posto.")]
        public Vector2[] rotaPatrulha;

        [Header("Visual")]
        public SpriteRenderer corpo;
        public SpriteRenderer olhos;
        [Tooltip("Sprite de dia (vista de cima, olhando para o topo da imagem). Vazio = mantém o do SpriteRenderer.")]
        public Sprite spriteDia;
        [Tooltip("Sprite possuído, à noite. Vazio = usa o de dia escurecido, com olhos vermelhos por cima.")]
        public Sprite spriteNoite;
        [Tooltip("Arte de corpo inteiro por direção (não gira). Se preenchido, substitui spriteDia/spriteNoite.")]
        public SpriteDirecional direcional;
        public Color corDia = new Color(0.8f, 0.8f, 0.85f);
        public Color corNoite = new Color(0.55f, 0.5f, 0.55f);

        public EstadoInimigo Estado { get; private set; } = EstadoInimigo.Rotina;
        public bool Iluminado { get; private set; }
        public event Action OnIluminado;
        public event Action OnSaiuDaLuz;

        public Vector2 Posicao => transform.position;
        public bool EhNoturno => Estado >= EstadoInimigo.Patrulha;

        readonly List<Vector2> caminho = new List<Vector2>();
        int passo;
        float repath;
        float timer;
        float espera;
        Vector2 destino;
        bool sussurrou;
        int noite = 1;
        float seed;
        Vector2 olhando = Vector2.down;

        Jogo J => Jogo.I;
        BalanceConfig Cfg => Jogo.Cfg;
        PlayerController Jogador => Jogo.I.jogador;

        void OnEnable() { if (!Ativos.Contains(this)) Ativos.Add(this); Barulho.OnBarulho += Ouvir; }
        void OnDisable() { Ativos.Remove(this); Barulho.OnBarulho -= Ouvir; }
        void Awake() => seed = UnityEngine.Random.value * 100f;

        // ------------------------------------------------------------------ consultas estáticas

        /// <summary>Usado pelo jogador para J-01/J-02.</summary>
        public static bool AlguemPerseguindo(Vector2 p, float raio)
        {
            foreach (var e in Ativos)
                if ((e.Estado == EstadoInimigo.Perseguindo || e.Estado == EstadoInimigo.Congelado)
                    && Vector2.Distance(e.Posicao, p) <= raio) return true;
            return false;
        }

        public static EnemyAI MaisProximoAtivo(Vector2 p, float raio)
        {
            EnemyAI m = null; float md = raio;
            foreach (var e in Ativos)
            {
                if (!e.EhNoturno || e.Estado == EstadoInimigo.Morto || e.Estado == EstadoInimigo.Atordoado) continue;
                float d = Vector2.Distance(e.Posicao, p);
                if (d <= md) { md = d; m = e; }
            }
            return m;
        }

        /// <summary>Inimigo no estado pedido, perto e mais ou menos à frente de Elias.</summary>
        public static EnemyAI AlvoDeAtaque(Vector2 p, Vector2 dir, float alcance, EstadoInimigo estado)
        {
            EnemyAI m = null; float md = alcance;
            foreach (var e in Ativos)
            {
                if (e.Estado != estado) continue;
                Vector2 v = e.Posicao - p;
                float d = v.magnitude;
                if (d > md || (d > 0.3f && Vector2.Angle(dir, v) > 70f)) continue;
                md = d; m = e;
            }
            return m;
        }

        /// <summary>Distância do possuído perseguidor mais próximo (para o som de sussurro).</summary>
        public static float DistanciaPerseguidorMaisProximo(Vector2 p)
        {
            float md = float.MaxValue;
            foreach (var e in Ativos)
                if (e.Estado == EstadoInimigo.Perseguindo || e.Estado == EstadoInimigo.Congelado)
                    md = Mathf.Min(md, Vector2.Distance(e.Posicao, p));
            return md;
        }

        // ------------------------------------------------------------------ configuração (NightManager)

        public void ConfigurarDia(int dia)
        {
            if (soNaFuga || J.Mundo.NpcMorto(npcId)) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);
            transform.position = postoDia;
            noite = dia;
            Iluminado = false;
            MudarEstado(EstadoInimigo.Rotina);                    // E-04: amanhece, volta à rotina
        }

        public void ConfigurarNoite(int n, bool ativo)
        {
            noite = n;
            sussurrou = false;
            if (!ativo || soNaFuga) { gameObject.SetActive(false); return; } // não acordou esta noite (ou está morto)
            gameObject.SetActive(true);
            transform.position = postoNoite;
            Iluminado = false;
            MudarEstado(EstadoInimigo.Patrulha);                  // E-03: começa a noite, vira possuído
        }

        /// <summary>Fuga da noite 3: o possuído extra surge no seu posto e já sai caçando.</summary>
        public void AtivarNaFuga(Vector2 jogador)
        {
            noite = 3;
            gameObject.SetActive(true);
            transform.position = postoNoite;
            Iluminado = false;
            MudarEstado(EstadoInimigo.Patrulha);
        }

        bool EmFuga => J != null && J.noites != null && J.noites.EmFuga;
        float RaioDeteccao => Cfg.raioDeteccao * (EmFuga ? Cfg.fugaMultRaio : 1f);
        float RaioPerseguicao => Cfg.raioPerseguicao * (EmFuga ? Cfg.fugaMultRaio : 1f);

        // ------------------------------------------------------------------ máquina de estados

        void MudarEstado(EstadoInimigo novo)
        {
            Estado = novo;
            caminho.Clear();
            repath = 0f;
            switch (novo)
            {
                case EstadoInimigo.Rotina: espera = UnityEngine.Random.Range(0.5f, 2.5f); destino = Posicao; break;
                case EstadoInimigo.Patrulha: destino = Posicao; espera = 0.3f; break;
                case EstadoInimigo.Atordoado: timer = Cfg.tempoAtordoado; break;
                case EstadoInimigo.Recuando: timer = Cfg.tempoRecuoToque; break;
            }
            AtualizarVisual();
        }

        void Update()
        {
            if (J == null || J.Travado || Jogador == null) return;
            float dt = Time.deltaTime;

            switch (Estado)
            {
                case EstadoInimigo.Rotina: TickRotina(dt); break;
                case EstadoInimigo.Seguindo: TickSeguindo(dt); break;
                case EstadoInimigo.Patrulha: TickPatrulha(dt); break;
                case EstadoInimigo.Perseguindo: TickPerseguindo(dt); break;
                case EstadoInimigo.Congelado:
                    if (!Iluminado) MudarEstado(EstadoInimigo.Perseguindo);              // E-08: saiu da luz
                    break;
                case EstadoInimigo.Atordoado:
                    timer -= dt;
                    if (timer <= 0f) MudarEstado(EstadoInimigo.Perseguindo);             // E-10
                    break;
                case EstadoInimigo.Recuando:
                    timer -= dt;
                    Vector2 fuga = (Posicao - Jogador.Posicao).normalized;
                    Mover(Posicao + fuga, Cfg.velocidadePatrulha * MultVelocidade, dt, direto: true);
                    if (timer <= 0f) MudarEstado(EstadoInimigo.Patrulha);                // E-13
                    break;
            }
            AtualizarVisual();
        }

        // ---------- dia
        void TickRotina(float dt)
        {
            espera -= dt;
            if (espera > 0f) return;
            if (Vector2.Distance(Posicao, destino) < 0.15f)
            {
                destino = postoDia + UnityEngine.Random.insideUnitCircle * 2f;
                espera = UnityEngine.Random.Range(1f, 3.5f);
                caminho.Clear();
                return;
            }
            AndarAte(destino, Cfg.velocidadePatrulha * 0.5f, dt);
        }

        /// <summary>E-01: o DialogueSystem chama quando o NPC decide seguir Elias.</summary>
        public void ComecarASeguir()
        {
            if (Estado == EstadoInimigo.Rotina) MudarEstado(EstadoInimigo.Seguindo);
        }

        void TickSeguindo(float dt)
        {
            float d = Vector2.Distance(Posicao, Jogador.Posicao);
            if (d > 7f) { MudarEstado(EstadoInimigo.Rotina); return; }               // E-02: Elias se afasta
            if (d > 1.6f) AndarAte(Jogador.Posicao, Cfg.velocidadePatrulha * 0.9f, dt);
        }

        // ---------- noite
        void TickPatrulha(float dt)
        {
            if (VeJogador() || Iluminado)
            {
                // E-05. [EXTRA] ser iluminado também denuncia Elias; no quadro seguinte, E-07 congela.
                MudarEstado(EstadoInimigo.Perseguindo);
                return;
            }
            espera -= dt;
            if (espera > 0f) return;
            if (Vector2.Distance(Posicao, destino) < 0.3f || caminho.Count == 0 && repath <= 0f)
            {
                destino = ProximoPontoDePatrulha();
                caminho.Clear();
                espera = UnityEngine.Random.Range(0.2f, 1.2f);
            }
            AndarAte(destino, Cfg.velocidadePatrulha * MultVelocidade, dt);
        }

        void TickPerseguindo(float dt)
        {
            if (Iluminado) { MudarEstado(EstadoInimigo.Congelado); return; }          // E-07: para de andar
            float d = Vector2.Distance(Posicao, Jogador.Posicao);
            if (!Jogador.Alcancavel && Jogador.Estado != EstadoJogador.Tocado || d > RaioPerseguicao)
            {
                MudarEstado(EstadoInimigo.Patrulha);                                     // E-06: perdeu Elias
                return;
            }
            Sussurrar();
            if (d <= Cfg.distanciaToque && Jogador.Alcancavel)
            {
                Tocar();                                                                  // E-12
                return;
            }
            AndarAte(Jogador.Posicao, Cfg.velocidadePerseguicao * MultVelocidade, dt);
        }

        void Tocar()
        {
            var w = J.Mundo;
            w.vezesTocado++;                                    // escritor: EnemyAI (14.1)
            J.possessao.AdicionarPossessao(Cfg.pulsoToque);     // D-14: um bloco grande da banheira
            Jogador.ReceberToque(Posicao);                      // J-06
            MudarEstado(EstadoInimigo.Recuando);                // o inimigo recua
        }

        void Sussurrar()
        {
            if (sussurrou || string.IsNullOrEmpty(sussurro)) return;
            if (noiteDoSussurro != 0 && noiteDoSussurro != noite) return;
            sussurrou = true;
            J.ui.Legenda($"{nome}: \"{sussurro}\"", 3f);
            J.audioJogo?.Tocar(SomId.Sussurro);
            if (npcId == "marta") J.Mundo.pistaNomePrevio = true; // pista 1 (GDD 12)
        }

        /// <summary>J-04: Elias foi encontrado no esconderijo.</summary>
        public void Encontrar() => MudarEstado(EstadoInimigo.Perseguindo);

        /// <summary>E-09: item de ataque (sedativo) num congelado. TODO(Q-04).</summary>
        public void Atordoar()
        {
            if (Estado == EstadoInimigo.Congelado) MudarEstado(EstadoInimigo.Atordoado);
        }

        /// <summary>E-11: golpe letal num atordoado. Consequência permanente (matouAlguem). TODO(Q-04).</summary>
        public void Matar()
        {
            if (Estado != EstadoInimigo.Atordoado) return;
            J.Mundo.MarcarMorto(npcId);                         // matouAlguem = verdadeiro; npcMorto[id] = verdadeiro
            MudarEstado(EstadoInimigo.Morto);
            J.audioJogo?.Tocar(SomId.Pancada);
            J.ui.Tremer(0.5f, 0.3f);
            J.ui.Mensagem($"{nome} não se mexe mais.");
        }

        void Ouvir(Vector2 origem, float raio)
        {
            if (Estado != EstadoInimigo.Patrulha) return;
            if (tipo == TipoInimigo.PacienteVelho) raio *= Cfg.multRaioBarulhoPacienteVelho;
            if (Vector2.Distance(Posicao, origem) <= raio) MudarEstado(EstadoInimigo.Perseguindo); // E-05: barulho
        }

        public void DefinirIluminado(bool lit)
        {
            if (lit == Iluminado) return;
            Iluminado = lit;
            if (lit) OnIluminado?.Invoke(); else OnSaiuDaLuz?.Invoke();
        }

        // ------------------------------------------------------------------ util

        float MultVelocidade
            => BalanceConfig.PorNoite(Cfg.multVelocidadeNoite, noite)
               * (tipo == TipoInimigo.PacienteVelho ? Cfg.multVelocidadePacienteVelho : 1f)
               * (EmFuga ? Cfg.fugaMultVelocidade : 1f);

        bool VeJogador()
        {
            if (!Jogador.Alcancavel) return false;
            float d = Vector2.Distance(Posicao, Jogador.Posicao);
            if (d > RaioDeteccao) return false;
            return !Physics2D.Linecast(Posicao, Jogador.Posicao, J.nav != null ? (int)J.nav.paredes : ~0);
        }

        Vector2 ProximoPontoDePatrulha()
        {
            // na fuga, a patrulha vai atrás de onde Elias está (todos convergem para ele)
            if (EmFuga) return Jogador.Posicao + UnityEngine.Random.insideUnitCircle * 3f;
            if (rotaPatrulha != null && rotaPatrulha.Length > 0)
                return rotaPatrulha[UnityEngine.Random.Range(0, rotaPatrulha.Length)] + UnityEngine.Random.insideUnitCircle;
            return postoNoite + UnityEngine.Random.insideUnitCircle * 4f;
        }

        void AndarAte(Vector2 alvo, float velocidade, float dt)
        {
            repath -= dt;
            if (repath <= 0f || caminho.Count == 0)
            {
                repath = Estado == EstadoInimigo.Perseguindo ? 0.3f : 1.5f;
                if (J.nav == null || !J.nav.Caminho(Posicao, alvo, caminho, Cfg.p06InimigosNaoEntramNoQuarto)) caminho.Clear();
                passo = 0;
            }
            if (caminho.Count == 0) return;
            while (passo < caminho.Count - 1 && Vector2.Distance(Posicao, caminho[passo]) < 0.2f) passo++;
            Mover(caminho[passo], velocidade, dt, direto: false);
        }

        void Mover(Vector2 alvo, float velocidade, float dt, bool direto)
        {
            Vector2 v = alvo - Posicao;
            if (v.sqrMagnitude < 0.0001f) return;
            Vector2 dir = v.normalized;
            // Paciente velho é errático (glossário): desvio lateral com ruído.
            if (tipo == TipoInimigo.PacienteVelho && EhNoturno)
            {
                float n = (Mathf.PerlinNoise(seed, Time.time * 1.7f) - 0.5f) * 1.4f;
                dir = (dir + new Vector2(-dir.y, dir.x) * n).normalized;
            }
            Vector2 novo = Posicao + dir * Mathf.Min(velocidade * dt, v.magnitude);
            // P-06: nunca pisa no quarto, nem recuando.
            if (direto && J.nav != null && Cfg.p06InimigosNaoEntramNoQuarto && J.nav.EmAreaProibida(novo)) return;
            if (direto && Physics2D.OverlapCircle(novo, 0.25f, J.nav != null ? (int)J.nav.paredes : 0)) return;
            transform.position = novo;
            olhando = dir;
            if (direcional == null)
            {
                // arte vista de cima: gira para onde anda (o sprite olha para o topo da imagem)
                float alvoAng = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, alvoAng), 540f * dt);
            }
        }

        void AtualizarVisual()
        {
            if (corpo == null) return;
            Color c = EhNoturno ? corNoite : corDia;
            if (Estado == EstadoInimigo.Congelado) c = Color.Lerp(c, new Color(0.7f, 0.85f, 1f), 0.5f);
            if (Estado == EstadoInimigo.Atordoado) c = Color.Lerp(c, new Color(0.5f, 0.9f, 0.5f), 0.4f);
            if (Estado == EstadoInimigo.Morto) c = new Color(0.25f, 0.2f, 0.2f);
            corpo.color = c;
            if (direcional != null && Jogador != null)
            {
                // parado e perto de Elias, olha para ele
                Vector2 ver = Estado == EstadoInimigo.Congelado || Estado == EstadoInimigo.Seguindo ? Jogador.Posicao - Posicao : olhando;
                direcional.Aplicar(ver, EhNoturno);
            }
            else if (spriteDia != null) corpo.sprite = EhNoturno && spriteNoite != null ? spriteNoite : spriteDia;
            if (olhos != null)
                olhos.enabled = EhNoturno && Estado != EstadoInimigo.Morto && Estado != EstadoInimigo.Atordoado;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, Jogo.Cfg.raioDeteccao);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, Jogo.Cfg.raioPerseguicao);
        }
    }
}
