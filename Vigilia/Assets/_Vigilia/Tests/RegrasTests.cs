using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Vigilia.Tests
{
    /// <summary>
    /// Testes das regras puras (rodam sem abrir cena). Window > General > Test Runner > EditMode.
    /// Cobrem itens da Definição de pronto (GDD 19).
    /// </summary>
    public class RegrasTests
    {
        BalanceConfig cfg;

        [SetUp] public void Criar() => cfg = ScriptableObject.CreateInstance<BalanceConfig>();
        [TearDown] public void Destruir() => Object.DestroyImmediate(cfg);

        // ---------------- EndingResolver: função total (GDD 11.3)

        [Test]
        public void EndingResolver_CobreTodasAsCombinacoes()
        {
            var faixas = new FaixasMateria();
            int combinacoes = 0;
            foreach (bool transbordou in new[] { false, true })
            foreach (bool matou in new[] { false, true })
            foreach (bool arrombou in new[] { false, true })
            foreach (bool viu in new[] { false, true })
            foreach (int evid in new[] { 0, 1, 2, 3, 7 })
            foreach (int toques in new[] { 0, 1, 3, 4, 10 })
            foreach (bool soAnotadas in new[] { false, true })
            {
                var w = new WorldState { matouAlguem = matou, arrombouPorta = arrombou, viuSalaSecreta = viu, vezesTocado = toques };
                for (int i = 0; i < evid; i++) { w.evidenciasColetadas.Add("e" + i); w.evidenciasAnotadas.Add("e" + i); }

                var r = EndingResolver.ResolverCompleto(w, transbordou, faixas, soAnotadas);
                combinacoes++;

                Assert.IsNotNull(r.blocos);
                Assert.IsNotEmpty(r.blocos);
                foreach (var b in r.blocos) Assert.IsFalse(string.IsNullOrEmpty(b.texto), $"bloco {b.id} vazio");

                // R-1 a R-4 (GDD 3.3)
                var esperado = transbordou ? FinalMateria.Louco : matou ? FinalMateria.Omite : FinalMateria.Honesta;
                Assert.AreEqual(esperado, r.final);
                Assert.AreEqual(transbordou && matou, r.variacaoSangue);

                var ids = r.blocos.ConvertAll(b => b.id);
                Assert.AreEqual(arrombou, ids.Contains("M-porta"));
                Assert.AreEqual(!viu, ids.Contains("M-sem-provas"));
                Assert.Contains(matou ? "X1" : "X0", ids);
            }
            Assert.Greater(combinacoes, 0);
        }

        [Test]
        public void EndingResolver_NuncaQuebraComMundoNulo()
        {
            var r = EndingResolver.ResolverCompleto(null, false, null, true);
            Assert.AreEqual(FinalMateria.Honesta, r.final);
            Assert.IsNotEmpty(r.blocos);
        }

        [Test]
        public void EndingResolver_FaixasDosEixos()
        {
            var f = new FaixasMateria();
            Assert.AreEqual("V0", EndingResolver.Verdade(0, f));
            Assert.AreEqual("V1", EndingResolver.Verdade(2, f));
            Assert.AreEqual("V2", EndingResolver.Verdade(3, f));
            Assert.AreEqual("E0", EndingResolver.Exposicao(0, f));
            Assert.AreEqual("E1", EndingResolver.Exposicao(3, f));
            Assert.AreEqual("E2", EndingResolver.Exposicao(4, f));
        }

        [Test]
        public void EvidenciasSoContamAnotadas_P07()
        {
            var w = new WorldState();
            w.evidenciasColetadas.AddRange(new[] { "a", "b", "c" });
            var r = EndingResolver.ResolverCompleto(w, false, new FaixasMateria(), true);
            Assert.AreEqual("V0", r.blocos.Find(x => x.id.StartsWith("V")).id);
            r = EndingResolver.ResolverCompleto(w, false, new FaixasMateria(), false);
            Assert.AreEqual("V2", r.blocos.Find(x => x.id.StartsWith("V")).id);
        }

        // ---------------- Idioma (decisão de 09/11: toggle português/inglês)

        [Test]
        public void Traducao_TodoBlocoDaMateriaTemIngles()
        {
            var en = new HashSet<string>(MateriaTextos.TodosOsIdsEN);
            foreach (var id in MateriaTextos.TodosOsIds) Assert.IsTrue(en.Contains(id), $"bloco {id} sem versão em inglês");
        }

        [Test]
        public void Traducao_FinalEmInglesNaoMisturaPortugues()
        {
            var antes = Idioma.Atual;
            try
            {
                Idioma.Atual = Lingua.EN;
                var w = new WorldState { matouAlguem = true, arrombouPorta = true, vezesTocado = 5 };
                foreach (bool transbordou in new[] { false, true })
                {
                    var r = EndingResolver.ResolverCompleto(w, transbordou, new FaixasMateria(), true);
                    foreach (var b in r.blocos)
                        Assert.IsFalse(b.texto.Contains(" não ") || b.texto.Contains("ção"), $"bloco {b.id} em português: {b.texto}");
                }
                Assert.AreEqual("Corridor", Idioma.Cena("Corredor"));
                Assert.AreEqual("Marta Quill", Idioma.Cena("Marta Quill"), "nome próprio sem tradução fica igual");
            }
            finally { Idioma.Atual = antes; }
        }

        // ---------------- Banheira (GDD 6.4)

        [Test]
        public void Possessao_ExemploNumericoDoGDD()
        {
            // 70 × 0.4 = 28 ; 63 × 0.4 = 25.2
            Assert.AreEqual(28f, CalculoPossessao.AposDormir(cfg, 70f), 0.001f);
            Assert.AreEqual(25.2f, CalculoPossessao.AposDormir(cfg, 63f), 0.001f);
            // morna 6/min, fria 12/min, quente 3/min
            Assert.AreEqual(6f, CalculoPossessao.Vazao(cfg, cfg.multMorna, 1f) * 60f, 0.001f);
            Assert.AreEqual(12f, CalculoPossessao.Vazao(cfg, cfg.multFria, 1f) * 60f, 0.001f);
            Assert.AreEqual(3f, CalculoPossessao.Vazao(cfg, cfg.multQuente, 1f) * 60f, 0.001f);
            // lúcido divide por dois
            Assert.AreEqual(3f, CalculoPossessao.Vazao(cfg, cfg.multMorna, cfg.multLucido) * 60f, 0.001f);
        }

        [Test]
        public void Possessao_NuncaSaiDe0a100()
        {
            Assert.AreEqual(100f, CalculoPossessao.Passo(cfg, 99.99f, cfg.multFria, 1f, 1000f));
            Assert.AreEqual(0f, CalculoPossessao.Limitar(cfg, -5f));
            Assert.LessOrEqual(CalculoPossessao.AposDormir(cfg, 100f), 40f); // herança máxima 40 (GDD 6.6)
        }

        // ---------------- Snapshot: cópia profunda (GDD 13.3)

        [Test]
        public void Snapshot_EhCopiaProfundaERepetivel()
        {
            var w = new WorldState { temCobertor = true };
            w.evidenciasColetadas.Add("ficha");
            var m = new NpcMemory();
            m.Registrar("marta", "x");
            var p = new PosicoesDaCena { jogador = new Vector2(1, 2) };
            var s = new SaveSnapshot();
            s.Capturar(w, m, p, 42f);

            for (int vez = 0; vez < 3; vez++)
            {
                // o jogo continua e muda tudo...
                w.temCobertor = false;
                w.evidenciasColetadas.Add("outra");
                w.MarcarMorto("almeida");
                m.Limpar();
                p.jogador = Vector2.zero;

                // ...e restaurar volta exatamente à foto
                s.Restaurar(w, m, p);
                Assert.IsTrue(w.temCobertor);
                CollectionAssert.AreEqual(new List<string> { "ficha" }, w.evidenciasColetadas);
                Assert.IsFalse(w.matouAlguem);
                Assert.IsEmpty(w.npcsMortos);
                Assert.IsTrue(m.Sabe("marta", "x"));
                Assert.AreEqual(new Vector2(1, 2), p.jogador);
                Assert.AreEqual(42f, s.possessao);
            }
        }

        [Test]
        public void WorldState_ClonarNaoCompartilhaListas()
        {
            var a = new WorldState();
            a.itensColetados.Add("x");
            var b = a.Clonar();
            b.itensColetados.Add("y");
            Assert.AreEqual(1, a.itensColetados.Count);
        }

        [Test]
        public void NpcMemory_LimparEsqueceTudo()
        {
            var m = new NpcMemory();
            m.Registrar("otis", "viuPortaQuebrada");
            m.Limpar();
            Assert.IsFalse(m.Sabe("otis", "viuPortaQuebrada"));
        }

        [Test]
        public void BalanceConfig_PorNoiteNaoEstoura()
        {
            Assert.AreEqual(2, BalanceConfig.PorNoite(cfg.inimigosPorNoite, 1));
            Assert.AreEqual(8, BalanceConfig.PorNoite(cfg.inimigosPorNoite, 3));
            Assert.AreEqual(8, BalanceConfig.PorNoite(cfg.inimigosPorNoite, 9));
            Assert.AreEqual(2, BalanceConfig.PorNoite(cfg.inimigosPorNoite, 0));
        }
    }
}
