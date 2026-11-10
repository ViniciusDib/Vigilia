using System.Collections.Generic;

namespace Vigilia
{
    /// <summary>
    /// Tradução dos textos que ficam gravados na cena pelo ConstrutorDeCenas
    /// (nomes de salas, itens, documentos, esconderijos, nomes e sussurros de NPCs).
    ///
    /// A chave é o texto exato em português que está no ConstrutorDeCenas.
    /// Se você mudar um texto lá, mude a chave aqui também; sem tradução, o jogo mostra o português.
    /// Os componentes chamam Idioma.Cena(texto) na hora de mostrar.
    /// </summary>
    public static class TextosCenaEN
    {
        public static readonly Dictionary<string, string> Tabela = new Dictionary<string, string>
        {
            // ---------------------------------------------------------------- salas
            ["Corredor"] = "Corridor",
            ["Quarto de Elias"] = "Elias's Room",
            ["Consultório"] = "Exam Room",
            ["Escritório do diretor"] = "Director's Office",
            ["Ala de pacientes"] = "Patient Ward",
            ["Depósito"] = "Storeroom",
            ["Oficina"] = "Workshop",
            ["Sala da caldeira"] = "Boiler Room",
            ["Arquivo do subsolo"] = "Basement Archive",
            ["Refeitório"] = "Dining Hall",
            ["Sala secreta"] = "Secret Room",
            ["Lado de fora"] = "Outside",

            // ---------------------------------------------------------------- itens
            ["cobertor"] = "blanket",
            ["chave do quarto"] = "room key",
            ["pé de cabra"] = "crowbar",
            ["um bilhete"] = "a note",
            ["uma pulseira de paciente"] = "a patient wristband",
            ["um memorando do diretor"] = "a memo from the director",
            ["ficha de internação"] = "admission form",
            ["registro de turnos"] = "shift log",
            ["foto antiga"] = "old photograph",
            ["diário de manutenção"] = "maintenance log",
            ["registros da ala de vigília"] = "vigil ward records",
            ["sedativo"] = "sedative",
            ["chave inglesa"] = "wrench",
            ["pilha"] = "battery",

            // ---------------------------------------------------------------- documentos
            ["Na sua letra. Você não lembra de ter escrito.\n\n\"Não confie na enfermeira. Ela sabe meu nome.\nAmanhã eu escrevo de novo, caso eu esqueça.\n— E.\""] =
                "In your handwriting. You don't remember writing it.\n\n\"Don't trust the nurse. She knows my name.\nTomorrow I'll write again, in case I forget.\n— E.\"",
            ["Pulseira de identificação, plástico amarelado.\n\nVAUGHN, E.\nAla de vigília · leito 7\nInternação: [data rasurada]"] =
                "ID wristband, yellowed plastic.\n\nVAUGHN, E.\nVigil ward · bed 7\nAdmitted: [date scratched out]",
            ["Memorando interno · Dr. A. Holt\n\n\"Os registros da ala de vigília ficam na sala atrás do arquivo do subsolo.\nNinguém desce sem minha autorização.\"\n\n(A passagem do subsolo agora está aberta.)"] =
                "Internal memo · Dr. A. Holt\n\n\"The vigil ward records are kept in the room behind the basement archive.\nNo one goes down without my authorization.\"\n\n(The basement passage is now open.)",
            ["Ficha de internação.\n\n\"Insônia persistente. Após a terceira noite sem sono, o paciente não reconhece a própria família.\nConduta: manter acordado. Observar.\""] =
                "Admission form.\n\n\"Persistent insomnia. After the third sleepless night, the patient no longer recognizes his own family.\nProcedure: keep awake. Observe.\"",
            ["Registro de turnos da enfermagem.\n\nToda noite, no mesmo horário, a mesma anotação, na mesma letra:\n\"Equipe dormiu. Nada a relatar.\""] =
                "Nursing shift log.\n\nEvery night, at the same hour, the same entry, in the same handwriting:\n\"Staff slept. Nothing to report.\"",
            ["Fotografia da equipe, 1962.\n\nMarta. Otis. O Dr. Holt.\nAs mesmas caras de hoje. Ninguém envelheceu."] =
                "Staff photograph, 1962.\n\nMarta. Otis. Dr. Holt.\nThe same faces as today. No one has aged.",
            ["Diário do zelador.\n\n\"Aquecimento desligado às 23h por ordem da direção. De novo.\nO doutor diz que o frio ajuda a manter eles acordados.\""] =
                "Caretaker's log.\n\n\"Heating shut off at 11 p.m. by order of the director. Again.\nThe doctor says the cold helps keep them awake.\"",
            ["Registros da ala de vigília.\n\nGráficos de horas de sono por interno, noite após noite. Nomes riscados, um por um.\nO último nome não está riscado. É o seu."] =
                "Vigil ward records.\n\nCharts of hours slept per patient, night after night. Names crossed out, one by one.\nThe last name is not crossed out. It's yours.",

            // ---------------------------------------------------------------- esconderijos
            ["embaixo da cama"] = "under the bed",
            ["armário"] = "wardrobe",
            ["embaixo da mesa"] = "under the table",
            ["embaixo da bancada"] = "under the workbench",

            // ---------------------------------------------------------------- NPCs
            ["Sr. Wren"] = "Mr. Wren",
            ["Paciente"] = "Patient",
            ["Interno"] = "Inmate",
            ["Elias..."] = "Elias...",
        };
    }
}
