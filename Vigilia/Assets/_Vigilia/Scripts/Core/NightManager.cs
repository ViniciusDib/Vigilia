using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Máquina macro do fim de semana (GDD 9.1): INTRO → DIA1 → NOITE1 → ... → FINAL.
    /// É o ÚNICO que captura e restaura o SaveSnapshot e o único que zera a NpcMemory às 06:00 (Figura 10).
    ///
    /// Padrão usado (GDD 9): enum + switch + MudarEstado(novo) com ações de entrada.
    /// Cada transição tem o ID da tabela (M-01 ... M-11) no comentário para você achar no GDD.
    /// </summary>
    public class NightManager : MonoBehaviour
    {
        public EstadoMacro Estado { get; private set; } = EstadoMacro.Intro;
        public event Action<EstadoMacro> OnEstadoMudou;

        /// <summary>Fica FORA do snapshot (GDD 14.1): é o que leva ao final C.</summary>
        public bool transbordouNoite3;

        public bool EhNoite => Estado == EstadoMacro.Noite1 || Estado == EstadoMacro.Noite2 || Estado == EstadoMacro.Noite3;
        public bool EhDia => Estado == EstadoMacro.Dia1 || Estado == EstadoMacro.Dia2 || Estado == EstadoMacro.Dia3;
        /// <summary>Noite 3, depois do monstro: Elias precisa sair do sanatório (decisão do grupo, 04/11).</summary>
        public bool EmFuga { get; private set; }

        public bool EmFinal => Estado == EstadoMacro.FinalA || Estado == EstadoMacro.FinalB || Estado == EstadoMacro.FinalC;

        readonly SaveSnapshot snapshot = new SaveSnapshot();
        readonly PosicoesDaCena posicoes = new PosicoesDaCena();

        // Tudo que o setup de cada noite liga, desliga e reposiciona. Coletado do Mapa (inclui inativos).
        Pickup[] itens;
        HidingSpot[] esconderijos;
        EnemyAI[] npcs;
        Sala[] salas;
        Porta[] portas;
        Caldeira[] caldeiras;
        PontoDeInicio[] pontos;

        bool emSequencia;

        Jogo J => Jogo.I;
        BalanceConfig Cfg => Jogo.Cfg;

        void Start()
        {
            Transform raiz = J.mapa != null ? J.mapa : transform.root;
            itens = FindAll<Pickup>(raiz);
            esconderijos = FindAll<HidingSpot>(raiz);
            npcs = FindAll<EnemyAI>(raiz);
            salas = FindAll<Sala>(raiz);
            portas = FindAll<Porta>(raiz);
            caldeiras = FindAll<Caldeira>(raiz);
            pontos = FindAll<PontoDeInicio>(raiz);

            J.possessao.OnTransbordou += AoTransbordar;
            if (J.cama != null) J.cama.OnDormiu += AoDormir;

            if (SaveJogo.ContinuarPendente && SaveJogo.Carregar() is SaveSnapshot salvo && !salvo.Vazio)
            {
                SaveJogo.ContinuarPendente = false;
                ContinuarDoSave(salvo);
            }
            else
            {
                SaveJogo.ContinuarPendente = false;
                MudarEstado(EstadoMacro.Intro);
            }
        }

        void OnDestroy()
        {
            if (J != null && J.possessao != null) J.possessao.OnTransbordou -= AoTransbordar;
            if (J != null && J.cama != null) J.cama.OnDormiu -= AoDormir;
        }

        static T[] FindAll<T>(Transform raiz) where T : Component => raiz.GetComponentsInChildren<T>(true);

        // ------------------------------------------------------------------ máquina

        public void MudarEstado(EstadoMacro novo)
        {
            Estado = novo;
            Entrar(novo);
            OnEstadoMudou?.Invoke(novo);
        }

        void Entrar(EstadoMacro e)
        {
            switch (e)
            {
                case EstadoMacro.Intro:
                    J.EmCena = true;
                    J.ui.MostrarIntro(FimDaIntro);
                    break;
                case EstadoMacro.Dia1: ComecarDia(1); break;
                case EstadoMacro.Dia2: ComecarDia(2); break;
                case EstadoMacro.Dia3: ComecarDia(3); break;
                case EstadoMacro.Noite1: StartCoroutine(ComecarNoite(1)); break;
                case EstadoMacro.Noite2: StartCoroutine(ComecarNoite(2)); break;
                case EstadoMacro.Noite3: StartCoroutine(ComecarNoite(3)); break;
                case EstadoMacro.FinalA:
                case EstadoMacro.FinalB:
                case EstadoMacro.FinalC:
                    MostrarFinal();
                    break;
            }
        }

        // M-01: jogador termina ou pula a intro → WorldState inicial; noiteAtual = 1
        void FimDaIntro()
        {
            J.Mundo.CopiarDe(new WorldState());
            J.Memoria.Limpar();
            J.possessao.DefinirValor(0f);
            transbordouNoite3 = false;
            J.Mundo.noiteAtual = 1;
            MudarEstado(EstadoMacro.Dia1);
        }

        /// <summary>M-02, M-05, M-08: o dia termina. TODO(Q-18): hoje o dia acaba por ação (deitar na cama de dia).</summary>
        public void TerminarDia()
        {
            if (!EhDia || emSequencia) return;
            StartCoroutine(SequenciaFimDoDia());
        }

        IEnumerator SequenciaFimDoDia()
        {
            emSequencia = true;
            J.EmCena = true;
            yield return J.ui.Fade(true, 1.2f);
            emSequencia = false;
            if (Estado == EstadoMacro.Dia1) MudarEstado(EstadoMacro.Noite1);
            else if (Estado == EstadoMacro.Dia2) MudarEstado(EstadoMacro.Noite2);
            else if (Estado == EstadoMacro.Dia3) MudarEstado(EstadoMacro.Noite3);
        }

        // ------------------------------------------------------------------ dia

        void ComecarDia(int dia)
        {
            J.Mundo.noiteAtual = dia; // o dia N antecede a noite N
            ConfigurarDia(dia);
            J.EmCena = false;
            StartCoroutine(CartaoDia(dia));
        }

        IEnumerator CartaoDia(int dia)
        {
            J.EmCena = true;
            yield return J.ui.Cartao(Idioma.T("DIA", "DAY") + $" {dia}", NomeDoDia(dia), 2.2f);
            yield return J.ui.Fade(false, 1f);
            J.EmCena = false;
            J.ui.Objetivo(ObjetivoDoDia(dia));
        }

        static string NomeDoDia(int d) => d == 1 ? Idioma.T("sexta-feira", "Friday") : d == 2 ? Idioma.T("sábado", "Saturday") : Idioma.T("domingo", "Sunday");

        string ObjetivoDoDia(int dia)
        {
            // TODO(Q-18): objetivos de dia ligados às pistas.
            if (dia == 1) return Idioma.T("Explore o sanatório e converse com quem encontrar. Quando quiser, deite na cama para esperar a noite.",
                                          "Explore the sanatorium and talk to whoever you meet. When you're ready, lie down on the bed to wait for night.");
            if (dia == 2) return Idioma.T("Pergunte sobre a noite passada. Alguém lembra? Escreva o que achou no caderno do quarto.",
                                          "Ask about last night. Does anyone remember? Write what you found in the notebook in your room.");
            return Idioma.T("O escritório do diretor fica no corredor norte. Procure algo que explique tudo. Depois, a cama.",
                            "The director's office is in the north corridor. Look for something that explains everything. Then, bed.");
        }

        void ConfigurarDia(int dia)
        {
            EmFuga = false;
            AplicarLuz(false);
            foreach (var s in salas) s.Aplicar(false, dia);
            foreach (var p in portas) p.Configurar(false, dia);
            foreach (var c in caldeiras) c.Configurar(false, dia);
            foreach (var i in itens) i.Configurar(false, dia);
            foreach (var h in esconderijos) h.Configurar(false, dia);
            foreach (var n in npcs) n.ConfigurarDia(dia);
            J.jogador.Teleportar(Ponto("dia"));
            J.jogador.PrepararDia();
            J.nav?.Reconstruir();
        }

        // ------------------------------------------------------------------ noite

        IEnumerator ComecarNoite(int n)
        {
            J.EmCena = true;
            J.Mundo.noiteAtual = n;
            ConfigurarNoite(n, primeiraVez: true);
            J.possessao.IniciarNoite();
            CapturarCheckpoint();
            yield return J.ui.Cartao(Idioma.T("NOITE", "NIGHT") + $" {n}", SituacaoInicial(n), 3.2f);
            yield return J.ui.Fade(false, 1.2f);
            J.EmCena = false;
            J.ui.Objetivo(ObjetivoDaNoite(n));
        }

        /// <summary>Monta a noite N do zero a partir do WorldState. Usado no começo e ao restaurar o checkpoint.</summary>
        void ConfigurarNoite(int n, bool primeiraVez)
        {
            var w = J.Mundo;
            if (primeiraVez)
            {
                // GDD 14.1: quartoDestrancado = verdadeiro nas noites 1 e 3, falso na noite 2 (trancado fora, D-16).
                w.quartoDestrancado = n != 2;
                // Q-17 (sugestão do GDD 6.7): a caldeira falha na noite 3.
                w.caldeiraFalhou = n == 3 && Cfg.noite3CaldeiraFalha;
            }

            EmFuga = false;
            AplicarLuz(true);
            foreach (var s in salas) s.Aplicar(true, n);
            foreach (var p in portas) p.Configurar(true, n);
            foreach (var c in caldeiras) c.Configurar(true, n);
            foreach (var i in itens) i.Configurar(true, n);
            foreach (var h in esconderijos) h.Configurar(true, n);

            // Alavanca "inimigos por noite": os N primeiros vivos por ordem de prioridade. Os outros estão dormindo.
            int vagas = BalanceConfig.PorNoite(Cfg.inimigosPorNoite, n);
            var ordenados = new List<EnemyAI>(npcs);
            ordenados.Sort((a, b) => a.ordemNoite.CompareTo(b.ordemNoite));
            foreach (var npc in ordenados)
            {
                bool ativo = vagas > 0 && npc.podeSerInimigo && !w.NpcMorto(npc.npcId);
                if (ativo) vagas--;
                npc.ConfigurarNoite(n, ativo);
            }

            J.jogador.Teleportar(Ponto("noite" + n));
            J.jogador.PrepararNoite();
            J.lanterna.PrepararNoite();
            J.caderno?.ResetarLucido();
            J.nav?.Reconstruir();
        }

        void CapturarCheckpoint()
        {
            posicoes.jogador = J.jogador.transform.position;
            snapshot.Capturar(J.Mundo, J.Memoria, posicoes, J.possessao.Valor);
            if (Cfg.p13SalvarCheckpoint) SaveJogo.Salvar(snapshot);
        }

        void RestaurarCheckpoint()
        {
            int n = J.Mundo.noiteAtual;
            snapshot.Restaurar(J.Mundo, J.Memoria, posicoes);
            ConfigurarNoite(n, primeiraVez: false);
            J.jogador.Teleportar(posicoes.jogador);
            J.possessao.DefinirValor(snapshot.possessao);
            J.possessao.IniciarNoite();
        }

        void ContinuarDoSave(SaveSnapshot salvo)
        {
            JsonUtility.FromJsonOverwrite(salvo.ParaJson(), snapshot);
            snapshot.Restaurar(J.Mundo, J.Memoria, posicoes);
            J.possessao.DefinirValor(snapshot.possessao);
            int n = Mathf.Clamp(J.Mundo.noiteAtual, 1, 3);
            Estado = n == 1 ? EstadoMacro.Noite1 : n == 2 ? EstadoMacro.Noite2 : EstadoMacro.Noite3;
            StartCoroutine(ContinuarNoite(n));
            OnEstadoMudou?.Invoke(Estado);
        }

        IEnumerator ContinuarNoite(int n)
        {
            J.EmCena = true;
            ConfigurarNoite(n, primeiraVez: false);
            J.jogador.Teleportar(posicoes.jogador);
            J.possessao.IniciarNoite();
            yield return J.ui.Cartao(Idioma.T("NOITE", "NIGHT") + $" {n}", Idioma.T("continuando do início da noite", "continuing from the start of the night"), 2.2f);
            yield return J.ui.Fade(false, 1f);
            J.EmCena = false;
            J.ui.Objetivo(ObjetivoDaNoite(n));
        }

        string SituacaoInicial(int n)
        {
            if (n == 1) return Idioma.T("O quarto está gelado. Sem cobertor, não há como dormir. No corredor, um barulho.",
                                        "The room is freezing. Without a blanket, there's no way to sleep. In the corridor, a noise.");
            if (n == 2) return Idioma.T("Um estrondo no corredor. Você sai para ver. A porta do quarto se fecha atrás de você.",
                                        "A crash in the corridor. You step out to look. The bedroom door shuts behind you.");
            if (Cfg.noite3Fuga)
                return Idioma.T("Você não consegue dormir. Do subsolo vem um rangido longo: uma porta se abrindo. Esta é a última noite.",
                                "You can't sleep. From the basement comes a long creak: a door opening. This is the last night.");
            return Cfg.noite3CaldeiraFalha
                ? Idioma.T("A caldeira silenciou. O quarto esfria depressa. Esta é a última noite.", "The boiler has gone silent. The room cools fast. This is the last night.")
                : Idioma.T("Esta é a última noite.", "This is the last night.");
        }

        string ObjetivoDaNoite(int n)
        {
            if (n == 1) return Idioma.T("Encontre um cobertor e volte ao quarto para dormir. Mantenha os possuídos na luz da lanterna.",
                                        "Find a blanket and go back to your room to sleep. Keep the possessed in your flashlight beam.");
            if (n == 2) return Idioma.T("Você está trancado fora do quarto. Ache a chave (escritório) ou um pé de cabra (oficina).",
                                        "You're locked out of your room. Find the key (office) or a crowbar (workshop).");
            if (EmFuga) return Idioma.T("FUJA! A porta principal fica no fim do corredor, a leste. Não pare.",
                                        "RUN! The main door is at the end of the east corridor. Don't stop.");
            if (Cfg.noite3Fuga) return Idioma.T("A porta atrás do arquivo do subsolo se abriu. Vá até lá.",
                                                "The door behind the basement archive has opened. Go there.");
            string s = Cfg.noite3CaldeiraFalha
                ? Idioma.T("Religue a caldeira e volte ao quarto para dormir antes de transbordar.", "Restart the boiler and get back to your room to sleep before you overflow.")
                : Idioma.T("Durma antes de transbordar.", "Sleep before you overflow.");
            if (J.Mundo.pistaDiretor && !J.Mundo.viuSalaSecreta) s += Idioma.T(" A passagem do subsolo está aberta.", " The basement passage is open.");
            return s;
        }

        // ------------------------------------------------------------------ eventos da noite

        void AoTransbordar()
        {
            if (!EhNoite || emSequencia) return;
            int n = J.Mundo.noiteAtual;
            if (n >= 3 && Cfg.p02TransbordoNoite3VaiAoFinalC)
                StartCoroutine(SequenciaFinalC());           // M-09
            else
                StartCoroutine(SequenciaTransbordo());       // M-03 / M-06
        }

        IEnumerator SequenciaTransbordo()
        {
            emSequencia = true;
            J.EmCena = true;
            J.jogador.MarcarTransbordou();
            J.audioJogo?.Tocar(SomId.Transbordo);
            if (Cfg.encontroAoTransbordar && J.encontro != null)
                yield return J.encontro.Tocar(Cfg.duracaoEncontro, "");
            // TODO(Q-20): texto e feedback finais da tela de transbordo.
            yield return J.ui.TelaTransbordo(Idioma.T("Você não conseguiu ficar acordado sem ser tomado.\nA noite recomeça.",
                                                      "You couldn't stay awake without being taken.\nThe night starts over."), 3f);
            RestaurarCheckpoint();
            int n = J.Mundo.noiteAtual;
            yield return J.ui.Cartao(Idioma.T("NOITE", "NIGHT") + $" {n}", Idioma.T("de novo, do começo", "again, from the beginning"), 1.8f);
            yield return J.ui.Fade(false, 1f);
            J.EmCena = false;
            emSequencia = false;
            J.ui.Objetivo(ObjetivoDaNoite(n));
        }

        IEnumerator SequenciaFinalC()
        {
            emSequencia = true;
            J.EmCena = true;
            transbordouNoite3 = true;                          // M-09
            J.jogador.MarcarTransbordou();
            J.audioJogo?.Tocar(SomId.Transbordo);
            if (Cfg.encontroAoTransbordar && J.encontro != null) yield return J.encontro.Tocar(Cfg.duracaoEncontro, "");
            yield return J.ui.TelaTransbordo(Idioma.T("Você parou de correr. Alguma coisa terminou de entrar.",
                                                      "You stopped running. Something finished getting in."), 3f);
            EmFuga = false;
            emSequencia = false;
            MudarEstado(EstadoMacro.FinalC);
        }

        /// <summary>
        /// OnDormiu: o SleepController já aplicou PossessionMeter.Dormir() (possessao × fatorSono).
        /// M-04, M-07: amanhece, NpcMemory.Limpar() às 06:00. M-10, M-11: fim do jogo.
        /// </summary>
        void AoDormir()
        {
            if (!EhNoite || emSequencia) return;
            StartCoroutine(SequenciaAmanhecer());
        }

        IEnumerator SequenciaAmanhecer()
        {
            emSequencia = true;
            J.EmCena = true;
            int n = J.Mundo.noiteAtual;
            yield return J.ui.Fade(true, 1.8f);
            if (n >= 3)
            {
                emSequencia = false;
                MudarEstado(J.Mundo.matouAlguem ? EstadoMacro.FinalB : EstadoMacro.FinalA); // M-10 / M-11
                yield break;
            }
            yield return J.ui.Cartao("06:00", Idioma.T("Ninguém lembra da noite. Só você.", "No one remembers the night. Only you."), 2.6f);
            J.Memoria.Limpar();                               // reset das 06:00 (D-07)
            J.Mundo.noiteAtual = n + 1;
            emSequencia = false;
            MudarEstado(n == 1 ? EstadoMacro.Dia2 : EstadoMacro.Dia3);
        }

        /// <summary>Chamado pela sala secreta: aciona a cena fixa do monstro (revelação, TODO(Q-01/Q-02)).</summary>
        public void RevelacaoSalaSecreta()
        {
            if (emSequencia) return;
            StartCoroutine(SequenciaRevelacao());
        }

        /// <summary>Verdadeiro se entrar na sala secreta agora dispara o monstro e a fuga.</summary>
        public bool PodeComecarFuga => Cfg.noite3Fuga && EhNoite && J.Mundo.noiteAtual >= 3 && !EmFuga && !emSequencia;

        /// <summary>Noite 3: a porta secreta abriu, Elias entra e dá de cara com o monstro. Depois, a fuga.</summary>
        public void EncontroComMonstro()
        {
            if (!PodeComecarFuga) return;
            StartCoroutine(SequenciaMonstroEFuga());
        }

        IEnumerator SequenciaMonstroEFuga()
        {
            emSequencia = true;
            J.EmCena = true;
            J.Mundo.viuSalaSecreta = true;
            if (Cfg.encontroNaSalaSecreta && J.encontro != null)
                yield return J.encontro.Tocar(Cfg.duracaoEncontro, "");
            IniciarFuga();
            J.ui.Tremer(0.6f, 0.6f);
            J.ui.Legenda(Idioma.T("CORRA. A porta principal está aberta, no fim do corredor leste.",
                                  "RUN. The main door is open, at the end of the east corridor."), 5f);
            J.EmCena = false;
            emSequencia = false;
            J.ui.Objetivo(ObjetivoDaNoite(3));
        }

        /// <summary>Liga a fuga: luz de alarme, porta principal aberta, possuídos extras, tudo mais rápido.</summary>
        void IniciarFuga()
        {
            EmFuga = true;
            foreach (var p in portas) p.Atualizar();
            var jog = J.jogador.Posicao;
            int extras = Cfg.fugaInimigosExtras;
            foreach (var npc in npcs)
            {
                if (!npc.soNaFuga || extras <= 0) continue;
                // não nasce em cima de Elias: só nos postos longe dele
                if (Vector2.Distance(npc.postoNoite, jog) < Cfg.fugaDistanciaMinimaExtras) continue;
                npc.AtivarNaFuga(jog);
                extras--;
            }
            J.nav?.Reconstruir();
        }

        void Update()
        {
            if (!EmFuga || J.luzGlobal == null) return;
            // luz de alarme pulsando
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
            J.luzGlobal.color = Cfg.corLuzFuga;
            J.luzGlobal.intensity = Mathf.Lerp(Cfg.intensidadeLuzGlobalNoite, Cfg.intensidadeLuzGlobalNoite * 4f, k);
        }

        /// <summary>Elias saiu pela porta principal durante a fuga. Substitui o "dormir na noite 3" (M-10/M-11).</summary>
        public void Escapou()
        {
            if (!EmFuga || emSequencia) return;
            StartCoroutine(SequenciaEscapou());
        }

        IEnumerator SequenciaEscapou()
        {
            emSequencia = true;
            J.EmCena = true;
            yield return J.ui.Fade(true, 1.5f);
            yield return J.ui.Cartao(Idioma.T("LÁ FORA", "OUTSIDE"), Idioma.T("A neve. O frio. O sanatório apagado atrás de você.",
                                                                             "The snow. The cold. The sanatorium dark behind you."), 3f);
            EmFuga = false;
            emSequencia = false;
            MudarEstado(J.Mundo.matouAlguem ? EstadoMacro.FinalB : EstadoMacro.FinalA); // M-10 / M-11 (pela saída)
        }

        IEnumerator SequenciaRevelacao()
        {
            emSequencia = true;
            J.EmCena = true;
            // Q-01/Q-02 resolvidos pelo grupo em 10/11: a sala guarda os registros da ala de vigília,
            // e a última ficha é a de Elias (mantém a ambiguidade investigador/interno, GDD 12).
            yield return J.ui.Documento(Idioma.T("Sala secreta", "Secret room"), Idioma.T(
                "Prontuários com o carimbo do Dr. Holt. Um gráfico de horas de sono por interno, noite após noite.\n" +
                "Na última ficha, a letra é a sua.\n\n" +
                "VAUGHN, E. · leito 7 · internação: rasurada.\n" +
                "Anotação do Dr. Holt: \"Acredita ser jornalista. Deixar que escreva. Escrever o acalma.\"",
                "Patient files stamped by Dr. Holt. A chart of hours slept per patient, night after night.\n" +
                "On the last file, the handwriting is yours.\n\n" +
                "VAUGHN, E. · bed 7 · admitted: scratched out.\n" +
                "Note by Dr. Holt: \"Believes he is a journalist. Let him write. Writing calms him.\""));
            J.EmCena = false;
            emSequencia = false;
        }

        // ------------------------------------------------------------------ final

        void MostrarFinal()
        {
            J.EmCena = true;
            var r = EndingResolver.ResolverCompleto(J.Mundo, transbordouNoite3, Cfg.faixasMateria, Cfg.p07EvidenciaSoContaAnotada);
            SaveJogo.Apagar();
            J.audioJogo?.PararCamadas();
            J.ui.MostrarFinal(r);
        }

        // ------------------------------------------------------------------ util

        void AplicarLuz(bool noite)
        {
            if (J.luzGlobal == null) return;
            J.luzGlobal.intensity = noite ? Cfg.intensidadeLuzGlobalNoite : Cfg.intensidadeLuzGlobalDia;
            J.luzGlobal.color = noite ? Cfg.corLuzGlobalNoite : Cfg.corLuzGlobalDia;
        }

        Vector2 Ponto(string id)
        {
            foreach (var p in pontos) if (p.id == id) return p.transform.position;
            return J.jogador.transform.position;
        }
    }
}
