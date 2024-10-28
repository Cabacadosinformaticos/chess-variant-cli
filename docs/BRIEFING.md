# Fundamentos da Programação [IADE](https://www.iade.europeia.pt/) <!-- omit in toc -->

## Projeto <!-- omit in toc -->

- [Datas Relevantes](#datas-relevantes)
- [Descrição](#descrição)
  - [Regras](#regras)
    - [Situações especiais](#situações-especiais)
    - [Operações especiais](#operações-especiais)
- [Instruções](#instruções)
  - [Registar jogador (RJ)](#registar-jogador-rj)
  - [Listar jogadores (LJ)](#listar-jogadores-lj)
  - [Iniciar jogo (IJ)](#iniciar-jogo-ij)
  - [Mover Peça (MP)](#mover-peça-mp)
  - [Operação especial (OS)](#operação-especial-os)
  - [Detalhes de jogo (DJ)](#detalhes-de-jogo-dj)
  - [Desistir de jogo (D)](#desistir-de-jogo-d)
  - [Gravar (G)](#gravar-g)
  - [Ler (L)](#ler-l)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Testes de *input*/*output*](#testes-de-inputoutput)
  - [Utilização de testes](#utilização-de-testes)
- [Entrega](#entrega)
- [Prova de Autoria](#prova-de-autoria)
- [Tecnologias](#tecnologias)
- [Grupo de trabalho](#grupo-de-trabalho)
- [Avaliação](#avaliação)

## Datas Relevantes

| Data                    | Evento                         |
| ----------------------- | ------------------------------ |
| 2024/10/28              | Disponibilização do enunciado. |
| 2025/01/05 23:59:59 GMT | Entrega final do trabalho.     |
| 2025/01/13-14           | Provas de autoria.             |

## Descrição

Este projeto pretende a implementação de uma variante do Xadrez.

No xadrez, dois jogadores movem peças num tabuleiro de 8x8 casas, com o objetivo de capturar o rei do adversário. O jogo termina quando um dos reis é capturado, ou quando um dos jogadores não tem mais jogadas possíveis. É também possível terminar o jogo por desistência de um jogador, ou com um empate, por acordo entre os jogadores. Não existe controlo de repetição de jogadas.

O tabuleiro de xadrez é composto por 64 casas dispostas em 8 linhas e 8 colunas. Cada jogador tem 16 peças: um rei (K), uma rainha(Q), duas torres (R), dois cavalos (H), dois bispos (B), e oito peões (P). Existem dois tipos de peças, um por jogador: as brancas (W), e as pretas (B). Cada peça tem também um índice inteiro positivo sequência, a começar em 1 (um), que permite distinguir peças do mesmo tipo. O símbolo de cada peça é composto por três símbolos: um prefixo com a cor da peça (W, B), a letra correspondente à peça (P, R, H, B, Q, K), e um sufixo com o índice (valor inteiro, maior que zero, sequencial no conjunto de peças do mesmo tipo).

No início do jogo, as peças são distribuídas da seguinte forma:

        A    B    C    D    E    F    G    H
    1  BR1  BH1  BB1  BQ1  BK1  BB2  BH2  BR2
    2  BP1  BP2  BP3  BP4  BP5  BP6  BP7  BP8
    3  
    4  
    5  
    6  
    7  WP1  WP2  WP3  WP4  WP5  WP6  WP7  WP8
    8  WR1  WH1  WB1  WQ1  WK1  WB2  WH2  WR2

Cada posição é identificada por uma letra e um número, onde a letra representa a coluna, e o número a linha. A coluna é identificada por uma letra entre `A` e `H`, e a linha por um número entre `1` e `8`.

O jogador com as peças brancas começa o jogo.

O projeto deve permitir o registo de vários jogadores. Cada jogador tem um nome (sem espaços), e um registo de jogos jogados, vitórias, empates, e derrotas. Um jogo decorre sempre entre dois jogadores, e só pode existir um jogo em curso de cada vez.

### Regras

A movimentação das peças é feita de acordo com as seguintes regras básicas:

- Peão (P): move-se uma casa para a frente, ou duas casas na primeira jogada. Captura uma peça adversária movendo-se uma casa na diagonal. O peão não pode mover-se para trás.
- Torre (R): move-se em linha reta, na vertical ou horizontal, para qualquer número de casas.
- Cavalo (H): move-se em forma de L, duas casas numa direção, e uma casa numa direção perpendicular.
- Bispo (B): move-se em linha reta, na diagonal, para qualquer número de casas.
- Rainha (Q): move-se em linha reta, na vertical, horizontal, ou diagonal, para qualquer número de casas.
- Rei (K): move-se uma casa em qualquer direção.

Nenhum movimento de peças pode colocar o próprio rei em cheque

Ver exceções a estas regras nas [operações especiais](#operações-especiais).

#### Situações especiais

Existem várias situações especiais no xadrez:

- Uma captura ocorre quando uma peça adversária se move para a casa ocupada por outra peça.
- O roque é uma jogada especial que envolve o rei e uma torre. O rei move duas casas em direção à torre, e a torre move-se para o lado do rei. O roque só pode ser efetuado se o rei e a torre não se moveram até ao momento, e se não existirem peças entre o rei e a torre.
- O *en passant* é uma jogada especial que envolve peões. Se um peão se mover duas casas, e ficar ao lado de um peão adversário, este pode capturar o peão como se este tivesse movido apenas uma casa.
- A promoção de peões é uma jogada especial que envolve peões. Se um peão chegar à última linha do tabuleiro, é promovido para uma torre, cavalo, bispo ou rainha. Neste projeto a promoção é automaticamente feita para uma rainha. O índice da peça promovida é o próximo disponível no conjunto atual de rainhas do jogador.
- Cheque: um rei está em cheque quando está sob ameaça de captura por uma peça adversária. O jogador em cheque deve mover o rei para uma casa segura, capturar a peça que ameaça o rei, ou bloquear a linha de ataque da peça adversária.
- Xeque-mate: um rei está em xeque-mate quando está em cheque, e não existem jogadas possíveis que o coloquem em segurança. O jogo termina com uma vitória para o adversário.
- Empate: o jogo termina em empate por acordo entre os jogadores, por repetição de jogadas, ou por falta de material para efetuar um xeque-mate. Neste projeto, o empate só vai ser considerado por acordo entre os jogadores.

#### Operações especiais

Neste projeto, as peças do jogo têm ainda a possibilidade de efetuar operações especiais:

- Peão (P): Uma vez durante o jogo, o peão pode andar uma casa para trás.
- Torre (R): Uma vez durante o jogo, a torre pode capturar duas peças adversárias numa única jogada, desde que estas estejam numa linha reta, e a primeira peça esteja a uma casa de distância da torre.
- Cavalo (H): Se o cavalo não estiver encostado a qualquer outra peça, pode avançar 4 casas na mesma direção, horizontal, vertical, ou diagonal, saltando por cima de qualquer peça, e eventualmente capturando a peça na casa de destino (é um movimento em linha reta, e não em L). Esta operação pode acontecer várias vezes no jogo.
- Bispo (B): Uma vez durante o jogo, o bispo captura todos os peões adversários numa área de 3x3 casas, centrada no bispo. O bispo não se move, e as peças são capturadas.
- Rainha (Q): A qualquer momento do jogo, a rainha pode trocar de posição com o rei, desde que o rei não esteja em cheque, e a troca não coloque o rei em cheque. Esta operação pode acontecer várias vezes no jogo.

## Instruções

Na descrição das várias instruções é indicada a sua sintaxe. Os argumentos são separados por espaços em branco, e cada linha é terminada por um caráter fim de linha (`\n`).

Para cada instrução são indicadas as expressões de saída, quer para execuções com sucesso, quer para insucesso.

No caso de insucesso só deve surgir uma mensagem de erro. Verificando-se várias situações de insucesso em simultâneo, deve surgir apenas a mensagem do primeiro cenário, de acordo com a ordem de saídas de insucesso descritas para cada instrução.

Caso o utilizador introduza uma instrução inválida, ou seja, não prevista na lista de instruções desta secção, ou um número de parâmetros errado para uma instrução existente, o programa deve escrever:

    Instrução inválida.

Pode assumir que não existem erros de representação de informação (e.g., texto em vez de valores numéricos).

A descrição de cada instrução pretende ser exaustiva, sem ambiguidades, e suficiente. Não deve ser possível optar entre vários comportamentos possíveis na mesma situação. Se essa situação ocorrer deve entrar em contacto com equipa docente.

Nem todos os casos possíveis de interação estão previstos pelas instruções a implementar. Nessas situações, deve considerar que casos não previstos neste enunciado não serão testados. A implementação não deve suportar mais instruções do que as que estão descritas, no entanto, se forem acrescentadas instruções, estas devem ter o prefixo `X` (e.g., `XJ`).

O programa termina quando for introduzida uma linha em branco, fora do contexto de uma instrução.

### Registar jogador (RJ)

Regista um novo jogador.

`NomeJogador` é o nome de um jogador (sem espaços). O nome não tem espaços em branco, e não tem limite de carateres (estas regras aplicam-se sempre que, em qualquer instruções, surgir um nome de jogador).

Entrada:

    RJ NomeJogador

Saída com sucesso:

    Jogador registado com sucesso.

Saída com insucesso:

- Quando já existe um jogador registado com o mesmo nome:

      Jogador existente.

### Listar jogadores (LJ)

Lista os jogadores registados, indicando os registos de jogos jogados. Os jogadores são ordenados por número decrescente  de vitórias, e alfabeticamente para jogadores com o mesmo número de vitórias.

`NomeJogador` é o nome de um jogador (sem espaços). `NumJogos` representa o número de jogos jogados, `NumVitórias` representa o número de vitórias, `NumEmpates` representa o número de empates, e `NumDerrotas` represente o número de derrotas do jogador.

Entrada:

    LJ

Saída com sucesso:

    NomeJogador NumJogos NumVitórias NumEmpates NumDerrotas
    NomeJogador NumJogos NumVitórias NumEmpates NumDerrotas
    ...

Saída com insucesso:

- Quando não existem jogadores registados:

      Sem jogadores registados.

### Iniciar jogo (IJ)

Inicia um jogo entre dois jogadores.

`NomeJogadorA` e `NomeJogadorB` são os nomes dos jogadores A e B, respetivamente. `TipoJogo` é o tipo de jogo, que pode ser `Novo` ou `Continuação`.

Para o tipo `Continuação`, deve ser indicado o estado do tabuleiro, com a posição de todas as peças separadas por vírgulas (ver exemplo). Cada peça é identificada pelo seu nome, de acordo com a descrição do tabuleiro na secção [Descrição](#descrição). As posições não ocupadas não são preenchidas. O tabuleiro não é avaliado, i.e., qualquer configuração do tabuleiro é considerada válidas, incluindo tabuleiros sem rei, ou com mais de um rei da mesma cor.

Para o tipo `Novo`, não é indicação do estado do tabuleiro.

Entrada:

- Para um jogo novo:

      IJ Novo NomeJogadorA NomeJogadorB

- Para um jogo em continuação:
  
      IJ Continuação NomeJogadorA NomeJogadorB
      BR1,BH1,BB1,BQ1,BK1,BB2,BH2,BR2
      BP1,BP2,BP3,,BP5,BP6,BP7,BP8
      ,,,,,,,
      ,,,BP4,,,,
      ,,,,,,,
      ,,WH1,,,,,
      WP1,WP2,WP3,WP4,WP5,WP6,WP7,WP8
      WR1,,WB1,WQ1,WK1,WB2,WH2,WR2

Saída com sucesso:

    Jogo iniciado com sucesso.

Saída com insucesso:

- Quando já existe um jogo em curso:

      Existe um jogo em curso.

- Quando um dos jogadores indicados não se encontra registado:

      Jogador inexistente.

### Mover Peça (MP)

Move uma peça no tabuleiro. A peça é identificada pela sua posição inicial e final. A posição é identificada por uma letra e um número, de acordo com a descrição do tabuleiro na secção [Descrição](#descrição).

`NomeJogador` é o nome de um jogador registado, `PosiçãoInicial` é a posição inicial da peça, e `PosiçãoFinal` é a posição final da peça. `NomePeçaCapturada` é o nome da peça capturada, se existir. `NomePeça` é o nome da peça movida.

Entrada:

    MP NomeJogador PosiçãoInicial PosiçãoFinal

Saída com sucesso:

- Quando o jogo termina:

      Checkmate. NomeJogador venceu.

- Quando ocorre cheque:

      Check.

- Quando ocorre uma captura:

      Peça NomePeçaCapturada capturada.

- Quando o movimento efetuado corresponde a uma promoção de peão:

      Peão promovido.

- Quando o movimento efetuado corresponde a um roque:

      Roque efetuado.

- Quando o movimento efetuado corresponde a um *en passant*:

      En passant efetuado.

- Quando o movimento é efetuado com sucesso:

      NomePeça movida com sucesso.

Saída com insucesso:

- Quando não existe um jogo em curso:

      Não existe jogo em curso.

- Quando o jogador indicado não se faz parte do jogo em curso:

      Jogador não participa no jogo em curso.

- Quando não é a vez do jogador indicado:

      Não é a vez do jogador.

- Quando a posição inicial ou final não são válidas:

      Posição inválida.

- Quando não existe peça na posição inicial:

      Não existe peça na posição inicial.

- Quando a peça não pode ser movida para a posição final:

      Movimento inválido.

### Operação especial (OS)

Efetua uma operação especial com uma peça. A operação é identificada pela peça. A peça é identificada pela sua posição. A posição é identificada por uma letra e um número, de acordo com a descrição do tabuleiro na secção [Descrição](#descrição).

`NomeJogador` é o nome de um jogador registado, `PosiçãoInicial` é a posição inicial da peça, e `PosiçãoFinal`(opcional) é a posição final da peça. `NomePeça` é o nome da peça. `Numero` é um número inteiro.

Entrada:

    OS NomeJogador PosiçãoInicial PosiçãoFinal

Saída com sucesso:

- Quando se trata do peão:

      Peão NomePeça recuou com sucesso.

- Quando se trata da torre:

      Torre NomePeça capturou duas peças com sucesso.
      
- Quando se trata do cavalo:

      Cavalo NomePeça avançou com sucesso.

- Quando se trata do bispo:

      Bispo NomePeça capturou Numero peões.

- Quando se trata da rainha:

      Rainha trocou de posição com o rei.

Saída com insucesso:

- Quando não existe um jogo em curso:

      Não existe jogo em curso.

- Quando o jogador indicado não se faz parte do jogo em curso:
  
      Jogador não participa no jogo em curso.

- Quando não é a vez do jogador indicado:

      Não é a vez do jogador.

- Quando a posição inicial não é válida:

      Posição inválida.

- Quando não existe peça na posição inicial:

      Não existe peça na posição inicial.

- Quando a posição final não é válida:

      Posição final inválida.

- Quando a peça não pode ser movida para a posição final:

      Movimento inválido.

### Detalhes de jogo (DJ)

Mostra o estado do tabuleiro, e o jogador que tem a vez de jogar.

Entrada:

    DJ

Saída com sucesso (exemplo, considerando a posição inicial do tabuleiro):

        A    B    C    D    E    F    G    H
    1  BR1  BH1  BB1  BQ1  BK1  BB2  BH2  BR2
    2  BP1  BP2  BP3  BP4  BP5  BP6  BP7  BP8
    3  
    4  
    5  
    6  
    7  WP1  WP2  WP3  WP4  WP5  WP6  WP7  WP8
    8  WR1  WH1  WB1  WQ1  WK1  WB2  WH2  WR2

O espaço em branco é preenchido com espaços (não tabs). Entre cada coluna existem dois espaços em branco. Cada símbolo ocupa 3 (três) espaços. As letras das colunas estão em maiúsculas, numa linha acima da linha 1, e centradas nas respetivas colunas. Os números das linhas estão à esquerda de cada linha, e separados por dois espaço em branco da primeira posição da linha.

Saída com insucesso:

- Quando não existe um jogo em curso:

      Não existe jogo em curso.

### Desistir de jogo (D)

Regista a desistência do jogo por um jogador, ou empate caso os dois desistam. É registada uma derrota para cada jogador que desistiu, ou empate para ambos, caso os dois desistam. É registada uma vitória para o jogador que não desistiu.

`NomeJogador` representa o nome de um jogador que participa no jogo em curso.

Entrada:

Nota: o segundo `NomeJogador` é opcional.

    D NomeJogador NomeJogador

Saída com sucesso:

    Jogo terminado com sucesso.

Saída com insucesso:

- Quando não existe jogo em curso:

      Não existe jogo em curso.

- Quando um dos nomes indicados não pertence a um jogador registado:

      Jogador inexistente.

- Quando um dos jogadores indicados não participa no jogo em curso:

      Jogador não participa no jogo em curso.

### Gravar (G)

Grava o registo jogadores e jogos jogados num ficheiro, incluindo o jogo em curso, se existir.

`NomeFicheiro` é o nome do ficheiro onde será feita a gravação.

Entrada:

    G NomeFicheiro

Saída com sucesso:

    Jogo gravado com sucesso.

Saída com insucesso: nenhuma.

### Ler (L)

Recupera o registo de jogadores e jogos jogados de um ficheiro, incluindo, eventualmente, o jogo em curso gravado. Esta operação substitui toda a informação existente.

`NomeFicheiro` é o nome do ficheiro de onde será feita a leitura.

Entrada:

    L NomeFicheiro

Saída com sucesso:

    Jogo lido com sucesso.

Saída com insucesso:

- Quando existe jogo em curso:

      Existe um jogo em curso.

## Estrutura do projeto

A estrutura to projeto está deve ser a seguinte:

    projeto
    |-- iotests/ : diretório com testes de output, a distribuir pela docência.
    |-- src/ : diretório com código fonte.
    |-- src/Project/Program.cs : ficheiro com o ponto de entrada do programa.
    |-- README.md : este ficheiro.
    |-- REPORT.md : relatório do projeto.

Para efetuar a atualizações:

1. Registar o repositório como `upstream` (só deve acontecer uma vez)

        git remote add upstream https://github.com/IADE-FP/FP-2024-2025-Project

2. Atualizar o `upstream` (sempre que existirem atualizações)

        git fetch upstream

3. Obter as alterações (e.g., ficheiro `README.md`)

        git checkout upstream/main README.md

## Testes de *input*/*output*

O projeto é validado através de um conjunto de baterias de teste de *input*/*output*.

Cada bateria é constituída por um ficheiro de entrada e outro e saída. O ficheiro de entrada contém uma sequência de instruções a passar pelo programa que, por sua vez, deve produzir uma sequência de saída *exatamente* igual ao ficheiro de saída. A comparação será feita *byte* a *byte*, pelo que não podem existir quaisquer diferenças para o programa ser considerado válido.

Os grupos de trabalho devem utilizar as baterias públicas para validar o desenvolvimento do projeto.

As baterias serão distribuídas através do repositório git de referência, na diretoria `iotests` (será necessário registar o repositório de referência como `upstream`, de acordo com as instruções na secção sobre [estrutura do projeto](#estrutura-do-projeto)).

### Utilização de testes

Os três testes disponibilizados devem ser utilizados por ordem, já que os cenários descritos num teste podem depender dos cenários dos testes anteriores. Cada teste deve ser utilizado da seguinte forma:

    ./program < 1.in > 1.mine.out

A instrução `<` redireciona o *standard input* para o ficheiro indicado, e a instrução `>` redireciona o *standard output* para o ficheiro indicado (cria se não existir, e escreve por cima se existir).

O `ficheiro 1.in` contém várias instruções para testar o programa. O ficheiro `1.out` contém as saídas correspondentes às instruções no `ficheiro 1.in`. O ficheiro `1.mine.out` será criado com as saídas que o programa gerar. Os ficheiros `1.out` e `1.mine.out` devem ser
idênticos.

A comparação entre ficheiros pode ser feita na linha de comandos, recorrendo ao programa `comp` em Windows, ou `diff` em Linux e MacOS.

A comparação também pode ser feita recorrendo a alguns IDEs ou editores de texto, tal como o *Visual Studio Code*. Para tal, abra o diretório do projeto em *File - Open Folder*. Selecione o diretório do projeto confirme em *Select Folder*. Caso não esteja visível, abra o explorador de ficheiros do *Visual Studio Code* em *View - Explorer*.

Selecione os dois ficheiros a comparar (selecione um, e selecione o outro enquanto pressiona na tecla `Ctrl`).

![](figures/vsc_two_file_selection.png)

Com os dois ficheiros selecionados, utilize o botão direito do rato em cima da seleção para obter um menu. Escolha *Compare Selected*.

![](figures/vsc_compare_selected.png)

O *Visual Studio Code* apresenta os dois ficheiros com as diferenças a vermelho e verde. Caso não existam diferenças, as linhas dos dois ficheiros surgem sem cores.

![](figures/vsc_diff.png)

## Entrega

A entrega do projeto é feita no *GitHub Classroom* e no *Canvas*.

A entrega no *Canvas* corresponde a um ficheiro `zip` do repositório *GitHub Classroom*.

Deve existir, na raiz do repositório um ficheiro de relatório `REPORT.md` com a identificação dos elementos do grupo de trabalho, e eventuais comentários relativos a estratégias de implementação adotadas, e/ou à distribuição de tarefas.

A estrutura do ficheiro pode ser aumentada, mas não alterada, i.e., tem que existir o ponto de entrada `src/Project/Program.cs`.

A ausência de identificação individual no ficheiro de relatório implica a anulação da participação individual no projeto.

O código fonte entregue será sujeito a validação por um conjunto de testes reservado para esse efeito.

A entrega no *Canvas* corresponde a um ficheiro `zip` do repositório *GitHub Classroom*, excluindo eventuais ficheiros compilados.

## Prova de Autoria

Todos os projetos entregues serão sujeitos a prova de autoria. Para esse efeito, cada grupo terá que efetuar uma discussão com a docência, de forma a demonstrar que o código entregue foi de facto feito pelo grupo, e que a distribuição de trabalho foi equilibrada.

O calendário das provas de autoria será disponibilizado no *Canvas*, após o prazo de entrega da implementação do projeto.

A não comparência na prova de autoria implica a anulação da participação individual no projeto.

## Tecnologias

O trabalho deve ser implementado em C# (dotnet 8). Não podem ser utilizadas bibliotecas externas à distribuição padrão da linguagem. No caso de dúvida, os corpo docente deve ser consultado.

## Grupo de trabalho

Os grupos de trabalho devem ter até 4 pessoas. Idealmente, todos os elementos do grupo devem participar em todos os aspetos do projeto. No entanto, pode existir divisão de tarefas, sendo que esta deve ser equilibrada.

Grupos com um número diferente de pessoas devem ser explicitamente autorizados pelo corpo docente.

## Avaliação

O projeto é avaliado com base em duas componentes: quantitativa (*A*), e qualitativa (*B*). A nota final do projeto é determinada por *(0.5 x A) + (0.5 x B)*.

| Instrução | Peso |
| --------- | ---- |
| RJ        | 2    |
| LJ        | 2    |
| IJ        | 2    |
| MP        | 4    |
| OS        | 4    |
| DJ        | 2    |
| D         | 2    |
| G         | 1    |
| L         | 1    |

A avaliação qualitativa irá considerar que existem várias formas de resolver o problema descrito, mas exige-se a utilização dos instrumentos e métodos apresentados na unidade curricular, nomeadamente:

- Justificação clara para as variáveis e operações implementadas.
- Organização da solução coerente com a metodologia apresentada na disciplina.

A implementação estrita de todas as instruções descritas neste enunciado assegura, sem prejuízo de reprovação por irregularidade académica, a nota mínima de 10 valores.

As notas finais do projeto serão disponibilizadas no *Canvas*.
