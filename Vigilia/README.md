# VIGÍLIA

Jogo 2D de suspense, visto de cima, em Unity (URP 2D, Light 2D), com build WebGL para o itch.io.
Projeto 2D da disciplina Jogos Digitais, Insper 2026.1.
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
- **Arte > Processar ArteBruta**: pega as imagens geradas por IA em `ArteBruta/` (fundo magenta), recorta, reduz para pixel art, aplica a paleta do jogo e salva em `Resources/Arte`. A arte em `Resources/Arte` tem prioridade sobre a provisória. Prompts e nomes: `../docs/prompts-arte.md`.
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

A documentação de design está em `../docs/` (GDD convertido do PDF) e o mapa do que foi implementado em `../docs/implementacao.md`.

## Testes

Window → General → Test Runner. **EditMode**: regras (EndingResolver em todas as combinações, banheira, snapshot).
**PlayMode**: joga o fim de semana inteiro sozinho e falha em qualquer exceção.

## Créditos

A disciplina exige crédito para todo asset e todo trecho de código de terceiros com mais de 2 ou 3 linhas.

| O quê | Origem | Licença / observação |
|---|---|---|
| Cena do monstro (`Scripts/Encontro/EncontroMonstro.cs`) | Port para C# do design handoff "Asylum Monster Room" (`Refs/Horror asylum monster encounter.zip`), protótipo HTML com o renderizador procedural, paleta e parâmetros | Material de referência do grupo. **TODO: confirmar autoria e permissão de uso** |
| Gerador mulberry32 e hash de ruído (dentro do mesmo arquivo) | Mesmo protótipo acima | idem |
| Modelos de corpo inteiro (`ArteBruta/_corpo_inteiro/`) e o cobertor | Gerados com PixelLab (pixellab.ai); os modelos servem de referência | **TODO: conferir os termos do plano usado** |
| Sprites vistos de cima, pisos das salas e parede (`topo_*`, `piso_*`, `parede`) | Gerados por código (`Ferramentas/gerar_arte_topo.py`) a partir das cores e roupas dos modelos acima | do grupo |
| Retratos, menu, logo, quadros da intro, piso e parede (`ArteBruta/`) | Gerados com IA de imagem a partir de `docs/prompts-arte.md` | **TODO: nome da ferramenta e termos de uso** |
| Móveis, objetos de cenário e itens (`cama`, `caldeira`, `maca`, `item_*`...) | Gerados por código (`Ferramentas/gerar_props.py`) | do grupo |
| Música (`Resources/Audio/musica_*.wav`) | Gerada por código (`Ferramentas/gerar_musica.py`), sem samples de terceiros | do grupo |
| Arte provisória (`Resources/Gerado`) | Gerada por código (`Editor/GeradorDeArte.cs`) | do grupo |
| Sons provisórios | Gerados por código (`Scripts/Audio/SomProcedural.cs`) | do grupo |
| Fonte da interface | LegacyRuntime (embutida no Unity) | Unity |
| Template do projeto | Unity 2D (URP) template 7.0.0 | Unity |
| Código do jogo | Escrito com auxílio do Claude Code (Anthropic) a partir do GDD do grupo | — |

Grupo: Vinicius Dib, Greta Faria, Enzo Christofoletti e Thomas Rudge.

TODO: assets finais (arte, sons, trilha) com as respectivas licenças.
