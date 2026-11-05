# Prompts de arte · VIGÍLIA

Pacote para gerar a arte final numa IA de imagem e colocar no jogo. O objetivo é chegar no nível de detalhe da cena do monstro (o jumpscare): pixel art escura, com muitos detalhes, na mesma paleta.

Os prompts estão em **inglês** porque as IAs de imagem respondem melhor assim. As explicações estão em português.

---

## 1. Como a arte entra no jogo

1. Gere a imagem com o prompt da seção 6.
2. Salve como PNG **com o nome exato** da tabela (por exemplo, `npc_marta.png`) na pasta `Vigilia/ArteBruta/`. No Unity, o menu **Vigilia > Arte > Abrir pasta ArteBruta** abre a pasta.
3. No Unity, rode **Vigilia > Arte > Processar ArteBruta (paleta do jogo)**. A ferramenta:
   - deixa o fundo magenta transparente;
   - reduz a imagem para o tamanho do jogo, com pixels nítidos;
   - aplica a paleta do jumpscare, para tudo combinar;
   - salva em `Assets/_Vigilia/Resources/Arte/`.
4. Rode **Vigilia > Construir tudo** para os sprites novos entrarem nas cenas.
5. Dê Play.

Se não gostar do resultado com a paleta, use **Processar ArteBruta (cores livres)**: ele só reduz e recorta.

A arte provisória continua valendo para tudo o que ainda não tiver versão final. Dá para trocar uma imagem por vez.

---

## 2. Qual IA usar

| Para quê | Sugestão | Por quê |
|---|---|---|
| Personagens vistos de cima (sprites do jogo) | **PixelLab** (pixellab.ai) ou **Retro Diffusion** | Feitas para pixel art; o PixelLab gera personagem visto de cima e em várias direções |
| Retratos, tela do menu, quadros da intro | **ChatGPT (geração de imagem)**, **Midjourney** ou **Leonardo** | Melhores em rosto, luz e composição. A ferramenta do Unity transforma em pixel art depois |
| Móveis, piso e itens | Qualquer uma das duas | Peças pequenas; o pixelizador resolve |

Dicas que valem para todas:
- **Fundo magenta liso (#FF00FF)** em tudo que precisa de transparência (personagens, itens, móveis). A ferramenta recorta essa cor.
- **Consistência:** gere primeiro o retrato de dia de cada personagem e use essa imagem como **referência** para gerar o retrato da noite e o sprite visto de cima. No Midjourney use `--cref`. No ChatGPT anexe a imagem e diga "same character as the attached image".
- Gere 3 ou 4 variações e escolha a melhor. Se a IA permitir, anote a seed.
- Gere **maior** que o tamanho final (por exemplo, 1024×1024). A redução é feita no Unity.

---

## 3. Estilo base (cole no fim de TODO prompt)

```
STYLE: highly detailed dark pixel art, 1990s survival horror PC game, grim and realistic proportions,
harsh flashlight lighting with deep black shadows, limited palette of blood reds, rust browns, bone,
grime green and cold teal-blue shadows, crisp hard pixels, no anti-aliasing, no blur, no text, no watermark
```

Prompt negativo, para IAs que aceitam:

```
cartoon, chibi, cute, anime, smooth vector, 3d render, glossy, bright colors, text, letters, watermark, signature, frame, blurry
```

Paleta do jogo, para IAs que aceitam paleta ou imagem de referência de cor:
`050304 0c0607 150909 1f0c0c 2a0a0b 3d0d0e 521012 6b1214 851417 a3191b c02a23 d9473a e0705f f09a86 2b1a10 3f2616 573520 70472a 8c5c38 a87748 c9b27a e3d29a f4ead0 262626 3a3836 55524d 7a756b 2f3328 4a5040 fff6e0` + frios `0b1418 142228 1f3540 2e4c5a 46697a 6b8fa0 9db8c4 d6e4ea` + pele `7a5444 a87a62 d2a68a e8c9b0`.

Referência de clima: as fotos em `Refs/` (corredor de azulejo verde-azulado, prédio nevado com janelas amarelas) e a cena do monstro.

---

## 4. Bíblia dos personagens (descrição fixa)

Use sempre a mesma descrição de cada personagem, palavra por palavra, para ele sair igual em todas as imagens. **Todos têm olheiras fundas**: é o tema do jogo (GDD 5.1).

| id | Quem | Descrição fixa (cole no prompt) |
|---|---|---|
| `elias` | Elias Vaughn, jornalista, o jogador | `Elias Vaughn, a gaunt 35-year-old investigative journalist, messy dark brown hair, three-day stubble, deep dark circles under tired eyes, worn brown wool overcoat, grey knitted scarf, holding a small flashlight` |
| `marta` | Marta Quill, enfermeira-chefe | `Marta Quill, a 50-year-old head nurse, greying auburn hair in a tight bun, vintage white nurse uniform and white nurse cap, warm but unsettling smile, heavy dark circles under her eyes` |
| `otis` | Otis Brandt, zelador | `Otis Brandt, a 60-year-old burly janitor, short grey hair, weathered face, navy blue work overalls, ring of old keys on his belt, dark circles under his eyes` |
| `almeida` | Dr. Henrik Almeida, médico | `Dr. Henrik Almeida, a 45-year-old thin physician, slicked-back black hair, round wire glasses, white doctor's coat over a grey vest, evasive nervous expression, dark circles under his eyes` |
| `wren` | Sr. Wren, paciente idoso | `Mr. Wren, a frail 80-year-old patient, bald with wispy white tufts, pale thin face, faded light blue hospital gown, distant unfocused stare, very deep dark circles under his eyes` |
| `holt` | Dr. Alaric Holt, diretor | `Dr. Alaric Holt, a 65-year-old sanatorium director, neatly combed silver hair, round tortoiseshell spectacles, dark charcoal three-piece suit, calm paternal smile that does not reach his eyes, dark circles under his eyes` |
| `paciente1` | paciente figurante | `an elderly male patient, shaved head, faded light blue hospital gown, hunched posture, dark circles under his eyes` |
| `paciente2` | paciente figurante | `a middle-aged female patient, long tangled grey hair, faded light blue hospital gown, barefoot, dark circles under her eyes` |

**Versão possuída (noite):** acrescente à descrição

```
now possessed at night: grey-pale waxy skin, sunken black eye sockets with faint red glowing irises, dark veins on the neck and temples, head slightly tilted, expression emptied, same clothes now dirty
```

---

## 5. Lista de arquivos

| Arquivo (nome exato) | Gerar em | Vira no jogo | Uso |
|---|---|---|---|
| `jogador_elias.png` | 1024×1024 | 96×96 | Elias visto de cima |
| `npc_marta.png`, `npc_otis.png`, `npc_almeida.png`, `npc_wren.png`, `npc_holt.png`, `npc_paciente1.png`, `npc_paciente2.png` | 1024×1024 | 96×96 | NPC de dia, visto de cima |
| `npc_<id>_noite.png` (os mesmos, menos `holt`) | 1024×1024 | 96×96 | NPC possuído, visto de cima (opcional) |
| `retrato_<id>_dia.png` (os 8 ids) | 1024×1024 | 160×160 | Retrato do diálogo e do canto da tela |
| `retrato_<id>_noite.png` (os 8 ids) | 1024×1024 | 160×160 | O de Elias aparece por cima do normal conforme a possessão sobe |
| `menu_fundo.png` | 1920×1080 | 640×360 | Tela principal |
| `logo.png` | 1600×600 | 480 de largura | Título "VIGÍLIA" (opcional) |
| `intro_1.png` … `intro_5.png` | 1920×1080 | 640×360 | Quadros da intro (opcional) |
| `cama.png`, `mesa.png`, `armario.png`, `caldeira.png` | 1024×1024 | 128 de largura | Móveis vistos de cima |
| `piso.png`, `parede.png` | 512×512 | 32×32 | Ladrilhos que repetem |
| `item_pilha.png`, `item_cobertor.png`, `item_chave.png`, `item_pedecabra.png`, `item_papel.png`, `item_pulseira.png`, `item_sedativo.png`, `item_pesado.png` | 512×512 | 48×48 | Itens no chão |

Regra para tudo que é visto de cima: o personagem **olha para o topo da imagem**. O jogo gira o sprite para a direção em que ele anda.

---

## 6. Prompts

Em cada prompt, troque `[DESCRIÇÃO]` pela descrição fixa da seção 4 e termine com o bloco STYLE da seção 3.

### 6.1 Personagens no jogo (vistos de cima)

```
Game sprite of [DESCRIÇÃO], seen strictly from directly above (top-down orthographic view, like Darkwood
or Hotline Miami), we see the top of the head, the shoulders and arms foreshortened below, the character
is facing toward the TOP edge of the image, standing still, centered, occupying about 70% of the frame,
single character only, flat solid magenta #FF00FF background, no ground, no cast shadow on the background.
[STYLE]
```

Para Elias, acrescente: `holding a flashlight pointed forward toward the top of the image`.

Para a versão possuída (`npc_<id>_noite.png`): use o mesmo prompt com a descrição + o bloco de possessão, e anexe o sprite de dia como referência.

> No **PixelLab**, use "Create character" com vista **top-down**, tamanho 64 ou 96, e cole só a descrição. Depois dá para pedir as outras direções e animação de caminhada. Se for por esse caminho, me avise que eu adapto o código para animação.

**Personagens de corpo inteiro (o que o PixelLab gerou).** O jogo aceita os dois estilos:
- **Vista de cima de verdade** (só cabeça e ombros): o sprite gira para onde o personagem anda.
- **Corpo inteiro visto do alto** (estilo Zelda, como os seus): o sprite não gira. O jogo escolhe a imagem pela direção, com estes nomes:

| Arquivo | Olhando para |
|---|---|
| `npc_marta.png` | cima (de costas), obrigatório |
| `npc_marta_sul.png` | baixo (de frente) |
| `npc_marta_leste.png` | direita (se faltar, espelha o oeste) |
| `npc_marta_oeste.png` | esquerda (se faltar, espelha o leste) |
| `npc_marta_noite.png`, `npc_marta_noite_sul.png`, … | o mesmo, possuída |

Vale igual para `jogador_elias`. Direção que falta usa a de costas. No PixelLab, gere as rotações do mesmo personagem (south, east, west) e salve com esses sufixos.

### 6.2 Retratos de diálogo (dia)

```
Dialogue portrait for a horror RPG text box, bust shot (head and shoulders), three-quarter view facing
slightly to the right, [DESCRIÇÃO], eyes looking at the viewer, very pronounced dark circles under the eyes,
dim institutional lighting from a single overhead bulb, plain dark muted background in [COR DE FUNDO],
portrait fills the square frame, Pokemon-style dialogue portrait composition.
[STYLE]
```

Cores de fundo, para cada um ter identidade: elias `dark slate blue`, marta `dusty teal`, otis `dark brown`, almeida `dark green-grey`, wren `faded violet-grey`, holt `deep oxblood red`, pacientes `cold grey`.

### 6.3 Retratos possuídos (noite)

Anexe o retrato de dia como referência:

```
Same character and same framing as the reference image, now at night and possessed:
[DESCRIÇÃO] + now possessed at night: grey-pale waxy skin, sunken black eye sockets with faint red glowing
irises, dark veins on the neck and temples, head slightly tilted, expression emptied.
Lit only from below by a weak flashlight, background almost black.
[STYLE]
```

O retrato possuído de **Elias** é especial: ele aparece por cima do normal conforme a possessão sobe. Peça que seja **o mesmo enquadramento exato**, só com as olheiras muito mais escuras, a pele pálida e os olhos vermelhos.

### 6.4 Tela principal (`menu_fundo.png`)

```
Wide establishing shot, 16:9, of the St. Brenna Sanatorium at night in deep Alaskan winter: a huge old
five-story sanatorium building with a snow-covered roof and rows of dark windows, only a few windows lit
with warm yellow light, one window with a faint human silhouette, snowy pine trees and dark mountains
behind, heavy snowfall, a lone tiny figure with a flashlight walking through the snow toward the entrance,
cold teal and black night, the LEFT THIRD of the image is darker and emptier (space for the game menu).
[STYLE]
```

Referência visual: `Refs/Ref_sanatorio_por_fora.jpg`.

### 6.5 Logo (opcional, `logo.png`)

IAs costumam errar letras. Se sair "VIGÍLIA" certinho, use. Se não, o jogo continua usando o título em texto.

```
Game title logo text "VIGÍLIA" in tall condensed serif capital letters, cracked and frost-covered bone-white
letters with a thin dark red drip under the I, centered, flat solid magenta #FF00FF background, no other text.
[STYLE]
```

### 6.6 Quadros da intro (opcionais, `intro_1` a `intro_5`)

O texto de cada quadro já aparece no jogo (aqui só a imagem).

1. `A lonely snowy mountain road at dusk in Alaska, a car's headlights, a distant huge sanatorium on a hill, road sign half-buried in snow.`
2. `A cluttered newspaper desk at night lit by a desk lamp, a pile of handwritten letters, one letter in focus with shaky handwriting, a coffee cup, cigarette smoke.`
3. `Elias Vaughn (journalist in brown overcoat and grey scarf, dark circles) standing in front of the sanatorium's big wooden doors in falling snow, seen from behind, small against the building.`
4. `Close-up of an old unsigned letter on a dark table, the handwriting partly visible, a single red drop on the paper, the rest in shadow.`
5. `A dark sanatorium corridor with green-blue tiles, a flashlight beam cutting through the darkness, far at the end of the beam a pale figure in a nurse uniform standing still.`

Adicione a cada um: `cinematic 16:9 illustration` + STYLE.

### 6.7 Móveis vistos de cima

```
Top-down orthographic view (seen from directly above) of [OBJETO], game prop, centered, flat solid magenta
#FF00FF background, no cast shadow on the background.
[STYLE]
```

- `cama`: `an old iron hospital bed with a thin stained mattress, white pillow at the TOP end, grey wool blanket`
- `mesa`: `an old wooden writing desk with an open notebook, a pen and a small oil lamp`
- `armario`: `a tall dented metal locker cabinet with two doors, rust stains`
- `caldeira`: `a round old cast-iron boiler with pipes and a small glowing furnace window`

### 6.8 Piso e parede (ladrilhos que repetem)

```
Seamless tileable texture, top-down, [MATERIAL], fills the whole square, no border, must repeat without
visible seams. [STYLE]
```

- `piso`: `worn institutional floor tiles, small square cream and grey-green tiles, grime in the joints, a few cracks`. Deixe **claro**: o jogo pinta por cima conforme a temperatura da sala.
- `parede`: `dark stained concrete wall top seen from above, cold grey-blue, cracks`.

### 6.9 Itens no chão

```
Single game item icon, top-down view, [ITEM], centered, slightly glowing pale outline so it reads in the dark,
flat solid magenta #FF00FF background. [STYLE]
```

`pilha`: `a C-size battery` · `cobertor`: `a folded thick red wool blanket` · `chave`: `an old brass room key with a paper tag` · `pedecabra`: `a red iron crowbar` · `papel`: `a sheet of old yellowed paper with handwriting` · `pulseira`: `a yellowed plastic hospital wristband` · `sedativo`: `a small glass vial of sedative with a syringe` · `pesado`: `a heavy steel wrench`

---

## 7. Situação em 24/10/2026

> **Decisão do grupo (24/10):** no jogo os personagens são **vistos de cima** e giram com a mira. Os modelos de corpo inteiro do PixelLab ficaram guardados em `ArteBruta/_corpo_inteiro/` como referência. Os sprites do jogo (`topo_<id>.png` e `topo_<id>_noite.png`), os pisos de cada sala (`piso_<sala>.png`) e a parede são gerados por `Vigilia/Ferramentas/gerar_arte_topo.py`, com as roupas e cores desses modelos. Para mudar uma cor ou roupa, edite a tabela `PERSONAGENS` no script, rode `python Ferramentas/gerar_arte_topo.py` e depois **Processar e construir tudo**. Se gerar sprites vistos de cima numa IA, salve como `topo_<id>.png` (olhando para o topo da imagem): eles substituem os do script.

Ordem de preferência no jogo: `topo_<id>` (vista de cima, gira) → `npc_<id>` (corpo inteiro por direção) → arte provisória.

**Já no jogo (36 imagens):** menu, logo, 5 quadros da intro, os 16 retratos, piso, parede, cobertor, Elias e os NPCs de costas (`almeida`, `holt`, `marta`, `otis`, `wren`) e as versões possuídas de `almeida`, `wren`, `paciente1` e `paciente2`.

**Ainda faltam (o jogo usa a arte provisória ou uma alternativa):**
- `npc_paciente1.png`, `npc_paciente2.png` (dia): por enquanto o dia usa a versão possuída.
- `npc_marta_noite.png`, `npc_otis_noite.png`: por enquanto escurece o sprite de dia.
- Direções `_sul`, `_leste` e `_oeste` de todos os personagens: hoje todos aparecem de costas, espelhados conforme o lado.
- Itens: `item_pilha`, `item_chave`, `item_pedecabra`, `item_papel`, `item_pulseira`, `item_sedativo`, `item_pesado`.
- Móveis: `cama`, `mesa`, `armario`, `caldeira`.

## 8. Checklist

- [ ] Nome do arquivo exatamente igual à tabela da seção 5 (minúsculas, sem acento).
- [ ] Fundo magenta liso nos sprites, itens e móveis.
- [ ] Personagem visto de cima olhando para o topo da imagem.
- [ ] Rodou **Processar ArteBruta** e depois **Construir tudo**.
- [ ] **Créditos:** anote no `Vigilia/README.md` a ferramenta usada para cada asset e confira os termos de uso dela (algumas exigem plano pago para uso comercial ou público).
- [ ] Mantenha os PNGs originais da IA guardados (em `ArteBruta/`) para reprocessar se a paleta mudar.
