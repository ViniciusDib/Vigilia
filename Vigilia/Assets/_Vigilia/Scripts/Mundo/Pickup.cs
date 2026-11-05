using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Item no chão. Cada item sabe em que dias e noites aparece. Ao pegar, o efeito vai para o WorldState.
    /// Itens "por noite" (pilha, sedativo, objeto pesado) voltam na noite seguinte; os outros somem para sempre.
    /// Como o WorldState está no snapshot, transbordar devolve os itens pegos naquela noite (GDD 6.5).
    /// </summary>
    public class Pickup : Interativo
    {
        public string id;
        public TipoItem tipo;
        public string titulo = "item";
        [Tooltip("Volta a aparecer em cada noite (ex.: pilhas).")]
        public bool porNoite;
        [Tooltip("Dias em que aparece (índice 0 = dia 1).")]
        public bool[] disponivelDia = new bool[3];
        [Tooltip("Noites em que aparece (índice 0 = noite 1).")]
        public bool[] disponivelNoite = new bool[3];
        [Tooltip("Pilhas: posição na fila. A noite N liga as pilhasPorNoite[N] primeiras. A ordem 0 deve ser alcançável logo no início (anti dead end, GDD 7.2). -1 = fora da fila.")]
        public int ordemPilha = -1;

        [Header("Documento (notas, evidências, pistas)")]
        [TextArea(3, 8)] public string textoDocumento;

        string ChaveColeta => porNoite ? $"{id}#n{Mundo.noiteAtual}" : id;

        public void Configurar(bool noite, int n)
        {
            bool disponivel;
            if (noite && ordemPilha >= 0) disponivel = ordemPilha < BalanceConfig.PorNoite(Jogo.Cfg.pilhasPorNoite, n);
            else disponivel = BalanceConfig.PorNoite(noite ? disponivelNoite : disponivelDia, n);
            bool coletado = Mundo.itensColetados.Contains(ChaveColeta);
            gameObject.SetActive(disponivel && !coletado);
        }

        public override string Prompt(PlayerController p) => $"[E] Pegar {titulo}";

        public override void Interagir(PlayerController p)
        {
            var w = Mundo;
            w.itensColetados.Add(ChaveColeta);
            string msg = $"Pegou: {titulo}";
            switch (tipo)
            {
                case TipoItem.Pilha:
                    J.lanterna.Recarregar();
                    msg = "Pilha nova na lanterna.";
                    break;
                case TipoItem.Cobertor:
                    w.temCobertor = true;
                    msg = "Um cobertor grosso. Agora o quarto pode ficar quente.";
                    break;
                case TipoItem.Chave: w.temChave = true; msg = "A chave do quarto."; break;
                case TipoItem.PeDeCabra: w.temPeDeCabra = true; msg = "Um pé de cabra. Rápido, mas barulhento."; break;
                case TipoItem.Evidencia:
                    if (!w.evidenciasColetadas.Contains(id)) w.evidenciasColetadas.Add(id);
                    msg = $"Evidência: {titulo}. Anote no caderno do quarto para não esquecer.";
                    break;
                case TipoItem.NotaPropria: w.pistaNotaPropria = true; break;
                case TipoItem.Pulseira: w.pistaPulseira = true; break;
                case TipoItem.PistaDiretor:
                    w.pistaDiretor = true;
                    foreach (var porta in Porta.Todas) porta.Atualizar();
                    break;
                case TipoItem.Sedativo: w.sedativos++; msg = "Um sedativo. [Q] num possuído parado na luz."; break;
                case TipoItem.ObjetoPesado: w.objetosPesados++; msg = "Algo pesado. [R] num possuído atordoado. Não tem volta."; break;
            }

            J.audioJogo?.Tocar(SomId.Pegar);
            gameObject.SetActive(false);
            if (!string.IsNullOrEmpty(textoDocumento)) J.ui.AbrirDocumento(titulo, textoDocumento);
            else J.ui.Mensagem(msg);
        }
    }
}
