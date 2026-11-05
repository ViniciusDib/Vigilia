using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Base de tudo que Elias pode usar com a tecla E (e, em alguns casos, uma ação alternativa com G).
    /// O PlayerController procura o Interativo mais perto e mostra o Prompt na tela.
    /// </summary>
    public abstract class Interativo : MonoBehaviour
    {
        public static readonly List<Interativo> Todos = new List<Interativo>();

        protected virtual void OnEnable() => Todos.Add(this);
        protected virtual void OnDisable() => Todos.Remove(this);

        protected Jogo J => Jogo.I;
        protected WorldState Mundo => Jogo.I.Mundo;

        /// <summary>Se dá para interagir agora (ex.: esconderijo só à noite).</summary>
        public virtual bool Disponivel(PlayerController p) => isActiveAndEnabled;

        /// <summary>Texto mostrado na tela. Se a ação não for possível, explique por quê (feedback, rubrica 3).</summary>
        public abstract string Prompt(PlayerController p);

        /// <summary>Se maior que zero, é preciso segurar E por esse tempo (ex.: abrir com a chave).</summary>
        public virtual float TempoSegurar(PlayerController p) => 0f;

        /// <summary>Se falso, o prompt aparece mas E não faz nada (ex.: porta trancada sem chave).</summary>
        public virtual bool PodeUsar(PlayerController p) => true;

        public abstract void Interagir(PlayerController p);

        /// <summary>Ação alternativa na tecla G (ex.: arrombar com pé de cabra). Nulo = não existe.</summary>
        public virtual string PromptAlternativo(PlayerController p) => null;
        public virtual void InteragirAlternativo(PlayerController p) { }

        public Vector2 Posicao => transform.position;
    }
}
