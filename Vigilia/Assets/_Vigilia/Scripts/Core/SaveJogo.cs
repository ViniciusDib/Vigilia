using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// P-13 (PROPOSTA): guarda o checkpoint do início da última noite em PlayerPrefs para o botão "Continuar".
    /// Em WebGL, PlayerPrefs vai para o IndexedDB do navegador (GDD 13.4). Sem arquivos, sem threads.
    /// </summary>
    public static class SaveJogo
    {
        const string Chave = "vigilia_checkpoint_v1";

        /// <summary>O menu liga isto antes de carregar a cena Jogo; o NightManager lê e desliga.</summary>
        public static bool ContinuarPendente;

        public static bool Existe => PlayerPrefs.HasKey(Chave);

        public static void Salvar(SaveSnapshot s)
        {
            PlayerPrefs.SetString(Chave, s.ParaJson());
            PlayerPrefs.Save();
        }

        public static SaveSnapshot Carregar()
        {
            if (!Existe) return null;
            try { return SaveSnapshot.DeJson(PlayerPrefs.GetString(Chave)); }
            catch { return null; }
        }

        public static void Apagar()
        {
            PlayerPrefs.DeleteKey(Chave);
            PlayerPrefs.Save();
        }

        // Opções
        const string ChaveVolume = "vigilia_volume";
        public static float Volume
        {
            get => PlayerPrefs.GetFloat(ChaveVolume, 0.8f);
            set { PlayerPrefs.SetFloat(ChaveVolume, value); PlayerPrefs.Save(); AudioListener.volume = value; }
        }
    }
}
