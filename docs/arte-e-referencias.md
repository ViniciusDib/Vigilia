# Arte e referências (`Refs/`)

Material de inspiração que o grupo juntou. **Referências não são assets do jogo**: nada daqui entra no build sem autorização e crédito no README.

## O que o GDD já decidiu sobre o visual

- Estética "Outlast em 2D": tela escura, só se vê o que a lanterna toca (D-05, seção 5.1).
- Alasca no inverno: neve, vidro congelado, vapor da respiração, aquecimento precário.
- Salas quentes com luz alaranjada (rubrica 3). Light 2D da URP, 1 lanterna e poucas luzes fixas (13.4).
- Retratos de diálogo estilo Pokémon, com olheiras fortes; 2 variantes por personagem (dia lúcido / noite possuída), cerca de 10 retratos (5.3).
- Feedback da possessão sem barra: vinheta, olheira no retrato de Elias, batimento e tremor da lanterna (P-10).
- Em aberto: estilo dos retratos 3D contra o mundo 2D, paleta e filtro comuns (Q-11).

## Imagens

| Arquivo | O que mostra | Útil para |
|---|---|---|
| `Refs/Ref_sanatorio_por_fora.jpg` | Prédio grande de vários andares, fachada clara, telhado com neve, montanhas e pinheiros nevados, poucas janelas acesas em amarelo, tom geral azul-esverdeado frio. | Intro, menu, tela de título; paleta do exterior (azul frio + janelas quentes). |
| `Refs/Ref_ambiente_noite.jpg` | Corredor pintado, parede azul-acinzentada, manchas de sangue, porta dupla de madeira ao fundo, papéis no chão molhado, uma luz de teto fria com faíscas. | Corredores da noite; contraste de luz fria e pontual; chão refletindo. |
| `Refs/Mais_ref_noite.jpg` | Corredor de hospital antigo, azulejos verde-azulados até meia parede, câmera de segurança, macas viradas, arco ao fundo, luz amarelada fraca, enquadramento letterbox. | Ala de pacientes; props (macas, câmeras); paleta verde-azulado + amarelo sujo. |

## `Refs/Horror asylum monster encounter.zip`

Um *design handoff* de uma cena em pixel art (320×180, **primeira pessoa**): sala de manicômio com sangue, um monstro encapuzado com boca vertical cheia de dentes, iluminada só por uma lanterna que segue o mouse. Contém `Asylum Room.dc.html` (protótipo que roda no navegador), um `README.md` com a especificação completa e duas imagens de referência de humor.

O jogo é 2D (provavelmente top-down, ver N-07 em [notas-de-consistencia.md](notas-de-consistencia.md)), então a cena **não encaixa diretamente** no gameplay. O que dá para aproveitar:

- **Modelo de lanterna**: núcleo forte, brilho externo suave, anel quente na borda, cor `rgb × (1.55, 1.4, 1.2)` (tom quente), tremor de mão e falhas ocasionais (`flicker`). Bom guia para configurar a Light 2D e o "tremor da lanterna" do P-10.
- **Poeira no feixe**: partículas visíveis só dentro do cone (efeito pedido na S3).
- **Olhos que brilham no escuro** e reagem à luz: ideia para possuídos fora do cone.
- **Paleta de 30 cores** (pretos, vermelhos, carne, marrons, osso, cinzas, verde-sujo, luz `#fff6e0`): candidata para o Q-11, se o grupo optar por pixel art.
- Possível uso como **cena de encontro / jumpscare** ou tela do Final C. Isso seria escopo novo: decidir com o grupo.

Paleta do handoff (para comparar com as fotos):

```
Pretos     #050304 #0c0607 #150909 #1f0c0c
Vermelhos  #2a0a0b #3d0d0e #521012 #6b1214 #851417 #a3191b #c02a23 #d9473a
Carne      #e0705f #f09a86
Marrons    #2b1a10 #3f2616 #573520 #70472a #8c5c38 #a87748
Osso       #c9b27a #e3d29a #f4ead0
Cinzas     #262626 #3a3836 #55524d #7a756b
Verde sujo #2f3328 #4a5040
Luz        #fff6e0
```

Observação: a paleta do handoff é dominada por vermelho e marrom; as fotos de referência puxam para azul e verde frios. Vale o grupo escolher uma direção (Q-11).
