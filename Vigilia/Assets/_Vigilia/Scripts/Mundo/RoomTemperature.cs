using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Descobre em que sala Elias está e expõe o multiplicador dessa sala para o PossessionMeter (GDD 13.2).
    /// Também dispara a revelação da sala secreta na primeira entrada (viuSalaSecreta).
    /// </summary>
    public class RoomTemperature : MonoBehaviour
    {
        Sala[] salas;
        public Sala SalaAtual { get; private set; }

        public TemperaturaSala TemperaturaAtual => SalaAtual != null ? SalaAtual.TemperaturaAtual : TemperaturaSala.Morna;

        /// <summary>multSala da fórmula: fria × 2, morna × 1, quente × 0.5 (valores em BalanceConfig).</summary>
        public float MultSalaAtual => Jogo.Cfg.MultiplicadorSala(TemperaturaAtual);

        public bool NoQuarto => SalaAtual != null && SalaAtual.ehQuarto;

        void Start()
        {
            var raiz = Jogo.I.mapa != null ? Jogo.I.mapa : transform.root;
            salas = raiz.GetComponentsInChildren<Sala>(true);
        }

        public Sala SalaEm(Vector2 p)
        {
            if (salas == null) return null;
            foreach (var s in salas) if (s.Contem(p)) return s;
            return null;
        }

        public Sala Quarto
        {
            get
            {
                if (salas != null) foreach (var s in salas) if (s.ehQuarto) return s;
                return null;
            }
        }

        void Update()
        {
            var j = Jogo.I;
            if (j == null || j.jogador == null) return;
            var nova = SalaEm(j.jogador.transform.position);
            if (nova != null && nova != SalaAtual)
            {
                SalaAtual = nova;
                j.ui?.NomeSala(Idioma.Cena(nova.nome));   // ajuda a reconhecer cada cômodo
            }
            if (SalaAtual == null || j.Travado) return;

            if (SalaAtual.ehSalaSecreta)
            {
                if (j.noites.PodeComecarFuga)
                    j.noites.EncontroComMonstro();          // noite 3: o monstro, e a fuga começa
                else if (!j.Mundo.viuSalaSecreta && !j.EhNoite)
                {
                    j.Mundo.viuSalaSecreta = true;          // GDD 3.4: seguir a pista do diretor até a sala secreta
                    j.noites.RevelacaoSalaSecreta();
                }
            }

            if (SalaAtual.ehSaida && j.noites.EmFuga)
                j.noites.Escapou();                         // saiu pela porta principal
        }
    }
}
