-- =============================================================================
-- PROJETO: Portal de Webnovels (FATEC Ribeirão Preto - POO / ILP007)
-- SGBD: SQLite
-- ARQUIVO: Database/script.sql
-- PARTE 1: DDL (criação das tabelas)   PARTE 2: DML (dados fictícios de teste)
-- Observação: o próprio sistema executa este script na primeira inicialização
-- (Database/DatabaseInitializer.cs), caso o arquivo webnovel.db não exista.
-- =============================================================================

-- =============================================================================
-- PARTE 1: DDL
-- =============================================================================

-- Usuários. Tipo diferencia as classes UsuarioComum e Administrador (herança).
CREATE TABLE IF NOT EXISTS Usuario (
    Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome               TEXT    NOT NULL,
    Email              TEXT    NOT NULL UNIQUE,
    SenhaHash          TEXT    NOT NULL, 
    Tipo               TEXT    NOT NULL CHECK (Tipo IN ('Comum', 'Administrador')),
    PublicacaoRevogada INTEGER NOT NULL DEFAULT 0 CHECK (PublicacaoRevogada IN (0, 1)),
    MotivoRevogacao    TEXT    NULL,
    DataCadastro       TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS Genero (
    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL UNIQUE
);

-- Status: 0 = Em andamento, 1 = Concluída, 2 = Hiato/Pausada, 3 = Abandonada
CREATE TABLE IF NOT EXISTS Webnovel (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Titulo      TEXT    NOT NULL CHECK (LENGTH(Titulo) >= 3),
    Sinopse     TEXT    NOT NULL CHECK (LENGTH(Sinopse) >= 50),
    Status      INTEGER NOT NULL DEFAULT 0 CHECK (Status BETWEEN 0 AND 3),
    AutorId     INTEGER NOT NULL REFERENCES Usuario (Id),
    DataCriacao TEXT    NOT NULL
);

-- Ligação N:N entre Webnovel e Genero (uma obra pode ter vários gêneros).
CREATE TABLE IF NOT EXISTS WebnovelGenero (
    WebnovelId INTEGER NOT NULL REFERENCES Webnovel (Id) ON DELETE CASCADE,
    GeneroId   INTEGER NOT NULL REFERENCES Genero (Id)   ON DELETE RESTRICT,
    PRIMARY KEY (WebnovelId, GeneroId)
);

-- Capítulos (composição: somem junto com a webnovel). O número 0 é aceito (prólogo).
CREATE TABLE IF NOT EXISTS Capitulo (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    WebnovelId       INTEGER NOT NULL REFERENCES Webnovel (Id) ON DELETE CASCADE,
    Numero           INTEGER NOT NULL CHECK (Numero >= 0),
    Titulo           TEXT    NOT NULL,
    Conteudo         TEXT    NOT NULL,
    DataPublicacao   TEXT    NOT NULL,
    DataModificacao  TEXT    NULL,
    UNIQUE (WebnovelId, Numero)
);

-- Avaliações: nota de 1 a 5 e texto opcional (até 10.000 caracteres). Uma por usuário e obra.
CREATE TABLE IF NOT EXISTS Avaliacao (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId  INTEGER NOT NULL REFERENCES Usuario (Id),
    WebnovelId INTEGER NOT NULL REFERENCES Webnovel (Id) ON DELETE CASCADE,
    Nota       INTEGER NOT NULL CHECK (Nota BETWEEN 1 AND 5),
    Texto      TEXT    NULL CHECK (Texto IS NULL OR LENGTH(Texto) <= 10000),
    Data       TEXT    NOT NULL,
    UNIQUE (UsuarioId, WebnovelId)
);

-- Comentários nos capítulos: não vazios e até 1000 caracteres.
CREATE TABLE IF NOT EXISTS Comentario (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId  INTEGER NOT NULL REFERENCES Usuario (Id),
    CapituloId INTEGER NOT NULL REFERENCES Capitulo (Id) ON DELETE CASCADE,
    Texto      TEXT    NOT NULL CHECK (LENGTH(Texto) BETWEEN 1 AND 1000),
    Data       TEXT    NOT NULL,
    DataEdicao TEXT    NULL
);

-- Biblioteca pessoal.
CREATE TABLE IF NOT EXISTS BibliotecaItem (
    UsuarioId   INTEGER NOT NULL REFERENCES Usuario (Id)   ON DELETE CASCADE,
    WebnovelId  INTEGER NOT NULL REFERENCES Webnovel (Id)  ON DELETE CASCADE,
    DataAdicao  TEXT    NOT NULL,
    PRIMARY KEY (UsuarioId, WebnovelId)
);

-- Favoritos: lista independente da biblioteca.
CREATE TABLE IF NOT EXISTS Favorito (
    UsuarioId       INTEGER NOT NULL REFERENCES Usuario (Id)   ON DELETE CASCADE,
    WebnovelId      INTEGER NOT NULL REFERENCES Webnovel (Id)  ON DELETE CASCADE,
    DataFavoritado  TEXT    NOT NULL,
    PRIMARY KEY (UsuarioId, WebnovelId)
);

-- Histórico: apenas o último capítulo lido de cada obra (um registro por usuário e obra).
CREATE TABLE IF NOT EXISTS HistoricoLeitura (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId   INTEGER NOT NULL REFERENCES Usuario (Id)   ON DELETE CASCADE,
    WebnovelId  INTEGER NOT NULL REFERENCES Webnovel (Id)  ON DELETE CASCADE,
    CapituloId  INTEGER NOT NULL REFERENCES Capitulo (Id)  ON DELETE CASCADE,
    DataLeitura TEXT    NOT NULL,
    UNIQUE (UsuarioId, WebnovelId)
);

-- =============================================================================
-- PARTE 2: DML (dados fictícios de teste)
-- =============================================================================

INSERT INTO Usuario (Nome, Email, SenhaHash, Tipo, PublicacaoRevogada, MotivoRevogacao, DataCadastro) VALUES
('Administrador Geral', 'admin@webnovels.com', 'PBKDF2$100000$cypTDNbq31khJt3mm1AONQ==$B+FxeR/s0/8HCgg+RoBUyCVnpVW7IBbhCxLBPzhuHc0=', 'Administrador', 0, NULL, '2026-10-01 09:00:00'),
('Ana Souza',           'ana@email.com', 'PBKDF2$100000$fxRVwDtD1+J3XTeua2XooA==$fFEmpSAE2+KEfXlqqs6qr+pTIxgpLpUhDXqopG/QwUY=', 'Comum',         0, NULL, '2026-10-02 10:15:00'),
('Bruno Lima',          'bruno@email.com', 'PBKDF2$100000$sIZNMZMroUrrZ5nGqpnojA==$kq42GF0y2/wvuQ+d10pwOY24gng0Zg9gRMap439G4DE=', 'Comum',         0, NULL, '2026-10-03 11:30:00'),
('Carla Mendes',        'carla@email.com', 'PBKDF2$100000$9/2I+n50ryhc9xEMoME7zg==$o/KqdfGFVvnkmNYMDIt1vGWpDJ/RowBvEHthF+Lj1BA=', 'Comum',         0, NULL, '2026-10-04 14:00:00'),
('Diego Alves',         'diego@email.com', 'PBKDF2$100000$0qh0YjmoPB5LUvFQI3MV/g==$XqCDTh9TlG1Agysh5Qa5cq/X5qTFzSsMbEgUxMAjaN0=', 'Comum',         1, 'Violação das regras da plataforma: conteúdo copiado.', '2026-10-05 16:45:00'),
('Elisa Rocha',         'elisa@email.com', 'PBKDF2$100000$5gWZsm1eEzwTaT7EZ/8ZlA==$goyLs8NmRZi64KfcQU7y4uzOEdI7nx0xagBdxf6q3/0=', 'Comum',         0, NULL, '2026-10-06 18:20:00');

INSERT INTO Genero (Nome) VALUES
('Fantasia'), ('Romance'), ('Aventura'), ('Ficção Científica'),
('Mistério'), ('Terror'), ('Drama'), ('Comédia');

INSERT INTO Webnovel (Titulo, Sinopse, Status, AutorId, DataCriacao) VALUES
('A Torre dos Ventos Esquecidos',
 'Uma jovem aprendiz de cartógrafa descobre que o mapa herdado da avó leva a uma torre que desaparece a cada amanhecer.',
 0, 2, '2026-10-10 09:00:00'),
('Cartas para o Último Verão',
 'Duas amigas de infância trocam cartas durante anos e, ao se reencontrarem, precisam decidir o que fazer com os segredos que guardaram.',
 1, 2, '2026-10-11 10:00:00'),
('Órbitas Quebradas',
 'Em uma estação espacial à deriva, um engenheiro tenta descobrir quem sabotou o sistema de oxigênio antes que o ar acabe.',
 0, 3, '2026-10-12 11:00:00'),
('O Caso da Biblioteca Fantasma',
 'Livros desaparecem toda sexta-feira de uma biblioteca fechada há trinta anos, e uma estudante decide investigar o culpado.',
 2, 4, '2026-10-13 12:00:00'),
('Sombras no Porão',
 'Uma família se muda para uma casa antiga e percebe que o porão guarda mais lembranças do que deveria guardar.',
 3, 5, '2026-10-14 13:00:00'),
('Reino de Cinzas e Mel',
 'Um príncipe exilado e uma apicultora improvável unem forças para recuperar um reino devastado por uma antiga maldição.',
 0, 6, '2026-10-15 14:00:00');

INSERT INTO WebnovelGenero (WebnovelId, GeneroId) VALUES
(1, 1), (1, 3),
(2, 2), (2, 7),
(3, 4), (3, 5),
(4, 5),
(5, 6),
(6, 1), (6, 3);

INSERT INTO Capitulo (WebnovelId, Numero, Titulo, Conteudo, DataPublicacao, DataModificacao) VALUES
(1, 1, 'O mapa da vovó',        'Mira encontrou o mapa dentro de uma caixa de costura. As linhas pareciam se mover quando ela piscava.', '2026-10-10 09:30:00', NULL),
(1, 2, 'A torre some ao amanhecer', 'Ao chegar ao topo da colina, a torre estava lá. Quando o sol nasceu, só restou um círculo de grama queimada.', '2026-10-17 09:30:00', NULL),
(1, 3, 'O guardião dos ventos', 'Um velho de capa azul disse que a torre só aparece para quem carrega um segredo que ainda não contou a ninguém.', '2026-10-24 09:30:00', '2026-10-25 20:10:00'),
(2, 0, 'Prólogo',               'A primeira carta chegou em um envelope amarelado, com selo de uma cidade que Clara jurava não conhecer.', '2026-10-11 10:30:00', NULL),
(2, 1, 'Querida Júlia',         'Querida Júlia, escrevo da varanda onde passamos o último verão. Tudo aqui continua igual, menos eu.', '2026-10-18 10:30:00', NULL),
(3, 1, 'Alarme vermelho',       'O alarme tocou às três da manhã. Em vinte minutos, o oxigênio do setor B cairia abaixo do limite seguro.', '2026-10-12 11:30:00', NULL),
(3, 2, 'Quem tinha acesso?',    'Apenas quatro pessoas possuíam a chave do painel principal. Uma delas estava mentindo sobre onde passou a noite.', '2026-10-19 11:30:00', NULL),
(4, 1, 'A sexta-feira dos livros', 'Na primeira sexta-feira do mês faltaram três livros nas estantes. A porta continuava trancada por dentro.', '2026-10-13 12:30:00', NULL),
(4, 2, 'O cartão sem nome',     'Entre as páginas de um dicionário antigo, Helena encontrou um cartão de empréstimo sem nome e com data de 1994.', '2026-10-20 12:30:00', NULL),
(5, 1, 'A mudança',             'O caminhão de mudança foi embora ao entardecer. No porão, algo arranhava a porta de madeira.', '2026-10-14 13:30:00', NULL),
(6, 1, 'O príncipe das abelhas', 'Exilado pela própria família, Teo atravessou a fronteira com uma coroa quebrada e uma colmeia nas costas.', '2026-10-15 14:30:00', NULL),
(6, 2, 'Mel e cinzas',          'A apicultora Lina ofereceu abrigo ao estranho, mas deixou claro que ninguém dormiria na casa sem trabalhar.', '2026-10-22 14:30:00', NULL),
(6, 3, 'A maldição antiga',     'No terceiro dia, as abelhas formaram no ar um símbolo que Lina só tinha visto em livros proibidos.', '2026-10-29 14:30:00', NULL);

INSERT INTO Avaliacao (UsuarioId, WebnovelId, Nota, Texto, Data) VALUES
(3, 1, 5, 'História envolvente, a ideia da torre que desaparece é original e o ritmo é ótimo.', '2026-11-01 10:00:00'),
(4, 1, 4, NULL, '2026-11-02 11:00:00'),
(6, 1, 5, NULL, '2026-11-03 12:00:00'),
(2, 3, 4, 'Boa ambientação, quero mais capítulos para descobrir o culpado.', '2026-11-04 13:00:00'),
(5, 2, 5, 'Final emocionante, as cartas fazem a história ficar muito íntima.', '2026-11-05 14:00:00'),
(3, 2, 4, NULL, '2026-11-06 15:00:00'),
(2, 6, 5, NULL, '2026-11-07 16:00:00');

INSERT INTO Comentario (UsuarioId, CapituloId, Texto, Data, DataEdicao) VALUES
(3, 1, 'Adorei o primeiro capítulo!', '2026-11-01 10:05:00', NULL),
(4, 1, 'A ideia do mapa que se move é ótima.', '2026-11-02 11:05:00', NULL),
(6, 2, 'Estou curiosa para saber quem é o velho de capa azul.', '2026-11-03 12:05:00', NULL),
(2, 6, 'Que tensão logo no início!', '2026-11-04 13:05:00', NULL),
(5, 4, 'Prólogo lindo, já quero ler o resto.', '2026-11-05 14:05:00', '2026-11-05 14:20:00'),
(3, 8, 'Quem será o culpado dos livros sumidos?', '2026-11-06 15:05:00', NULL),
(2, 11, 'Gostei muito da apicultora!', '2026-11-07 16:05:00', NULL);

INSERT INTO BibliotecaItem (UsuarioId, WebnovelId, DataAdicao) VALUES
(3, 1, '2026-11-01 10:00:00'),
(4, 1, '2026-11-02 11:00:00'),
(6, 1, '2026-11-03 12:00:00'),
(2, 3, '2026-11-04 13:00:00'),
(5, 2, '2026-11-05 14:00:00'),
(3, 4, '2026-11-06 15:00:00'),
(2, 6, '2026-11-07 16:00:00');

INSERT INTO Favorito (UsuarioId, WebnovelId, DataFavoritado) VALUES
(3, 1, '2026-11-01 10:10:00'),
(6, 1, '2026-11-03 12:10:00'),
(5, 2, '2026-11-05 14:10:00'),
(2, 3, '2026-11-04 13:10:00'),
(2, 6, '2026-11-07 16:10:00'),
(4, 4, '2026-11-08 17:00:00');

INSERT INTO HistoricoLeitura (UsuarioId, WebnovelId, CapituloId, DataLeitura) VALUES
(3, 1, 3,  '2026-11-01 10:30:00'),
(4, 1, 2,  '2026-11-02 11:30:00'),
(6, 1, 1,  '2026-11-03 12:30:00'),
(2, 3, 7,  '2026-11-04 13:30:00'),
(5, 2, 5,  '2026-11-05 14:30:00'),
(2, 6, 12, '2026-11-07 16:30:00');
