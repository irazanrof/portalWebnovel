# Portal de Webnovels

Projeto P2 – Programação Orientada a Objetos – FATEC Ribeirão Preto – Prof. Me. Douglas Samuel Gonçalves

## Integrantes
|       Nome completo       |      RA       |
|---------------------------|---------------|
| Eduardo Gabriel Fornazari | 2840482511011 |
| Layla Beatriz Damas Alves | 2840482511003 |

## Sobre
Portal web para publicar e ler webnovels (ASP.NET Core MVC, .NET 10+, Dapper, SQLite).
Qualquer usuário cadastrado lê e publica; o administrador gerencia gêneros, modera comentários e revoga/restaura a permissão de publicar.

## Como executar (passo a passo)
1. Instale o .NET SDK 10 ou superior (`dotnet --version` para conferir).

2. Clone o repositório e entre na pasta do projeto (a que contém o `.csproj`):
   git clone <https://github.com/irazanrof/portalWebnovel>

3. **String de conexão**: já está em `appsettings.json`:
   "ConnectionStrings": { "SqliteConnection": "Data Source=Database/webnovel.db" }

4. **Script do banco**: `Database/script.sql`. Na primeira execução o sistema cria o arquivo `Database/webnovel.db` e roda o script sozinho (`Database/DatabaseInitializer.cs`). Para recriar o banco do zero, apague `Database/webnovel.db` e rode de novo.

5. Execute:
   dotnet run
   
6. Abra o endereço mostrado no terminal.


## Contas de teste (todas com a senha `123456`)
|                           E-mail                                 |                 Perfil                    |
|------------------------------------------------------------------|-------------------------------------------|
| admin@webnovels.com                                              |            Administrador                  |
| ana@email.com, bruno@email.com, carla@email.com, elisa@email.com |            Usuário comum                  |
| diego@email.com                                                  | Usuário comum com publicação **revogada** |


## Diagrama de classes (UML)

Código-fonte do diagrama (Mermaid):

```
classDiagram
direction TB

class Usuario {
  <<abstract>>
  +int Id
  +string Nome
  +string Email
  +string SenhaHash
  +DateTime DataCadastro
  +VerificarSenha(senha) bool
  +PodePublicar()* bool
  +PodeExcluirComentario(Comentario, Webnovel)* bool
  +AtualizarPerfil(nome)
}
class UsuarioComum {
  +bool PublicacaoRevogada
  +string MotivoRevogacao
  +PodePublicar() bool
  +PodeExcluirComentario(Comentario, Webnovel) bool
  +RevogarPublicacao(motivo)
  +RestaurarPublicacao()
}
class Administrador {
  +PodePublicar() bool
  +PodeExcluirComentario(Comentario, Webnovel) bool
  +RevogarPublicacaoDe(UsuarioComum, motivo)
}

class Genero {
  +int Id
  +string Nome
  +AlterarNome(nome)
}
class StatusObra {
  <<enumeration>>
  EmAndamento
  Concluida
  Hiato
  Abandonada
}
class Webnovel {
  +int Id
  +string Titulo
  +string Sinopse
  +StatusObra Status
  +DateTime DataCriacao
  +IReadOnlyCollection~Genero~ Generos
  +IReadOnlyCollection~Capitulo~ Capitulos
  +IReadOnlyCollection~Avaliacao~ Avaliacoes
  +DefinirGeneros(Generos)
  +AdicionarCapitulo(titulo, conteudo) Capitulo
  +EditarCapitulo(numero, titulo, conteudo)
  +AlterarStatus(StatusObra)
  +EditarDados(titulo, sinopse)
  +AdicionarAvaliacao(usuarioId, nota, texto)
  +EditarAvaliacao(usuarioId, nota, texto)
  +MediaAvaliacoes() double
}
class Capitulo {
  +int Id
  +int Numero
  +string Titulo
  +string Conteudo
  +DateTime DataPublicacao
  +DateTime DataModificacao
  +Editar(titulo, conteudo)
}
class Interacao {
  <<abstract>>
  +int Id
  +DateTime Data
  +Validar()* void
}
class Comentario {
  +string Texto
  +DateTime DataEdicao
  +Validar() void
  +Editar(texto)
}
class Avaliacao {
  +int Nota
  +string Texto
  +Validar() void
  +Alterar(nota, texto)
}
class Biblioteca {
  +int Id
  +IReadOnlyCollection~Webnovel~ Obras
  +Contem(Webnovel) bool
  +Adicionar(Webnovel)
  +Remover(Webnovel)
}
class Favoritos {
  +int Id
  +IReadOnlyCollection~Webnovel~ Obras
  +Adicionar(Webnovel)
  +Remover(Webnovel)
}
class HistoricoLeitura {
  +int Id
  +DateTime DataLeitura
  +AtualizarLeitura(Webnovel, Capitulo)
}

Usuario <|-- UsuarioComum
Usuario <|-- Administrador

Usuario "1" --> "*" Webnovel : publica como autor
Webnovel "*" o-- "*" Genero : agregação
Webnovel ..> StatusObra
Webnovel "1" *-- "*" Capitulo : composição
Webnovel "1" *-- "*" Avaliacao : composição
Capitulo "1" --> "*" Comentario : recebe
Interacao <|-- Comentario
Interacao <|-- Avaliacao
Usuario "1" --> "*" Interacao : realiza
Usuario "1" *-- "1" Biblioteca : possui
Usuario "1" *-- "1" Favoritos : possui
Biblioteca "*" o-- "*" Webnovel : agregação
Favoritos "*" o-- "*" Webnovel : agregação
Usuario "1" --> "*" HistoricoLeitura : tem
HistoricoLeitura "*" --> "1" Capitulo : último capítulo lido
HistoricoLeitura "*" --> "1" Webnovel : da obra
```






