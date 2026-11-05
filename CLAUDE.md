# VIGÍLIA — instruções para o Claude Code

Jogo 2D de suspense em **Unity 2D (URP + Light 2D), build WebGL no itch.io**. Projeto da disciplina Jogos Digitais (Insper 2026.1), grupo de 4 pessoas. Elias Vaughn, jornalista, passa 3 dias e 3 noites num sanatório no Alasca: **quem não dorme é possuído**. À noite os possuídos andam até ele e só param sob a luz da lanterna; ele precisa chegar à cama antes que `possessao` chegue a 100.

## Fonte da verdade

1. `Vigilia_GDD_tecnico_v0.1.pdf` (v0.1, 22/10/2026) é a fonte oficial.
2. `docs/gdd/*.md` é a conversão fiel do PDF, uma seção por arquivo (as figuras viraram Mermaid). Se algo divergir, vale o PDF; e dentro do PDF, **vale a tabela, não o diagrama** (avisar o Enzo do erro).
3. `docs/vigilia.yaml` é o resumo compacto (Apêndice A). Use para consulta rápida.
4. `docs/notas-de-consistencia.md` lista ambiguidades achadas na conversão e como tratá-las até o grupo decidir.

Leia só o arquivo da seção relevante para a tarefa. O mapa está em `docs/README.md`.

## Status das decisões (todo item do GDD tem um)

| Status | O que fazer |
|---|---|
| **DECIDIDO** (D-xx) | Implementar. |
| **PROPOSTA** (P-xx) | Implementar atrás de configuração fácil de trocar (flag/campo em `BalanceConfig`) e avisar o Enzo. |
| **ABERTO** (Q-xx) | **Não inventar.** Placeholder com `// TODO(Q-xx)` e perguntar. |
| **TESTE** | Número vai para `BalanceConfig`, nunca fixo no código. |

## Regras de código (GDD seção 1.2 e 13.5)

- **Nenhum número de balanceamento no código.** Tudo no ScriptableObject `BalanceConfig` (valores em `docs/gdd/14-flags-e-balanceconfig.md`).
- **Nomes exatos do glossário** (`docs/gdd/04-glossario.md`): identificadores em português sem acento, camelCase; classes em PascalCase; eventos com prefixo `On` (`OnTransbordou`, `OnDormiu`, `OnToque`, `OnIluminado`, `OnSaiuDaLuz`).
- **Só o `PossessionMeter` escreve em `possessao`**: `AdicionarPossessao(x)`, `Dormir()`, `DefinirValor(v)` (este só para restaurar checkpoint). Valor sempre limitado a 0..100; `OnTransbordou` dispara **uma vez por noite**.
- **Máquinas de estado**: `enum` + `switch` + `MudarEstado(novo)` com ações de entrada e saída. Implementar as tabelas M-xx, J-xx, E-xx de `docs/gdd/09-maquinas-de-estado.md` linha por linha (citar o ID no comentário).
- **Congelar inimigo é lógica, não pixels**: `LightFreezeDetector` testa distância ≤ alcance, ângulo ≤ meia abertura, raycast sem parede, lanterna ligada e bateria > 0. A Light 2D é só visual.
- **SaveSnapshot = cópia profunda** de WorldState, NpcMemory e posições. Nunca copiar por referência. `transbordouNoite3` fica fora do snapshot.
- **NightManager é o único** que captura/restaura o snapshot e chama `NpcMemory.Limpar()`.
- **EndingResolver é função pura e total**: qualquer combinação de flags dá final + blocos; nunca lança exceção nem devolve lista vazia. Ter teste automático de todas as combinações.
- **Sem IA/LLM em runtime** (D-01). Falas e finais são determinísticos, por blocos de texto.

## WebGL (seção 13.4)

Sem threads, sem acesso a arquivos (persistir com `PlayerPrefs`/`JsonUtility`). Áudio só depois de um clique (cena Boot com botão "Jogar"). Poucas luzes: 1 lanterna + poucas fixas, nada de luz por inimigo. Sem botão "Sair": usar "Voltar ao menu". Testar em Chrome e Firefox. Cenas sugeridas: Boot, Menu, Intro, Jogo (dia e noite no mesmo mapa), Final.

## Como trabalhar com o Enzo

- Ele quer **entender** o código: passos pequenos e testáveis, explicar o porquê em poucas frases, apontar onde olhar para aprender.
- Créditos de assets e de trechos de código de terceiros (mais de 2–3 linhas) vão no `README.md`. Proibido copiar código de colegas.
- Antes de dizer que uma tarefa terminou, conferir `docs/gdd/19-definicao-de-pronto.md`.

## Projeto Unity (`Vigilia/`)

- Editor **6000.6.6f1** (`C:/Program Files/Unity/Hub/Editor/6000.6.6f1/Editor/Unity.exe`), template 2D URP, Input System novo (`activeInputHandler: 1`).
- **Câmera de cima** (decidido em 24/10/2026); cenas fixas para o encontro com o monstro (`EncontroMonstro`, port do zip em `Refs/`).
- **Noite 3 = fuga** (decidido em 04/11/2026): o monstro aparece só na sala secreta, na noite 3; depois Elias foge pela porta principal (final A/B). Detalhes e outras mudanças de design em `docs/implementacao.md` ("Mudanças de design decididas pelo grupo").
- Arte e música geradas por scripts em `Vigilia/Ferramentas/` (`gerar_arte_topo.py`, `gerar_props.py`, `gerar_musica.py`); depois, no Unity, Vigilia > Arte > Processar e construir tudo.
- Código em `Vigilia/Assets/_Vigilia/` (namespace `Vigilia`). Mapa GDD → código e escolhas provisórias: `docs/implementacao.md`.
- As cenas são geradas por `Editor/ConstrutorDeCenas.cs` (menu Vigilia > Construir tudo). Mudanças no mapa ou nos objetos da cena Jogo vão nesse arquivo; reconstruir apaga edições manuais da cena.
- Layers: 8 Paredes, 9 Jogador, 10 Inimigos, 11 Interativos.
- APIs do Unity 6: `Rigidbody2D.linearVelocity`; `FindObjectsByType` sem `FindObjectsSortMode` (obsoleto); campos de `UnityEngine.Object` não usam `??=`.

Verificação (o Unity não pode estar aberto no mesmo projeto):

```bash
U="/c/Program Files/Unity/Hub/Editor/6000.6.6f1/Editor/Unity.exe"; P="C:/Users/vinic/OneDrive/Desktop/NOVOJOGO/Vigilia"
"$U" -batchmode -quit -projectPath "$P" -executeMethod Vigilia.EditorTools.ConstrutorDeCenas.ConstruirTudoBatch -logFile build.log   # compila + reconstrói cenas
"$U" -batchmode -projectPath "$P" -runTests -testPlatform EditMode -assemblyNames Vigilia.Tests -testResults edit.xml
VIGILIA_SHOTS=<pasta> "$U" -batchmode -projectPath "$P" -runTests -testPlatform PlayMode -assemblyNames Vigilia.PlayTests -testResults play.xml   # joga o fim de semana e salva capturas
"$U" -batchmode -quit -projectPath "$P" -buildTarget WebGL -executeMethod Vigilia.EditorTools.BuildWebGL.Construir -logFile webgl.log
```

## Cronograma (seção 18)

| Sprint | Período | Foco |
|---|---|---|
| **S1** | até **29/10/2026** | Protótipo de 2 noites: paredes, 2–3 salas, lanterna com cone, congelar inimigos, PossessionMeter, temperatura da sala, checkpoint/transbordo, máquinas de estado, 1 tipo de inimigo, esconderijos básicos, **build WebGL de teste no itch.io**. |
| S2 | 29/10 a 05/11 | Noite 3, WorldState/NpcMemory, diálogos com retratos, dias, EndingResolver e matéria, intro, ataque, pistas, sala secreta. |
| S3 | 05/11 a 11/11 | Som, efeitos visuais, UI/menus, balanceamento, testes de dead end, página do itch.io, README, GDD final. |

**Entrega final: 11/11/2026 às 23:50** (itch.io + GitHub + Milanote + Blackboard). Atraso é punido. Se faltar tempo, cortar itens de S3 — nunca o checkpoint nem o EndingResolver.
