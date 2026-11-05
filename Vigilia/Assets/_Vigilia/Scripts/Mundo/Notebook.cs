using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// O caderno na escrivaninha do quarto (GDD 6.3, D-13, P-07).
    /// Escrever: as evidências coletadas viram evidenciasAnotadas e Elias fica lúcido por duracaoLucido
    /// (a vazão da banheira é multiplicada por multLucido).
    /// </summary>
    public class Notebook : Interativo
    {
        float lucidoRestante;

        /// <summary>multLucido da fórmula: 1 normalmente, cfg.multLucido enquanto lúcido.</summary>
        public float MultLucidoAtual => lucidoRestante > 0f ? Jogo.Cfg.multLucido : 1f;
        public bool Lucido => lucidoRestante > 0f;
        public float LucidoRestante => lucidoRestante;

        int Novas
        {
            get
            {
                int n = 0;
                foreach (var e in Mundo.evidenciasColetadas) if (!Mundo.evidenciasAnotadas.Contains(e)) n++;
                return n;
            }
        }

        public override string Prompt(PlayerController p)
        {
            int n = Novas;
            return n > 0 ? $"[E] Escrever no caderno ({n} evidência{(n > 1 ? "s" : "")} nova{(n > 1 ? "s" : "")})" : "[E] Escrever no caderno";
        }

        public override void Interagir(PlayerController p)
        {
            p.Escrever(Jogo.Cfg.tempoEscrita, Anotar);  // J-10 → J-11
        }

        void Anotar()
        {
            int n = 0;
            foreach (var e in Mundo.evidenciasColetadas)
                if (!Mundo.evidenciasAnotadas.Contains(e)) { Mundo.evidenciasAnotadas.Add(e); n++; }
            lucidoRestante = Jogo.Cfg.duracaoLucido;
            J.ui.Mensagem(n > 0
                ? $"Anotado: {n} evidência{(n > 1 ? "s" : "")}. Escrever acalma. Você está lúcido por um tempo."
                : "Você escreve o que lembra. Escrever acalma. Você está lúcido por um tempo.");
        }

        /// <summary>O estado lúcido não está no snapshot: ao restaurar a noite, ele zera.</summary>
        public void ResetarLucido() => lucidoRestante = 0f;

        void Update()
        {
            if (lucidoRestante > 0f && Jogo.I != null && !Jogo.I.Travado)
                lucidoRestante -= Time.deltaTime;
        }
    }
}
