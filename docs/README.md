# Documentação do VIGÍLIA

GDD do VIGÍLIA em Markdown. A **v0.2 (10/11/2026)** descreve o jogo entregue; o que mudou desde a v0.1 (22/10/2026) está na tabela "O que mudou" em [00-capa-e-status.md](gdd/00-capa-e-status.md).

## Arquivos

| Arquivo | Para que serve |
|---|---|
| [GDD_completo.md](GDD_completo.md) | O GDD inteiro num arquivo só (busca e leitura corrida). |
| [itch.md](itch.md) | Texto da página do itch.io (português e inglês), configuração e capturas em [itch/](itch/). |

## GDD por seção ([gdd/](gdd/))

| # | Arquivo | Conteúdo | Use quando for mexer em… |
|---|---|---|---|
| — | [00-capa-e-status.md](gdd/00-capa-e-status.md) | Disciplina, prazos, legenda de status | — |
| 1 | [01-como-usar.md](gdd/01-como-usar.md) | Índice original e regras para o Claude Code | qualquer coisa |
| 2 | [02-resumo-do-jogo.md](gdd/02-resumo-do-jogo.md) | Premissa, pilares, escopo | escopo |
| 3 | [03-arvore-de-decisao.md](gdd/03-arvore-de-decisao.md) | Ação → flag → final (figuras 1 e 2) | flags, finais |
| 4 | [04-glossario.md](gdd/04-glossario.md) | Termos e identificadores oficiais | **nomes no código** |
| 5 | [05-mundo-e-elenco.md](gdd/05-mundo-e-elenco.md) | Ambientação, personagens, retratos | NPCs, diálogo, arte |
| 6 | [06-banheira-possessao.md](gdd/06-banheira-possessao.md) | `possessao`: fluxos, cálculo, checkpoint | PossessionMeter, sono, salas |
| 7 | [07-luz-bateria-ataque.md](gdd/07-luz-bateria-ataque.md) | Teste de iluminação, bateria, ataque | lanterna, LightFreezeDetector |
| 8 | [08-dia-noite-memoria.md](gdd/08-dia-noite-memoria.md) | Dia, WorldState × NpcMemory, ciclo da noite | dia, diálogo, reset 06:00 |
| 9 | [09-maquinas-de-estado.md](gdd/09-maquinas-de-estado.md) | Tabelas M-xx, J-xx, E-xx | NightManager, jogador, EnemyAI |
| 10 | [10-as-tres-noites.md](gdd/10-as-tres-noites.md) | Objetivos e alavancas de cada noite | level design |
| 11 | [11-finais-e-materia.md](gdd/11-finais-e-materia.md) | Finais A/B/C, blocos da matéria | EndingResolver |
| 12 | [12-pistas.md](gdd/12-pistas.md) | Pistas do final C | roteiro |
| 13 | [13-arquitetura-de-codigo.md](gdd/13-arquitetura-de-codigo.md) | Módulos, eventos, assinaturas, WebGL | **arquitetura** |
| 14 | [14-flags-e-balanceconfig.md](gdd/14-flags-e-balanceconfig.md) | Todas as flags e números iniciais | WorldState, BalanceConfig |
| 15 | [15-decisoes-tomadas.md](gdd/15-decisoes-tomadas.md) | D-01 a D-21 | — |
| 16 | [16-propostas-a-confirmar.md](gdd/16-propostas-a-confirmar.md) | P-01 a P-14 | — |
| 17 | [17-em-aberto.md](gdd/17-em-aberto.md) | Q-01 a Q-20 (trabalho do grupo) | — |
| 18 | [18-sprints-e-rubricas.md](gdd/18-sprints-e-rubricas.md) | Sprints, rubricas, riscos | planejamento |
| 19 | [19-definicao-de-pronto.md](gdd/19-definicao-de-pronto.md) | Checklist antes de fechar tarefa | **fim de toda tarefa** |
| A | [20-apendice-yaml.md](gdd/20-apendice-yaml.md) | Apêndice A | — |

## Abertos que bloqueiam a S1 (até 29/10)

Do [17-em-aberto.md](gdd/17-em-aberto.md), marcados para S1: **Q-04** (ataque, mínimo), **Q-07** (esconderijos), **Q-09** (duração da noite, fim do dia), **Q-10** (mapa: 2–3 salas e temperaturas), **Q-11** (arte), **Q-16** (divisão de tarefas), **Q-18** (como o dia termina), **Q-20** (tela de transbordo). Urgentes com o professor: **Q-14** e **Q-15**.
