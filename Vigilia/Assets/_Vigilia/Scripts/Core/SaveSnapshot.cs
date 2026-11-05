using UnityEngine;

namespace Vigilia
{
    /// <summary>Posições que a cena precisa restaurar. Os inimigos e itens são recolocados pelo setup da noite.</summary>
    [System.Serializable]
    public class PosicoesDaCena
    {
        public Vector2 jogador;
    }

    /// <summary>
    /// Checkpoint do início de cada noite (GDD 6.5 e 13.3). Guarda tudo como texto JSON:
    /// assim a cópia é sempre PROFUNDA e o snapshot nunca muda junto com o jogo.
    ///
    /// Analogia: é uma foto. Tirar a foto (Capturar) não liga a foto à cena; mexer na cena depois não muda a foto.
    /// </summary>
    [System.Serializable]
    public class SaveSnapshot
    {
        public string mundoJson;
        public string memoriaJson;
        public string posicoesJson;
        /// <summary>possessao no início da noite. Restaurada via PossessionMeter.DefinirValor (única porta de entrada).</summary>
        public float possessao;

        public bool Vazio => string.IsNullOrEmpty(mundoJson);

        public void Capturar(WorldState w, NpcMemory m, PosicoesDaCena p, float possessaoAtual)
        {
            mundoJson = JsonUtility.ToJson(w);
            memoriaJson = JsonUtility.ToJson(m);
            posicoesJson = JsonUtility.ToJson(p);
            possessao = possessaoAtual;
        }

        /// <summary>Assinatura do GDD. Copia os dados guardados para dentro dos objetos recebidos.</summary>
        public void Restaurar(WorldState w, NpcMemory m, PosicoesDaCena p)
        {
            if (Vazio) return;
            JsonUtility.FromJsonOverwrite(mundoJson, w);
            JsonUtility.FromJsonOverwrite(memoriaJson, m);
            JsonUtility.FromJsonOverwrite(posicoesJson, p);
        }

        public string ParaJson() => JsonUtility.ToJson(this);
        public static SaveSnapshot DeJson(string json) => JsonUtility.FromJson<SaveSnapshot>(json);
    }
}
