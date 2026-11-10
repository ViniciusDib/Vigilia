using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// TODO(Q-17): o que força Elias a sair do quarto na noite 3 ainda está em aberto.
    /// Esta é a sugestão do GDD 6.7: a caldeira falha, o quarto esfria e religá-la exige ir até ela.
    /// Liga/desliga em BalanceConfig.noite3CaldeiraFalha.
    /// </summary>
    public class Caldeira : Interativo
    {
        public SpriteRenderer visual;

        public void Configurar(bool noite, int n) => AtualizarVisual();

        public override bool Disponivel(PlayerController p) => base.Disponivel(p) && J.EhNoite && Mundo.caldeiraFalhou;

        public override string Prompt(PlayerController p) => Idioma.T("[Segure E] Religar a caldeira", "[Hold E] Restart the boiler");

        public override float TempoSegurar(PlayerController p) => Jogo.Cfg.tempoReligarCaldeira;

        public override void Interagir(PlayerController p)
        {
            Mundo.caldeiraFalhou = false;
            J.audioJogo?.Tocar(SomId.Caldeira);
            J.ui.Mensagem(Idioma.T("A caldeira volta a roncar. O quarto vai esquentar.", "The boiler roars back to life. The room will warm up."));
            AtualizarVisual();
        }

        void Update() => AtualizarVisual();

        void AtualizarVisual()
        {
            if (visual == null || Jogo.I == null) return;
            visual.color = Mundo.caldeiraFalhou ? new Color(0.45f, 0.45f, 0.5f) : new Color(1f, 0.75f, 0.55f);
        }
    }
}
