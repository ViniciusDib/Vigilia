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

        /// <summary>Nome do item no idioma atual (tabela TextosCenaEN).</summary>
        public string Titulo => Idioma.Cena(titulo);

        public override string Prompt(PlayerController p) => Idioma.T("[E] Pegar ", "[E] Take ") + Titulo;

        public override void Interagir(PlayerController p)
        {
            var w = Mundo;
            w.itensColetados.Add(ChaveColeta);
            string msg = Idioma.T("Pegou: ", "Picked up: ") + Titulo;
            switch (tipo)
            {
                case TipoItem.Pilha:
                    J.lanterna.Recarregar();
                    msg = Idioma.T("Pilha nova na lanterna.", "Fresh battery in the flashlight.");
                    break;
                case TipoItem.Cobertor:
                    w.temCobertor = true;
                    msg = Idioma.T("Um cobertor grosso. Agora o quarto pode ficar quente.", "A thick blanket. Now the room can stay warm.");
                    break;
                case TipoItem.Chave: w.temChave = true; msg = Idioma.T("A chave do quarto.", "The room key."); break;
                case TipoItem.PeDeCabra: w.temPeDeCabra = true; msg = Idioma.T("Um pé de cabra. Rápido, mas barulhento.", "A crowbar. Fast, but loud."); break;
                case TipoItem.Evidencia:
                    if (!w.evidenciasColetadas.Contains(id)) w.evidenciasColetadas.Add(id);
                    msg = Idioma.T($"Evidência: {Titulo}. Anote no caderno do quarto para não esquecer.",
                                   $"Evidence: {Titulo}. Write it in the notebook in your room so you don't forget.");
                    break;
                case TipoItem.NotaPropria: w.pistaNotaPropria = true; break;
                case TipoItem.Pulseira: w.pistaPulseira = true; break;
                case TipoItem.PistaDiretor:
                    w.pistaDiretor = true;
                    foreach (var porta in Porta.Todas) porta.Atualizar();
                    break;
                case TipoItem.Sedativo: w.sedativos++; msg = Idioma.T("Um sedativo. [Q] num possuído parado na luz.", "A sedative. [Q] on a possessed one frozen in the light."); break;
                case TipoItem.ObjetoPesado: w.objetosPesados++; msg = Idioma.T("Algo pesado. [R] num possuído atordoado. Não tem volta.", "Something heavy. [R] on a stunned possessed one. There's no going back."); break;
            }

            J.audioJogo?.Tocar(SomId.Pegar);
            gameObject.SetActive(false);
            if (!string.IsNullOrEmpty(textoDocumento)) J.ui.AbrirDocumento(Titulo, Idioma.Cena(textoDocumento));
            else J.ui.Mensagem(msg);
        }
    }
}
