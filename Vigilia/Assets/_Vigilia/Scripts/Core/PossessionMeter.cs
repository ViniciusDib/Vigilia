using System;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Conta pura da banheira (GDD 6.4). Fica separada do MonoBehaviour para dar para testar sem abrir o Unity.
    /// </summary>
    public static class CalculoPossessao
    {
        /// <summary>Quanto sobe por segundo: taxaBase × multSala × multLucido.</summary>
        public static float Vazao(BalanceConfig cfg, float multSala, float multLucido)
            => cfg.taxaBase * multSala * multLucido;

        /// <summary>Um quadro: soma a vazão × dt e limita a 0..max.</summary>
        public static float Passo(BalanceConfig cfg, float valor, float multSala, float multLucido, float dt)
            => Limitar(cfg, valor + Vazao(cfg, multSala, multLucido) * dt);

        public static float Limitar(BalanceConfig cfg, float v) => Mathf.Clamp(v, 0f, cfg.maxPossessao);

        public static float AposDormir(BalanceConfig cfg, float v) => Limitar(cfg, v * cfg.fatorSono);
    }

    /// <summary>
    /// A banheira (GDD 6). ÚNICA autoridade sobre possessao: ninguém mais escreve esse valor.
    /// Portas de entrada: AdicionarPossessao (pulsos), Dormir (ralo) e DefinirValor (só para restaurar checkpoint).
    /// </summary>
    public class PossessionMeter : MonoBehaviour
    {
        /// <summary>possessao, 0..100.</summary>
        public float Valor { get; private set; }

        /// <summary>Dispara UMA vez por noite quando possessao chega ao máximo.</summary>
        public event Action OnTransbordou;

        /// <summary>Valor normalizado 0..1, útil para efeitos de tela e som (P-10).</summary>
        public float Nivel01 => Jogo.Cfg.maxPossessao > 0 ? Valor / Jogo.Cfg.maxPossessao : 0f;

        bool transbordouNestaNoite;

        /// <summary>Pulsos: toque (pulsoToque) e barulho (pulsoBarulho).</summary>
        public void AdicionarPossessao(float x) => Escrever(Valor + x);

        /// <summary>Ralo: Valor = Valor × fatorSono.</summary>
        public void Dormir() => Valor = CalculoPossessao.AposDormir(Jogo.Cfg, Valor);

        /// <summary>Só para SaveSnapshot.Restaurar e para começar um jogo novo.</summary>
        public void DefinirValor(float v)
        {
            Valor = CalculoPossessao.Limitar(Jogo.Cfg, v);
            transbordouNestaNoite = false;
        }

        /// <summary>Chamado pelo NightManager no início de cada tentativa de noite.</summary>
        public void IniciarNoite() => transbordouNestaNoite = false;

        void Update()
        {
            var jogo = Jogo.I;
            if (jogo == null || !jogo.PossessaoAtiva) return;

            // Ordem do GDD 6.4: 1) multiplicadores, 2) soma, 3) limite, 4) transbordo.
            float multSala = jogo.temperatura != null ? jogo.temperatura.MultSalaAtual : 1f;
            float multLucido = jogo.caderno != null ? jogo.caderno.MultLucidoAtual : 1f;
            if (jogo.noites != null && jogo.noites.EmFuga) multLucido *= Jogo.Cfg.fugaMultPossessao; // fuga: tudo mais urgente
            Escrever(Valor + CalculoPossessao.Vazao(Jogo.Cfg, multSala, multLucido) * Time.deltaTime);
        }

        void Escrever(float v)
        {
            Valor = CalculoPossessao.Limitar(Jogo.Cfg, v);
            if (Valor >= Jogo.Cfg.maxPossessao && !transbordouNestaNoite && Jogo.I != null && Jogo.I.EhNoite)
            {
                transbordouNestaNoite = true;
                OnTransbordou?.Invoke();
            }
        }
    }
}
