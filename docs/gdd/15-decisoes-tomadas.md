<!-- Fonte: Vigilia_GDD_tecnico_v0.1.pdf (v0.1, 22/10/2026). Convertido para Markdown; se divergir do PDF, vale o PDF. -->

## 15. Decisões tomadas

|**ID**|**Decisão**|
|---|---|
|D-01|Sem IA ou LLM no jogo. Falas e finais são determinísticos, montados por regras e blocos de texto.|
|D-02|Unity 2D (URP, Light 2D), build WebGL no itch.io, repositório GitHub com créditos no README, GDD no Milanote atualizado.|
|D-03|Premissa: quem não dorme é possuído. A regra é apresentada em uma intro obrigatória, curta e pulável.|
|D-04|Estrutura: 3 dias e 3 noites (um fim de semana), dificuldade crescente (noite 1 a mais fácil, noite 3 a mais difícil).|
|D-05|Ambientação no Alasca, inverno, noites longas. Estética “Outlast em 2D”. Retratos de diálogo estilo Pokémon, com olheiras.|
|D-06|Os possuídos andam até Elias e só param enquanto iluminados pela lanterna. De dia os funcionários são inofensivos, à noite ficam suspeitos e os velhos saem.|
|D-07|Às 06:00 os NPCs esquecem tudo; só Elias lembra. O mundo (WorldState) não reseta.|
|D-08|Um estoque único, possessao (a banheira), de 0 a 100. 100 = possuído.|
|D-09|Dormir esvazia 60% do nível (valor de teste).|
|D-10|A possessão acumula entre noites, sem teto de herança.|
|D-11|Transbordar nas noites 1 e 2 repete a noite com os dados iguais aos do checkpoint do início da noite.|
|D-12|O calor é atributo da sala (fria, morna, quente) e regula a vazão. Não há barra de temperatura.|
|D-13|Escrever no caderno é possível só no quarto (lugar seguro) e desacelera a vazão.|
|D-14|O toque de um possuído enche um bloco grande da banheira e o inimigo recua. Não é game over.|
|D-15|Três finais (A honesta, B omite, C o louco). O C tem duas variações conforme matouAlguem.|
|D-16|Noite 1: sem cobertor, busca. Noite 2: trancado fora do quarto, chave ou pé de cabra (os dois sempre disponíveis). Noite 3: pista do diretor, a pior noite; dormir ou ficar acordado.|
|D-17|Pistas do final C: nota na letra de Elias e funcionário que o chama pelo nome antes de ele se apresentar. Escrita deliberadamente confusa.|
|D-18|A matéria final é montada por blocos de texto escolhidos pelas flags.|
|D-19|Entrega em 11/11/2026 às 23:50. Marco intermediário: protótipo de 2 noites em 29/10/2026.|
|D-20|A lanterna tem bateria.|
|D-21|Elias pode atacar com itens encontrados no mapa. Matar é permanente (matouAlguem). Como funciona o ataque está em aberto (Q-04).|
|D-22|(24/10) Câmera vista de cima; personagens giram para onde olham. Cenas fixas só para o encontro com o monstro.|
|D-23|(04/11) O monstro aparece só na sala secreta, na noite 3. Transbordar não mostra o monstro.|
|D-24|(04/11) Noite 3 é uma fuga: sem dormir; a porta secreta se abre; depois do monstro, Elias foge pela porta principal. Sair dá A ou B; transbordar dá C. Substitui “dormir na noite 3” em D-15/D-16.|
|D-25|(04/11) Mais possuídos: 2/6/8 por noite, mais 4 extras na fuga.|
|D-26|(09/11) O jogo tem versão em português e em inglês, com troca no menu principal.|
|D-27|(10/11) A sala secreta guarda os registros da ala de vigília; a última ficha é a de Elias (resolve Q-01 e Q-02 no escopo do jogo).|
