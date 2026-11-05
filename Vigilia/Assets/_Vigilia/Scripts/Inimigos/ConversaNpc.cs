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

        public override string Prompt(PlayerController p) => $"[E] Falar com {npc.nome}";

        public override void Interagir(PlayerController p) => J.dialogo.Conversar(npc);
    }
}
