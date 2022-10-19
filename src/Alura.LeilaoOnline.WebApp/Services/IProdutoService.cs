using System.Collections.Generic;
using Alura.LeilaoOnline.WebApp.Models;

namespace Alura.LeilaoOnline.WebApp.Services
{
    // Abstraimos os serviços do produto
    public interface IProdutoService
    {
        IEnumerable<Leilao> PesquisaLeiloesEmPregaoPorTermo(string termo);
        IEnumerable<CategoriaComInfoLeilao> ConsultaCategoriasComTotalDeLeiloesEmPregao();
        Categoria ConsultaCategoriaPorIdComLeiloesEmPregao(int id);
    }
}