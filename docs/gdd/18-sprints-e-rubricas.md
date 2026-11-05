<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 18. Sprints e rubricas

Hoje é 22/10/2026 e a entrega é em 11/11/2026 às 23:50. A data é rígida e há punição por dia de atraso. A equipe é de 4 pessoas.

|**Sprint**|**Período**|**Entregas**|
|---|---|---|
|S1|até 29/10|Protótipo de **2 noites** com paredes, 2 a 3 salas diferentes, luz (lanterna com cone), congelar inimigos, PossessionMeter, temperatura da sala, checkpoint e transbordo, máquinas de estado, 1 tipo de inimigo, esconderijos básicos. **Build WebGL de teste no itch.io** (projeto privado). Critério: jogável do início da noite 1 ao fim da noite 2, transbordar repete a noite, sem dead end.|
|S2|29/10 a 05/11|Noite 3, WorldState e NpcMemory, diálogos com retratos, dias, EndingResolver e blocos da matéria, intro, itens de ataque, pistas, sala secreta.|
|S3|05/11 a 11/11|Som, efeitos visuais (neve, vapor da respiração, poeira no feixe da lanterna, vinheta), UI e menus, balanceamento, testes de dead end, página do itch.io (imagens, vídeo, como jogar), README com créditos, GDD final, envio no Blackboard, GitHub e itch.io.|

### 18.1 Cobertura das 12 rubricas

|**Rubrica**|**Como este projeto cobre**|
|---|---|
|1 Conceito e narrativa|Sono e possessão, amnésia, diretor, matéria como registro. Pistas plantadas para o final C.|
|2 Mecânicas|Lanterna que congela, banheira, esconder, escrever, atacar, pé de cabra ou chave.|
|3 Feedback|O mundo avisa: vapor, vidro congelado, vinheta, olheira, batimento, luz alaranjada nas salas quentes.|
|4 UI e menus|Menu, pausa, opções, créditos, diálogo com retratos, indicador de bateria.|
|5 Engajamento|Propósito claro (dormir e escrever a matéria), tensão crescente.|
|6 Level design e progressão|Três noites com alavancas concretas (seção 10); dias como respiro.|
|7 Som|Camadas de áudio ligadas a possessao; sons de passos e lanterna.|
|8 Efeitos visuais|Light 2D, neve, vapor, poeira no feixe, vinheta e sombras.|
|9 Rejogabilidade|Três finais, matar ou poupar, chave ou pé de cabra, eixos da matéria.|
|10 Balanceamento|BalanceConfig com valores calibráveis; alavancas por noite.|
|11 Estados e erros (obrigatória)|Checkpoint por noite, anti dead end, EndingResolver total, feedback ao transbordar.|
|12 GDD (obrigatória)|Este documento é a base; manter no Milanote atualizado.|

As duas rubricas de maior peso (peso 2) e as duas desconsideradas ainda dependem de conversa com o professor (Q-14). Nota: com 2 rubricas de maior importância, A+ exige 7 rubricas A, incluindo as importantes, no máximo 1 C e nenhuma D.

### 18.2 Riscos

- **Escopo:** muita ideia para 20 dias. Cortar primeiro itens de S3 (arte extra, variações de diálogo), nunca o checkpoint ou o EndingResolver.

- **Light 2D em WebGL:** testar o build na S1.

- **Retratos:** estilo 3D contra mundo 2D pode destoar; decidir paleta cedo (Q-11).

- **Dead ends:** rubrica obrigatória; testar a cada sprint.
