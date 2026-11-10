<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 7. Luz, bateria e ataque

### 7.1 Congelar inimigos: lógica, não pixels

A luz que o jogador vê (Light 2D da URP) é só a aparência. Quem decide se um inimigo para é a **lógica**, em um script próprio (LightFreezeDetector). Analogia: a luz é a pintura e a lógica é a planta da casa; o jogo decide pela planta. Um inimigo está **iluminado** quando as quatro condições são verdadeiras ao mesmo tempo:

- 1. distância entre Elias e o inimigo ≤ alcance da lanterna;

- 2. ângulo entre a direção da lanterna e a direção do inimigo ≤ meia abertura do cone;

- 3. um raycast de Elias até o inimigo não encontra parede;

- 4. a lanterna está ligada e bateria > 0.

```text
 Teste de "está iluminado?" (lógica, não pixels)

                       (B) fora do ângulo → continua andando
                    /
                  /   meia abertura
   Elias ●  ──►  <     (A)      ┃parede      (D) além do alcance → continua andando
                  \            ┃  ·····(C) parede no raycast → continua andando
                    \
   |←────────── alcance (distância máxima) ──────────→|

   A: passa nos 4 testes → CONGELA
```

Figura 4 (descrição): Elias aponta a lanterna; o cone tem uma meia abertura e um alcance.
- **A** está dentro do cone, dentro do alcance e sem parede no caminho → **congela**.
- **B** está fora do ângulo → continua andando.
- **C** está dentro do cone, mas o raycast bate numa parede → continua andando.
- **D** está além do alcance → continua andando.

_Figura 4 · Teste de iluminação: só o inimigo A passa nos quatro testes e congela._

### 7.2 Bateria

A lanterna tem bateria **DECIDIDO**. Ela cai enquanto a lanterna está ligada. Com 0, a luz apaga e os inimigos não param mais. Pilhas encontradas no mapa recarregam. Regra anti dead end **PROPOSTA**: sempre existe ao menos uma pilha alcançável em cada noite. Valores (consumo por segundo, recarga por pilha, quantas pilhas por noite) são **TESTE** e estão em BalanceConfig. Na v0.2 o consumo subiu para 0.7 por segundo (a bateria cheia dura cerca de 2 min 20 s de luz contínua), para a luz ser um recurso a administrar.

### 7.3 Ataque

Elias pode atacar com itens encontrados no mapa **DECIDIDO** (decisão D-21), e matar tem consequência permanente: `matouAlguem`. **Como** o ataque funciona está **ABERTO** (Q-04). Implementado (v0.2): o **sedativo** (tecla Q) atordoa um possuído congelado na luz, bem perto; sobre um possuído atordoado, a **chave inglesa** (tecla R, duas vezes, com confirmação) dá o golpe letal. Um de cada por noite, para a luz continuar sendo a defesa principal e o ataque ser uma escolha moral (a base dos finais B e C).
