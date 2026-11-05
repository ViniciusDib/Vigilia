<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 14. Flags e configuração

### 14.1 WorldState e NpcMemory

|**Nome**|**Tipo**|**Inicial**|**Quem escreve**|**Quem lê**|**Snapshot**|
|---|---|---|---|---|---|
|possessao|float 0 a 100|0|PossessionMeter|PossessionMeter, efeitos de tela, NightManager|sim|
|noiteAtual|int 1 a 3|1|NightManager|todos|sim|
|temCobertor|bool|falso|interação com o cobertor (noite 1)|RoomTemperature, SleepController|sim|
|temChave, temPeDeCabra|bool|falso|itens|porta do quarto (noite 2)|sim|
|quartoDestrancado|bool|verdadeiro (noites 1 e 3), falso (noite 2)|porta|NightManager, SleepController|sim|
|arrombouPorta|bool|falso|porta (pé de cabra)|EndingResolver, DialogueSystem|sim|
|bateria|float 0 a 100|100|FlashlightController, pilhas|FlashlightController|sim|
|evidenciasColetadas, evidenciasAnotadas|conjunto de ids|vazio|itens; Notebook|Notebook, EndingResolver|sim|
|matouAlguem|bool|falso|EnemyAI|EndingResolver, DialogueSystem|sim|
|npcMorto[id]|dicionário id → bool|falso|EnemyAI|DialogueSystem|sim|
|vezesTocado|int|0|EnemyAI|EndingResolver|sim|
|viuSalaSecreta|bool|falso|gatilho da sala secreta|EndingResolver|sim|
|pistaNomePrevio, pistaNotaPropria, pistaPulseira|bool|falso|gatilhos de pista|DialogueSystem, EndingResolver|sim|
|transbordouNoite3|bool|falso|NightManager|EndingResolver|não|
|NpcMemory.sabe[npcId]|conjunto de ids de eventos|vazio|DialogueSystem, eventos presenciados|DialogueSystem|sim; zera às 06:00|

### 14.2 BalanceConfig (valores iniciais de TESTE)

|**Campo**|**Tipo**|**Valor inicial**|**Descrição**|
|---|---|---|---|
|maxPossessao|float|100|limite da banheira|
|taxaBase|float por segundo|0.10|vazão base à noite|
|multFria, multMorna, multQuente|float|2.0, 1.0, 0.5|multiplicador por temperatura da sala|
|multLucido, duracaoLucido|float, segundos|0.5, 60|efeito de escrever no caderno|
|pulsoToque|float|25|soma de um toque|
|pulsoBarulho|float|10|soma de arrombar com o pé de cabra|
|fatorSono|float|0.4|possessao × fatorSono ao dormir (esvazia 60%)|
|tempoRecuoToque|segundos|2.5|tempo que o inimigo recua depois do toque|
|raioDeteccao, raioPerseguicao|unidades|6, 8|quando o inimigo percebe e persegue Elias|
|velocidadePatrulha, velocidadePerseguicao|unidades por segundo|1.5, 2.2|multiplicadas pela velocidade da noite (× 0.8, 1.0, 1.2)|
|alcanceLanterna, meiaAberturaGraus|unidades, graus|8, 28|cone da lanterna|
|consumoBateria, recargaPilha|por segundo, pontos|0.4, 40|bateria 100 dura cerca de 4 minutos de luz contínua|
|tempoAtordoado|segundos|8|duração do atordoamento (Q-04)|
|inimigosPorNoite, esconderijosPorNoite, pilhasPorNoite|int ×3|2/4/6, 5/4/3, 4/3/2|alavancas de dificuldade (seção 10)|

Todos estes números são **TESTE** (Q-08): servem para a primeira jogada e serão calibrados no protótipo. Alvo sugerido **ABERTO** (Q-09): cada noite com 6 a 8 minutos.
