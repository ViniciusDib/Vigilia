# Implementação: o que existe e onde

Estado do projeto Unity em `Vigilia/` (24/10/2026). Liga cada parte do GDD ao código e lista as escolhas provisórias que o grupo precisa confirmar.

## Como verificar que funciona

- **Testes EditMode** (10): EndingResolver em todas as combinações de flags, contas da banheira (exemplo numérico do GDD 6.4), snapshot com cópia profunda repetida, faixas da matéria.
- **Teste PlayMode** (`FimDeSemanaTest`): pula a intro, conversa com Marta, passa o dia 1, verifica que a possessão sobe à noite, força um transbordo (cena do monstro + checkpoint desfaz itens), toma um toque, dorme (−60%), noite 2 trancado fora com 4 inimigos, noite 3 com a caldeira apagada, final A. Falha em qualquer exceção.

## GDD → código

| GDD | O quê | Arquivo |
|---|---|---|
| 6 | Banheira: única autoridade sobre `possessao`; fórmula por quadro; transbordo 1× por noite | `Scripts/Core/PossessionMeter.cs` |
| 6.5, 13.3 | Checkpoint com cópia profunda (JSON) | `Scripts/Core/SaveSnapshot.cs` |
| 7.1 | Iluminado = 4 condições (lógica, não pixels) | `Scripts/Jogador/LightFreezeDetector.cs` |
| 7.2 | Lanterna, bateria, pilhas, piscar com bateria fraca | `Scripts/Jogador/FlashlightController.cs` |
| 8.2 | WorldState (fatos) e NpcMemory (zera às 06:00) | `Scripts/Core/WorldState.cs`, `NpcMemory.cs` |
| 8.3 | Diálogo combina os dois cadernos; NPCs negam quem morreu | `Scripts/Dialogo/DialogueSystem.cs` |
| 9.1 | Máquina macro M-01 a M-11 | `Scripts/Core/NightManager.cs` |
| 9.2 | Jogador J-01 a J-14 | `Scripts/Jogador/PlayerController.cs` |
| 9.3 | Inimigo E-01 a E-13, A* em grade, paciente velho errático e que ouve mais longe | `Scripts/Inimigos/EnemyAI.cs`, `Scripts/Mundo/GridNav.cs` |
| 10 | Alavancas por noite (inimigos, velocidade, esconderijos, pilhas) | `NightManager.ConfigurarNoite` + `BalanceConfig` |
| 11 | Final por duas flags + matéria em blocos | `Scripts/Core/EndingResolver.cs`, `MateriaTextos.cs` |
| 13.4 | WebGL: clique antes do áudio, sem "Sair", PlayerPrefs, gzip + decompression fallback | `Scripts/UI/MenuPrincipal.cs`, `Editor/ConstrutorDeCenas.cs` |
| 14.2 | Todos os números | `Assets/_Vigilia/Config/BalanceConfig.asset` |
| P-10 | Possessão sem barra: vinheta (cor puxa para frio/quente), olheira no retrato de Elias, batimento, sussurros, tremor da lanterna | `Scripts/UI/UIJogo.cs`, `Scripts/Audio/AudioJogo.cs` |
| — | Cena fixa do monstro (port do handoff em `Refs/`) | `Scripts/Encontro/EncontroMonstro.cs` |

## Propostas (P-xx) atrás de configuração

Todas estão em `BalanceConfig` como caixas de marcar: P-02, P-03, P-04, P-05 (duas: quarto quente e porta aberta), P-06, P-07, P-10/Q-13 (`mostrarBarraPossessao`), P-13. P-01, P-08, P-09, P-11, P-12 e P-14 estão aplicadas direto (escopo, nomes, estrutura da matéria).

## Escolhas provisórias para o grupo revisar

Itens ABERTO do GDD que precisaram de um placeholder para o jogo rodar. Todos marcados com `TODO(Q-xx)` no código.

| Q | O que foi feito | Onde mudar |
|---|---|---|
| Q-02 | Sala secreta atrás do arquivo do subsolo; abre ao pegar o **memorando do diretor** (escritório, dia 3 e noite 3). Entrar à noite toca a cena do monstro. Conteúdo é placeholder. | `ConstrutorDeCenas.CriarItens`, `NightManager.SequenciaRevelacao` |
| Q-03 | Intro de 5 quadros, falas, notas, pistas e blocos da matéria são **rascunhos** | `UIJogo.QuadrosIntro`, `DialogoDados`, `MateriaTextos`, `ConstrutorDeCenas.CriarItens` |
| Q-04 | Sedativo (Q) num congelado → atordoado por 8 s; objeto pesado (R duas vezes, com aviso) num atordoado → morto. 1 de cada por noite (consultório e oficina). | `PlayerController.AtualizarAtaque`, `EnemyAI` |
| Q-05 | 5 evidências em papel (ficha, registro de turnos, foto, diário do zelador, registros da sala secreta) + 1 entregue pelo Dr. Almeida no dia 2 | `ConstrutorDeCenas.CriarItens`, `DialogoDados` |
| Q-07 | Armários, baixo de camas e de mesas. Depois de 12 s escondido, um possuído perto encontra Elias. | `BalanceConfig.tempoMaxEscondido` |
| Q-09/Q-18 | O dia termina quando Elias deita na cama ("esperar a noite"). Sem duração alvo ainda. | `SleepController`, `NightManager.TerminarDia` |
| Q-10 | Mapa com 11 salas (sugestão do GDD) e temperatura por noite | `ConstrutorDeCenas.Salas()` |
| Q-11 | Arte provisória gerada por código; retratos com olheiras, versão dia e noite | `Editor/GeradorDeArte.cs` |
| Q-12 | Sons gerados por código; camadas crescem com `possessao` | `SomProcedural`, `AudioJogo` (campos de clipe no Inspector) |
| Q-17 | Noite 3: a caldeira falha, o quarto esfria; segurar E na caldeira religa | `Caldeira.cs`, `BalanceConfig.noite3CaldeiraFalha` |
| Q-20 | Transbordo: cena do monstro, tela vermelha com texto curto, volta ao início da noite | `NightManager.SequenciaTransbordo` |

## Mudanças de design decididas pelo grupo (atualizar o GDD)

| Data | Mudança | Onde |
|---|---|---|
| 24/10 | Câmera vista de cima; personagens giram com a mira | `PlayerController`, `EnemyAI`, `Ferramentas/gerar_arte_topo.py` |
| 04/11 | **O monstro (jumpscare) aparece só na sala secreta.** Transbordar não mostra mais o monstro (`encontroAoTransbordar = false`). | `NightManager` |
| 04/11 | **Noite 3 vira fuga:** não dá para dormir; a porta da sala secreta se abre sozinha; ao entrar, o monstro aparece e começa a fuga. Luz de alarme, porta principal (fim do corredor leste) destrancada, 10 possuídos extras, todos 1,3× mais rápidos, raio de visão 2,2× e banheira 1,6×. **Sair pela porta principal dá o final A ou B** (substitui "dormir na noite 3", D-15/D-16 e M-10/M-11). Transbordar na fuga dá o C. Isso também resolve o Q-17 (o que tira Elias do quarto na noite 3); a caldeira que falha ficou desligada (`noite3CaldeiraFalha = false`). | `NightManager.EncontroComMonstro / IniciarFuga / Escapou`, `Porta` (tipo Saida), `Sala.ehSaida`, `BalanceConfig` (seção "Noite 3") |
| 04/11 | **Mais possuídos:** inimigos por noite 2/6/8 (antes 2/4/6); dois internos novos no elenco. | `BalanceConfig.inimigosPorNoite`, `ConstrutorDeCenas` |
| 04/11 | Cômodos identificáveis: móveis próprios de cada sala, piso próprio e o nome do cômodo na tela ao entrar. | `Ferramentas/gerar_props.py`, `ConstrutorDeCenas.CriarCenario`, `UIJogo.NomeSala` |
| 04/11 | Música de fundo (dia, noite, fuga) com fade cruzado. | `Ferramentas/gerar_musica.py`, `AudioJogo` |

## Comportamentos [EXTRA] (não estão no GDD)

- **Câmera de cima** (confirmado pelo grupo em 24/10), com cenas fixas para o encontro com o monstro.
- Um possuído em **Patrulha** que é iluminado passa a perseguir (e congela no quadro seguinte). Sem isso, um inimigo parado fora do raio de detecção não reagiria à luz.
- **Perseguido → No quarto** é permitido (fugir para o quarto funciona, P-06).
- Ordem em que os NPCs acordam como possuídos: Marta, Wren, Otis, Almeida, paciente, paciente. O Dr. Holt nunca caça.
- Pista 1 (Marta chama Elias pelo nome) acontece no diálogo do dia 1 **e** num sussurro na noite 1.
- A chave abre a porta segurando E por 4 s ("gasta tempo"); o pé de cabra é instantâneo, +10 e atrai inimigos num raio de 14.
- O Dr. Almeida entrega uma evidência por diálogo no dia 2 (o diálogo escreve em `evidenciasColetadas`, o que a tabela 14.1 não prevê).
- Pista 2 (bilhete na letra de Elias) fica no consultório na noite 2; pista 3 (pulseira) no escritório no dia 3 e na noite 3.

## Próximos passos sugeridos

1. Jogar e calibrar `BalanceConfig` (Q-08), uma alavanca por vez.
2. Confirmar com o professor Q-14 e Q-15.
3. Substituir os rascunhos de texto (Q-03) e a arte provisória (Q-11).
4. Subir o build WebGL num projeto privado do itch.io e testar em Chrome e Firefox.
