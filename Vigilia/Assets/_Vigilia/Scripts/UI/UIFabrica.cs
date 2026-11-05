using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Vigilia
{
    /// <summary>
    /// Atalhos para montar UI por código (uGUI). Toda a interface do jogo é criada assim, o que evita
    /// depender de prefabs e deixa claro, lendo o código, o que existe na tela.
    /// </summary>
    public static class UIFabrica
    {
        static Font fonte;
        public static Font Fonte
        {
            get
            {
                if (fonte == null) fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return fonte;
            }
        }

        public static readonly Color Papel = new Color(0.93f, 0.9f, 0.82f);
        public static readonly Color Tinta = new Color(0.12f, 0.1f, 0.09f);
        public static readonly Color Sangue = new Color(0.64f, 0.1f, 0.11f);
        public static readonly Color Gelo = new Color(0.78f, 0.86f, 0.95f);

        public static void GarantirEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(go);
        }

        public static Canvas Canvas(string nome, int ordem)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = ordem;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            s.matchWidthOrHeight = 0.5f;
            return c;
        }

        public static RectTransform Rect(string nome, Transform pai)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Âncoras em fração da tela (0..1) e margens em pixels de referência.</summary>
        public static RectTransform Ancorar(RectTransform r, Vector2 min, Vector2 max, Vector2 offMin = default, Vector2 offMax = default)
        {
            r.anchorMin = min; r.anchorMax = max; r.offsetMin = offMin; r.offsetMax = offMax;
            return r;
        }

        public static RectTransform Cheio(RectTransform r) => Ancorar(r, Vector2.zero, Vector2.one);

        public static Image Imagem(string nome, Transform pai, Color cor, Sprite sprite = null)
        {
            var r = Rect(nome, pai);
            var img = r.gameObject.AddComponent<Image>();
            img.color = cor;
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        public static Text Texto(string nome, Transform pai, string s, int tamanho, TextAnchor alin, Color cor, FontStyle estilo = FontStyle.Normal)
        {
            var r = Rect(nome, pai);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Fonte;
            t.text = s;
            t.fontSize = tamanho;
            t.alignment = alin;
            t.color = cor;
            t.fontStyle = estilo;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.supportRichText = true;
            return t;
        }

        public static Outline Contorno(Graphic g, Color cor, float d = 2f)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = cor;
            o.effectDistance = new Vector2(d, -d);
            return o;
        }

        public static Button Botao(string rotulo, Transform pai, UnityEngine.Events.UnityAction acao, int tamanho = 34)
        {
            var img = Imagem("Botao_" + rotulo, pai, new Color(0.08f, 0.07f, 0.08f, 0.85f));
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<Button>();
            var cores = b.colors;
            cores.normalColor = new Color(1, 1, 1, 1);
            cores.highlightedColor = new Color(1.6f, 0.7f, 0.7f, 1);
            cores.pressedColor = new Color(0.7f, 0.4f, 0.4f, 1);
            cores.selectedColor = cores.highlightedColor;
            b.colors = cores;
            b.onClick.AddListener(acao);
            var t = Texto("Rotulo", img.transform, rotulo, tamanho, TextAnchor.MiddleCenter, new Color(0.9f, 0.86f, 0.8f));
            Cheio(t.rectTransform);
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = tamanho * 2f;
            le.preferredWidth = 520;
            return b;
        }

        public static VerticalLayoutGroup Coluna(RectTransform r, float espaco)
        {
            var v = r.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = espaco;
            v.childAlignment = TextAnchor.MiddleCenter;
            v.childControlHeight = true; v.childControlWidth = true;
            v.childForceExpandHeight = false; v.childForceExpandWidth = false;
            return v;
        }

        public static Slider Slider(Transform pai, float valor, UnityEngine.Events.UnityAction<float> aoMudar)
        {
            var raiz = Rect("Slider", pai);
            raiz.sizeDelta = new Vector2(520, 40);
            var le = raiz.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 520; le.preferredHeight = 40;
            var fundo = Imagem("Fundo", raiz, new Color(0.15f, 0.15f, 0.17f));
            Ancorar(fundo.rectTransform, new Vector2(0, 0.35f), new Vector2(1, 0.65f));
            var area = Rect("Area", raiz); Ancorar(area, new Vector2(0, 0.35f), new Vector2(1, 0.65f));
            var fill = Imagem("Fill", area, Sangue); Ancorar(fill.rectTransform, Vector2.zero, Vector2.one);
            var areaH = Rect("AreaHandle", raiz); Cheio(areaH);
            var handle = Imagem("Handle", areaH, Papel);
            handle.rectTransform.sizeDelta = new Vector2(24, 0);
            handle.raycastTarget = true;
            var s = raiz.gameObject.AddComponent<UnityEngine.UI.Slider>();
            s.fillRect = fill.rectTransform;
            s.handleRect = handle.rectTransform;
            s.targetGraphic = handle;
            s.minValue = 0; s.maxValue = 1; s.value = valor;
            s.onValueChanged.AddListener(aoMudar);
            return s;
        }

        /// <summary>Textura radial para a vinheta: transparente no centro, preta nas bordas.</summary>
        public static Sprite SpriteVinheta(int tam = 256)
        {
            var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[tam * tam];
            for (int y = 0; y < tam; y++)
                for (int x = 0; x < tam; x++)
                {
                    float dx = (x + 0.5f) / tam * 2f - 1f, dy = (y + 0.5f) / tam * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.414f;
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1f, d));
                    px[y * tam + x] = new Color32(0, 0, 0, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f));
        }
    }
}
