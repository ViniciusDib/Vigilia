<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 4. Glossário e identificadores

Cada termo é definido uma vez e usado sempre igual, no texto, nas tabelas e no código.

|**Termo**|**Identificador**|**Definição**|
|---|---|---|
|Possessão (a banheira)|possessao|float de 0 a 100: quanto Elias já está possuído. 100 = possuído. Única variável de risco do jogo.|
|Transbordar|OnTransbordou|Evento disparado uma vez por noite quando possessao chega a 100.|
|Vazão base|taxaBase|Quanto possessao sobe por segundo enquanto Elias está acordado à noite.|
|Temperatura da sala|TemperaturaSala {Fria, Morna, Quente}|Atributo de cada sala. Multiplica a vazão base (multFria, multMorna, multQuente).|
|Lúcido|lucido|Estado temporário depois de escrever no caderno. Multiplica a vazão por multLucido durante duracaoLucido.|
|Pulso|AdicionarPossessao(x)|Soma de uma vez em possessao (toque: pulsoToque; barulho: pulsoBarulho).|
|Dormir|Dormir(), OnDormiu|Ação na cama (quarto quente, porta aberta). possessao passa a possessao × fatorSono (0.4).|
|Quarto (porto seguro)|quarto|Sala de Elias: inimigos não entram (P-06), é onde ele escreve e dorme.|
|Caderno|Notebook|Ação de escrever, só no quarto. Anota evidências e deixa Elias lúcido.|
|Evidência|evidenciasColetadas, evidenciasAnotadas|Itens de prova. Só contam para a matéria depois de anotados (P-07).|
|WorldState|mundo|Fatos reais do jogo. Persistem entre dias e noites.|
|NpcMemory|memoriaNpc|O que cada NPC lembra. Zerada às 06:00.|
|Checkpoint|SaveSnapshot|Cópia profunda do estado no início de cada noite. Restaurada quando se transborda nas noites 1 e 2.|
|Toque|OnToque|Contato de um possuído com Elias: soma pulsoToque e o inimigo recua.|
|Congelado|Congelado|Estado do inimigo enquanto está iluminado (4 condições, seção 7.1).|
|Bateria|bateria|float 0 a 100. Cai enquanto a lanterna está ligada. Com 0, a lanterna apaga e nada congela.|
|Matéria|Materia|Texto final montado por blocos escolhidos pelas flags.|
|Final A, B, C|FinalMateria.Honesta, Omite, Louco|Os três finais (seção 11).|
|Funcionário possuído|tipoFuncionario|Inimigo de velocidade normal que patrulha e persegue.|
|Paciente velho|tipoPacienteVelho|Inimigo mais rápido, errático, que reage a barulho.|
