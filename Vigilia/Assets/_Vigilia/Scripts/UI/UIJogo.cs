using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vigilia
{
    /// <summary>
    /// Toda a interface da cena Jogo, montada por código no Awake:
    /// HUD (bateria, inventário, retrato de Elias), sinais de possessão sem barra (P-10: vinheta, olheira),
    /// mensagens, prompt de interação, diálogo com retrato, documentos, cartões de dia/noite, intro,
    /// tela de transbordo (Q-20), final com a matéria, pausa (Esc) e painel de debug (F1).
    /// </summary>
    public class UIJogo : MonoBehaviour
    {
        public CameraSegue cameraSegue;

        Canvas canvas;
        Image vinheta, flash, fade, retratoElias, olheiraElias, barraBateria, barraPossessao, progressoBarra;
        GameObject grupoPossessao, grupoProgresso;
        Text objetivo, mensagem, legenda, prompt, hudBateria, hudInventario, hudLucido, debug, progressoTexto;
        float msgAte, legendaAte, salaAte;
        Text nomeSala;
        bool debugAtivo;

        GameObject painelDialogo; Image retratoDialogo; Text nomeDialogo, textoDialogo;
        GameObject painelDoc; Text docTitulo, docTexto;
        GameObject painelCartao; Text cartaoTitulo, cartaoSub;
        GameObject painelIntro; Text introTexto, introDica; Image introImagem;
        GameObject painelTransbordo; Text transbordoTexto;
        GameObject painelFinal; Text finalRotulo, finalManchete, finalCorpo;
        GameObject painelPausa;

        Jogo J => Jogo.I;

        // ================================================================== montagem

        void Awake()
        {
            UIFabrica.GarantirEventSystem();
            canvas = UIFabrica.Canvas("CanvasJogo", 10);
            canvas.transform.SetParent(transform, false);
            var raiz = canvas.transform;

            // Camadas de tela (de trás para frente)
            vinheta = UIFabrica.Imagem("Vinheta", raiz, new Color(0, 0, 0, 0.4f), UIFabrica.SpriteVinheta());
            UIFabrica.Cheio(vinheta.rectTransform);
            flash = UIFabrica.Imagem("FlashVermelho", raiz, new Color(0.6f, 0f, 0f, 0f));
            UIFabrica.Cheio(flash.rectTransform);

            MontarHud(raiz);
            MontarDialogo(raiz);
            MontarDocumento(raiz);
            MontarPausa(raiz);
            MontarTransbordo(raiz);
            MontarFinal(raiz);

            fade = UIFabrica.Imagem("Fade", raiz, Color.black);
            UIFabrica.Cheio(fade.rectTransform);
            MontarCartao(raiz);
            MontarIntro(raiz);
        }

        void MontarHud(Transform raiz)
        {
            objetivo = UIFabrica.Texto("Objetivo", raiz, "", 26, TextAnchor.UpperCenter, new Color(0.85f, 0.85f, 0.8f), FontStyle.Italic);
            UIFabrica.Ancorar(objetivo.rectTransform, new Vector2(0.2f, 1f), new Vector2(0.8f, 1f), new Vector2(0, -110), new Vector2(0, -24));
            UIFabrica.Contorno(objetivo, Color.black);

            mensagem = UIFabrica.Texto("Mensagem", raiz, "", 30, TextAnchor.MiddleCenter, UIFabrica.Papel);
            UIFabrica.Ancorar(mensagem.rectTransform, new Vector2(0.15f, 0.62f), new Vector2(0.85f, 0.72f));
            UIFabrica.Contorno(mensagem, Color.black);

            legenda = UIFabrica.Texto("Legenda", raiz, "", 34, TextAnchor.MiddleCenter, new Color(0.85f, 0.3f, 0.3f), FontStyle.Italic);
            UIFabrica.Ancorar(legenda.rectTransform, new Vector2(0.15f, 0.22f), new Vector2(0.85f, 0.3f));
            UIFabrica.Contorno(legenda, Color.black);

            prompt = UIFabrica.Texto("Prompt", raiz, "", 28, TextAnchor.MiddleCenter, Color.white);
            UIFabrica.Ancorar(prompt.rectTransform, new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.17f));
            UIFabrica.Contorno(prompt, Color.black);

            // Progresso (segurar E, escrever)
            var prog = UIFabrica.Rect("Progresso", raiz);
            UIFabrica.Ancorar(prog, new Vector2(0.4f, 0.18f), new Vector2(0.6f, 0.2f));
            grupoProgresso = prog.gameObject;
            var pf = UIFabrica.Imagem("Fundo", prog, new Color(0, 0, 0, 0.6f)); UIFabrica.Cheio(pf.rectTransform);
            progressoBarra = UIFabrica.Imagem("Barra", prog, UIFabrica.Papel);
            UIFabrica.Ancorar(progressoBarra.rectTransform, Vector2.zero, new Vector2(0, 1));
            progressoTexto = UIFabrica.Texto("Texto", prog, "", 22, TextAnchor.MiddleCenter, Color.white);
            UIFabrica.Ancorar(progressoTexto.rectTransform, new Vector2(0, 1), new Vector2(1, 2.5f));
            grupoProgresso.SetActive(false);

            // Retrato de Elias (canto inferior esquerdo): as olheiras crescem com a possessão (P-10)
            var moldura = UIFabrica.Imagem("MolduraElias", raiz, new Color(0, 0, 0, 0.7f));
            UIFabrica.Ancorar(moldura.rectTransform, Vector2.zero, Vector2.zero, new Vector2(24, 24), new Vector2(184, 184));
            retratoElias = UIFabrica.Imagem("RetratoElias", moldura.transform, Color.white, DialogueSystem.Retrato("elias", false));
            UIFabrica.Ancorar(retratoElias.rectTransform, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6));
            olheiraElias = UIFabrica.Imagem("OlheiraElias", moldura.transform, new Color(1, 1, 1, 0), DialogueSystem.Retrato("elias", true));
            UIFabrica.Ancorar(olheiraElias.rectTransform, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6));
            hudLucido = UIFabrica.Texto("Lucido", raiz, "", 22, TextAnchor.LowerLeft, new Color(0.7f, 0.85f, 1f));
            UIFabrica.Ancorar(hudLucido.rectTransform, Vector2.zero, Vector2.zero, new Vector2(24, 190), new Vector2(400, 220));

            // Bateria e inventário (rubrica 4: bateria e inventário visíveis)
            hudBateria = UIFabrica.Texto("Bateria", raiz, "", 24, TextAnchor.LowerLeft, UIFabrica.Papel);
            UIFabrica.Ancorar(hudBateria.rectTransform, Vector2.zero, Vector2.zero, new Vector2(200, 120), new Vector2(560, 150));
            var fb = UIFabrica.Imagem("BateriaFundo", raiz, new Color(0, 0, 0, 0.6f));
            UIFabrica.Ancorar(fb.rectTransform, Vector2.zero, Vector2.zero, new Vector2(200, 100), new Vector2(420, 116));
            barraBateria = UIFabrica.Imagem("BateriaBarra", fb.transform, new Color(0.95f, 0.85f, 0.5f));
            UIFabrica.Ancorar(barraBateria.rectTransform, Vector2.zero, Vector2.one);
            hudInventario = UIFabrica.Texto("Inventario", raiz, "", 22, TextAnchor.LowerLeft, new Color(0.8f, 0.8f, 0.78f));
            UIFabrica.Ancorar(hudInventario.rectTransform, Vector2.zero, Vector2.zero, new Vector2(200, 24), new Vector2(1100, 92));

            // Barra de possessão opcional (Q-13) – escondida por padrão
            var gp = UIFabrica.Imagem("PossessaoFundo", raiz, new Color(0, 0, 0, 0.6f));
            UIFabrica.Ancorar(gp.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-344, 24), new Vector2(-24, 44));
            barraPossessao = UIFabrica.Imagem("PossessaoBarra", gp.transform, UIFabrica.Sangue);
            UIFabrica.Ancorar(barraPossessao.rectTransform, Vector2.zero, Vector2.one);
            grupoPossessao = gp.gameObject;

            nomeSala = UIFabrica.Texto("NomeSala", raiz, "", 30, TextAnchor.UpperLeft, UIFabrica.Papel, FontStyle.Bold);
            UIFabrica.Ancorar(nomeSala.rectTransform, new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(28, -110), new Vector2(0, -60));
            UIFabrica.Contorno(nomeSala, Color.black);

            debug = UIFabrica.Texto("Debug", raiz, "", 20, TextAnchor.UpperRight, new Color(0.6f, 1f, 0.6f));
            UIFabrica.Ancorar(debug.rectTransform, new Vector2(0.6f, 0.3f), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-20, -20));
            UIFabrica.Contorno(debug, Color.black, 1);
            debug.gameObject.SetActive(false);
        }

        void MontarDialogo(Transform raiz)
        {
            var p = UIFabrica.Imagem("Dialogo", raiz, new Color(0.04f, 0.035f, 0.04f, 0.94f));
            UIFabrica.Ancorar(p.rectTransform, new Vector2(0.08f, 0.03f), new Vector2(0.92f, 0.3f));
            UIFabrica.Contorno(p, new Color(0.5f, 0.45f, 0.4f), 3);
            painelDialogo = p.gameObject;
            var mold = UIFabrica.Imagem("Moldura", p.transform, new Color(0.15f, 0.13f, 0.12f));
            UIFabrica.Ancorar(mold.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(20, 20), new Vector2(270, -20));
            retratoDialogo = UIFabrica.Imagem("Retrato", mold.transform, Color.white);
            UIFabrica.Ancorar(retratoDialogo.rectTransform, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6));
            retratoDialogo.preserveAspect = true;
            nomeDialogo = UIFabrica.Texto("Nome", p.transform, "", 32, TextAnchor.UpperLeft, new Color(0.9f, 0.75f, 0.55f), FontStyle.Bold);
            UIFabrica.Ancorar(nomeDialogo.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(300, -70), new Vector2(-30, -20));
            textoDialogo = UIFabrica.Texto("Texto", p.transform, "", 32, TextAnchor.UpperLeft, UIFabrica.Papel);
            UIFabrica.Ancorar(textoDialogo.rectTransform, Vector2.zero, Vector2.one, new Vector2(300, 50), new Vector2(-30, -80));
            var dica = UIFabrica.Texto("Dica", p.transform, "[E] continuar", 20, TextAnchor.LowerRight, new Color(0.6f, 0.6f, 0.6f));
            UIFabrica.Ancorar(dica.rectTransform, Vector2.zero, new Vector2(1, 0), new Vector2(0, 12), new Vector2(-24, 40));
            painelDialogo.SetActive(false);
        }

        void MontarDocumento(Transform raiz)
        {
            var fundo = UIFabrica.Imagem("Documento", raiz, new Color(0, 0, 0, 0.75f));
            UIFabrica.Cheio(fundo.rectTransform);
            painelDoc = fundo.gameObject;
            var papel = UIFabrica.Imagem("Papel", fundo.transform, UIFabrica.Papel);
            UIFabrica.Ancorar(papel.rectTransform, new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.88f));
            docTitulo = UIFabrica.Texto("Titulo", papel.transform, "", 40, TextAnchor.UpperCenter, UIFabrica.Tinta, FontStyle.Bold);
            UIFabrica.Ancorar(docTitulo.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -110), new Vector2(-40, -40));
            docTexto = UIFabrica.Texto("Texto", papel.transform, "", 30, TextAnchor.UpperLeft, UIFabrica.Tinta, FontStyle.Italic);
            UIFabrica.Ancorar(docTexto.rectTransform, Vector2.zero, Vector2.one, new Vector2(50, 80), new Vector2(-50, -130));
            var dica = UIFabrica.Texto("Dica", papel.transform, "[E] fechar", 22, TextAnchor.LowerCenter, new Color(0.4f, 0.35f, 0.3f));
            UIFabrica.Ancorar(dica.rectTransform, Vector2.zero, new Vector2(1, 0), new Vector2(0, 20), new Vector2(0, 60));
            painelDoc.SetActive(false);
        }

        void MontarCartao(Transform raiz)
        {
            var r = UIFabrica.Rect("Cartao", raiz); UIFabrica.Cheio(r);
            painelCartao = r.gameObject;
            cartaoTitulo = UIFabrica.Texto("Titulo", r, "", 110, TextAnchor.MiddleCenter, UIFabrica.Papel, FontStyle.Bold);
            UIFabrica.Ancorar(cartaoTitulo.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.7f));
            cartaoSub = UIFabrica.Texto("Sub", r, "", 32, TextAnchor.UpperCenter, new Color(0.75f, 0.72f, 0.68f), FontStyle.Italic);
            UIFabrica.Ancorar(cartaoSub.rectTransform, new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.48f));
            painelCartao.SetActive(false);
        }

        void MontarIntro(Transform raiz)
        {
            var f = UIFabrica.Imagem("Intro", raiz, Color.black); UIFabrica.Cheio(f.rectTransform);
            painelIntro = f.gameObject;
            introImagem = UIFabrica.Imagem("Imagem", f.transform, Color.white);
            UIFabrica.Ancorar(introImagem.rectTransform, new Vector2(0.12f, 0.3f), new Vector2(0.88f, 0.96f));
            introImagem.preserveAspect = true;
            introImagem.gameObject.SetActive(false);
            introTexto = UIFabrica.Texto("Texto", f.transform, "", 40, TextAnchor.MiddleCenter, UIFabrica.Papel);
            UIFabrica.Ancorar(introTexto.rectTransform, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.75f));
            introDica = UIFabrica.Texto("Dica", f.transform, "[Espaço] continuar    [Esc] pular", 22, TextAnchor.LowerCenter, new Color(0.5f, 0.5f, 0.5f));
            UIFabrica.Ancorar(introDica.rectTransform, Vector2.zero, new Vector2(1, 0), new Vector2(0, 30), new Vector2(0, 70));
            painelIntro.SetActive(false);
        }

        void MontarTransbordo(Transform raiz)
        {
            var f = UIFabrica.Imagem("Transbordo", raiz, new Color(0.08f, 0f, 0f, 1f)); UIFabrica.Cheio(f.rectTransform);
            painelTransbordo = f.gameObject;
            transbordoTexto = UIFabrica.Texto("Texto", f.transform, "", 42, TextAnchor.MiddleCenter, new Color(0.85f, 0.3f, 0.28f));
            UIFabrica.Ancorar(transbordoTexto.rectTransform, new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.7f));
            painelTransbordo.SetActive(false);
        }

        void MontarFinal(Transform raiz)
        {
            var f = UIFabrica.Imagem("Final", raiz, new Color(0.03f, 0.03f, 0.035f, 1f)); UIFabrica.Cheio(f.rectTransform);
            painelFinal = f.gameObject;
            finalRotulo = UIFabrica.Texto("Rotulo", f.transform, "", 26, TextAnchor.UpperCenter, new Color(0.6f, 0.55f, 0.5f));
            UIFabrica.Ancorar(finalRotulo.rectTransform, new Vector2(0, 0.92f), new Vector2(1, 0.98f));
            var papel = UIFabrica.Imagem("Jornal", f.transform, UIFabrica.Papel);
            UIFabrica.Ancorar(papel.rectTransform, new Vector2(0.15f, 0.13f), new Vector2(0.85f, 0.91f));
            var jornal = UIFabrica.Texto("NomeJornal", papel.transform, "THE ANCHORAGE LEDGER", 30, TextAnchor.UpperCenter, UIFabrica.Tinta, FontStyle.Bold);
            UIFabrica.Ancorar(jornal.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -70), new Vector2(-40, -24));
            var linha = UIFabrica.Imagem("Linha", papel.transform, UIFabrica.Tinta);
            UIFabrica.Ancorar(linha.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -78), new Vector2(-40, -74));
            finalManchete = UIFabrica.Texto("Manchete", papel.transform, "", 46, TextAnchor.UpperLeft, UIFabrica.Tinta, FontStyle.Bold);
            UIFabrica.Ancorar(finalManchete.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(50, -260), new Vector2(-50, -100));
            finalCorpo = UIFabrica.Texto("Corpo", papel.transform, "", 28, TextAnchor.UpperLeft, UIFabrica.Tinta);
            // a matéria cresce conforme as escolhas: a fonte diminui sozinha para caber no papel
            finalCorpo.verticalOverflow = VerticalWrapMode.Truncate;
            finalCorpo.resizeTextForBestFit = true;
            finalCorpo.resizeTextMinSize = 16;
            finalCorpo.resizeTextMaxSize = 28;
            UIFabrica.Ancorar(finalCorpo.rectTransform, Vector2.zero, Vector2.one, new Vector2(50, 40), new Vector2(-50, -280));
            var botoes = UIFabrica.Rect("Botoes", f.transform);
            UIFabrica.Ancorar(botoes, new Vector2(0.35f, 0.02f), new Vector2(0.65f, 0.12f));
            UIFabrica.Coluna(botoes, 10);
            UIFabrica.Botao("Voltar ao menu", botoes, VoltarAoMenu);
            painelFinal.SetActive(false);
        }

        void MontarPausa(Transform raiz)
        {
            var f = UIFabrica.Imagem("Pausa", raiz, new Color(0, 0, 0, 0.82f)); UIFabrica.Cheio(f.rectTransform);
            f.raycastTarget = true;
            painelPausa = f.gameObject;
            var col = UIFabrica.Rect("Coluna", f.transform);
            UIFabrica.Ancorar(col, new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f));
            UIFabrica.Coluna(col, 18);
            var t = UIFabrica.Texto("Titulo", col, "PAUSA", 70, TextAnchor.MiddleCenter, UIFabrica.Papel, FontStyle.Bold);
            t.gameObject.AddComponent<LayoutElement>().preferredHeight = 120;
            UIFabrica.Botao("Continuar", col, () => Pausar(false));
            var v = UIFabrica.Texto("Volume", col, "Volume", 26, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));
            v.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            UIFabrica.Slider(col, SaveJogo.Volume, x => SaveJogo.Volume = x);
            UIFabrica.Botao("Voltar ao menu", col, VoltarAoMenu);
            var ajuda = UIFabrica.Texto("Controles", col,
                "WASD andar · Mouse mirar · F lanterna · E interagir · G arrombar · Q sedativo · R golpe · Esc pausa · F1 debug",
                20, TextAnchor.MiddleCenter, new Color(0.55f, 0.55f, 0.55f));
            ajuda.gameObject.AddComponent<LayoutElement>().preferredHeight = 80;
            painelPausa.SetActive(false);
        }

        // ================================================================== API usada pelos sistemas

        public void Objetivo(string s) { if (objetivo != null) objetivo.text = s; }

        public void Mensagem(string s, float duracao = 3.5f)
        {
            mensagem.text = s;
            msgAte = Time.unscaledTime + duracao;
        }

        /// <summary>Mostra o nome do cômodo por alguns segundos ao entrar nele.</summary>
        public void NomeSala(string nome)
        {
            if (nomeSala == null || string.IsNullOrEmpty(nome)) return;
            nomeSala.text = nome;
            salaAte = Time.unscaledTime + 2.5f;
        }

        public void Legenda(string s, float duracao)
        {
            legenda.text = s;
            legendaAte = Time.unscaledTime + duracao;
        }

        public void Prompt(string s)
        {
            if (prompt != null) prompt.text = s ?? "";
        }

        /// <summary>v &lt; 0 esconde.</summary>
        public void Progresso(float v, string texto)
        {
            if (grupoProgresso == null) return;
            grupoProgresso.SetActive(v >= 0f);
            if (v < 0f) return;
            progressoBarra.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(v), 1);
            progressoTexto.text = texto ?? "";
        }

        public void Tremer(float intensidade, float duracao) => cameraSegue?.Tremer(intensidade, duracao);

        public void FlashVermelho() => StartCoroutine(Flash());

        IEnumerator Flash()
        {
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                flash.color = new Color(0.6f, 0f, 0f, 0.45f * (1f - t / 0.5f));
                yield return null;
            }
            flash.color = new Color(0.6f, 0, 0, 0);
        }

        public IEnumerator Fade(bool paraPreto, float duracao)
        {
            float a0 = fade.color.a, a1 = paraPreto ? 1f : 0f;
            for (float t = 0; t < duracao; t += Time.unscaledDeltaTime)
            {
                fade.color = new Color(0, 0, 0, Mathf.Lerp(a0, a1, t / duracao));
                yield return null;
            }
            fade.color = new Color(0, 0, 0, a1);
        }

        public IEnumerator Cartao(string titulo, string sub, float duracao)
        {
            fade.color = Color.black;
            painelCartao.SetActive(true);
            cartaoTitulo.text = titulo;
            cartaoSub.text = sub;
            float t = 0f;
            while (t < duracao)
            {
                float a = Mathf.Min(1f, t / 0.6f) * Mathf.Min(1f, (duracao - t) / 0.6f);
                cartaoTitulo.color = new Color(UIFabrica.Papel.r, UIFabrica.Papel.g, UIFabrica.Papel.b, a);
                cartaoSub.color = new Color(0.75f, 0.72f, 0.68f, a);
                t += Time.unscaledDeltaTime;
                if (t > 0.5f && Avancou()) t = Mathf.Max(t, duracao - 0.3f);
                yield return null;
            }
            painelCartao.SetActive(false);
        }

        public IEnumerator TelaTransbordo(string texto, float duracao)
        {
            painelTransbordo.SetActive(true);
            transbordoTexto.text = texto;
            for (float t = 0; t < duracao; t += Time.unscaledDeltaTime) yield return null;
            fade.color = Color.black;
            painelTransbordo.SetActive(false);
        }

        public void AbrirDocumento(string titulo, string texto) => StartCoroutine(Documento(titulo, texto));

        public IEnumerator Documento(string titulo, string texto)
        {
            bool antes = J.EmCena;
            J.EmCena = true;
            painelDoc.SetActive(true);
            docTitulo.text = titulo;
            docTexto.text = texto;
            yield return null;
            yield return EsperarAvancar();
            painelDoc.SetActive(false);
            J.EmCena = antes;
        }

        Coroutine dialogoAtual;

        public void MostrarDialogo(List<Linha> linhas, Action fim) => dialogoAtual = StartCoroutine(RodarDialogo(linhas, fim));

        /// <summary>Fecha o diálogo na hora, sem chamar o fim (usado pelos testes).</summary>
        public void FecharDialogo()
        {
            if (dialogoAtual != null) StopCoroutine(dialogoAtual);
            painelDialogo.SetActive(false);
        }

        IEnumerator RodarDialogo(List<Linha> linhas, Action fim)
        {
            painelDialogo.SetActive(true);
            yield return null;
            foreach (var l in linhas)
            {
                nomeDialogo.text = l.falante;
                retratoDialogo.sprite = DialogueSystem.Retrato(l.falanteId, false);
                retratoDialogo.enabled = retratoDialogo.sprite != null;
                // Máquina de escrever: E completa a linha; E de novo avança.
                float chars = 0f;
                while (chars < l.texto.Length)
                {
                    chars += Time.unscaledDeltaTime * 50f;
                    textoDialogo.text = l.texto.Substring(0, Mathf.Min(l.texto.Length, (int)chars));
                    if (Avancou()) chars = l.texto.Length;
                    yield return null;
                }
                textoDialogo.text = l.texto;
                yield return null;
                yield return EsperarAvancar();
            }
            painelDialogo.SetActive(false);
            fim?.Invoke();
        }

        /// <summary>Intro obrigatória, curta e pulável (D-03). TODO(Q-03): texto final dos 5 quadros.</summary>
        public void MostrarIntro(Action fim) => StartCoroutine(RodarIntro(fim));

        static readonly string[] QuadrosIntro =
        {
            "Alasca, inverno.\nO Sanatório St. Brenna fica a quatro horas da cidade mais próxima.\nA estrada fecha com a primeira neve.",
            "Há meses chegam cartas à redação.\nInternos e funcionários que, toda manhã,\nnão lembram nada da noite anterior.",
            "Elias Vaughn, jornalista, vai passar o fim de semana lá.\nTrês dias. Três noites.",
            "Antes de ir, uma última carta, sem assinatura:\n\n<i>\"Aqui existe uma regra.\nQuem não dorme é possuído.\"</i>",
            "Durma antes que seja tarde.\nMantenha-os na luz da lanterna: só o que a luz toca fica parado.\nE escreva tudo. De manhã, só você vai lembrar.\n\n<size=26>WASD andar · Mouse mirar · F lanterna · E interagir · Esc pausa</size>",
        };

        IEnumerator RodarIntro(Action fim)
        {
            painelIntro.SetActive(true);
            fade.color = Color.black;
            yield return null;
            for (int i = 0; i < QuadrosIntro.Length; i++)
            {
                introTexto.text = QuadrosIntro[i];
                // Arte final opcional: Resources/Arte/intro_1 ... intro_5 (docs/prompts-arte.md)
                var img = ArteJogo.TemFinal($"intro_{i + 1}") ? ArteJogo.Sprite($"intro_{i + 1}") : null;
                introImagem.gameObject.SetActive(img != null);
                introImagem.sprite = img;
                if (img != null) UIFabrica.Ancorar(introTexto.rectTransform, new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.28f));
                else UIFabrica.Ancorar(introTexto.rectTransform, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.75f));
                introTexto.fontSize = img != null ? 32 : 40;
                for (float a = 0; a < 1f; a += Time.unscaledDeltaTime * 1.5f)
                {
                    introTexto.color = new Color(1, 1, 1, a);
                    introImagem.color = new Color(1, 1, 1, a);
                    if (Pulou()) { Encerrar(); yield break; }
                    yield return null;
                }
                introTexto.color = Color.white;
                while (!Avancou())
                {
                    if (Pulou()) { Encerrar(); yield break; }
                    yield return null;
                }
                yield return null;
            }
            Encerrar();

            void Encerrar()
            {
                painelIntro.SetActive(false);
                fim?.Invoke();
            }
        }

        public void MostrarFinal(EndingResolver.Resultado r) => StartCoroutine(RodarFinal(r));

        IEnumerator RodarFinal(EndingResolver.Resultado r)
        {
            fade.color = Color.black;
            painelFinal.SetActive(true);
            finalManchete.text = "";
            finalCorpo.text = "";
            string rotulo = r.final == FinalMateria.Honesta ? "FINAL A · A MATÉRIA HONESTA"
                          : r.final == FinalMateria.Omite ? "FINAL B · A MATÉRIA QUE OMITE"
                          : r.variacaoSangue ? "FINAL C · O JORNALISTA É O LOUCO (variação 2)" : "FINAL C · O JORNALISTA É O LOUCO";
            finalRotulo.text = rotulo;
            yield return Fade(false, 1.5f);

            finalManchete.text = r.blocos.Count > 0 ? r.blocos[0].texto : "";
            var sb = new StringBuilder();
            sb.Append("<i>").Append(MateriaTextos.Autoria(r.final)).Append("</i>\n\n");
            for (int i = 1; i < r.blocos.Count; i++)
            {
                sb.Append(r.blocos[i].texto).Append("\n\n");
                finalCorpo.text = sb.ToString();
                for (float t = 0; t < 1.2f; t += Time.unscaledDeltaTime) yield return null;
            }
            if (r.variacaoSangue)
                sb.Append("<color=#851417><i>Na manga do casaco, uma mancha escura. Na última página do caderno, na letra dele, uma única pergunta: \"fui eu?\"</i></color>");
            finalCorpo.text = sb.ToString();
        }

        // ================================================================== pausa e quadro a quadro

        public void Pausar(bool p)
        {
            if (J == null) return;
            J.Pausado = p;
            Time.timeScale = p ? 0f : 1f;
            painelPausa.SetActive(p);
        }

        void VoltarAoMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Menu");
        }

        void OnDestroy() => Time.timeScale = 1f;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame && J != null && !painelIntro.activeSelf && !painelFinal.activeSelf)
            {
                if (J.Pausado) Pausar(false);
                else if (!J.EmCena && !J.EmDialogo) Pausar(true);
            }
            if (kb != null && kb.f1Key.wasPressedThisFrame) { debugAtivo = !debugAtivo; debug.gameObject.SetActive(debugAtivo); }

            if (Time.unscaledTime > msgAte) mensagem.text = "";
            if (Time.unscaledTime > legendaAte) legenda.text = "";
            if (nomeSala != null)
            {
                float a = Mathf.Clamp01((salaAte - Time.unscaledTime) / 0.8f);
                nomeSala.color = new Color(UIFabrica.Papel.r, UIFabrica.Papel.g, UIFabrica.Papel.b, a);
            }
            AtualizarHud();
        }

        void AtualizarHud()
        {
            if (J == null || J.possessao == null) return;
            var w = J.Mundo;
            var cfg = Jogo.Cfg;
            float nivel = J.possessao.Nivel01;
            bool noite = J.EhNoite;

            // P-10: o mundo avisa. Vinheta e olheira crescem com o nível; a cor puxa para frio ou quente.
            Color baseVinheta = Color.black;
            if (noite && J.temperatura != null)
            {
                var t = J.temperatura.TemperaturaAtual;
                if (t == TemperaturaSala.Fria) baseVinheta = new Color(0.02f, 0.06f, 0.12f);
                else if (t == TemperaturaSala.Quente) baseVinheta = new Color(0.12f, 0.05f, 0.0f);
            }
            float pulso = 1f + Mathf.Sin(Time.time * Mathf.Lerp(4f, 11f, nivel)) * 0.06f * nivel;
            float alvo = noite ? Mathf.Lerp(0.45f, 1f, Mathf.Pow(nivel, 1.3f)) * pulso : 0.25f;
            vinheta.color = new Color(baseVinheta.r, baseVinheta.g, baseVinheta.b, alvo);
            float esc = noite ? Mathf.Lerp(1.15f, 0.8f, nivel) : 1.2f;
            vinheta.rectTransform.localScale = new Vector3(esc, esc, 1);
            olheiraElias.color = new Color(1, 1, 1, Mathf.Clamp01(nivel * 1.2f));

            hudLucido.text = J.caderno != null && J.caderno.Lucido ? $"lúcido · {Mathf.CeilToInt(J.caderno.LucidoRestante)}s" : "";

            hudBateria.text = $"Lanterna {Mathf.CeilToInt(w.bateria)}%" + (J.lanterna != null && !J.lanterna.Acesa ? "  (desligada)" : "");
            barraBateria.rectTransform.anchorMax = new Vector2(w.bateria / 100f, 1);
            barraBateria.color = w.bateria < cfg.bateriaFraca ? UIFabrica.Sangue : new Color(0.95f, 0.85f, 0.5f);

            var sb = new StringBuilder();
            if (w.temCobertor) sb.Append("Cobertor   ");
            if (w.temChave) sb.Append("Chave   ");
            if (w.temPeDeCabra) sb.Append("Pé de cabra   ");
            if (w.sedativos > 0) sb.Append($"Sedativo ×{w.sedativos}   ");
            if (w.objetosPesados > 0) sb.Append($"Objeto pesado ×{w.objetosPesados}   ");
            sb.Append($"\nEvidências: {w.evidenciasColetadas.Count} ({w.evidenciasAnotadas.Count} anotadas)");
            hudInventario.text = sb.ToString();

            grupoPossessao.SetActive(cfg.mostrarBarraPossessao || debugAtivo);
            barraPossessao.rectTransform.anchorMax = new Vector2(nivel, 1);

            if (debugAtivo) debug.text = TextoDebug();
        }

        string TextoDebug()
        {
            var w = J.Mundo;
            var sb = new StringBuilder();
            sb.AppendLine($"Macro: {J.noites.Estado}   noiteAtual: {w.noiteAtual}");
            sb.AppendLine($"Jogador: {J.jogador.Estado}");
            sb.AppendLine($"possessao: {J.possessao.Valor:0.0}   ativa: {J.PossessaoAtiva}");
            if (J.temperatura != null && J.temperatura.SalaAtual != null)
                sb.AppendLine($"Sala: {J.temperatura.SalaAtual.nome} ({J.temperatura.TemperaturaAtual}, ×{J.temperatura.MultSalaAtual})");
            sb.AppendLine($"multLucido: {(J.caderno != null ? J.caderno.MultLucidoAtual : 1f)}   iluminados: {(J.detectorLuz != null ? J.detectorLuz.IluminadosAgora : 0)}");
            sb.AppendLine($"bateria {w.bateria:0}  cobertor {w.temCobertor}  chave {w.temChave}  pé {w.temPeDeCabra}");
            sb.AppendLine($"quartoDestrancado {w.quartoDestrancado}  arrombou {w.arrombouPorta}  caldeiraFalhou {w.caldeiraFalhou}");
            sb.AppendLine($"matouAlguem {w.matouAlguem}  mortos [{string.Join(",", w.npcsMortos)}]  tocado {w.vezesTocado}");
            sb.AppendLine($"pistas: nome {w.pistaNomePrevio} nota {w.pistaNotaPropria} pulseira {w.pistaPulseira} diretor {w.pistaDiretor}  salaSecreta {w.viuSalaSecreta}");
            foreach (var e in EnemyAI.Ativos) sb.AppendLine($"  {e.nome}: {e.Estado}{(e.Iluminado ? " (luz)" : "")}");
            return sb.ToString();
        }

        // ================================================================== entrada

        static bool Avancou()
        {
            var kb = Keyboard.current;
            var m = Mouse.current;
            return (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                   || (m != null && m.leftButton.wasPressedThisFrame);
        }

        bool pularIntro;

        /// <summary>Pula a intro como se o jogador apertasse Esc (usado também pelos testes).</summary>
        public void PularIntro() => pularIntro = true;

        bool Pulou()
        {
            var kb = Keyboard.current;
            if (pularIntro) { pularIntro = false; return true; }
            return kb != null && kb.escapeKey.wasPressedThisFrame;
        }

        static IEnumerator EsperarAvancar()
        {
            while (!Avancou()) yield return null;
        }
    }
}
