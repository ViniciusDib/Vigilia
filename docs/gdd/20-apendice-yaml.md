<!-- VIGÍLIA · GDD v0.2 (10/11/2026). Baseado no GDD técnico v0.1 (22/10/2026); mudanças na tabela de 00-capa-e-status.md. -->

## Apêndice A · Resumo em YAML (para leitura por IA)

```yaml
projeto: VIGILIA            # titulo provisorio (P-12)
plataforma: Unity 2D (URP, Light 2D), build WebGL, itch.io
entrega: 2026-11-11T23:50   # marco intermediario: 2026-10-29 (2 noites jogaveis)
ia_em_runtime: false        # D-01
estrutura: {dias: 3, noites: 3}
estoque_unico:
  nome: possessao
  faixa: [0, 100]
  enche:
    - {fonte: tempo_acordado_noite, tipo: continuo, valor: taxaBase}
    - {fonte: toque, tipo: pulso, valor: pulsoToque}
    - {fonte: barulho_pe_de_cabra, tipo: pulso, valor: pulsoBarulho}   # P-03
  regulam_vazao:
    - {sala: {fria: 2.0, morna: 1.0, quente: 0.5}}
    - {lucido_apos_escrever: 0.5, duracao_s: 60}
  esvazia: {dormir: "possessao = possessao * 0.4"}   # D-09
  transbordar: {noite_1_2: restaurar_checkpoint, noite_3: final_C}   # D-11, P-02
reset_06h: {zera: NpcMemory, mantem: WorldState}   # D-07
checkpoint: {quando: inicio_da_noite, copia: profunda}
maquina_macro:
  estados: [INTRO, DIA1, NOITE1, DIA2, NOITE2, DIA3, NOITE3, FINAL_A, FINAL_B, FINAL_C]
  transicoes_ids: [M-01, M-02, M-03, M-04, M-05, M-06, M-07, M-08, M-09, M-10, M-11]
maquina_jogador:
  estados: [Explorando, Perseguido, Escondido, Tocado, NoQuarto, Escrevendo, Dormindo, Transbordou]
  transicoes_ids: [J-01 .. J-14]
maquina_inimigo:
  estados_dia: [Rotina, Seguindo]
  estados_noite: [Patrulha, Perseguindo, Congelado, Atordoado, Recuando, Morto]
  transicoes_ids: [E-01 .. E-13]
iluminado_quando:
  - distancia <= alcance
  - angulo <= meiaAbertura
  - raycast_sem_parede
  - lanterna_ligada_e_bateria_maior_que_0
finais:
  resolver: |
    if transbordouNoite3: C (variacao = matouAlguem ? sangue : sem_sangue)
    elif matouAlguem:     B
    else:                 A
  modificadores: [viuSalaSecreta=falso -> sem_provas, arrombouPorta, evidenciasAnotadas, vezesTocado]
flags_worldstate: [possessao, noiteAtual, temCobertor, temChave, temPeDeCabra, quartoDestrancado,
  arrombouPorta, bateria, evidenciasColetadas, evidenciasAnotadas, matouAlguem, npcMorto,
  vezesTocado, viuSalaSecreta, pistaNomePrevio, pistaNotaPropria, pistaPulseira]
flags_fora_do_snapshot: [transbordouNoite3]
elenco: [Elias Vaughn (jogador), Marta Quill, Otis Brandt, Dr. Henrik Almeida, Sr. Wren, Dr. Alaric Holt]
decididos: D-01 .. D-21     # secao 15
propostas: P-01 .. P-14     # secao 16 (implementar atras de config e avisar)
em_aberto: Q-01 .. Q-20     # secao 17 (nao inventar: TODO(Q-xx) e perguntar)
urgentes_com_professor: [Q-14, Q-15]
```
