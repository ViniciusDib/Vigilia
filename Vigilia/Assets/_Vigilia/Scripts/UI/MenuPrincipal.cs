using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vigilia
{
    /// <summary>
    /// Cena Menu. A primeira tela pede um clique: navegadores só liberam áudio depois de uma interação (GDD 13.4).
    /// Botões: Jogar, Continuar (P-13), Opções, Créditos. Sem "Sair" em WebGL (rubrica 4, GDD 13.4).
    /// </summary>
    public class MenuPrincipal : MonoBehaviour
    {
        GameObject telaClique, principal, opcoes, creditos;
        Button botaoContinuar;
        readonly List<RectTransform> flocos = new List<RectTransform>();
        readonly List<float> velFlocos = new List<float>();
        AudioSource vento;

        // TODO: preencher com os nomes do grupo e os créditos de assets/código de terceiros (também no README).
        const string TextoCreditos =
            "VIGÍLIA\n\nJogos Digitais · Insper 2026.1\nProf. Tiago Sanches · Tec. Pedro Emil · Prof. Gustavo Braga\n\n" +
            "Grupo: [nomes do grupo]\n\nCena do monstro baseada no design handoff \"Asylum Monster Room\" (Refs/).\n" +
            "Sons e artes provisórios gerados por código.\n\n[créditos de assets de terceiros]";

        void Awake()
        {
            Time.timeScale = 1f;
            AudioListener.volume = SaveJogo.Volume;
            UIFabrica.GarantirEventSystem();
            var c = UIFabrica.Canvas("CanvasMenu", 0);
            var raiz = c.transform;

            var fundo = UIFabrica.Imagem("Fundo", raiz, new Color(0.03f, 0.05f, 0.08f));
            UIFabrica.Cheio(fundo.rectTransform);
            if (ArteJogo.TemFinal("menu_fundo"))
            {
                // arte final: ilustração em tela cheia, cortando as sobras para nunca deixar faixa preta
                var arte = UIFabrica.Imagem("ArteFundo", raiz, Color.white, ArteJogo.Sprite("menu_fundo"));
                arte.rectTransform.anchorMin = arte.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                arte.rectTransform.sizeDelta = new Vector2(1920, 1080);
                var fit = arte.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = 16f / 9f;
            }
            else
            {
                var predio = UIFabrica.Imagem("Predio", raiz, Color.white, ArteJogo.Sprite("menu_sanatorio"));
                UIFabrica.Ancorar(predio.rectTransform, new Vector2(0.45f, 0f), new Vector2(1f, 0.75f));
                predio.preserveAspect = true;
            }
            for (int i = 0; i < 90; i++)
            {
                var f = UIFabrica.Imagem("Floco", raiz, new Color(0.85f, 0.9f, 1f, Random.Range(0.25f, 0.8f)));
                float s = Random.Range(3f, 8f);
                f.rectTransform.anchorMin = f.rectTransform.anchorMax = new Vector2(Random.value, Random.value);
                f.rectTransform.sizeDelta = new Vector2(s, s);
                flocos.Add(f.rectTransform);
                velFlocos.Add(Random.Range(0.02f, 0.07f));
            }
            var vin = UIFabrica.Imagem("Vinheta", raiz, new Color(0, 0, 0, 0.85f), UIFabrica.SpriteVinheta());
            UIFabrica.Cheio(vin.rectTransform);

            // Tela "clique para começar" (libera o áudio)
            var r = UIFabrica.Rect("TelaClique", raiz); UIFabrica.Cheio(r);
            telaClique = r.gameObject;
            if (ArteJogo.TemFinal("logo"))
            {
                // com a arte final, o título fica no terço esquerdo (a ilustração deixa esse espaço escuro)
                var logo = UIFabrica.Imagem("Logo", r, Color.white, ArteJogo.Sprite("logo"));
                UIFabrica.Ancorar(logo.rectTransform, new Vector2(0.04f, 0.5f), new Vector2(0.42f, 0.78f));
                logo.preserveAspect = true;
                var cl = UIFabrica.Texto("Clique", r, "clique para começar", 30, TextAnchor.MiddleCenter, UIFabrica.Papel, FontStyle.Italic);
                UIFabrica.Ancorar(cl.rectTransform, new Vector2(0.04f, 0.38f), new Vector2(0.42f, 0.46f));
            }
            else
            {
                var tc = UIFabrica.Texto("Texto", r, "VIGÍLIA\n\n<size=30>clique para começar</size>", 120, TextAnchor.MiddleCenter, UIFabrica.Papel, FontStyle.Bold);
                UIFabrica.Cheio(tc.rectTransform);
            }

            // Principal
            principal = UIFabrica.Rect("Principal", raiz).gameObject;
            UIFabrica.Ancorar((RectTransform)principal.transform, new Vector2(0.06f, 0.1f), new Vector2(0.45f, 0.9f));
            UIFabrica.Coluna((RectTransform)principal.transform, 18);
            if (ArteJogo.TemFinal("logo"))
            {
                var logo = UIFabrica.Imagem("Logo", principal.transform, Color.white, ArteJogo.Sprite("logo"));
                logo.preserveAspect = true;
                var le = logo.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 220; le.preferredWidth = 640;
            }
            else
            {
                var titulo = UIFabrica.Texto("Titulo", principal.transform, "VIGÍLIA", 140, TextAnchor.MiddleCenter, UIFabrica.Papel, FontStyle.Bold);
                titulo.gameObject.AddComponent<LayoutElement>().preferredHeight = 200;
            }
            var sub = UIFabrica.Texto("Sub", principal.transform, "quem não dorme é possuído", 30, TextAnchor.MiddleCenter, new Color(0.65f, 0.25f, 0.25f), FontStyle.Italic);
            sub.gameObject.AddComponent<LayoutElement>().preferredHeight = 70;
            UIFabrica.Botao("Jogar", principal.transform, Jogar);
            botaoContinuar = UIFabrica.Botao("Continuar", principal.transform, Continuar);
            UIFabrica.Botao("Opções", principal.transform, () => Mostrar(opcoes));
            UIFabrica.Botao("Créditos", principal.transform, () => Mostrar(creditos));
            if (Application.platform != RuntimePlatform.WebGLPlayer)
                UIFabrica.Botao("Sair", principal.transform, Application.Quit);
            principal.SetActive(false);

            // Opções
            opcoes = UIFabrica.Rect("Opcoes", raiz).gameObject;
            UIFabrica.Ancorar((RectTransform)opcoes.transform, new Vector2(0.06f, 0.25f), new Vector2(0.45f, 0.75f));
            UIFabrica.Coluna((RectTransform)opcoes.transform, 20);
            var tv = UIFabrica.Texto("Volume", opcoes.transform, "Volume", 36, TextAnchor.MiddleCenter, UIFabrica.Papel);
            tv.gameObject.AddComponent<LayoutElement>().preferredHeight = 60;
            UIFabrica.Slider(opcoes.transform, SaveJogo.Volume, v => SaveJogo.Volume = v);
            UIFabrica.Botao("Voltar", opcoes.transform, () => Mostrar(principal));
            opcoes.SetActive(false);

            // Créditos
            creditos = UIFabrica.Rect("Creditos", raiz).gameObject;
            UIFabrica.Ancorar((RectTransform)creditos.transform, new Vector2(0.06f, 0.1f), new Vector2(0.5f, 0.9f));
            UIFabrica.Coluna((RectTransform)creditos.transform, 20);
            var tcr = UIFabrica.Texto("Texto", creditos.transform, TextoCreditos, 28, TextAnchor.MiddleCenter, UIFabrica.Papel);
            tcr.gameObject.AddComponent<LayoutElement>().preferredHeight = 600;
            UIFabrica.Botao("Voltar", creditos.transform, () => Mostrar(principal));
            creditos.SetActive(false);
        }

        void Mostrar(GameObject tela)
        {
            principal.SetActive(tela == principal);
            opcoes.SetActive(tela == opcoes);
            creditos.SetActive(tela == creditos);
            if (tela == principal) botaoContinuar.interactable = SaveJogo.Existe;
        }

        void Update()
        {
            for (int i = 0; i < flocos.Count; i++)
            {
                var a = flocos[i].anchorMin;
                a.y -= velFlocos[i] * Time.deltaTime;
                a.x += Mathf.Sin(Time.time * 0.7f + i) * 0.003f * Time.deltaTime;
                if (a.y < -0.02f) { a.y = 1.02f; a.x = Random.value; }
                flocos[i].anchorMin = flocos[i].anchorMax = a;
            }

            if (telaClique.activeSelf)
            {
                var m = Mouse.current; var kb = Keyboard.current;
                if ((m != null && m.leftButton.wasPressedThisFrame) || (kb != null && kb.anyKey.wasPressedThisFrame))
                {
                    telaClique.SetActive(false);
                    Mostrar(principal);
                    TocarVento();
                }
            }
        }

        void TocarVento()
        {
            vento = gameObject.AddComponent<AudioSource>();
            vento.clip = SomProcedural.Vento(6f);
            vento.loop = true;
            vento.volume = 0.3f;
            vento.Play();
            var musica = Resources.Load<AudioClip>("Audio/musica_dia");
            if (musica != null)
            {
                var m = gameObject.AddComponent<AudioSource>();
                m.clip = musica; m.loop = true; m.volume = 0.85f; m.Play();
            }
        }

        void Jogar()
        {
            SaveJogo.ContinuarPendente = false;
            SceneManager.LoadScene("Jogo");
        }

        void Continuar()
        {
            if (!SaveJogo.Existe) return;
            SaveJogo.ContinuarPendente = true;
            SceneManager.LoadScene("Jogo");
        }
    }
}
