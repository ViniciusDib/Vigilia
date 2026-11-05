using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Vigilia.Tests
{
    /// <summary>
    /// Joga o fim de semana inteiro sem mãos: intro → dia 1 → noite 1 → transbordo (checkpoint)
    /// → dormir → dia 2 → noite 2 → dia 3 → noite 3 → final. Falha se qualquer exceção aparecer.
    /// Se a variável de ambiente VIGILIA_SHOTS apontar para uma pasta, salva capturas de tela lá.
    /// </summary>
    public class FimDeSemanaTest
    {
        string pastaShots;

        IEnumerator Esperar(float seg)
        {
            float fim = Time.realtimeSinceStartup + seg;
            while (Time.realtimeSinceStartup < fim) yield return null;
        }

        void Foto(string nome)
        {
            if (string.IsNullOrEmpty(pastaShots)) return;
            var cam = Camera.main;
            var rt = new RenderTexture(1280, 720, 24);
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>();
            foreach (var c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1; }
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            foreach (var c in canvases) c.renderMode = RenderMode.ScreenSpaceOverlay;
            File.WriteAllBytes(Path.Combine(pastaShots, nome + ".png"), tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            rt.Release();
        }

        static void Chamar(object alvo, string metodo)
            => alvo.GetType().GetMethod(metodo, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(alvo, null);

        IEnumerator AteEstado(EstadoMacro e, float limite)
        {
            float fim = Time.realtimeSinceStartup + limite;
            while (Jogo.I.noites.Estado != e && Time.realtimeSinceStartup < fim) yield return null;
            Assert.AreEqual(e, Jogo.I.noites.Estado, "não chegou ao estado " + e);
        }

        IEnumerator AteDestravar(float limite)
        {
            float fim = Time.realtimeSinceStartup + limite;
            while (Jogo.I.EmCena && Time.realtimeSinceStartup < fim) yield return null;
            Assert.IsFalse(Jogo.I.EmCena, "a cena não destravou");
        }

        IEnumerator Dormir()
        {
            var j = Jogo.I;
            j.Mundo.temCobertor = true;
            j.Mundo.caldeiraFalhou = false;
            j.Mundo.quartoDestrancado = true;
            j.jogador.Teleportar(new Vector2(-26f, 8f));
            yield return Esperar(0.3f);
            Assert.IsTrue(j.cama.PodeDormir(out var motivo), motivo);
            j.cama.Interagir(j.jogador);
        }

        [UnityTest]
        public IEnumerator JogaOFimDeSemanaInteiro()
        {
            pastaShots = Environment.GetEnvironmentVariable("VIGILIA_SHOTS");
            if (!string.IsNullOrEmpty(pastaShots)) Directory.CreateDirectory(pastaShots);
            SaveJogo.Apagar();

            SceneManager.LoadScene("Menu");
            yield return Esperar(1f);
            Foto("00_menu_clique");

            SceneManager.LoadScene("Jogo");
            yield return Esperar(1.5f);
            var j = Jogo.I;
            Assert.IsNotNull(j, "Jogo.I não existe");
            Assert.AreEqual(EstadoMacro.Intro, j.noites.Estado);
            Foto("01_intro");

            // M-01: pula a intro
            j.ui.PularIntro();
            yield return AteEstado(EstadoMacro.Dia1, 2f);
            yield return AteDestravar(8f);
            yield return Esperar(0.5f);
            Foto("02_dia1");
            Assert.AreEqual(0f, j.possessao.Valor, "possessao não pode subir de dia (P-04)");

            // Conversa com Marta (pista 1)
            var marta = GameObject.Find("NPC_marta").GetComponent<EnemyAI>();
            j.jogador.Teleportar(marta.Posicao + Vector2.right);
            yield return Esperar(0.2f);
            var linhas = DialogoDados.Montar(marta, j.Mundo, j.Memoria, 1);
            Assert.IsNotEmpty(linhas);
            Assert.IsTrue(j.Mundo.pistaNomePrevio, "Marta deveria chamar Elias pelo nome (pista 1)");
            j.dialogo.Conversar(marta);
            yield return Esperar(0.6f);
            Foto("03_dialogo");
            j.ui.FecharDialogo();
            j.EmDialogo = false;

            // M-02: o dia termina
            j.noites.TerminarDia();
            yield return AteEstado(EstadoMacro.Noite1, 4f);
            yield return AteDestravar(8f);
            Assert.AreEqual(2, EnemyAI.Ativos.Count, "noite 1 tem 2 inimigos");
            Assert.IsTrue(j.Mundo.quartoDestrancado);
            yield return Esperar(2f);
            Assert.Greater(j.possessao.Valor, 0f, "possessao deve subir à noite");
            j.jogador.Teleportar(new Vector2(-14f, 0f));
            yield return Esperar(1.5f);
            Foto("04_noite1_corredor");

            // Transbordo na noite 1 → restaura checkpoint (M-03)
            float valorInicio = 0f;
            j.Mundo.temChave = true; // algo que o checkpoint deve desfazer
            j.possessao.AdicionarPossessao(500f);
            yield return Esperar(1.5f);
            Assert.IsFalse(j.encontro.Tocando, "o monstro agora só aparece na sala secreta");
            yield return AteDestravar(20f);
            Assert.AreEqual(EstadoMacro.Noite1, j.noites.Estado);
            Assert.AreEqual(valorInicio, j.possessao.Valor, 0.5f, "restaurou a possessão do início da noite");
            Assert.IsFalse(j.Mundo.temChave, "restaurar o snapshot desfaz itens");

            // Toque de inimigo
            int antes = j.Mundo.vezesTocado;
            var inimigo = EnemyAI.Ativos[0];
            j.jogador.Teleportar(inimigo.Posicao + new Vector2(0.3f, 0f));
            inimigo.Encontrar();
            yield return Esperar(1f);
            Debug.Log($"[Teste] vezesTocado {antes} → {j.Mundo.vezesTocado}, possessao {j.possessao.Valor:0.0}");

            // M-04: dormir
            float p = j.possessao.Valor;
            yield return Dormir();
            yield return AteEstado(EstadoMacro.Dia2, 10f);
            Assert.AreEqual(p * 0.4f, j.possessao.Valor, 0.5f, "dormir esvazia 60%");
            Assert.AreEqual(2, j.Mundo.noiteAtual);
            yield return AteDestravar(8f);

            // Noite 2: trancado fora
            j.noites.TerminarDia();
            yield return AteEstado(EstadoMacro.Noite2, 4f);
            yield return AteDestravar(8f);
            Assert.IsFalse(j.Mundo.quartoDestrancado, "noite 2: trancado fora do quarto");
            Assert.AreEqual(6, EnemyAI.Ativos.Count, "noite 2 tem 6 inimigos (decisão de 04/11)");
            yield return Esperar(1f);
            Foto("06_noite2_trancado");
            yield return Dormir();
            yield return AteEstado(EstadoMacro.Dia3, 10f);
            yield return AteDestravar(8f);

            // Noite 3: a porta secreta abre, o monstro aparece, começa a fuga
            j.noites.TerminarDia();
            yield return AteEstado(EstadoMacro.Noite3, 4f);
            yield return AteDestravar(8f);
            Assert.AreEqual(8, EnemyAI.Ativos.Count, "noite 3 tem 8 inimigos");
            Assert.IsFalse(j.cama.PodeDormir(out _), "na noite 3 não se dorme: é a fuga");
            var portaSecreta = GameObject.Find("PortaSecreta").GetComponent<Porta>();
            Assert.IsTrue(portaSecreta.Aberta, "noite 3: a porta da sala secreta se abre");
            j.jogador.Teleportar(new Vector2(-12f, -6f));
            yield return Esperar(1.5f);
            Foto("07_noite3_caldeira");
            j.jogador.Teleportar(new Vector2(2f, -16f));          // entra na sala secreta
            yield return Esperar(1.5f);
            Assert.IsTrue(j.encontro.Tocando, "o monstro aparece na sala secreta");
            Foto("05_encontro");
            yield return AteDestravar(15f);
            Assert.IsTrue(j.noites.EmFuga, "depois do monstro, a fuga");
            Assert.GreaterOrEqual(EnemyAI.Ativos.Count, 8 + j.config.fugaInimigosExtras, "possuídos extras na fuga");
            Assert.IsTrue(GameObject.Find("PortaPrincipal").GetComponent<Porta>().Aberta, "porta principal aberta na fuga");
            j.jogador.Teleportar(new Vector2(18f, 0f));
            yield return Esperar(0.6f);
            Foto("10_fuga");
            j.jogador.Teleportar(new Vector2(31f, 0f));            // sai pela porta principal
            yield return AteEstado(EstadoMacro.FinalA, 12f);
            yield return Esperar(6f);
            Foto("08_final");
            Assert.IsFalse(SaveJogo.Existe, "o final apaga o checkpoint salvo");

            // pior caso de texto: final C, variação 2, com todos os blocos possíveis (confere se cabe no papel)
            var piorCaso = new WorldState { matouAlguem = true, arrombouPorta = true, viuSalaSecreta = false, vezesTocado = 6 };
            for (int i = 0; i < 4; i++) { piorCaso.evidenciasColetadas.Add("e" + i); piorCaso.evidenciasAnotadas.Add("e" + i); }
            j.ui.MostrarFinal(EndingResolver.ResolverCompleto(piorCaso, true, new FaixasMateria(), true));
            yield return Esperar(13f);
            Foto("11_final_c2");

            // textura da cena do monstro
            if (!string.IsNullOrEmpty(pastaShots))
            {
                var tex = (Texture2D)typeof(EncontroMonstro).GetField("tex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(j.encontro);
                File.WriteAllBytes(Path.Combine(pastaShots, "09_monstro_textura.png"), tex.EncodeToPNG());
            }
        }
    }
}
