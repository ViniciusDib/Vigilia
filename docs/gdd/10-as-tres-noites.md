<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 10. As três noites

A dificuldade sobe de uma noite para a outra por **alavancas concretas** (número de inimigos, velocidade, esconderijos, pilhas, salas quentes). Os valores das alavancas são **TESTE** e vivem em BalanceConfig.

|**Aspecto**|**Noite 1**|**Noite 2**|**Noite 3**|
|---|---|---|---|
|Dificuldade|baixa; ensina as regras|média|alta|
|Situação inicial|Quarto frio, sem cobertor. Elias não consegue dormir de frio e ouve um barulho (**DECIDIDO**)|Elias tenta dormir, ouve um barulho grande, sai atrás dele e fica trancado fora do quarto (**DECIDIDO**)|Elias não consegue dormir. Do subsolo vem o rangido de uma porta se abrindo (v0.2)|
|Objetivo|Achar um cobertor, conhecer o elenco no caminho e voltar ao quarto|Entrar no quarto: chave ou pé de cabra. Os dois caminhos existem sempre (anti dead end)|Ir até a sala secreta, encontrar o monstro e **fugir pela porta principal** (fim do corredor leste) antes de transbordar (v0.2)|
|Alívio da banheira|Quarto quente com o cobertor|Sala com aquecedor (caldeira) no meio do caminho|Caldeira no caminho; não há cama (v0.2, resolve Q-17)|
|O que ensina|Lanterna e congelar, esconder, bateria, escrever|Calor das salas e o dilema “vale arrombar?” (Q-06)|Tudo junto, sob pressão: a fuga|
|Pistas do final C|Pista 1: Marta chama Elias pelo nome|Pista 2: nota na letra de Elias|Pista 3 (pulseira, P) e revelação|
|Inimigos **TESTE**|2|6 (v0.2)|8, mais 4 na fuga (v0.2)|
|Velocidade **TESTE**|× 0.8|× 1.0|× 1.2|
|Esconderijos **TESTE**|5|4|3|
|Pilhas **TESTE**|4|3|2|
|Salas quentes **TESTE**|2|2|1|
|Se transbordar|restaura checkpoint e repete|restaura checkpoint e repete|não repete: Final C|

### 10.1 A fuga da noite 3 (v0.2)

Na noite 3 a cama não serve: Elias não consegue dormir. A porta atrás do arquivo do subsolo se abre sozinha. Ao entrar na sala secreta, toca a cena fixa do monstro; depois dela começa a **fuga**:

- a luz do prédio vira um alarme vermelho pulsando;
- a porta principal, no fim do corredor leste, se destranca;
- surgem 4 possuídos extras, sempre a pelo menos 12 unidades de Elias (dá tempo de reagir);
- todos ficam 1,1× mais rápidos, enxergam 1,5× mais longe e a banheira enche 1,2× mais rápido.

Sair pela porta principal leva ao final A ou B (conforme `matouAlguem`). Transbordar durante a fuga leva ao C. Os valores da fuga são **TESTE** e ficam em BalanceConfig (seção “Noite 3”); foram reduzidos duas vezes depois de playtests em que o final ficou difícil demais.

### 10.2 Pontos delicados

- **Dilema “vale arrombar?” (Q-06).** Para o dilema ter peso, os dois caminhos precisam ter custo. Chave: tempo (a possessão sobe) e encontros no caminho. Pé de cabra: rápido, mas faz barulho (+10 e atrai inimigos) e marca arrombouPorta. Consequências além do barulho estão em aberto.

- **Noite 3, variação “ficou acordado” (Q-15).** O sofrimento do personagem inclui a ideia de se matar. Sugestão: tratar de forma **sugerida**, sem mostrar o ato, e levar o tema ao professor (o enunciado pede discutir propostas que possam ofender).

- **Dias baratos, noites fortes.** Dia e noite podem ser o mesmo mapa com comportamento e iluminação diferentes, o que reduz muito o custo de arte.
