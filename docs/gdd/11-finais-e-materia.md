<!-- VIGÍLIA · GDD v0.2 (10/11/2026). Baseado no GDD técnico v0.1 (22/10/2026); mudanças na tabela de 00-capa-e-status.md. -->

## 11. Finais e matéria

O jogo termina com Elias escrevendo (ou não) a matéria. O final é escolhido por **duas flags** (seção 3.3). A matéria é montada por blocos, como peças de Lego: uma manchete (depende do final) mais um parágrafo por eixo, mais blocos de modificadores. Nenhuma IA é usada **DECIDIDO** (D-01).

|**Final**|**Gatilho**|**Tom da matéria**|
|---|---|---|
|A · honesta|fugiu pela porta principal na noite 3 (v0.2), matouAlguem = falso|Elias escreve que foi uma experiência ruim, relata o que viu.|
|B · omite|fugiu pela porta principal na noite 3 (v0.2), matouAlguem = verdadeiro|Elias matou alguém, mas suaviza o relato (“não é tão ruim assim”) para a polícia não ir atrás.|
|C · o louco|transbordou na noite 3|Elias nunca fez a matéria: ficou no sanatório. Quem escreve é outro: “Jornalista encontrado vagando, sem lembrar o próprio nome”. Variação 2: sangue na manga (“fui eu?”).|

### 11.1 Eixos e blocos de texto (P-08)

|**Eixo**|**Flag**|**Faixas**|**Blocos**|
|---|---|---|---|
|Verdade|evidenciasAnotadas|0 / 1 a 2 / 3 ou mais **TESTE**|V0, V1, V2|
|Violência|matouAlguem|falso / verdadeiro|X0, X1|
|Exposição|vezesTocado|0 / 1 a 3 / 4 ou mais **TESTE**|E0, E1, E2|
|Modificador|arrombouPorta|verdadeiro|M-porta|
|Modificador|viuSalaSecreta|falso|M-sem-provas|
|Manchete|final (A, B, C1, C2)|4 casos|H-A, H-B, H-C1, H-C2|

Total de cerca de 15 blocos. O trabalho principal é de **escrita**, não de código, e é bom de dividir no grupo (Q-03).

### 11.2 Rascunhos de manchete (PROPOSTA, para o grupo reescrever)

- **H-A:** “Fim de semana no Sanatório St. Brenna: o que vi no Alasca”.

- **H-B:** “Visita a sanatório no Alasca revela problemas de infraestrutura”. (omite o que Elias fez)

- **H-C1 e H-C2:** “Jornalista é encontrado vagando em sanatório no Alasca e não lembra o próprio nome”.

### 11.3 Garantias (rubrica 11)

- O EndingResolver é uma **função total**: qualquer combinação de flags produz um final e uma matéria. Nunca falha e nunca devolve lista vazia.

- Sem provas não é dead end: a matéria sai “sem provas” (M-sem-provas) e o jogo termina normalmente.

- Testar automaticamente todas as combinações de flags (2 × 2 × as faixas de cada eixo).
