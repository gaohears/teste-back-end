using Chapter.WebApi.Contexts;
using Chapter.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace Chapter.WebApi.Repositories
{
    public class LivroRepository
    {
        // possui acesso aos dados
        private readonly ChapterContext _context;
        // somente um data context na memória da aplicação na requisição, evitar o usar o new
        // para o repositório existir, precisa do contexto, a aplicacao cria
        // configurar no startup
        public LivroRepository(ChapterContext context)
        {
            _context = context;
        }
        // retorna a lista de livros
        public List<Livro> Listar()
        {
            // SELECT Id, Titulo, QuantidadePaginas, Disponivel FROM Livros;
            return _context.Livros.ToList();
        }

        // cadastrar um livro no banco de dados
        public void Cadastrar(Livro livro)
        {
            // INSERT INTO Livros VALUES ([Titulo], [QuantidadePaginas], [Disponivel]);
            // Adiciona o novo livro
            _context.Livros.Add(livro);
            // Salva (persistir) as informações para serem gravadas no banco de dados
            _context.SaveChanges();
        }

        // busca as informações de um livro por id
        public Livro BuscarPorId(int id)
        {
            // SELECT Id, Titulo, QuantidadePaginas, Disponivel 
            // FROM Livros WHERE Id = $idRecebido;
            return _context.Livros.Find(id);
        }

        // atualizar as informacoes de um livro, a partir da busca de livro por id
        // retorna false quando o livro nao existe, para o controller responder 404
        public bool Atualizar(int id, Livro livro)
        {
            // Busca o livro por id
            Livro livroBuscado = _context.Livros.Find(id);
            // CORRECAO DEF-02: antes, o Update(null) era chamado mesmo sem encontrar o livro,
            // lancando excecao (500). Agora o metodo para aqui e informa que nao encontrou.
            if (livroBuscado == null)
            {
                return false;
            }
            // Atribui os novos valores
            livroBuscado.Titulo = livro.Titulo;
            livroBuscado.QuantidadePaginas = livro.QuantidadePaginas;
            livroBuscado.Disponivel = livro.Disponivel;
            // UPDATE Livros SET Titulo = @Titulo,
            // QuantidadePaginas = @QuantidadePaginas,
            // Disponivel = @Disponivel
            // WHERE Id = @IdRecebido;
            // Adiciona o livro atualizado
            _context.Livros.Update(livroBuscado);
            // Salva (persistir) as informações para serem gravadas no banco de dados
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Deleta um livro existente a partir do id
        /// </summary>
        /// <param name="id">id do livro que será deletado</param>
        /// <returns>false quando o livro não existe, para o controller responder 404</returns>
        public bool Deletar(int id)
        {
            // Busca um livro através do seu id
            Livro livroBuscado = _context.Livros.Find(id);
            // CORRECAO DEF-03: antes, o Remove(null) era chamado mesmo sem encontrar o livro,
            // lancando excecao (500). Agora o metodo para aqui e informa que nao encontrou.
            if (livroBuscado == null)
            {
                return false;
            }
            // Remove o livro que foi buscado
            _context.Livros.Remove(livroBuscado);
            // Salva as alterações
            _context.SaveChanges();
            return true;
        }
    }
}
