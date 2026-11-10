using System.Collections.Generic;

namespace Vigilia
{
    /// <summary>
    /// Textos dos blocos da matéria (GDD 11.1 e 11.2), reescritos em 04/11 para a versão com a fuga da noite 3.
    ///
    /// Cada bloco pode ter uma versão por final, com o sufixo "@A", "@B" ou "@C" no id.
    /// Sem versão própria, vale o texto geral (sem sufixo).
    /// - Finais A e B: quem escreve é o próprio Elias, em primeira pessoa. No B ele esconde o que fez.
    /// - Final C: Elias nunca escreveu a matéria. Quem escreve é a redação, sobre ele, em terceira pessoa.
    /// Escrita deliberadamente ambígua (GDD 12): dá para ler Elias como investigador ou como interno.
    /// TODO(Q-03): o grupo pode revisar à vontade.
    /// </summary>
    public static class MateriaTextos
    {
        static readonly Dictionary<string, string> textos = new Dictionary<string, string>
        {
            // ---------------------------------------------------------------- manchetes
            ["H-A"] = "O sanatório onde ninguém dorme: três noites no St. Brenna",
            ["H-B"] = "Sanatório no Alasca sofre com aquecimento precário e falta de pessoal",
            ["H-C1"] = "Jornalista desaparecido é encontrado na neve em frente a sanatório no Alasca",
            ["H-C2"] = "Jornalista é encontrado com sangue nas roupas em frente a sanatório no Alasca",

            // ---------------------------------------------------------------- a fuga (noite 3)
            ["F@A"] = "Na terceira noite, uma porta no subsolo se abriu sozinha. O que esperava lá dentro não cabe neste texto, " +
                      "e não sei se caberia em algum. Corri pelo corredor leste com eles atrás de mim, todos de olhos abertos, " +
                      "e só parei quando senti a neve no rosto.",
            ["F@B"] = "Deixei o prédio na madrugada de domingo, pela porta principal, um pouco antes do previsto. " +
                      "Motivos pessoais. Não houve incidentes que mereçam registro.",
            ["F@C"] = "Funcionários afirmam que o homem passou a noite de sábado andando pelos corredores com uma lanterna acesa, " +
                      "repetindo que precisava encontrar a saída. Pela manhã, ninguém se lembrava de tê-lo visto entrar.",

            // ---------------------------------------------------------------- verdade (evidências anotadas)
            ["V0"] = "Não trouxe documentos. Tudo o que tenho são lembranças que ninguém mais confirma, e aqui lembrar é exatamente o problema.",
            ["V1"] = "Trouxe algumas anotações, feitas às pressas num caderno: funcionários que esquecem tudo ao amanhecer, " +
                     "sempre na mesma hora, sempre do mesmo jeito.",
            ["V2"] = "Trouxe fichas, registros de plantão e um caderno inteiro de anotações. Todos dizem a mesma coisa: " +
                     "quem não dorme no St. Brenna deixa de ser quem era.",
            ["V0@B"] = "A direção não forneceu documentos. A reportagem também não encontrou.",
            ["V1@B"] = "Os registros internos a que a reportagem teve acesso são poucos e desencontrados.",
            ["V2@B"] = "A reportagem teve acesso a fichas e registros internos. Não cabe aqui detalhá-los.",
            ["V0@C"] = "Ele não carregava documentos. Só uma lanterna, com a pilha no fim.",
            ["V1@C"] = "No bolso do casaco havia um caderno com poucas páginas escritas. A letra muda no meio de uma frase.",
            ["V2@C"] = "No bolso do casaco havia um caderno cheio. As anotações descrevem uma regra, \"quem não dorme é possuído\", " +
                       "e terminam com o nome dele, escrito como se fosse o de outra pessoa.",

            // ---------------------------------------------------------------- violência (matouAlguem)
            ["X0"] = "Ninguém se feriu. Ao menos, ninguém de quem alguém se lembre.",
            ["X1"] = "Sobre a ala leste, prefiro não entrar em detalhes. A equipe garante que nunca houve ninguém com aquele nome, " +
                     "e por enquanto vou acreditar nela.",
            ["X0@C"] = "A polícia estadual não registrou feridos.",
            ["X1@C"] = "A equipe nega que tenha faltado alguém no plantão. Mas há um nome riscado no quadro da enfermaria, " +
                       "e ninguém sabe dizer quem o riscou.",

            // ---------------------------------------------------------------- exposição (vezesTocado)
            ["E0"] = "Atravessei as noites sem que nenhum deles me alcançasse. Não sei se tive sorte ou se me deixaram passar.",
            ["E1"] = "Houve mãos frias no escuro, mais de uma vez. Ainda sinto os dedos no ombro quando apago a luz.",
            ["E2"] = "Releio as últimas páginas do caderno e a letra já não é a minha. Em algumas, falo de mim mesmo como de um interno.",
            ["E0@B"] = "A estadia transcorreu sem contato com os pacientes.",
            ["E0@C"] = "Não havia marcas no corpo.",
            ["E1@C"] = "Havia marcas de dedos nos ombros e nos pulsos, já arroxeadas.",
            ["E2@C"] = "Os braços estavam cobertos de marcas de dedos. Um médico que pediu para não ser identificado disse que " +
                       "\"parecia que muitas mãos o seguraram ao mesmo tempo\".",

            // ---------------------------------------------------------------- modificadores
            ["M-porta"] = "A porta do quarto onde fiquei hospedado foi arrombada na segunda noite. Ninguém perguntou por quê.",
            ["M-porta@B"] = "Houve danos materiais pontuais nas instalações, já comunicados à direção.",
            ["M-porta@C"] = "A porta do quarto em que ele ficou hospedado estava arrombada.",
            ["M-sem-provas"] = "Não cheguei ao subsolo. Sem provas do que existe lá embaixo, a direção nega qualquer irregularidade.",
            ["M-sem-provas@C"] = "O diretor, Dr. Alaric Holt, afirma que o subsolo do prédio está desativado há décadas.",
        };

        /// <summary>Mesmos ids, em inglês (traduzidos em 09/11). Um teste confere que nenhum id ficou sem tradução.</summary>
        static readonly Dictionary<string, string> textosEN = new Dictionary<string, string>
        {
            // ---------------------------------------------------------------- headlines
            ["H-A"] = "The sanatorium where no one sleeps: three nights at St. Brenna",
            ["H-B"] = "Alaska sanatorium struggles with poor heating and understaffing",
            ["H-C1"] = "Missing journalist found in the snow outside Alaska sanatorium",
            ["H-C2"] = "Journalist found with blood on his clothes outside Alaska sanatorium",

            // ---------------------------------------------------------------- the escape (night 3)
            ["F@A"] = "On the third night, a door in the basement opened by itself. What was waiting inside doesn't fit in this story, " +
                      "and I'm not sure it would fit in any. I ran down the east corridor with them behind me, all of them with their eyes open, " +
                      "and I only stopped when I felt the snow on my face.",
            ["F@B"] = "I left the building early Sunday morning, through the main door, a little ahead of schedule. " +
                      "Personal reasons. There were no incidents worth reporting.",
            ["F@C"] = "Staff say the man spent Saturday night walking the corridors with a lit flashlight, " +
                      "repeating that he had to find the way out. In the morning, no one remembered seeing him come in.",

            // ---------------------------------------------------------------- truth (evidence written down)
            ["V0"] = "I brought back no documents. All I have are memories no one else confirms, and here, remembering is exactly the problem.",
            ["V1"] = "I brought back a few notes, scribbled in a notebook: staff who forget everything at dawn, " +
                     "always at the same hour, always the same way.",
            ["V2"] = "I brought back files, shift logs and a whole notebook of notes. They all say the same thing: " +
                     "whoever doesn't sleep at St. Brenna stops being who they were.",
            ["V0@B"] = "The administration provided no documents. Nor did this reporter find any.",
            ["V1@B"] = "The few internal records this reporter was able to see are scattered and inconsistent.",
            ["V2@B"] = "This reporter had access to files and internal records. It is not the place to detail them here.",
            ["V0@C"] = "He carried no documents. Only a flashlight, its battery nearly dead.",
            ["V1@C"] = "In his coat pocket was a notebook with only a few pages filled. The handwriting changes in the middle of a sentence.",
            ["V2@C"] = "In his coat pocket was a full notebook. The notes describe a rule, \"whoever doesn't sleep is possessed\", " +
                       "and end with his own name, written as if it belonged to someone else.",

            // ---------------------------------------------------------------- violence (matouAlguem)
            ["X0"] = "No one was hurt. At least, no one anyone remembers.",
            ["X1"] = "About the east wing, I'd rather not go into detail. The staff insist there was never anyone by that name, " +
                     "and for now I'll believe them.",
            ["X0@C"] = "State police reported no injuries.",
            ["X1@C"] = "Staff deny anyone was missing from the night shift. But there is a name crossed out on the ward board, " +
                       "and no one can say who crossed it out.",

            // ---------------------------------------------------------------- exposure (vezesTocado)
            ["E0"] = "I made it through the nights without any of them reaching me. I don't know if I was lucky or if they let me pass.",
            ["E1"] = "There were cold hands in the dark, more than once. I still feel the fingers on my shoulder when I turn off the light.",
            ["E2"] = "I reread the last pages of the notebook and the handwriting is no longer mine. In some, I speak of myself as a patient.",
            ["E0@B"] = "The stay passed without contact with the patients.",
            ["E0@C"] = "There were no marks on the body.",
            ["E1@C"] = "There were finger marks on his shoulders and wrists, already turning purple.",
            ["E2@C"] = "His arms were covered in finger marks. A doctor who asked not to be named said it " +
                       "\"looked like many hands had held him at the same time\".",

            // ---------------------------------------------------------------- modifiers
            ["M-porta"] = "The door to the room where I stayed was broken down on the second night. No one asked why.",
            ["M-porta@B"] = "There was minor property damage to the facilities, already reported to the administration.",
            ["M-porta@C"] = "The door to the room where he stayed had been broken down.",
            ["M-sem-provas"] = "I never made it to the basement. Without proof of what lies down there, the administration denies any wrongdoing.",
            ["M-sem-provas@C"] = "The director, Dr. Alaric Holt, says the building's basement has been out of use for decades.",
        };

        static Dictionary<string, string> Tabela => Idioma.Ingles ? textosEN : textos;

        /// <summary>Texto geral do bloco. Nunca devolve vazio: bloco sem texto vira um aviso visível.</summary>
        public static string Texto(string id)
            => Tabela.TryGetValue(id, out var t) ? t : textos.TryGetValue(id, out var pt) ? pt : $"[bloco {id} sem texto · TODO(Q-03)]";

        /// <summary>Texto do bloco para um final: usa a versão "@A/@B/@C" se existir; senão, a geral.</summary>
        public static string Texto(string id, FinalMateria final)
        {
            string sufixo = final == FinalMateria.Honesta ? "@A" : final == FinalMateria.Omite ? "@B" : "@C";
            return Tabela.TryGetValue(id + sufixo, out var t) ? t : Texto(id);
        }

        /// <summary>Linha de autoria embaixo da manchete.</summary>
        public static string Autoria(FinalMateria final)
            => final == FinalMateria.Louco
                ? Idioma.T("Da redação, com informações da polícia estadual do Alasca", "Staff report, with information from the Alaska State Police")
                : Idioma.T("Por Elias Vaughn, enviado especial", "By Elias Vaughn, special correspondent");

        public static IEnumerable<string> TodosOsIds => textos.Keys;
        public static IEnumerable<string> TodosOsIdsEN => textosEN.Keys;
    }
}
