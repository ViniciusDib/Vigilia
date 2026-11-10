using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Esconderijo (armário, baixo da cama). Faz o inimigo perder o alvo (J-03, E-06).
    /// TODO(Q-07): tipos, regras e tempo seguro são decisão do grupo. Hoje: tempoMaxEscondido em BalanceConfig;
    /// passado esse tempo, um possuído perto encontra Elias (J-04).
    /// </summary>
    public class HidingSpot : Interativo
    {
        public string nome = "armário";
        [Tooltip("Posição na fila. A noite N libera os esconderijosPorNoite[N] primeiros.")]
        public int ordem;
        public SpriteRenderer visual;

        bool utilizavel;
        public bool Ocupado { get; set; }

        public void Configurar(bool noite, int n)
        {
            utilizavel = noite && ordem < BalanceConfig.PorNoite(Jogo.Cfg.esconderijosPorNoite, n);
            Ocupado = false;
            if (visual != null) visual.color = utilizavel ? Color.white : new Color(0.75f, 0.75f, 0.75f);
        }

        public override bool Disponivel(PlayerController p) => base.Disponivel(p) && utilizavel && J.EhNoite;

        public override string Prompt(PlayerController p)
            => Ocupado ? Idioma.T("[E] Sair", "[E] Come out") : Idioma.T("[E] Esconder-se", "[E] Hide") + $" ({Idioma.Cena(nome)})";

        public override void Interagir(PlayerController p)
        {
            if (Ocupado) p.SairDoEsconderijo();
            else p.Esconder(this);
        }
    }
}
