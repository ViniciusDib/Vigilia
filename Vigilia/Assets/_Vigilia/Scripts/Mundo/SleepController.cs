using System;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// A cama de Elias (GDD 13.2: "Ação de dormir").
    /// À noite: se o quarto está quente e a porta aberta (P-05), chama PossessionMeter.Dormir() e emite OnDormiu.
    /// De dia: deitar encerra o dia e começa a noite. TODO(Q-18): como o dia termina ainda está em aberto.
    /// </summary>
    public class SleepController : Interativo
    {
        public event Action OnDormiu;

        public override string Prompt(PlayerController p)
        {
            if (!J.EhNoite) return Idioma.T("[E] Deitar e esperar a noite", "[E] Lie down and wait for night");
            return PodeDormir(out var motivo) ? Idioma.T("[E] Dormir", "[E] Sleep") : motivo;
        }

        public override bool PodeUsar(PlayerController p) => !J.EhNoite || PodeDormir(out _);

        public bool PodeDormir(out string motivo)
        {
            var cfg = Jogo.Cfg;
            var w = Mundo;
            if (cfg.noite3Fuga && J.EhNoite && w.noiteAtual >= 3)
            {
                motivo = J.noites.EmFuga ? Idioma.T("Não há tempo. FUJA.", "No time. RUN.")
                                         : Idioma.T("Impossível dormir esta noite. Algo chama do subsolo.", "No sleeping tonight. Something is calling from the basement.");
                return false;
            }
            var quarto = J.temperatura != null ? J.temperatura.Quarto : null;
            bool quente = quarto == null || quarto.TemperaturaAtual == TemperaturaSala.Quente;
            if (cfg.p05DormirExigeQuartoQuente && !quente)
            {
                if (!w.temCobertor) motivo = Idioma.T("Frio demais para dormir. Preciso de um cobertor.", "Too cold to sleep. I need a blanket.");
                else if (w.caldeiraFalhou) motivo = Idioma.T("A caldeira apagou. O quarto está gelado. Preciso religá-la.", "The boiler went out. The room is freezing. I need to restart it.");
                else motivo = Idioma.T("Frio demais para dormir.", "Too cold to sleep.");
                return false;
            }
            if (cfg.p05DormirExigePortaAberta && !w.quartoDestrancado)
            {
                motivo = Idioma.T("A porta precisa estar aberta.", "The door needs to be open.");
                return false;
            }
            motivo = null;
            return true;
        }

        public override void Interagir(PlayerController p)
        {
            if (!J.EhNoite)
            {
                J.noites.TerminarDia();                 // M-02 / M-05 / M-08
                return;
            }
            if (!PodeDormir(out _)) return;
            p.Dormir();                                 // J-12: No quarto → Dormindo
            J.possessao.Dormir();                       // possessao × fatorSono (D-09)
            OnDormiu?.Invoke();                         // NightManager: amanhecer ou final (M-04, M-07, M-10, M-11)
        }
    }
}
