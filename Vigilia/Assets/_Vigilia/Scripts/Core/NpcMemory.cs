using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// O que cada NPC lembra (GDD 8.2). Zerada às 06:00 pelo NightManager (único que chama Limpar()).
    /// Só o DialogueSystem e eventos presenciados escrevem aqui.
    /// </summary>
    [System.Serializable]
    public class NpcMemory
    {
        [System.Serializable]
        public class Entrada
        {
            public string npcId;
            public List<string> eventos = new List<string>();
        }

        public List<Entrada> sabe = new List<Entrada>();

        public bool Sabe(string npcId, string evento)
        {
            var e = sabe.Find(x => x.npcId == npcId);
            return e != null && e.eventos.Contains(evento);
        }

        public void Registrar(string npcId, string evento)
        {
            var e = sabe.Find(x => x.npcId == npcId);
            if (e == null)
            {
                e = new Entrada { npcId = npcId };
                sabe.Add(e);
            }
            if (!e.eventos.Contains(evento)) e.eventos.Add(evento);
        }

        public void Limpar() => sabe.Clear();

        public NpcMemory Clonar() => JsonUtility.FromJson<NpcMemory>(JsonUtility.ToJson(this));
        public void CopiarDe(NpcMemory outra) => JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(outra), this);
    }
}
