using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    /// <summary>Uma fala: quem fala e o texto.</summary>
    public struct Linha
    {
        public string falanteId;
        public string falante;
        public string texto;
        public Linha(string id, string nome, string texto) { falanteId = id; falante = nome; this.texto = texto; }
    }

    /// <summary>
    /// Conversas de dia com retrato ao lado (estilo Pokémon, GDD 5.3).
    /// Regra do GDD 8.3: a linha é escolhida combinando os dois cadernos.
    /// - Fato existe no WorldState mas o NPC não tem em NpcMemory → ele age como se nunca tivesse acontecido.
    /// - npcMorto[x] → os NPCs negam que x existiu ("Dr. Almeida? Não conheço nenhum Almeida").
    /// Os textos estão em DialogoDados. TODO(Q-03, Q-19): roteiro final do grupo.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        Jogo J => Jogo.I;

        public void Conversar(EnemyAI npc)
        {
            if (J.EmDialogo) return;
            var linhas = DialogoDados.Montar(npc, J.Mundo, J.Memoria, J.Noite);
            J.EmDialogo = true;
            J.audioJogo?.Tocar(SomId.Dialogo);
            J.ui.MostrarDialogo(linhas, () =>
            {
                J.EmDialogo = false;
                J.Memoria.Registrar(npc.npcId, "conversouHoje");
                if (DialogoDados.SegueDepoisDeConversar(npc.npcId)) npc.ComecarASeguir();   // E-01
            });
        }

        /// <summary>Retrato de cada personagem: arte final em Resources/Arte, senão a provisória (ArteJogo).</summary>
        public static Sprite Retrato(string id, bool noite)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var s = ArteJogo.Sprite($"retrato_{id}_{(noite ? "noite" : "dia")}");
            return s != null ? s : ArteJogo.Sprite($"retrato_{id}_dia");
        }
    }

    /// <summary>
    /// Falas do dia. RASCUNHO para o jogo rodar. TODO(Q-03): o grupo reescreve.
    /// Escrita deliberadamente ambígua (GDD 12): funciona lendo Elias como investigador ou como paciente.
    /// </summary>
    public static class DialogoDados
    {
        static readonly Dictionary<string, string> nomes = new Dictionary<string, string>
        {
            ["elias"] = "Elias", ["marta"] = "Marta Quill", ["otis"] = "Otis Brandt", ["almeida"] = "Dr. Henrik Almeida",
            ["wren"] = "Sr. Wren", ["holt"] = "Dr. Alaric Holt", ["paciente1"] = "Paciente", ["paciente2"] = "Paciente",
        };

        public static string Nome(string id) => nomes.TryGetValue(id, out var n) ? n : id;

        public static bool SegueDepoisDeConversar(string id) => id == "marta" || id == "wren";

        static Linha L(string id, string texto) => new Linha(id, Nome(id), texto);
        static Linha E(string texto) => new Linha("elias", "Elias", texto);

        public static List<Linha> Montar(EnemyAI npc, WorldState w, NpcMemory m, int dia)
        {
            var r = new List<Linha>();
            string id = npc.npcId;
            bool jaFalouHoje = m.Sabe(id, "conversouHoje");

            // 8.3: quem morreu deixou de existir para os outros (Q-19: uma fala por NPC e por fato).
            foreach (var morto in w.npcsMortos)
            {
                if (morto == id || m.Sabe(id, "negou_" + morto)) continue;
                r.Add(E($"Onde está {Nome(morto)}? Não vejo desde ontem à noite."));
                r.Add(L(id, Negacao(id, morto)));
                m.Registrar(id, "negou_" + morto);
                return r;
            }

            switch (id)
            {
                case "marta":
                    if (dia == 1 && !jaFalouHoje)
                    {
                        r.Add(L(id, "Elias! Que bom que voltou. Digo... que bom que chegou. A estrada deve ter sido horrível."));
                        r.Add(E("Eu não disse meu nome."));
                        r.Add(L(id, "Disse sim, querido. Na portaria. Todo mundo aqui sabe quem você é."));
                        w.pistaNomePrevio = true; // pista 1 (GDD 12)
                    }
                    else if (w.arrombouPorta && !m.Sabe(id, "viuPortaQuebrada"))
                    {
                        // Q-06 (sugestão): Marta comenta a porta quebrada no dia seguinte.
                        r.Add(L(id, "Alguém arrombou a porta do seu quarto. Esses internos... não se lembram de nada, coitados."));
                        r.Add(E("E você? Lembra de ontem à noite?"));
                        r.Add(L(id, "Dormi a noite inteira. Como sempre."));
                        m.Registrar(id, "viuPortaQuebrada");
                    }
                    else if (dia >= 2)
                    {
                        r.Add(E("Ontem à noite você me seguiu pelo corredor. Sussurrando."));
                        r.Add(L(id, "Eu? Querido, às dez eu já estou na cama. Aqui todo mundo dorme cedo. Faz bem."));
                    }
                    else r.Add(L(id, "Se precisar de alguma coisa, me chame. Eu cuido de todos aqui. De você também."));
                    break;

                case "otis":
                    if (dia == 1) r.Add(L(id, "Prédio velho, moço. A oficina fica no corredor sul. Tenho de tudo lá, até pé de cabra."));
                    else if (dia == 2) r.Add(L(id, "A caldeira tá nas últimas. Se apagar, isso aqui vira geladeira em uma hora."));
                    else r.Add(L(id, "Escada pro subsolo fica depois da caldeira. O doutor não gosta que ninguém desça."));
                    if (dia >= 2) { r.Add(E("Você lembra da noite passada?")); r.Add(L(id, "Lembro de deitar. Depois, só o café.")); }
                    break;

                case "almeida":
                    if (dia == 2 && !w.evidenciasColetadas.Contains("prontuario_almeida"))
                    {
                        r.Add(L(id, "Não deveria mostrar isto a um jornalista."));
                        r.Add(L(id, "Horas de sono por interno. Quem fica abaixo de quatro horas... muda. Guarde. Não diga que fui eu."));
                        w.evidenciasColetadas.Add("prontuario_almeida");
                        r.Add(E("(Evidência: prontuário do Dr. Almeida. Preciso anotar no caderno.)"));
                    }
                    else if (dia == 3) r.Add(L(id, "O diretor guarda as fichas antigas no escritório. As que não estão no arquivo."));
                    else r.Add(L(id, "Estou ocupado. Volte outra hora. Ou não volte."));
                    break;

                case "wren":
                    string[] frases =
                    {
                        "O moço do quarto sete voltou. Ele sempre volta.",
                        "Ninguém dorme aqui. Nem você. Principalmente você.",
                        "Eu escrevia também. Num caderno igual ao seu.",
                        "A enfermeira sabe seu nome. Ela sabe o nome de todos que ficam.",
                    };
                    r.Add(L(id, frases[(dia + (jaFalouHoje ? 1 : 0)) % frases.Length]));
                    break;

                case "holt":
                    if (dia == 1) r.Add(L(id, "Sr. Vaughn. Seja bem-vindo ao St. Brenna. Aqui todos dormem bem. É a nossa única regra."));
                    else if (dia == 2) r.Add(L(id, "Teve uma noite agitada? É comum nas primeiras semanas."));
                    else r.Add(L(id, "Amanhã o senhor vai embora. Ou fica conosco. Muitos ficam."));
                    break;

                default:
                    r.Add(L(id, dia == 1 ? "Você é novo? Parece cansado." : "Bom dia. Ontem? Ontem não aconteceu nada."));
                    break;
            }
            return r;
        }

        static string Negacao(string quem, string morto)
        {
            string sobrenome = Nome(morto).Split(' ')[^1];
            switch (quem)
            {
                case "marta": return $"{Nome(morto)}? Não conheço nenhum {sobrenome}. Você está dormindo bem, querido?";
                case "otis": return $"{sobrenome}? Trabalho aqui há vinte anos. Nunca teve ninguém com esse nome.";
                case "holt": return $"Não temos nenhum {sobrenome} no quadro, Sr. Vaughn. Talvez o senhor tenha sonhado.";
                case "wren": return "Quem? Quem? Quem?";
                default: return $"{sobrenome}? Quem?";
            }
        }
    }
}
