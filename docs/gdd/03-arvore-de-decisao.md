<!-- VIGÍLIA · GDD v0.2 (10/11/2026). Baseado no GDD técnico v0.1 (22/10/2026); mudanças na tabela de 00-capa-e-status.md. -->

## 3. Árvore de decisão (consulta rápida)

Seção para quando ninguém lembrar de uma decisão. Leia da esquerda para a direita: **ação do jogador → flag que ela liga → o que muda no final e na matéria**. Os nomes das flags estão no glossário (seção 4) e na tabela da seção 14.1.

### 3.1 O que cada ação do jogador liga

```mermaid
flowchart LR
  a1(["Esconder-se ou<br/>deixar-se tocar"]) --> f1["vezesTocado"] --> o1["bloco de texto: exposição<br/>(0 / 1-3 / 4+ toques)"]
  a2(["Escrever no caderno<br/>(no quarto)"]) --> f2["evidenciasAnotadas"] --> o2["bloco de texto: verdade<br/>(nenhuma / algumas / muitas)"]
  a3(["Dia 3 / noite 3:<br/>seguir a pista do diretor"]) --> f3["viuSalaSecreta"] --> o3["bloco de texto: 'sem provas'<br/>(se falso)"]
  a4(["Noite 2: abrir a porta<br/>com chave ou pé de cabra"]) --> f4["arrombouPorta"] --> o4["bloco de texto: porta arrombada"]
  a5(["Matar um possuído<br/>ou poupar"]) --> f5["matouAlguem"] --> o5["bloco de texto: violência"]
  a6(["Noite 3: fugir pela porta principal<br/>antes de transbordar"]) --> f6["transbordouNoite3"]
  f5 -- "A ou B; em C muda a variação" --> FIN{{"DEFINE O FINAL<br/>A (honesta), B (omite) ou C (o louco)"}}
  f6 -- "verdadeiro = C" --> FIN
  classDef acao fill:#d6e8f7,stroke:#3a7ab8,color:#000
  classDef flag fill:#eeeeee,stroke:#999,color:#000
  classDef saida fill:#d5f5e3,stroke:#2e8b57,color:#000
  classDef final fill:#fdf3cf,stroke:#c9a227,color:#000
  class a1,a2,a3,a4,a5,a6 acao
  class f1,f2,f3,f4,f5,f6 flag
  class o1,o2,o3,o4,o5 saida
  class FIN final
```

_Figura 1 · Ações do jogador (azul), flags (cinza) e saídas (verde e amarelo). Versão em texto na tabela 3.3._

### 3.2 Como o final é escolhido

```mermaid
flowchart TD
  S(["Fim da noite 3"]) --> Q1{"possessao chegou<br/>a 100 na noite 3?"}
  Q1 -- sim --> Q2{"matouAlguem?<br/>(transbordou)"}
  Q1 -- "não (chegou à cama)" --> Q3{"matouAlguem?<br/>(dormiu)"}
  Q2 -- sim --> C2["FINAL C · o louco<br/>variação 2: sangue na manga<br/>(será que fui eu?)"]
  Q2 -- não --> C1["FINAL C · o louco<br/>variação 1: sem sangue"]
  Q3 -- sim --> B["FINAL B · matéria que omite<br/>ele matou, mas esconde da polícia"]
  Q3 -- não --> A["FINAL A · matéria honesta<br/>'foi uma experiência ruim'"]
  M["Modificadores de texto (não mudam qual final é)<br/>• viuSalaSecreta = falso → 'sem provas'<br/>• arrombouPorta<br/>• evidenciasAnotadas: nenhuma / algumas / muitas<br/>• vezesTocado: 0 / 1-3 / 4+"]
  M -.-> C1
  M -.-> B
  M -.-> A
  classDef c fill:#f4b6b0,stroke:#b03a2e,color:#000
  classDef b fill:#fdf3cf,stroke:#c9a227,color:#000
  classDef a fill:#d5f5e3,stroke:#2e8b57,color:#000
  classDef m fill:#f3e8f7,stroke:#8e44ad,color:#000
  class C1,C2 c
  class B b
  class A a
  class M m
```

> No original, as setas tracejadas dos modificadores vão para todos os finais (C, B e A).

_Figura 2 · Árvore dos finais. O final é função apenas de transbordouNoite3 e matouAlguem; os modificadores só mudam blocos de texto._

### 3.3 Versão em texto: regra de resolução do final

|**Regra**|**transbordouNoite3**|**matouAlguem**|**Final**|**Variação**|
|---|---|---|---|---|
|R-1|falso|falso|A · matéria honesta|—|
|R-2|falso|verdadeiro|B · matéria que omite|—|
|R-3|verdadeiro|falso|C · o jornalista é o louco|variação 1: sem sangue|
|R-4|verdadeiro|verdadeiro|C · o jornalista é o louco|variação 2: sangue na manga (“fui eu?”)|

Modificadores (não mudam o final, só acrescentam blocos de texto à matéria): `viuSalaSecreta = falso` acrescenta o bloco “sem provas”; `arrombouPorta = verdadeiro` acrescenta o bloco “porta arrombada”; `evidenciasAnotadas` escolhe o bloco de verdade (nenhuma, algumas, muitas); `vezesTocado` escolhe o bloco de exposição (0, 1 a 3, 4 ou mais).

### 3.4 Cola rápida: escolha → consequência → efeito no final

|**Momento**|**Escolha do jogador**|**Consequência imediata**|**Efeito no final**|
|---|---|---|---|
|Qualquer noite|Matar um possuído (ou poupar)|matouAlguem = verdadeiro; npcMorto[id] = verdadeiro; no dia seguinte os NPCs negam que a pessoa existiu. Repetir a noite (checkpoint) desfaz a morte|Final B se dormir na noite 3; Final C variação 2 se transbordar na noite 3|
|Qualquer noite|Esconder-se ou ser tocado|Cada toque soma +25 em possessao e incrementa vezesTocado|Só texto: bloco de exposição|
|Qualquer noite, no quarto|Escrever no caderno|Evidências coletadas viram evidenciasAnotadas (P-07); ativa o estado lúcido (× 0.5 por 60 s)|Só texto: bloco de verdade|
|Noite 1|Achar o cobertor|temCobertor = verdadeiro; o quarto fica quente; passa a ser possível dormir (P-05)|Nenhum direto|
|Noite 2|Abrir a porta com chave ou pé de cabra|Chave: gasta tempo. Pé de cabra: rápido, +10 em possessao, arrombouPorta = verdadeiro|Só texto: bloco “porta arrombada”|
|Dia 3 e noite 3|Seguir a pista do diretor até a sala secreta|viuSalaSecreta = verdadeiro|Só texto: sem esta flag a matéria sai “sem provas”|
|Noite 3|Entrar na sala secreta, encontrar o monstro e fugir pela porta principal antes de transbordar (v0.2)|Sair leva a A ou B. Transbordar leva a C (a noite 3 não repete)|Decide A ou B versus C|
