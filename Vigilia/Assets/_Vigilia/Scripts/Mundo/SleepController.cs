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
            if (!J.EhNoite) return "[E] Deitar e esperar a noite";
            return PodeDormir(out var motivo) ? "[E] Dormir" : motivo;
        }

        public override bool PodeUsar(PlayerController p) => !J.EhNoite || PodeDormir(out _);

        public bool PodeDormir(out string motivo)
        {
            var cfg = Jogo.Cfg;
            var w = Mundo;
            if (cfg.noite3Fuga && J.EhNoite && w.noiteAtual >= 3)
            {
                motivo = J.noites.EmFuga ? "Não há tempo. FUJA." : "Impossível dormir esta noite. Algo chama do subsolo.";
                return false;
            }
            var quarto = J.temperatura != null ? J.temperatura.Quarto : null;
            bool quente = quarto == null || quarto.TemperaturaAtual == TemperaturaSala.Quente;
            if (cfg.p05DormirExigeQuartoQuente && !quente)
            {
                if (!w.temCobertor) motivo = "Frio demais para dormir. Preciso de um cobertor.";
                else if (w.caldeiraFalhou) motivo = "A caldeira apagou. O quarto está gelado. Preciso religá-la.";
                else motivo = "Frio demais para dormir.";
                return false;
            }
            if (cfg.p05DormirExigePortaAberta && !w.quartoDestrancado)
            {
                motivo = "A porta precisa estar aberta.";
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
