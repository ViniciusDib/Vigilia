using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Vigilia.EditorTools
{
    /// <summary>
    /// Monta as cenas Menu e Jogo a partir do mapa descrito aqui embaixo (sugestão do Q-10).
    /// Menu: Vigilia > Construir tudo. Também roda em linha de comando (-executeMethod).
    ///
    /// ATENÇÃO: reconstruir APAGA a cena Jogo e cria de novo. Se o grupo começar a editar o mapa
    /// à mão no editor, pare de usar este construtor para a cena Jogo (ou mude o mapa aqui).
    /// </summary>
    public static class ConstrutorDeCenas
    {
        const string PastaCenas = "Assets/_Vigilia/Cenas";
        const string CaminhoConfig = "Assets/_Vigilia/Config/BalanceConfig.asset";
        const float Espessura = 0.3f;          // colisor (navegação e física)
        const float EspessuraVisual = 0.5f;    // o que se vê
        const float LarguraSombra = 0.7f;      // sombra no chão junto às paredes

        static int LayerParedes => LayerMask.NameToLayer("Paredes");
        static int LayerJogador => LayerMask.NameToLayer("Jogador");
        static int LayerInimigos => LayerMask.NameToLayer("Inimigos");

        // ================================================================== mapa (TODO(Q-10))

        class SalaDef
        {
            public string id, nome;
            public RectInt r;
            public TemperaturaSala[] temp;
            public bool quarto, caldeira, secreta, saida;
            public SalaDef(string id, string nome, int x, int y, int w, int h, TemperaturaSala n1, TemperaturaSala n2, TemperaturaSala n3)
            { this.id = id; this.nome = nome; r = new RectInt(x, y, w, h); temp = new[] { n1, n2, n3 }; }
        }

        const TemperaturaSala F = TemperaturaSala.Fria, M = TemperaturaSala.Morna, Q = TemperaturaSala.Quente;

        static List<SalaDef> Salas() => new List<SalaDef>
        {
            new SalaDef("corredor", "Corredor", -28, -2, 56, 4, M, F, F),
            new SalaDef("quarto", "Quarto de Elias", -28, 2, 8, 10, F, F, F) { quarto = true },
            new SalaDef("consultorio", "Consultório", -20, 2, 10, 10, M, M, F),
            new SalaDef("escritorio", "Escritório do diretor", -10, 2, 10, 10, M, M, M),
            new SalaDef("ala", "Ala de pacientes", 0, 2, 16, 10, F, F, F),
            new SalaDef("deposito", "Depósito", 16, 2, 12, 10, F, F, F),
            new SalaDef("oficina", "Oficina", -28, -12, 10, 10, M, M, F),
            new SalaDef("caldeira", "Sala da caldeira", -18, -12, 12, 10, Q, Q, Q) { caldeira = true },
            new SalaDef("subsolo", "Arquivo do subsolo", -6, -12, 18, 10, F, F, F),
            new SalaDef("refeitorio", "Refeitório", 12, -12, 16, 10, M, M, M),
            new SalaDef("secreta", "Sala secreta", -2, -20, 8, 8, F, F, F) { secreta = true },
            new SalaDef("exterior", "Lado de fora", 28, -4, 7, 8, F, F, F) { saida = true },
        };

        // Aberturas nas paredes: (x da célula, y da linha) para paredes horizontais; (x da linha, y da célula) para verticais.
        static readonly HashSet<Vector2Int> AberturasH = new HashSet<Vector2Int>
        {
            new Vector2Int(-25, 2), new Vector2Int(-24, 2),   // quarto (porta)
            new Vector2Int(-16, 2), new Vector2Int(-15, 2),   // consultório
            new Vector2Int(-6, 2), new Vector2Int(-5, 2),     // escritório
            new Vector2Int(6, 2), new Vector2Int(7, 2),       // ala
            new Vector2Int(21, 2), new Vector2Int(22, 2),     // depósito
            new Vector2Int(-24, -2), new Vector2Int(-23, -2), // oficina
            new Vector2Int(-13, -2), new Vector2Int(-12, -2), // caldeira
            new Vector2Int(2, -2), new Vector2Int(3, -2),     // subsolo
            new Vector2Int(19, -2), new Vector2Int(20, -2),   // refeitório
            new Vector2Int(1, -12), new Vector2Int(2, -12),   // sala secreta (porta)
        };
        static readonly HashSet<Vector2Int> AberturasV = new HashSet<Vector2Int>
        {
            new Vector2Int(16, 8), new Vector2Int(16, 9),     // ala ↔ depósito
            new Vector2Int(-6, -8), new Vector2Int(-6, -7),   // caldeira ↔ subsolo
            new Vector2Int(28, -1), new Vector2Int(28, 0),    // porta principal (corredor → lado de fora)
        };

        static Vector2 Centro(SalaDef s) => s.r.center;

        // ================================================================== entrada

        [MenuItem("Vigilia/Construir tudo (arte + config + cenas)")]
        public static void ConstruirTudo()
        {
            GeradorDeArte.GerarTudo();
            var cfg = GarantirConfig();
            ConstruirMenu();
            ConstruirJogo(cfg);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{PastaCenas}/Menu.unity", true),
                new EditorBuildSettingsScene($"{PastaCenas}/Jogo.unity", true),
            };
            ConfigurarPlayer();
            AssetDatabase.SaveAssets();
            Debug.Log("[Vigilia] Construção concluída: Menu e Jogo.");
        }

        static BalanceConfig GarantirConfig()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<BalanceConfig>(CaminhoConfig);
            if (cfg != null) return cfg;
            Directory.CreateDirectory(Path.GetDirectoryName(CaminhoConfig));
            cfg = ScriptableObject.CreateInstance<BalanceConfig>();
            AssetDatabase.CreateAsset(cfg, CaminhoConfig);
            return cfg;
        }

        static void ConfigurarPlayer()
        {
            PlayerSettings.companyName = "Insper Jogos Digitais";
            PlayerSettings.productName = "VIGILIA";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;   // GDD 13.4: evita tela preta no itch.io
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
        }

        // ================================================================== Menu

        static void ConstruirMenu()
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = CriarCamera();
            cam.transform.position = new Vector3(0, 0, -10);
            var luz = new GameObject("LuzGlobal").AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Global;
            new GameObject("Menu").AddComponent<MenuPrincipal>();
            Directory.CreateDirectory(PastaCenas);
            EditorSceneManager.SaveScene(cena, $"{PastaCenas}/Menu.unity");
        }

        static Camera CriarCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.015f);
            go.AddComponent<UniversalAdditionalCameraData>();
            go.AddComponent<AudioListener>();
            return cam;
        }

        // ================================================================== Jogo

        static void ConstruirJogo(BalanceConfig cfg)
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = CriarCamera();
            var luzGlobal = new GameObject("LuzGlobal").AddComponent<Light2D>();
            luzGlobal.lightType = Light2D.LightType.Global;
            luzGlobal.intensity = cfg.intensidadeLuzGlobalNoite;

            var mapa = new GameObject("Mapa").transform;
            var defs = Salas();
            var salasPorId = new Dictionary<string, Sala>();
            foreach (var d in defs) salasPorId[d.id] = CriarSala(d, mapa);
            CriarParedes(defs, mapa);

            // Portas
            CriarPorta("PortaQuarto", Porta.Tipo.Quarto, new Vector2(-24, 2), mapa);
            CriarPorta("PortaSecreta", Porta.Tipo.Secreta, new Vector2(2, -12), mapa);
            CriarPorta("PortaPrincipal", Porta.Tipo.Saida, new Vector2(28, 0), mapa, vertical: true);

            // Luzes quentes (poucas: GDD 13.4)
            salasPorId["quarto"].luzQuente = LuzQuente(new Vector2(-24, 7), mapa, 5f);
            salasPorId["caldeira"].luzQuente = LuzQuente(new Vector2(-12, -7), mapa, 6.5f);

            // Quarto: cama e escrivaninha
            var cama = Objeto("Cama", "cama", new Vector2(-26.6f, 9.6f), mapa, new Vector2(1.1f, 2.1f), -5);
            var sono = cama.AddComponent<SleepController>();
            var mesa = Objeto("Escrivaninha", "mesa", new Vector2(-21.8f, 10.8f), mapa, new Vector2(2f, 1f), -5);
            var caderno = mesa.AddComponent<Notebook>();

            // Caldeira
            var cald = Objeto("Caldeira", "caldeira", new Vector2(-12, -8.5f), mapa, new Vector2(3f, 3f), -5);
            var caldeira = cald.AddComponent<Caldeira>();
            caldeira.visual = cald.GetComponent<SpriteRenderer>();

            CriarCenario(mapa);
            CriarItens(mapa);
            CriarEsconderijos(mapa);
            CriarPontos(mapa);

            // Jogador
            var jogador = CriarJogador(cfg, out var lanterna, out var detector);
            jogador.transform.position = new Vector3(-25, 8, 0);

            // NPCs
            var rota = new List<Vector2>();
            foreach (var d in defs) if (!d.quarto && !d.secreta) rota.Add(Centro(d));
            CriarNpc("marta", "Marta Quill", TipoInimigo.Funcionario, 0, true, new Vector2(-15, 6), new Vector2(10, 0), new Color(0.92f, 0.92f, 0.96f), rota, mapa, "Elias...");
            CriarNpc("wren", "Sr. Wren", TipoInimigo.PacienteVelho, 1, true, new Vector2(8, 7), new Vector2(10, 8), new Color(0.7f, 0.78f, 0.82f), rota, mapa, null);
            CriarNpc("otis", "Otis Brandt", TipoInimigo.Funcionario, 2, true, new Vector2(-23, -6), new Vector2(-12, -5), new Color(0.45f, 0.5f, 0.65f), rota, mapa, null);
            CriarNpc("almeida", "Dr. Henrik Almeida", TipoInimigo.Funcionario, 3, true, new Vector2(-13, 8), new Vector2(-5, 7), new Color(0.95f, 0.95f, 0.9f), rota, mapa, null);
            CriarNpc("paciente1", "Paciente", TipoInimigo.PacienteVelho, 4, true, new Vector2(20, -7), new Vector2(20, -7), new Color(0.65f, 0.72f, 0.75f), rota, mapa, null);
            CriarNpc("paciente2", "Paciente", TipoInimigo.PacienteVelho, 5, true, new Vector2(12, 5), new Vector2(22, 7), new Color(0.65f, 0.7f, 0.7f), rota, mapa, null);
            // decisão do grupo (04/11): mais possuídos (noite 2 = 6, noite 3 = 8)
            CriarNpc("paciente3", "Paciente", TipoInimigo.PacienteVelho, 6, true, new Vector2(14, 8), new Vector2(-15, 7), new Color(0.82f, 0.9f, 0.84f), rota, mapa, null, arte: "paciente1");
            CriarNpc("paciente4", "Paciente", TipoInimigo.Funcionario, 7, true, new Vector2(24, -9), new Vector2(4, -7), new Color(0.9f, 0.84f, 0.84f), rota, mapa, null, arte: "paciente2");
            // possuídos extras: só aparecem na fuga da noite 3, espalhados entre o subsolo e a porta principal
            Vector2[] postosFuga =
            {
                new Vector2(12, 0), new Vector2(18, -0.5f), new Vector2(24, 0.5f), new Vector2(8, 7), new Vector2(22, 9),
                new Vector2(17, -6), new Vector2(25, -10), new Vector2(-1, -7), new Vector2(9, -9), new Vector2(-14, 5),
                new Vector2(4, 10), new Vector2(-8, -5),
            };
            for (int i = 0; i < postosFuga.Length; i++)
                CriarNpc($"interno{i}", "Interno", i % 2 == 0 ? TipoInimigo.PacienteVelho : TipoInimigo.Funcionario, 100 + i, false,
                         postosFuga[i], postosFuga[i], Color.Lerp(Color.white, new Color(0.75f, 0.85f, 0.8f), (i % 3) / 2f), rota, mapa, null,
                         arte: i % 2 == 0 ? "paciente1" : "paciente2", soNaFuga: true);
            CriarNpc("holt", "Dr. Alaric Holt", TipoInimigo.Funcionario, 99, false, new Vector2(-5, 8), new Vector2(-5, 8), new Color(0.3f, 0.3f, 0.34f), rota, mapa, null);

            // Sistemas
            var sis = new GameObject("Sistemas");
            var jogo = sis.AddComponent<Jogo>();
            jogo.config = cfg;
            jogo.noites = sis.AddComponent<NightManager>();
            jogo.possessao = sis.AddComponent<PossessionMeter>();
            jogo.temperatura = sis.AddComponent<RoomTemperature>();
            jogo.dialogo = sis.AddComponent<DialogueSystem>();
            jogo.audioJogo = sis.AddComponent<AudioJogo>();
            jogo.encontro = sis.AddComponent<EncontroMonstro>();
            var nav = sis.AddComponent<GridNav>();
            nav.origem = new Vector2(-30, -22);
            nav.tamanho = new Vector2(66, 36);
            nav.paredes = 1 << LayerParedes;
            jogo.nav = nav;
            var ui = sis.AddComponent<UIJogo>();
            var segue = cam.gameObject.AddComponent<CameraSegue>();
            segue.alvo = jogador.transform;
            cam.transform.position = new Vector3(-25, 8, -10);
            ui.cameraSegue = segue;
            jogo.ui = ui;
            jogo.jogador = jogador;
            jogo.lanterna = lanterna;
            jogo.detectorLuz = detector;
            jogo.caderno = caderno;
            jogo.cama = sono;
            jogo.luzGlobal = luzGlobal;
            jogo.mapa = mapa;

            EditorSceneManager.SaveScene(cena, $"{PastaCenas}/Jogo.unity");
        }

        // ---------------------------------------------------------------- salas e paredes

        static Sala CriarSala(SalaDef d, Transform mapa)
        {
            var go = new GameObject("Sala_" + d.id);
            go.transform.SetParent(mapa);
            var s = go.AddComponent<Sala>();
            s.id = d.id; s.nome = d.nome;
            s.area = new Rect(d.r.x, d.r.y, d.r.width, d.r.height);
            s.temperaturaPorNoite = d.temp;
            s.ehQuarto = d.quarto; s.ehSaida = d.saida; s.dependeDaCaldeira = d.caldeira; s.ehSalaSecreta = d.secreta;

            var piso = new GameObject("Piso");
            piso.transform.SetParent(go.transform);
            piso.transform.position = new Vector3(d.r.center.x, d.r.center.y, 0);
            var sr = piso.AddComponent<SpriteRenderer>();
            // piso próprio da sala (piso_<id>) se existir; senão o piso geral
            sr.sprite = GeradorDeArte.TemFinal("piso_" + d.id) ? GeradorDeArte.Sprite("piso_" + d.id) : GeradorDeArte.Sprite("piso");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(d.r.width, d.r.height);
            sr.sortingOrder = -10;
            s.piso = sr;
            if (GeradorDeArte.TemFinal("piso") || GeradorDeArte.TemFinal("piso_" + d.id))
            {
                // a textura final já tem cor: o tom da temperatura fica sutil
                s.corPisoFria = new Color(0.78f, 0.86f, 1f);
                s.corPisoMorna = new Color(0.95f, 0.93f, 0.9f);
                s.corPisoQuente = new Color(1f, 0.84f, 0.7f);
            }
            return s;
        }

        static void CriarParedes(List<SalaDef> defs, Transform mapa)
        {
            int Dono(int x, int y)
            {
                for (int k = 0; k < defs.Count; k++) if (defs[k].r.Contains(new Vector2Int(x, y))) return k;
                return -1;
            }
            var raiz = new GameObject("Paredes").transform;
            raiz.SetParent(mapa);
            const int X0 = -31, X1 = 36, Y0 = -23, Y1 = 15;

            // horizontais: fronteira entre (x, y-1) e (x, y), na linha y
            for (int y = Y0; y <= Y1; y++)
            {
                int ini = int.MinValue;
                for (int x = X0; x <= X1 + 1; x++)
                {
                    bool parede = x <= X1 && Dono(x, y - 1) != Dono(x, y) && !AberturasH.Contains(new Vector2Int(x, y));
                    if (parede && ini == int.MinValue) ini = x;
                    if (!parede && ini != int.MinValue)
                    {
                        Parede(raiz, new Vector2((ini + x) / 2f, y), new Vector2(x - ini + Espessura, Espessura));
                        ini = int.MinValue;
                    }
                }
            }
            // verticais: fronteira entre (x-1, y) e (x, y), na linha x
            for (int x = X0; x <= X1; x++)
            {
                int ini = int.MinValue;
                for (int y = Y0; y <= Y1 + 1; y++)
                {
                    bool parede = y <= Y1 && Dono(x - 1, y) != Dono(x, y) && !AberturasV.Contains(new Vector2Int(x, y));
                    if (parede && ini == int.MinValue) ini = y;
                    if (!parede && ini != int.MinValue)
                    {
                        Parede(raiz, new Vector2(x, (ini + y) / 2f), new Vector2(Espessura, y - ini + Espessura));
                        ini = int.MinValue;
                    }
                }
            }
        }

        static GameObject Parede(Transform pai, Vector2 centro, Vector2 tamanho)
        {
            var go = new GameObject("Parede");
            go.transform.SetParent(pai);
            go.transform.position = centro;
            go.layer = LayerParedes;
            bool horizontal = tamanho.x >= tamanho.y;
            // visual mais grosso que o colisor: a parede lê como bloco sólido sem estreitar as portas para a navegação
            Vector2 visual = horizontal ? new Vector2(tamanho.x + (EspessuraVisual - Espessura), EspessuraVisual)
                                        : new Vector2(EspessuraVisual, tamanho.y + (EspessuraVisual - Espessura));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GeradorDeArte.Sprite("parede");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = visual;
            sr.sortingOrder = 3;

            // contorno escuro em volta do topo da parede
            var borda = new GameObject("Contorno");
            borda.transform.SetParent(go.transform, false);
            var sb = borda.AddComponent<SpriteRenderer>();
            sb.sprite = GeradorDeArte.Sprite("quadrado");
            sb.drawMode = SpriteDrawMode.Tiled;
            sb.size = visual + Vector2.one * 0.12f;
            sb.color = new Color(0.05f, 0.04f, 0.05f);
            sb.sortingOrder = 2;

            // sombra no chão dos dois lados (oclusão): separa os cômodos e dá volume
            for (int lado = 0; lado < 2; lado++)
            {
                var ao = new GameObject("SombraChao");
                ao.transform.SetParent(go.transform, false);
                var sa = ao.AddComponent<SpriteRenderer>();
                sa.sprite = GeradorDeArte.Sprite("sombra_borda");
                sa.sortingOrder = -9;
                float comprimento = horizontal ? visual.x : visual.y;
                float desloc = EspessuraVisual / 2f + LarguraSombra / 2f;
                var b = sa.sprite != null ? (Vector2)sa.sprite.bounds.size : Vector2.one;
                ao.transform.localScale = new Vector3(comprimento / b.x, LarguraSombra / b.y, 1);
                // o lado escuro do gradiente fica no topo do sprite: gira para encostar na parede
                if (horizontal)
                {
                    ao.transform.localPosition = new Vector3(0, lado == 0 ? -desloc : desloc, 0);
                    ao.transform.localRotation = Quaternion.Euler(0, 0, lado == 0 ? 0 : 180);
                }
                else
                {
                    ao.transform.localPosition = new Vector3(lado == 0 ? -desloc : desloc, 0, 0);
                    ao.transform.localRotation = Quaternion.Euler(0, 0, lado == 0 ? -90 : 90); // topo escuro aponta para a parede
                }
            }

            var col = go.AddComponent<BoxCollider2D>();
            col.size = tamanho;
            var sombra = go.AddComponent<ShadowCaster2D>();
            sombra.selfShadows = true;
            return go;
        }

        static void CriarPorta(string nome, Porta.Tipo tipo, Vector2 pos, Transform mapa, bool vertical = false)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(mapa);
            go.transform.position = pos;
            go.layer = LayerParedes;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GeradorDeArte.Sprite("porta");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = vertical ? new Vector2(EspessuraVisual, 2f) : new Vector2(2f, EspessuraVisual);
            sr.color = tipo == Porta.Tipo.Quarto ? new Color(0.45f, 0.3f, 0.2f)
                     : tipo == Porta.Tipo.Saida ? new Color(0.35f, 0.18f, 0.12f) : new Color(0.3f, 0.28f, 0.25f);
            sr.sortingOrder = 2;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = vertical ? new Vector2(Espessura, 2f) : new Vector2(2f, Espessura);
            var sombra = go.AddComponent<ShadowCaster2D>();
            var p = go.AddComponent<Porta>();
            p.tipo = tipo; p.colisor = col; p.visual = sr; p.sombra = sombra;
        }

        static Light2D LuzQuente(Vector2 pos, Transform mapa, float raio)
        {
            var go = new GameObject("LuzQuente");
            go.transform.SetParent(mapa);
            go.transform.position = pos;
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = new Color(1f, 0.55f, 0.25f);
            l.intensity = 0.7f;
            l.pointLightOuterRadius = raio;
            l.pointLightInnerRadius = 0.5f;
            l.shadowsEnabled = true;
            return l;
        }

        static GameObject Objeto(string nome, string sprite, Vector2 pos, Transform pai, Vector2 tamanho, int ordem)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(pai);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GeradorDeArte.Sprite(sprite);
            sr.sortingOrder = ordem;
            if (sr.sprite != null)
            {
                var b = sr.sprite.bounds.size;
                if (GeradorDeArte.TemFinal(sprite))
                {
                    float k = Mathf.Min(tamanho.x / b.x, tamanho.y / b.y);   // arte final: mantém a proporção
                    go.transform.localScale = new Vector3(k, k, 1);
                }
                else go.transform.localScale = new Vector3(tamanho.x / b.x, tamanho.y / b.y, 1);
            }
            return go;
        }

        // ---------------------------------------------------------------- itens

        static readonly bool[] Nunca = { false, false, false };
        static readonly bool[] Sempre = { true, true, true };
        static bool[] SoNoite(int n) { var b = new bool[3]; b[n - 1] = true; return b; }

        static Pickup Item(Transform pai, string id, TipoItem tipo, string titulo, string sprite, Vector2 pos,
                           bool[] dia, bool[] noite, bool porNoite = false, int ordemPilha = -1, string doc = null)
        {
            var go = Objeto("Item_" + id, sprite, pos, pai, new Vector2(0.7f, 0.7f), 0);
            var p = go.AddComponent<Pickup>();
            p.id = id; p.tipo = tipo; p.titulo = titulo; p.porNoite = porNoite;
            p.disponivelDia = (bool[])dia.Clone(); p.disponivelNoite = (bool[])noite.Clone();
            p.ordemPilha = ordemPilha; p.textoDocumento = doc;
            return p;
        }

        static void CriarItens(Transform mapa)
        {
            var pai = new GameObject("Itens").transform;
            pai.SetParent(mapa);

            Item(pai, "cobertor", TipoItem.Cobertor, "cobertor", "item_cobertor", new Vector2(25.5f, 10.5f), Nunca, SoNoite(1));
            Item(pai, "chave", TipoItem.Chave, "chave do quarto", "item_chave", new Vector2(-8.5f, 10.5f), Nunca, SoNoite(2));
            Item(pai, "pedecabra", TipoItem.PeDeCabra, "pé de cabra", "item_pedecabra", new Vector2(-26.5f, -10.5f), Nunca, SoNoite(2));

            // TODO(Q-03): textos das pistas. Escrita ambígua de propósito (GDD 12).
            Item(pai, "nota_propria", TipoItem.NotaPropria, "um bilhete", "item_papel", new Vector2(-11.5f, 10.5f), Nunca, SoNoite(2),
                doc: "Na sua letra. Você não lembra de ter escrito.\n\n\"Não confie na enfermeira. Ela sabe meu nome.\nAmanhã eu escrevo de novo, caso eu esqueça.\n— E.\"");
            Item(pai, "pulseira", TipoItem.Pulseira, "uma pulseira de paciente", "item_pulseira", new Vector2(-1.5f, 3.5f), new[] { false, false, true }, SoNoite(3),
                doc: "Pulseira de identificação, plástico amarelado.\n\nVAUGHN, E.\nAla de vigília · leito 7\nInternação: [data rasurada]");
            Item(pai, "memorando", TipoItem.PistaDiretor, "um memorando do diretor", "item_papel", new Vector2(-3.5f, 10.5f), new[] { false, false, true }, SoNoite(3),
                doc: "Memorando interno · Dr. A. Holt\n\n\"Os registros da ala de vigília ficam na sala atrás do arquivo do subsolo.\nNinguém desce sem minha autorização.\"\n\n(A passagem do subsolo agora está aberta.)");

            // Evidências (Q-05 em aberto: documentos e fotos, sugestão do GDD). Ficam de dia e de noite até serem pegas.
            Item(pai, "ficha_internacao", TipoItem.Evidencia, "ficha de internação", "item_papel", new Vector2(14.5f, 10.5f), Sempre, Sempre,
                doc: "Ficha de internação.\n\n\"Insônia persistente. Após a terceira noite sem sono, o paciente não reconhece a própria família.\nConduta: manter acordado. Observar.\"");
            Item(pai, "registro_turnos", TipoItem.Evidencia, "registro de turnos", "item_papel", new Vector2(-9f, 3.5f), Sempre, Sempre,
                doc: "Registro de turnos da enfermagem.\n\nToda noite, no mesmo horário, a mesma anotação, na mesma letra:\n\"Equipe dormiu. Nada a relatar.\"");
            Item(pai, "foto_antiga", TipoItem.Evidencia, "foto antiga", "item_papel", new Vector2(18.5f, 3.5f), Sempre, Sempre,
                doc: "Fotografia da equipe, 1962.\n\nMarta. Otis. O Dr. Holt.\nAs mesmas caras de hoje. Ninguém envelheceu.");
            Item(pai, "diario_manutencao", TipoItem.Evidencia, "diário de manutenção", "item_papel", new Vector2(-16.5f, -3.5f), Sempre, Sempre,
                doc: "Diário do zelador.\n\n\"Aquecimento desligado às 23h por ordem da direção. De novo.\nO doutor diz que o frio ajuda a manter eles acordados.\"");
            Item(pai, "aparato", TipoItem.Evidencia, "registros da ala de vigília", "item_papel", new Vector2(-0.5f, -15f), Sempre, Sempre,
                doc: "Registros da ala de vigília.\n\nGráficos de horas de sono por interno, noite após noite. Nomes riscados, um por um.\nO último nome não está riscado. É o seu.");

            // Itens de ataque (Q-04): um de cada por noite
            Item(pai, "sedativo", TipoItem.Sedativo, "sedativo", "item_sedativo", new Vector2(-19f, 3.5f), Nunca, Sempre, porNoite: true);
            Item(pai, "chave_inglesa", TipoItem.ObjetoPesado, "chave inglesa", "item_pesado", new Vector2(-19.5f, -3.5f), Nunca, Sempre, porNoite: true);

            // Pilhas: a ordem 0 fica no corredor perto do quarto (alcançável logo no início, anti dead end)
            Vector2[] pilhas = { new Vector2(-20f, -0.8f), new Vector2(-26.5f, -3.5f), new Vector2(-7.5f, -10.5f), new Vector2(26.5f, -10.5f), new Vector2(1.5f, 11f) };
            for (int i = 0; i < pilhas.Length; i++)
                Item(pai, "pilha" + i, TipoItem.Pilha, "pilha", "item_pilha", pilhas[i], Nunca, Nunca, porNoite: true, ordemPilha: i);
        }

        /// <summary>Móveis que dizem o que é cada cômodo (só visual, sem colisão).</summary>
        static void CriarCenario(Transform mapa)
        {
            var pai = new GameObject("Cenario").transform;
            pai.SetParent(mapa);
            void M(string sprite, float x, float y, float w, float h, float rot = 0)
            {
                // w × h é o tamanho do móvel na posição original; a rotação gira depois
                var go = Objeto(sprite, sprite, new Vector2(x, y), pai, new Vector2(w, h), -6);
                go.transform.rotation = Quaternion.Euler(0, 0, rot);
            }
            // quarto
            M("armario", -21.6f, 3.0f, 1.4f, 1.0f);
            // consultório: macas e armário de remédios
            M("maca", -18.3f, 7.5f, 1.1f, 2.2f);
            M("maca", -12.0f, 7.5f, 1.1f, 2.2f);
            M("armario_remedios", -15.0f, 11.4f, 1.8f, 0.8f);
            // escritório do diretor: tapete, escrivaninha grande e estantes
            M("tapete", -5.0f, 6.5f, 3.2f, 2.2f);
            M("mesa", -5.0f, 9.3f, 2.4f, 1.2f);
            M("estante", -5.0f, 11.5f, 2.4f, 0.75f);
            M("estante", -9.5f, 7.0f, 2.4f, 0.75f, 90);
            // ala de pacientes: segunda fileira de camas
            for (int i = 0; i < 5; i++) M("cama", 2.5f + i * 2.4f, 4.3f, 1.1f, 2.1f, 180);
            // depósito: prateleiras com caixas
            M("prateleira", 22.0f, 11.4f, 3.0f, 0.9f);
            M("prateleira", 22.0f, 7.0f, 3.0f, 0.9f);
            M("prateleira", 18.5f, 5.0f, 3.0f, 0.9f, 90);
            // oficina: bancada de ferramentas
            M("bancada", -23.0f, -11.2f, 3.0f, 1.2f);
            M("prateleira", -27.3f, -7.0f, 3.0f, 0.9f, 90);
            // arquivo do subsolo: fileiras de arquivos
            for (int i = 0; i < 4; i++) M("arquivo", -4.5f + i * 1.05f, -3.0f, 1.0f, 1.0f);
            for (int i = 0; i < 4; i++) M("arquivo", 6.0f + i * 1.05f, -3.0f, 1.0f, 1.0f);
            for (int i = 0; i < 3; i++) M("arquivo", 11.3f, -6.0f - i * 1.05f, 1.0f, 1.0f, 90);
            // refeitório: mesas compridas
            M("mesa_refeitorio", 22.5f, -4.8f, 3.8f, 1.5f);
            M("mesa_refeitorio", 22.5f, -8.0f, 3.8f, 1.5f);
            M("mesa_refeitorio", 22.5f, -11.0f, 3.8f, 1.5f);
            // sala secreta: cadeira de contenção e o aparato
            M("cadeira_contencao", 2.0f, -17.0f, 1.3f, 1.3f);
            M("aparato", 4.6f, -13.5f, 1.6f, 1.2f);
            // lado de fora: pinheiros na neve
            M("pinheiro", 33.2f, 2.6f, 2.4f, 2.4f);
            M("pinheiro", 33.5f, -2.6f, 2.0f, 2.0f);
        }

        static void CriarEsconderijos(Transform mapa)
        {
            var pai = new GameObject("Esconderijos").transform;
            pai.SetParent(mapa);
            (Vector2 pos, string nome, string sprite, Vector2 tam)[] lista =
            {
                (new Vector2(4f, 10.2f), "embaixo da cama", "cama", new Vector2(1.1f, 2.1f)),
                (new Vector2(27f, 4f), "armário", "armario", new Vector2(1.5f, 1f)),
                (new Vector2(-11f, 2.9f), "armário", "armario", new Vector2(1.5f, 1f)),
                (new Vector2(16.5f, -7.5f), "embaixo da mesa", "mesa_refeitorio", new Vector2(3f, 1.2f)),
                (new Vector2(10.5f, -11.1f), "armário", "armario", new Vector2(1.5f, 1f)),
                (new Vector2(-19.5f, -11.1f), "embaixo da bancada", "bancada", new Vector2(2.2f, 0.9f)),
                (new Vector2(12f, 10.2f), "embaixo da cama", "cama", new Vector2(1.1f, 2.1f)),
            };
            for (int i = 0; i < lista.Length; i++)
            {
                var go = Objeto("Esconderijo" + i, lista[i].sprite, lista[i].pos, pai, lista[i].tam, -5);
                var h = go.AddComponent<HidingSpot>();
                h.nome = lista[i].nome; h.ordem = i; h.visual = go.GetComponent<SpriteRenderer>();
            }
            // camas decorativas da ala
            for (int i = 0; i < 3; i++) Objeto("CamaAla" + i, "cama", new Vector2(6f + i * 2f, 10.2f), pai, new Vector2(1.1f, 2.1f), -5);
        }

        static void CriarPontos(Transform mapa)
        {
            void Ponto(string id, Vector2 p)
            {
                var go = new GameObject("Inicio_" + id);
                go.transform.SetParent(mapa);
                go.transform.position = p;
                go.AddComponent<PontoDeInicio>().id = id;
            }
            Ponto("dia", new Vector2(-25f, 8f));
            Ponto("noite1", new Vector2(-25.5f, 6f));
            Ponto("noite2", new Vector2(-24f, -0.5f));   // trancado fora do quarto (D-16)
            Ponto("noite3", new Vector2(-25.5f, 6f));
        }

        // ---------------------------------------------------------------- personagens

        static PlayerController CriarJogador(BalanceConfig cfg, out FlashlightController lanterna, out LightFreezeDetector detector)
        {
            var go = new GameObject("Elias");
            go.layer = LayerJogador;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;

            var corpo = new GameObject("Corpo");
            corpo.transform.SetParent(go.transform, false);
            var sr = corpo.AddComponent<SpriteRenderer>();
            // Preferência: vista de cima (topo_, gira com a mira) > corpo inteiro por direção (jogador_) > provisória
            bool eliasTopo = GeradorDeArte.TemFinal("topo_elias");
            bool eliasFinal = !eliasTopo && GeradorDeArte.TemFinal("jogador_elias");
            sr.sprite = eliasTopo ? GeradorDeArte.Sprite("topo_elias") : eliasFinal ? GeradorDeArte.Sprite("jogador_elias") : GeradorDeArte.Sprite("personagem");
            sr.color = eliasTopo || eliasFinal ? Color.white : new Color(0.62f, 0.5f, 0.38f);
            sr.sortingOrder = 6;
            corpo.transform.localScale = Vector3.one * (eliasTopo ? 1f : 0.9f);

            SpriteDirecional dirElias = null;
            if (eliasFinal)
            {
                corpo.transform.localScale = Vector3.one;
                dirElias = go.AddComponent<SpriteDirecional>();
                dirElias.alvo = sr;
                dirElias.dia = Direcionais("jogador_elias");
            }

            var mira = new GameObject("Mira");
            mira.transform.SetParent(go.transform, false);
            var srm = mira.AddComponent<SpriteRenderer>();
            srm.sprite = GeradorDeArte.Sprite("mira");
            srm.color = new Color(1f, 0.9f, 0.7f, 0.6f);
            srm.sortingOrder = 7;
            mira.transform.localScale = Vector3.one * 1.6f;
            if (!eliasFinal) corpo.transform.SetParent(mira.transform, true); // arte vista de cima gira junto com a mira

            var luzGo = new GameObject("Lanterna");
            luzGo.transform.SetParent(go.transform, false);
            var luz = luzGo.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightOuterRadius = cfg.alcanceLanterna;
            luz.pointLightOuterAngle = cfg.meiaAberturaGraus * 2;
            luz.pointLightInnerAngle = cfg.meiaAberturaGraus * 1.3f;
            luz.intensity = cfg.intensidadeLanterna;
            luz.color = cfg.corLanterna;
            luz.shadowsEnabled = true;
            luz.shadowIntensity = 1f;

            // halo pequeno em volta de Elias para ele nunca sumir totalmente
            var haloGo = new GameObject("Halo");
            haloGo.transform.SetParent(go.transform, false);
            var halo = haloGo.AddComponent<Light2D>();
            halo.lightType = Light2D.LightType.Point;
            halo.pointLightOuterRadius = 1.6f;
            halo.pointLightInnerRadius = 0.1f;
            halo.intensity = 0.35f;
            halo.color = new Color(0.7f, 0.75f, 0.9f);

            var pc = go.AddComponent<PlayerController>();
            pc.corpo = sr;
            pc.pivoMira = mira.transform;
            pc.direcional = dirElias;
            lanterna = go.AddComponent<FlashlightController>();
            lanterna.luz = luz;
            lanterna.jogador = pc;
            detector = go.AddComponent<LightFreezeDetector>();
            detector.lanterna = lanterna;
            detector.paredes = 1 << LayerParedes;
            return pc;
        }

        static void CriarNpc(string id, string nome, TipoInimigo tipo, int ordem, bool inimigo, Vector2 dia, Vector2 noite,
                             Color cor, List<Vector2> rota, Transform mapa, string sussurro, string arte = null, bool soNaFuga = false)
        {
            string idArte = arte ?? id;   // figurantes reaproveitam a arte de outro interno, com outra cor
            var go = new GameObject("NPC_" + id);
            go.transform.SetParent(mapa);
            go.transform.position = dia;
            go.layer = LayerInimigos;
            var sr = go.AddComponent<SpriteRenderer>();
            // Preferência: vista de cima (topo_<id>, gira) > corpo inteiro por direção (npc_<id>) > provisória
            var topoDia = GeradorDeArte.TemFinal("topo_" + idArte) ? GeradorDeArte.Sprite("topo_" + idArte) : null;
            var topoNoite = GeradorDeArte.TemFinal($"topo_{idArte}_noite") ? GeradorDeArte.Sprite($"topo_{idArte}_noite") : null;
            bool topo = topoDia != null || topoNoite != null;
            var setDia = topo ? new Sprite[4] : Direcionais("npc_" + id);
            var setNoite = topo ? new Sprite[4] : Direcionais($"npc_{id}_noite");
            bool final = Tem(setDia) || Tem(setNoite);
            if (final && !Tem(setDia)) setDia = setNoite;   // só existe a versão da noite: usa nas duas
            sr.sprite = topo ? (topoDia != null ? topoDia : topoNoite) : final ? Primeiro(setDia) : GeradorDeArte.Sprite("personagem");
            sr.sortingOrder = 5;
            go.transform.localScale = Vector3.one * (topo ? (tipo == TipoInimigo.PacienteVelho ? 0.95f : 1f) : final ? (tipo == TipoInimigo.PacienteVelho ? 0.94f : 1f) : (tipo == TipoInimigo.PacienteVelho ? 0.85f : 0.95f));
            var olhos = new GameObject("Olhos");
            olhos.transform.SetParent(go.transform, false);
            var so = olhos.AddComponent<SpriteRenderer>();
            so.sprite = topo && GeradorDeArte.TemFinal("olhos_topo") ? GeradorDeArte.Sprite("olhos_topo") : GeradorDeArte.Sprite("olhos");
            so.sortingOrder = 8;
            // os olhos brilham no escuro: um sprite sem luz (material Sprite-Unlit, se existir)
            var unlit = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            if (unlit != null) so.sharedMaterial = unlit;

            var e = go.AddComponent<EnemyAI>();
            e.npcId = id; e.nome = nome; e.tipo = tipo; e.ordemNoite = ordem; e.podeSerInimigo = inimigo;
            e.postoDia = dia; e.postoNoite = noite; e.rotaPatrulha = rota.ToArray();
            e.corpo = sr; e.olhos = so;
            if (topo)
            {
                // vista de cima: gira para onde anda; a versão possuída (braços estendidos) entra à noite
                e.spriteDia = topoDia != null ? topoDia : topoNoite;
                e.spriteNoite = topoNoite;
                e.corDia = Color.white;
                e.corNoite = topoNoite != null ? new Color(0.9f, 0.88f, 0.9f) : new Color(0.62f, 0.5f, 0.52f);
            }
            else if (final)
            {
                // arte final já tem as cores: sem tingir. Sem versão possuída, escurece e puxa para o vermelho.
                var d = go.AddComponent<SpriteDirecional>();
                d.alvo = sr;
                d.dia = setDia;
                d.noite = Tem(setNoite) ? setNoite : new Sprite[4];
                e.direcional = d;
                e.olhos = null;
                so.enabled = false;
                e.corDia = Color.white;
                e.corNoite = Tem(setNoite) ? new Color(0.88f, 0.85f, 0.88f) : new Color(0.62f, 0.5f, 0.52f);
            }
            else
            {
                e.corDia = cor;
                e.corNoite = Color.Lerp(cor, new Color(0.35f, 0.3f, 0.35f), 0.5f);
            }
            e.sussurro = sussurro; e.noiteDoSussurro = 1;
            e.soNaFuga = soNaFuga;
            if (arte != null && topo) { e.corDia = cor; e.corNoite = cor * 0.9f; }
            if (!soNaFuga) go.AddComponent<ConversaNpc>();
        }

        // ---------------------------------------------------------------- arte por direção

        /// <summary>base = olhando para cima; base_sul, base_leste, base_oeste opcionais (SpriteDirecional).</summary>
        static Sprite[] Direcionais(string baseNome)
        {
            Sprite F(string n) => GeradorDeArte.TemFinal(n) ? GeradorDeArte.Sprite(n) : null;
            return new[] { F(baseNome), F(baseNome + "_sul"), F(baseNome + "_leste"), F(baseNome + "_oeste") };
        }

        static bool Tem(Sprite[] s) { foreach (var x in s) if (x != null) return true; return false; }
        static Sprite Primeiro(Sprite[] s) { foreach (var x in s) if (x != null) return x; return null; }

        // ================================================================== linha de comando

        /// <summary>Unity.exe -batchmode -executeMethod Vigilia.EditorTools.ConstrutorDeCenas.ConstruirTudoBatch -quit</summary>
        public static void ConstruirTudoBatch() => ConstruirTudo();
    }
}
