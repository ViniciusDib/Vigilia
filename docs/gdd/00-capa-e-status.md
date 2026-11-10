<!-- v0.2 (10/11/2026): atualizado em Markdown a partir do PDF v0.1 com as mudanças que o grupo decidiu durante o desenvolvimento. Descreve o jogo entregue. -->

# VIGÍLIA · GDD técnico v0.2

## Documento de design e guia de implementação

jogo 2D de suspense em Unity (WebGL) · Projeto 2D, Jogos Digitais, Insper 2026.2

|**Campo**|**Valor**|
|---|---|
|Disciplina|Jogos Digitais · Jogos e Interação · 2026.2 · Insper (Prof. Luciano Soares, Tec. Pedro Emil, Prof. Gustavo Braga)|
|Grupo|Vinicius Dib, Greta Faria, Enzo Christofoletti e Thomas Rudge|
|Tipo|Projeto 2D, multi developer, grupo de 4 pessoas|
|Entrega|10/11/2026 às 23:50 (enunciado do Projeto 2D): build WebGL no itch.io (jam da disciplina), repositório GitHub (com créditos no README), GDD no Milanote, e envio completo no Blackboard|
|Versão deste documento|0.2 · 10/11/2026 · atualizada com as mudanças feitas durante o desenvolvimento (tabela abaixo). A v0.1 (22/10/2026) foi consolidada a partir da conversa de design entre Enzo e Claude|
|Para quem|Grupo (leitura humana) e Claude Code (leitura por IA). Os dois usam o mesmo texto: tabelas com identificadores fixos, status explícito em cada decisão|
|Prazo do primeiro marco|Quinta, 29/10/2026: protótipo jogável de 2 noites, com paredes, salas diferentes e luz (seção 18)|

#### O que mudou da v0.1 para a v0.2 (o jogo entregue)

|**Data**|**Mudança**|**Seções**|
|---|---|---|
|24/10|Câmera vista de cima; Elias e os possuídos giram para onde olham. Cenas fixas só para o encontro com o monstro.|2, 5|
|04/11|O monstro (cena fixa) aparece **só na sala secreta**, na noite 3. Transbordar não mostra mais o monstro.|5, 10|
|04/11|**Noite 3 vira fuga:** não dá para dormir; a porta da sala secreta se abre sozinha; ao entrar, Elias encontra o monstro e precisa fugir pela porta principal, com luz de alarme e 4 possuídos extras, mais rápidos. Sair dá o final A ou B; transbordar na fuga dá o C. Resolve Q-02 e Q-17.|3, 9, 10, 11|
|04/11|Mais possuídos: 2 / 6 / 8 por noite (antes 2 / 4 / 6).|10, 14|
|04/11|Cada cômodo tem piso e móveis próprios, e o nome aparece na tela ao entrar. Música de fundo para dia, noite e fuga.|5|
|09/11|Mais difícil: a bateria dura cerca de 2 min 20 s de luz contínua (antes 4 min). Música mais baixa, passos mais altos.|7, 14|
|09/11|Jogo em **português e inglês**, com troca no menu principal.|2|
|10/11|Sala secreta definida: guarda os registros da ala de vigília; a última ficha é a de Elias (Q-01 e Q-02).|5, 12, 17|

#### Status usados em todo o documento

|**Status**|**Significa**|**Como a IA deve tratar**|
|---|---|---|
|**DECIDIDO**|O grupo decidiu. Está na conversa de design e foi aceito.|Implementar.|
|**PROPOSTA**|A IA sugeriu e ninguém contestou. O grupo ainda precisa confirmar.|Implementar atrás de configuração fácil de trocar e avisar o Enzo.|
|**ABERTO**|Ninguém decidiu. É trabalho do grupo (seção 17).|Não inventar. Deixar placeholder marcado com TODO(Q-xx) e perguntar.|
|**TESTE**|Número inicial para testar. Será calibrado jogando.|Pôr em BalanceConfig, nunca fixo no código.|
