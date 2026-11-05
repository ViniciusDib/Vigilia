using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Vigilia
{
    /// <summary>
    /// Porta que pode trancar. Duas usadas no jogo:
    /// - Quarto: na noite 2 Elias fica trancado fora (D-16). Abre com a chave (lento) ou com o pé de cabra (rápido e barulhento, P-03).
    /// - Secreta: passagem do subsolo para a sala secreta; abre quando Elias acha a pista do diretor (TODO(Q-02)).
    /// Fechada, ela tem colisor na layer Paredes: bloqueia o jogador, os inimigos, a luz e o raycast da lanterna.
    /// </summary>
    public class Porta : Interativo
    {
        public enum Tipo { Quarto, Secreta, Saida }

        public static readonly List<Porta> Todas = new List<Porta>();

        public Tipo tipo;
        public Collider2D colisor;
        public SpriteRenderer visual;
        public ShadowCaster2D sombra;

        bool aberta = true;
        public bool Aberta => aberta;

        void Awake() { if (!Todas.Contains(this)) Todas.Add(this); }
        void OnDestroy() => Todas.Remove(this);

        public void Configurar(bool noite, int n) => Atualizar();

        /// <summary>Lê o WorldState e aplica: aberta ou fechada.</summary>
        public void Atualizar()
        {
            if (Jogo.I == null) return;
            bool novaAberta;
            switch (tipo)
            {
                case Tipo.Quarto: novaAberta = Mundo.quartoDestrancado; break;
                // noite 3: a porta secreta se abre sozinha (decisão do grupo, 04/11); de dia 3, com a pista do diretor
                case Tipo.Secreta:
                    novaAberta = Mundo.pistaDiretor || (Jogo.Cfg.noite3Fuga && J.EhNoite && Mundo.noiteAtual >= 3);
                    break;
                default: novaAberta = J.noites != null && J.noites.EmFuga; break; // porta principal só abre na fuga
            }
            bool mudou = novaAberta != aberta;
            aberta = novaAberta;
            if (colisor != null) colisor.enabled = !aberta;
            if (sombra != null) sombra.castsShadows = !aberta;
            if (visual != null)
            {
                var c = visual.color;
                c.a = aberta ? 0.18f : 1f;
                visual.color = c;
            }
            if (mudou)
            {
                Physics2D.SyncTransforms();
                J.nav?.Reconstruir();
            }
        }

        public override bool Disponivel(PlayerController p) => base.Disponivel(p) && !aberta;

        public override string Prompt(PlayerController p)
        {
            if (tipo == Tipo.Secreta) return "Trancada. Atrás das prateleiras há uma passagem... (falta alguma pista)";
            if (tipo == Tipo.Saida) return "A porta principal. Trancada por fora.";
            if (Mundo.temChave) return "[Segure E] Abrir com a chave";
            if (Mundo.temPeDeCabra) return "Trancada. [G] Arrombar com o pé de cabra";
            return "Trancada. Chave: escritório do diretor · Pé de cabra: oficina";
        }

        public override bool PodeUsar(PlayerController p) => tipo == Tipo.Quarto && Mundo.temChave;

        public override float TempoSegurar(PlayerController p) => Jogo.Cfg.tempoAbrirComChave;

        public override void Interagir(PlayerController p)
        {
            if (!PodeUsar(p)) return;
            Mundo.quartoDestrancado = true;
            J.audioJogo?.Tocar(SomId.Porta);
            J.ui.Mensagem("A fechadura cede. O quarto está aberto.");
            Atualizar();
        }

        public override string PromptAlternativo(PlayerController p)
            => tipo == Tipo.Quarto && Mundo.temPeDeCabra && Mundo.temChave ? "[G] Arrombar com o pé de cabra" : null;

        public override void InteragirAlternativo(PlayerController p)
        {
            if (tipo != Tipo.Quarto || !Mundo.temPeDeCabra) return;
            Mundo.quartoDestrancado = true;
            Mundo.arrombouPorta = true;                                 // GDD 3.4: bloco "porta arrombada"
            if (Jogo.Cfg.p03PeDeCabraFazBarulho)
            {
                J.possessao.AdicionarPossessao(Jogo.Cfg.pulsoBarulho);  // P-03: +10
                Barulho.Emitir(transform.position, Jogo.Cfg.raioBarulhoPeDeCabra);
            }
            J.audioJogo?.Tocar(SomId.Pancada);
            J.ui.Tremer(0.35f, 0.25f);
            J.ui.Mensagem("CRACK. A porta cede. Isso deve ter sido ouvido.");
            Atualizar();
        }
    }
}
