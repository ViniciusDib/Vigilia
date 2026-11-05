<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 1. Como usar este documento

Este PDF reúne, em um lugar só, tudo o que foi decidido sobre o jogo e tudo o que ainda falta decidir. Cada diagrama tem uma versão em texto (tabela) logo abaixo, e o apêndice A traz o essencial em YAML. Se um diagrama e uma tabela divergirem, vale a tabela e o erro deve ser avisado ao Enzo.

### 1.1 Índice

|**Seção**|**Conteúdo**|**Seção**|**Conteúdo**|
|---|---|---|---|
|2|Resumo do jogo|11|Finais e matéria|
|3|Árvore de decisão (consulta rápida)|12|Pistas e escrita confusa|
|4|Glossário e identificadores|13|Arquitetura de código|
|5|Mundo e elenco|14|Flags e configuração|
|6|A banheira (possessao)|15|Decisões tomadas|
|7|Luz, bateria e ataque|16|Propostas a confirmar|
|8|Dia, noite e memória|17|Em aberto (trabalho do grupo)|
|9|Máquinas de estado|18|Sprints e rubricas|
|10|As três noites|19|Definição de pronto · Apêndice A (YAML)|

### 1.2 Regras para o Claude Code

- **Fonte da verdade:** este documento. Se faltar informação, não invente: pergunte.

- **Itens ABERTO:** criar placeholder com comentário `// TODO(Q-xx)` e seguir com o resto. Itens PROPOSTA: implementar atrás de configuração e avisar.

- **Nenhum número de balanceamento no código.** Tudo vai para o ScriptableObject `BalanceConfig` (seção 14.2).

- **Nomes exatos:** usar os identificadores do glossário (seção 4), em português sem acento, camelCase, eventos com prefixo `On`.

- **Uma única porta de entrada para possessao:** só o PossessionMeter escreve nessa variável (AdicionarPossessao, Dormir, DefinirValor para restaurar checkpoint).

- **Máquinas de estado:** enum + switch + método MudarEstado(novo) com ações de entrada e saída. As transições são as tabelas da seção 9, linha por linha.

- **O Enzo quer entender o código.** Implemente em passos pequenos e testáveis, explique o porquê de cada escolha em poucas frases, e aponte onde ele deve olhar para aprender. Ele vai conduzir o aprendizado com o Claude Code.

- **Compatível com WebGL** (seção 13.4): sem threads, sem acesso direto a arquivos, áudio só depois de um clique.

- **Créditos:** assets e trechos de código de terceiros entram no README. A disciplina exige crédito quando se copia mais de 2 ou 3 linhas, e proíbe copiar código de colegas.

- **Antes de dizer que terminou** uma tarefa, passar pela lista “Definição de pronto” (seção 19).
