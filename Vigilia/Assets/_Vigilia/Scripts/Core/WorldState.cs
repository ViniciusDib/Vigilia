using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// O "caderno de fatos" (GDD 8.2 e 14.1): o que realmente aconteceu no jogo. Nunca reseta às 06:00.
    /// É uma classe simples serializável para o SaveSnapshot poder fazer cópia profunda via JSON.
    ///
    /// possessao NÃO fica aqui: só o PossessionMeter guarda e escreve esse valor (regra 1.2 do GDD).
    /// transbordouNoite3 também não: ele fica fora do snapshot (14.1) e vive no NightManager.
    /// </summary>
    [System.Serializable]
    public class WorldState
    {
        public int noiteAtual = 1;
        public bool temCobertor;
        public bool temChave;
        public bool temPeDeCabra;
        public bool quartoDestrancado = true;
        public bool arrombouPorta;
        public float bateria = 100f;
        public List<string> evidenciasColetadas = new List<string>();
        public List<string> evidenciasAnotadas = new List<string>();
        public bool matouAlguem;
        /// <summary>npcMorto[id] do GDD. JsonUtility não serializa dicionário, então guardamos a lista dos ids mortos.</summary>
        public List<string> npcsMortos = new List<string>();
        public int vezesTocado;
        public bool viuSalaSecreta;
        public bool pistaNomePrevio;
        public bool pistaNotaPropria;
        public bool pistaPulseira;

        // [EXTRA] fatos necessários para a implementação (não estão na tabela 14.1).
        /// <summary>Itens já pegos (ids). Itens "por noite", como pilhas, usam o id com sufixo da noite.</summary>
        public List<string> itensColetados = new List<string>();
        public int sedativos;
        public int objetosPesados;
        /// <summary>Documento do diretor que leva à sala secreta (D-16: "pista do diretor"). TODO(Q-02).</summary>
        public bool pistaDiretor;
        /// <summary>Q-17 (sugestão): na noite 3 a caldeira falha até Elias religá-la.</summary>
        public bool caldeiraFalhou;

        public bool NpcMorto(string id) => npcsMortos.Contains(id);

        public void MarcarMorto(string id)
        {
            if (!npcsMortos.Contains(id)) npcsMortos.Add(id);
            matouAlguem = true;
        }

        /// <summary>Quantas evidências contam para a matéria (P-07 liga ou desliga a exigência de anotar).</summary>
        public int EvidenciasQueContam(bool soAnotadas) => soAnotadas ? evidenciasAnotadas.Count : evidenciasColetadas.Count;

        /// <summary>Cópia profunda. JSON garante que listas não sejam compartilhadas (cuidado clássico do GDD 13.3).</summary>
        public WorldState Clonar() => JsonUtility.FromJson<WorldState>(JsonUtility.ToJson(this));

        /// <summary>Sobrescreve ESTE objeto com os dados de outro, mantendo a mesma referência para quem já a tem.</summary>
        public void CopiarDe(WorldState outro) => JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(outro), this);
    }
}
