# VIGÍLIA

Jogo 2D de suspense, visto de cima, em Unity (URP 2D, Light 2D), com build WebGL para o itch.io.
Projeto 2D da disciplina Jogos Digitais, Insper 2026.2.
Feito por **Vinicius Dib, Greta Faria, Enzo Christofoletti e Thomas Rudge**.
Disponível em português e inglês (botão "Idioma / Language" no menu).

> Quem não dorme é possuído. Elias Vaughn, jornalista, passa três dias e três noites num sanatório no Alasca.
> À noite, os possuídos andam até ele e só param sob a luz da lanterna. Ele precisa dormir antes de transbordar.

## Como abrir

1. Unity Hub → **Add project from disk** → esta pasta (`Vigilia`). Versão do editor: **6000.6.6f1**.
2. Abra a cena `Assets/_Vigilia/Cenas/Menu.unity` e aperte Play.

## Controles

| Tecla | Ação |
|---|---|
| WASD / setas | andar |
| Mouse | mirar a lanterna |
| F | ligar e desligar a lanterna |
| E | interagir: pegar, conversar, esconder, escrever, dormir (segure quando a tela pedir) |
| G | arrombar a porta com o pé de cabra |
| Q | sedativo num possuído parado na luz |
| R (duas vezes) | golpe letal num possuído atordoado |
| Esc | pausa |
| F1 | painel de debug (estados, possessão, flags) |

## Menu Vigilia (no editor)

- **Construir tudo (arte + config + cenas)**: gera a arte provisória, o `BalanceConfig` (se não existir) e **recria** as cenas Menu e Jogo a partir do mapa em `Editor/ConstrutorDeCenas.cs`. Atenção: apaga as edições feitas à mão na cena Jogo.
- **Gerar arte provisória**: só os PNGs em `Resources/Gerado`. Para usar arte final, troque o PNG mantendo o nome.
- **Arte > Processar ArteBruta**: pega as imagens geradas por IA em `ArteBruta/` (fundo magenta), recorta, reduz para pixel art, aplica a paleta do jogo e salva em `Resources/Arte`. A arte em `Resources/Arte` tem prioridade sobre a provisória. As imagens brutas (`ArteBruta/`, ~67 MB) não estão no repositório; o resultado processado está em `Resources/Arte`.
- **Build WebGL**: gera `Builds/WebGL`. Para o itch.io, zipe o conteúdo dessa pasta (com o `index.html` na raiz do zip).

## Estrutura

```
Assets/_Vigilia/
  Config/BalanceConfig.asset   todos os números do jogo (GDD 14.2) – calibrar aqui
  Cenas/                       Menu e Jogo
  Scripts/Core/                WorldState, NpcMemory, SaveSnapshot, PossessionMeter, NightManager, EndingResolver...
  Scripts/Mundo/               salas, temperatura, portas, itens, esconderijos, cama, caderno, caldeira, navegação
  Scripts/Jogador/             PlayerController (estados J-xx), lanterna, detector de luz
  Scripts/Inimigos/            EnemyAI (estados E-xx), conversa
  Scripts/Dialogo/             diálogos e falas (rascunho)
  Scripts/UI/                  HUD, telas, menu, câmera
  Scripts/Audio/               sons gerados por código e camadas que crescem com a possessão
  Scripts/Encontro/            cena fixa do monstro (jumpscare)
  Editor/                      construtor de cenas, gerador de arte, build WebGL
  Tests/                       testes EditMode (regras) e PlayMode (fim de semana inteiro)
```

O GDD (v0.2, o jogo entregue) está em `../docs/`; o que mudou durante o desenvolvimento está na tabela "O que mudou" de `../docs/gdd/00-capa-e-status.md`.

## Testes

Window → General → Test Runner. **EditMode**: regras (EndingResolver em todas as combinações, banheira, snapshot).
**PlayMode**: joga o fim de semana inteiro sozinho e falha em qualquer exceção.

## Créditos

A disciplina exige crédito para todo asset e todo trecho de código de terceiros com mais de 2 ou 3 linhas.

| O quê | Origem | Licença / observação |
|---|---|---|
| Cena do monstro (`Scripts/Encontro/EncontroMonstro.cs`) | Port para C# do design handoff "Asylum Monster Room": protótipo HTML com o renderizador procedural, paleta e parâmetros, feito por Vinicius Dib (grupo) no Claude Design | do grupo |
| Gerador mulberry32 e hash de ruído (dentro do mesmo arquivo) | Algoritmos de domínio público, vindos do mesmo protótipo acima | do grupo |
| Modelos de corpo inteiro dos personagens e o cobertor | Gerados com PixelLab (pixellab.ai); os modelos servem de referência | Pelos [termos do PixelLab](https://www.pixellab.ai/termsofservice), quem gera é dono das imagens e pode usá-las em qualquer projeto, inclusive comercial, sem exigência de crédito (só é proibido treinar outros modelos com elas). Creditamos mesmo assim. |
| Sprites vistos de cima, pisos das salas e parede (`topo_*`, `piso_*`, `parede`) | Gerados por código (`Ferramentas/gerar_arte_topo.py`) a partir das cores e roupas dos modelos acima | do grupo |
| Retratos, menu, logo, quadros da intro, piso e parede (`Resources/Arte`) | Gerados com IA de imagem a partir de prompts escritos pelo grupo e processados pelo `PixelizadorDeArte` | **TODO: nome da ferramenta e termos de uso** |
| Móveis, objetos de cenário e itens (`cama`, `caldeira`, `maca`, `item_*`...) | Gerados por código (`Ferramentas/gerar_props.py`) | do grupo |
| Música (`Resources/Audio/musica_*.wav`) | Gerada por código (`Ferramentas/gerar_musica.py`), sem samples de terceiros | do grupo |
| Arte provisória (`Resources/Gerado`) | Gerada por código (`Editor/GeradorDeArte.cs`) | do grupo |
| Sons provisórios | Gerados por código (`Scripts/Audio/SomProcedural.cs`) | do grupo |
| Fonte da interface | LegacyRuntime (embutida no Unity) | Unity |
| Template do projeto | Unity 2D (URP) template 7.0.0 | Unity |
| Código do jogo | Escrito pelo grupo com auxílio do Claude Code (Anthropic), a partir do GDD do grupo. Nenhum trecho copiado de terceiros ou de colegas | — |

Grupo: Vinicius Dib, Greta Faria, Enzo Christofoletti e Thomas Rudge.

Nenhum asset de loja ou de terceiros foi usado: toda a arte, música e som vêm das ferramentas acima ou de scripts do grupo.
