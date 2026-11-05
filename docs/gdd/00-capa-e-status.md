<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

# VIGÍLIA · GDD técnico v0.1

## Documento de design e guia de implementação

título provisório da proposta P-12 · jogo 2D de suspense em Unity (WebGL) · Projeto 2D, Jogos Digitais, Insper 2026.1

|**Campo**|**Valor**|
|---|---|
|Disciplina|Jogos Digitais · Jogos e Interação · 2026.1 · Insper (Prof. Tiago Sanches, Tec. Pedro Emil, Prof. Gustavo Braga)|
|Tipo|Projeto 2D, multi developer, grupo de 4 pessoas|
|Entrega|11/11/2026 às 23:50: build WebGL no itch.io (jam da disciplina), repositório GitHub (com créditos no README), GDD no Milanote, e envio completo no Blackboard|
|Versão deste documento|0.1 · 22/10/2026 · consolidado a partir da conversa de design entre Enzo e Claude|
|Para quem|Grupo (leitura humana) e Claude Code (leitura por IA). Os dois usam o mesmo texto: tabelas com identificadores fixos, status explícito em cada decisão|
|Prazo do primeiro marco|Quinta, 29/10/2026: protótipo jogável de 2 noites, com paredes, salas diferentes e luz (seção 18)|

#### Status usados em todo o documento

|**Status**|**Significa**|**Como a IA deve tratar**|
|---|---|---|
|**DECIDIDO**|O grupo decidiu. Está na conversa de design e foi aceito.|Implementar.|
|**PROPOSTA**|A IA sugeriu e ninguém contestou. O grupo ainda precisa confirmar.|Implementar atrás de configuração fácil de trocar e avisar o Enzo.|
|**ABERTO**|Ninguém decidiu. É trabalho do grupo (seção 17).|Não inventar. Deixar placeholder marcado com TODO(Q-xx) e perguntar.|
|**TESTE**|Número inicial para testar. Será calibrado jogando.|Pôr em BalanceConfig, nunca fixo no código.|
