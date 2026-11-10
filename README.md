# VIGÍLIA

> **Quem não dorme é possuído.**
> Elias Vaughn, jornalista, passa três dias e três noites num sanatório isolado no Alasca. À noite, os possuídos andam até ele e só param sob a luz da lanterna.

Jogo 2D de suspense, visto de cima, feito em Unity (URP 2D, Light 2D) para o Projeto 2D da disciplina **Jogos Digitais · Insper 2026.2**. Disponível em **português e inglês**.

**Grupo:** Vinicius Dib · Greta Faria · Enzo Christofoletti · Thomas Rudge

| | |
|---|---|
| Jogar no navegador | itch.io: **TODO: link da página do jogo** (jam: [Projeto Jogo 2D Insper 2026.2](https://itch.io/jam/projeto-jogo-2d-insper-20262)) |
| GDD | [`docs/GDD_completo.md`](docs/GDD_completo.md) (v0.2, o jogo entregue) · Milanote: **TODO: link** |
| Créditos e licenças | [`Vigilia/README.md#créditos`](Vigilia/README.md#créditos) |

## Como jogar

| Tecla | Ação |
|---|---|
| WASD / setas | andar |
| Mouse | mirar a lanterna |
| F | ligar e desligar a lanterna |
| E | interagir: pegar, conversar, esconder-se, escrever no caderno, dormir (segure quando a tela pedir) |
| G | arrombar a porta com o pé de cabra |
| Q | sedativo num possuído parado na luz |
| R (duas vezes) | golpe letal num possuído atordoado (tem consequências) |
| Esc | pausa (volume, voltar ao menu) |

**De dia**, converse com a equipe e os internos, explore e junte evidências. Deite na cama para esperar a noite.
**À noite**, a possessão sobe sem parar: mais rápido em salas frias, mais devagar perto do calor. Mantenha os possuídos na luz (só o que a luz toca fica parado), cuide da bateria, esconda-se e chegue à cama antes de ser tomado. Não há barra: o mundo avisa, com a tela escurecendo e as olheiras de Elias crescendo.

- **Noite 1:** o quarto está gelado. Ache um cobertor.
- **Noite 2:** você fica trancado fora do quarto. Chave ou pé de cabra?
- **Noite 3:** não há sono. Uma porta se abre no subsolo...

Ser tomado nas noites 1 e 2 recomeça a noite do início. Na noite 3, não há segunda chance.
Há **três finais** (e variações): a matéria que Elias publica depende do que ele anotou, de quantas vezes foi tocado e de quem ele feriu.

## Estrutura do repositório

| Pasta | O que tem |
|---|---|
| [`Vigilia/`](Vigilia/) | projeto Unity (editor 6000.6.6f1). Como abrir, testes e créditos completos no [README do projeto](Vigilia/README.md) |
| [`docs/`](docs/) | GDD em Markdown (uma seção por arquivo), mapa GDD → código e mudanças de design |
| [`Vigilia/Ferramentas/`](Vigilia/Ferramentas/) | scripts Python que geram a arte, os móveis e a música do jogo |

## Créditos (resumo)

Música, sons, cenário e sprites vistos de cima foram gerados por scripts do grupo. Modelos de personagens feitos com PixelLab; retratos, menu e intro feitos com IA de imagem; cena do monstro baseada num protótipo do próprio grupo. Código escrito pelo grupo com auxílio do Claude Code. Detalhes e licenças: [`Vigilia/README.md#créditos`](Vigilia/README.md#créditos).
