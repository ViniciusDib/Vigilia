# VIGÍLIA · GDD final (DDE)

Suspense 2D visto de cima · Unity, WebGL no itch.io · PT/EN
Vinicius Dib, Greta Faria, Enzo Christofoletti, Thomas Rudge · Jogos Digitais, Insper 2026.2

**Premissa:** Elias, jornalista, passa 3 dias e 3 noites num sanatório no Alasca onde todos esquecem tudo ao amanhecer. A regra: **quem não dorme é possuído.** À noite, os possuídos só param sob a luz da lanterna. No fim, Elias escreve uma matéria, e o texto depende do que ele fez.

**Pilares:** a luz é a mecânica · o sono é a única saída · a memória é o tema.

---

## 1. Design

### Blueprint
- **Mundo:** uma ala do sanatório com 11 cômodos, cada um com temperatura própria (fria, morna ou quente), e a saída. Neve, escuridão e aquecimento precário.
- **Personagens:** Elias (jogador); a enfermeira Marta, o zelador Otis, o médico Dr. Almeida e o paciente Sr. Wren (inofensivos de dia, possuídos à noite); o diretor Dr. Holt (o vilão, que nunca aparece); e o monstro (uma única cena).
- **Narrativa:** intro → dia 1 → noite 1 → dia 2 → noite 2 → dia 3 → noite 3 (fuga) → matéria. A revelação está na sala secreta: o diretor mantém os internos acordados de propósito, e a última ficha é a de Elias, que talvez nunca tenha sido jornalista.
- **Pistas:** três, plantadas ao longo do jogo: Marta sabe o nome dele, um bilhete com a letra dele e uma pulseira de paciente.

### Mecânicas
- **Possessão (0 a 100):** é o único medidor de risco, e não há barra de vida.
  - Sobe com o tempo acordado à noite: o dobro em sala fria, metade em sala quente.
  - Cada toque de possuído soma +25.
  - Dormir esvazia 60%. Elias só dorme no quarto, aquecido e com a porta aberta.
  - Chegar a 100 nas noites 1 e 2 recomeça a noite. Na noite 3, leva ao final C.
- **Lanterna:** congela os possuídos que estão no cone de luz. A bateria dura cerca de 2 min 20 s, e pilhas pelo mapa a recarregam.
- **Esconderijos:** o possuído perde Elias de vista, por até 12 s.
- **Ataque (escolha moral):** o sedativo atordoa e a chave inglesa mata. Quem morre é apagado do mundo: no dia seguinte, ninguém lembra que a pessoa existiu.
- **Dia:** sem perigo. Elias conversa, coleta evidências e as anota no caderno, no quarto. Anotar também deixa Elias lúcido, e a possessão sobe mais devagar.
- **Finais:**

| | Não matou | Matou |
|---|---|---|
| **Fugiu na noite 3** | A · matéria honesta | B · matéria que omite |
| **Foi possuído na noite 3** | C · "o louco" | C · com sangue na manga |

A matéria muda conforme as evidências anotadas, as vezes em que Elias foi tocado e se ele arrombou a porta.

### Interface
- **Controles:** WASD para andar; o mouse mira a lanterna; F liga e desliga; E interage; G arromba; Q usa o sedativo; R ataca; Esc pausa.
- **Feedback sem barra:** conforme a possessão sobe, a tela escurece nas bordas, as olheiras de Elias crescem, o coração acelera e a lanterna treme.
- **Telas:** menu (Jogar, Continuar, Opções, Idioma, Créditos), pausa, diálogos com retrato, aviso de transbordo e a matéria final em formato de jornal.

---

## 2. Dinâmica

**Loop:** de dia, investigar → à noite, sair do quarto para cumprir o objetivo → administrar luz, bateria e calor → voltar e dormir.

| | Noite 1 | Noite 2 | Noite 3 |
|---|---|---|---|
| Objetivo | achar um cobertor (o quarto está gelado) | entrar no quarto trancado: chave (lenta) ou pé de cabra (rápido, mas barulhento) | ir à sala secreta e fugir pela porta principal |
| Possuídos | 2 | 6 | 8 + 4 na fuga |
| Pilhas / esconderijos | 4 / 5 | 3 / 4 | 2 / 3 |

**Decisões que surgem do jogo:** gastar bateria agora ou guardar; desviar pela sala quente; arrombar a porta ou perder tempo com a chave; matar e marcar o final, ou poupar.

**Sem becos sem saída:**
- O checkpoint fica no início da noite.
- Sempre há uma pilha alcançável.
- A chave e o pé de cabra estão sempre disponíveis.
- Toda combinação de escolhas gera um final.

---

## 3. Experiência

| | Objetivo | Como |
|---|---|---|
| Sensação | escuro e frio opressivos | quase nada visível fora do cone de luz, neve, passos altos e batimento |
| Emoção | medo → pânico → dúvida | noites mais cheias, o alarme e o monstro na fuga, o final C |
| Desafio | administrar recursos sob pressão | luz, calor e tempo competindo entre si |
| Motivação | descobrir a verdade | pistas ambíguas e 3 finais que convidam a jogar de novo |

---

## 4. Mudanças durante o desenvolvimento

| Data | Mudança | Por quê |
|---|---|---|
| 24/10 | Câmera vista de cima | a lanterna aponta para onde Elias olha |
| 04/11 | A noite 3 virou fuga, com o monstro na sala secreta | dar um clímax ao jogo e um motivo para sair do quarto |
| 04/11 | Possuídos por noite: de 2/4/6 para 2/6/8 | pouca pressão |
| 09/11 | Bateria: de 4 min para 2 min 20 s | a luz precisava ser um recurso escasso |
| 09/11 | Versão em inglês | público do itch.io |
| 10/11 | Fuga mais fácil | playtests: difícil demais |

**Cortes:** fome, barra de possessão (trocada pelo feedback no mundo) e IA em tempo real.
