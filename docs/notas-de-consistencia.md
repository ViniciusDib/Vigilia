# Notas de consistência do GDD v0.1

Pontos que ficaram ambíguos ou que o GDD não diz, achados ao converter o PDF. **Nada aqui é decisão**: cada item diz o que o código faz até o grupo decidir, sempre de um jeito fácil de trocar. Quando o grupo decidir, atualizar o GDD e apagar o item.

| # | Onde | O que está ambíguo | Tratamento provisório no código |
|---|---|---|---|
| N-01 | 9.2 J-12 × P-05 × 13.2 SleepController | J-12 pede só "quarto quente"; P-05 e o SleepController pedem quarto quente **e** porta aberta. | `SleepController` checa as duas condições, cada uma atrás de um bool em `BalanceConfig`. |
| N-02 | 3.4 e Figura 1 | "Esconder-se ou ser tocado" aparece junto, mas só o **toque** soma `pulsoToque` e incrementa `vezesTocado` (J-06, E-12). | Só o toque incrementa `vezesTocado`. Esconder-se não muda nada na matéria. |
| N-03 | 9.3 E-04 × Figura 10 | E-04 diz que o inimigo faz `NpcMemory.Limpar()` ao amanhecer; a Figura 10 diz que o **NightManager é o único** que zera. | Só o `NightManager` chama `Limpar()`. O inimigo apenas volta para `Rotina` ao receber o evento de amanhecer. |
| N-04 | 14.1 `quartoDestrancado` | Inicial "verdadeiro (noites 1 e 3), falso (noite 2)". Não diz quem configura nem a ordem em relação ao snapshot. | O `NightManager` monta o estado da noite (porta, inimigos, pilhas) **antes** de capturar o `SaveSnapshot`, para o restaurar voltar a esse setup. |
| N-05 | 14.1 snapshot | `vezesTocado`, `bateria` e mortes estão no snapshot: repetir a noite desfaz toques, bateria gasta e mortes daquela noite. | Seguir o GDD (é intencional pela seção 6.5). Só registrar para ninguém estranhar no teste. |
| N-06 | 8.1 e P-04 | De dia `possessao` não sobe, mas Elias pode escrever no caderno. O estado lúcido de dia não tem efeito. | Escrever de dia só anota evidências (o lúcido de 60 s acaba antes da noite e não tem efeito). Se o grupo quiser outro comportamento, decidir junto com o Q-18. |
| N-07 | Todo o GDD | A perspectiva da câmera não estava escrita. | **Resolvido em 24/10/2026:** câmera vista de cima; algumas cenas são fixas (o jumpscare e o encontro com o monstro, que usam a cena do zip em `Refs/`). Atualizar o GDD. |
| N-08 | 10 (tabela) | "Pista 3 (pulseira, P)": o "P" significa PROPOSTA (confirma a seção 12). | Implementar `pistaPulseira` atrás de configuração. |
| N-09 | 3.3 × 9.1 | A máquina macro tem um só `FINAL_C`; as duas variações (sem sangue / sangue na manga) vêm do `EndingResolver`, não de estados diferentes. | Um estado `FINAL_C`; a variação é escolhida pelo `EndingResolver` via `matouAlguem`. |
| N-10 | 17 | A coluna "Responsável" de Q-01 a Q-20 está vazia no PDF. | Preencher quando o grupo dividir as tarefas (Q-16). |
