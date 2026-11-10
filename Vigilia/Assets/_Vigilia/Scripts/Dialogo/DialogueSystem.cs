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

        public static string Nome(string id)
        {
            if (Idioma.Ingles)
            {
                if (id == "wren") return "Mr. Wren";
                if (id.StartsWith("paciente")) return "Patient";
            }
            return nomes.TryGetValue(id, out var n) ? n : id;
        }

        /// <summary>Atalho: Idioma.T dentro das falas.</summary>
        static string T(string pt, string en) => Idioma.T(pt, en);

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
                r.Add(E(T($"Onde está {Nome(morto)}? Não vejo desde ontem à noite.", $"Where is {Nome(morto)}? I haven't seen them since last night.")));
                r.Add(L(id, Negacao(id, morto)));
                m.Registrar(id, "negou_" + morto);
                return r;
            }

            switch (id)
            {
                case "marta":
                    if (dia == 1 && !jaFalouHoje)
                    {
                        r.Add(L(id, T("Elias! Que bom que voltou. Digo... que bom que chegou. A estrada deve ter sido horrível.",
                                      "Elias! So good to have you back. I mean... so good you made it. The road must have been awful.")));
                        r.Add(E(T("Eu não disse meu nome.", "I didn't tell you my name.")));
                        r.Add(L(id, T("Disse sim, querido. Na portaria. Todo mundo aqui sabe quem você é.",
                                      "You did, dear. At the front desk. Everyone here knows who you are.")));
                        w.pistaNomePrevio = true; // pista 1 (GDD 12)
                    }
                    else if (w.arrombouPorta && !m.Sabe(id, "viuPortaQuebrada"))
                    {
                        // Q-06 (sugestão): Marta comenta a porta quebrada no dia seguinte.
                        r.Add(L(id, T("Alguém arrombou a porta do seu quarto. Esses internos... não se lembram de nada, coitados.",
                                      "Someone broke down your door. These patients... they don't remember a thing, poor souls.")));
                        r.Add(E(T("E você? Lembra de ontem à noite?", "And you? Do you remember last night?")));
                        r.Add(L(id, T("Dormi a noite inteira. Como sempre.", "I slept through the whole night. As always.")));
                        m.Registrar(id, "viuPortaQuebrada");
                    }
                    else if (dia >= 2)
                    {
                        r.Add(E(T("Ontem à noite você me seguiu pelo corredor. Sussurrando.", "Last night you followed me down the corridor. Whispering.")));
                        r.Add(L(id, T("Eu? Querido, às dez eu já estou na cama. Aqui todo mundo dorme cedo. Faz bem.",
                                      "Me? Dear, I'm in bed by ten. Everyone here goes to sleep early. It's good for you.")));
                    }
                    else r.Add(L(id, T("Se precisar de alguma coisa, me chame. Eu cuido de todos aqui. De você também.",
                                       "If you need anything, just call me. I look after everyone here. You too.")));
                    break;

                case "otis":
                    if (dia == 1) r.Add(L(id, T("Prédio velho, moço. A oficina fica no corredor sul. Tenho de tudo lá, até pé de cabra.",
                                                "Old building, son. The workshop's in the south corridor. I've got everything there, even a crowbar.")));
                    else if (dia == 2) r.Add(L(id, T("A caldeira tá nas últimas. Se apagar, isso aqui vira geladeira em uma hora.",
                                                     "The boiler's on its last legs. If it goes out, this place turns into an icebox in an hour.")));
                    else r.Add(L(id, T("Escada pro subsolo fica depois da caldeira. O doutor não gosta que ninguém desça.",
                                       "The basement stairs are past the boiler. The doctor doesn't like anyone going down.")));
                    if (dia >= 2)
                    {
                        r.Add(E(T("Você lembra da noite passada?", "Do you remember last night?")));
                        r.Add(L(id, T("Lembro de deitar. Depois, só o café.", "I remember lying down. After that, just breakfast.")));
                    }
                    break;

                case "almeida":
                    if (dia == 2 && !w.evidenciasColetadas.Contains("prontuario_almeida"))
                    {
                        r.Add(L(id, T("Não deveria mostrar isto a um jornalista.", "I shouldn't be showing this to a journalist.")));
                        r.Add(L(id, T("Horas de sono por interno. Quem fica abaixo de quatro horas... muda. Guarde. Não diga que fui eu.",
                                      "Hours of sleep per patient. Anyone under four hours... changes. Keep it. Don't say it came from me.")));
                        w.evidenciasColetadas.Add("prontuario_almeida");
                        r.Add(E(T("(Evidência: prontuário do Dr. Almeida. Preciso anotar no caderno.)",
                                  "(Evidence: Dr. Almeida's patient file. I need to write it down in the notebook.)")));
                    }
                    else if (dia == 3) r.Add(L(id, T("O diretor guarda as fichas antigas no escritório. As que não estão no arquivo.",
                                                     "The director keeps the old files in his office. The ones that aren't in the archive.")));
                    else r.Add(L(id, T("Estou ocupado. Volte outra hora. Ou não volte.", "I'm busy. Come back later. Or don't.")));
                    break;

                case "wren":
                    string[] frases =
                    {
                        T("O moço do quarto sete voltou. Ele sempre volta.", "The young man from room seven is back. He always comes back."),
                        T("Ninguém dorme aqui. Nem você. Principalmente você.", "Nobody sleeps here. Not even you. Especially you."),
                        T("Eu escrevia também. Num caderno igual ao seu.", "I used to write too. In a notebook just like yours."),
                        T("A enfermeira sabe seu nome. Ela sabe o nome de todos que ficam.", "The nurse knows your name. She knows the name of everyone who stays."),
                    };
                    r.Add(L(id, frases[(dia + (jaFalouHoje ? 1 : 0)) % frases.Length]));
                    break;

                case "holt":
                    if (dia == 1) r.Add(L(id, T("Sr. Vaughn. Seja bem-vindo ao St. Brenna. Aqui todos dormem bem. É a nossa única regra.",
                                                "Mr. Vaughn. Welcome to St. Brenna. Everyone sleeps well here. It's our only rule.")));
                    else if (dia == 2) r.Add(L(id, T("Teve uma noite agitada? É comum nas primeiras semanas.",
                                                     "A restless night? That's common in the first few weeks.")));
                    else r.Add(L(id, T("Amanhã o senhor vai embora. Ou fica conosco. Muitos ficam.",
                                       "Tomorrow you leave. Or you stay with us. Many do.")));
                    break;

                default:
                    r.Add(L(id, dia == 1 ? T("Você é novo? Parece cansado.", "Are you new? You look tired.")
                                         : T("Bom dia. Ontem? Ontem não aconteceu nada.", "Good morning. Yesterday? Nothing happened yesterday.")));
                    break;
            }
            return r;
        }

        static string Negacao(string quem, string morto)
        {
            string sobrenome = Nome(morto).Split(' ')[^1];
            switch (quem)
            {
                case "marta": return T($"{Nome(morto)}? Não conheço nenhum {sobrenome}. Você está dormindo bem, querido?",
                                       $"{Nome(morto)}? I don't know any {sobrenome}. Are you sleeping well, dear?");
                case "otis": return T($"{sobrenome}? Trabalho aqui há vinte anos. Nunca teve ninguém com esse nome.",
                                      $"{sobrenome}? I've worked here twenty years. Never been anyone by that name.");
                case "holt": return T($"Não temos nenhum {sobrenome} no quadro, Sr. Vaughn. Talvez o senhor tenha sonhado.",
                                      $"We have no {sobrenome} on staff, Mr. Vaughn. Perhaps you dreamed it.");
                case "wren": return T("Quem? Quem? Quem?", "Who? Who? Who?");
                default: return T($"{sobrenome}? Quem?", $"{sobrenome}? Who?");
            }
        }
    }
}
