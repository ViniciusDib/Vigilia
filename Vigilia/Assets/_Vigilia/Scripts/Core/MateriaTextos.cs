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

        /// <summary>Texto geral do bloco. Nunca devolve vazio: bloco sem texto vira um aviso visível.</summary>
        public static string Texto(string id)
            => textos.TryGetValue(id, out var t) ? t : $"[bloco {id} sem texto · TODO(Q-03)]";

        /// <summary>Texto do bloco para um final: usa a versão "@A/@B/@C" se existir; senão, a geral.</summary>
        public static string Texto(string id, FinalMateria final)
        {
            string sufixo = final == FinalMateria.Honesta ? "@A" : final == FinalMateria.Omite ? "@B" : "@C";
            return textos.TryGetValue(id + sufixo, out var t) ? t : Texto(id);
        }

        /// <summary>Linha de autoria embaixo da manchete.</summary>
        public static string Autoria(FinalMateria final)
            => final == FinalMateria.Louco ? "Da redação, com informações da polícia estadual do Alasca" : "Por Elias Vaughn, enviado especial";

        public static IEnumerable<string> TodosOsIds => textos.Keys;
    }
}
