using UnityEngine;

namespace Vigilia
{
    /// <summary>Deixa conversar com o NPC de dia (tecla E). À noite ele é um possuído e não conversa.</summary>
    [RequireComponent(typeof(EnemyAI))]
    public class ConversaNpc : Interativo
    {
        EnemyAI npc;

        void Awake() => npc = GetComponent<EnemyAI>();

        public override bool Disponivel(PlayerController p)
            => base.Disponivel(p) && !J.EhNoite && !npc.EhNoturno;

        public override string Prompt(PlayerController p) => Idioma.T("[E] Falar com ", "[E] Talk to ") + npc.Nome;

        public override void Interagir(PlayerController p) => J.dialogo.Conversar(npc);
    }
}
