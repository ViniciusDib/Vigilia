using System.Collections.Generic;

namespace Vigilia
{
    /// <summary>Um bloco de texto da matéria (uma "peça de Lego", GDD 11).</summary>
    [System.Serializable]
    public struct BlocoTexto
    {
        public string id;
        public string texto;
        public BlocoTexto(string id, string texto) { this.id = id; this.texto = texto; }
        public override string ToString() => id;
    }

    /// <summary>
    /// Escolhe o final e monta a matéria (GDD 3.3, 11 e 13.3).
    /// Função PURA e TOTAL: qualquer combinação de flags devolve um final e uma lista não vazia de blocos;
    /// nunca lança exceção. Há teste automático cobrindo todas as combinações (Tests/EndingResolverTests).
    /// </summary>
    public static class EndingResolver
    {
        public struct Resultado
        {
            public FinalMateria final;
            /// <summary>Só faz sentido no final C: verdadeiro = variação 2 (sangue na manga).</summary>
            public bool variacaoSangue;
            public List<BlocoTexto> blocos;
        }

        /// <summary>Assinatura do GDD. Usa as faixas padrão de teste.</summary>
        public static (FinalMateria final, List<BlocoTexto> blocos) Resolver(WorldState w, bool transbordouNoite3)
        {
            var r = ResolverCompleto(w, transbordouNoite3, null, true);
            return (r.final, r.blocos);
        }

        public static Resultado ResolverCompleto(WorldState w, bool transbordouNoite3, FaixasMateria faixas, bool soEvidenciasAnotadas)
        {
            if (w == null) w = new WorldState();          // total: nem um mundo nulo quebra o final
            if (faixas == null) faixas = new FaixasMateria();

            // Regras R-1 a R-4 (GDD 3.3)
            FinalMateria f = transbordouNoite3 ? FinalMateria.Louco
                           : w.matouAlguem     ? FinalMateria.Omite
                           :                     FinalMateria.Honesta;

            var blocos = new List<BlocoTexto>();
            blocos.Add(Bloco(Manchete(f, w.matouAlguem), f));
            blocos.Add(Bloco("F", f));                                   // a fuga da noite 3 (texto próprio de cada final)
            blocos.Add(Bloco(Verdade(w.EvidenciasQueContam(soEvidenciasAnotadas), faixas), f));
            blocos.Add(Bloco(w.matouAlguem ? "X1" : "X0", f));
            blocos.Add(Bloco(Exposicao(w.vezesTocado, faixas), f));
            if (w.arrombouPorta) blocos.Add(Bloco("M-porta", f));
            if (!w.viuSalaSecreta) blocos.Add(Bloco("M-sem-provas", f));

            return new Resultado
            {
                final = f,
                variacaoSangue = f == FinalMateria.Louco && w.matouAlguem,
                blocos = blocos
            };
        }

        public static string Manchete(FinalMateria f, bool matouAlguem)
        {
            switch (f)
            {
                case FinalMateria.Honesta: return "H-A";
                case FinalMateria.Omite: return "H-B";
                default: return matouAlguem ? "H-C2" : "H-C1";
            }
        }

        public static string Verdade(int evidencias, FaixasMateria fx)
            => evidencias >= fx.verdadeV2 ? "V2" : evidencias >= fx.verdadeV1 ? "V1" : "V0";

        public static string Exposicao(int vezesTocado, FaixasMateria fx)
            => vezesTocado >= fx.exposicaoE2 ? "E2" : vezesTocado >= fx.exposicaoE1 ? "E1" : "E0";

        static BlocoTexto Bloco(string id, FinalMateria f) => new BlocoTexto(id, MateriaTextos.Texto(id, f));
    }
}
