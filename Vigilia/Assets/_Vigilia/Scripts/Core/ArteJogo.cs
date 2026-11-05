using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Procura arte pelo nome: primeiro a arte FINAL (Resources/Arte), depois a provisória (Resources/Gerado).
    /// Para trocar qualquer imagem do jogo, basta colocar um PNG com o mesmo nome em Resources/Arte.
    /// Lista de nomes e prompts: docs/prompts-arte.md.
    /// </summary>
    public static class ArteJogo
    {
        public static Sprite Sprite(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return null;
            var s = Resources.Load<Sprite>("Arte/" + nome);
            return s != null ? s : Resources.Load<Sprite>("Gerado/" + nome);
        }

        /// <summary>Verdadeiro se já existe a versão final desta arte.</summary>
        public static bool TemFinal(string nome) => Resources.Load<Sprite>("Arte/" + nome) != null;
    }
}
