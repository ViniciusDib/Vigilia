using UnityEngine;

namespace Vigilia
{
    public enum Lingua { PT, EN }

    /// <summary>
    /// Idioma do jogo (português ou inglês), escolhido no menu e salvo em PlayerPrefs (WebGL não tem arquivos).
    ///
    /// Como traduzir um texto: troque "texto" por Idioma.T("texto", "text").
    /// O português e o inglês ficam lado a lado no código, então dá para ver as duas versões sem abrir outro arquivo.
    /// Textos longos (finais, diálogos) têm tabelas próprias em inglês (MateriaTextos, DialogueSystem).
    ///
    /// O idioma é lido quando cada tela é montada; por isso trocar o idioma recarrega o menu.
    /// </summary>
    public static class Idioma
    {
        const string Chave = "vigilia_idioma";
        static Lingua? atual;

        public static Lingua Atual
        {
            get
            {
                if (atual == null) atual = PlayerPrefs.GetInt(Chave, 0) == 1 ? Lingua.EN : Lingua.PT;
                return atual.Value;
            }
            set
            {
                atual = value;
                PlayerPrefs.SetInt(Chave, value == Lingua.EN ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool Ingles => Atual == Lingua.EN;

        /// <summary>Devolve o texto no idioma atual.</summary>
        public static string T(string pt, string en) => Ingles ? en : pt;

        public static void Alternar() => Atual = Ingles ? Lingua.PT : Lingua.EN;

        /// <summary>Texto gravado na cena (em português) traduzido pela tabela TextosCenaEN. Sem tradução, devolve o original.</summary>
        public static string Cena(string pt)
        {
            if (!Ingles || string.IsNullOrEmpty(pt)) return pt;
            return TextosCenaEN.Tabela.TryGetValue(pt.Replace("\r", ""), out var en) ? en : pt;
        }
    }
}
