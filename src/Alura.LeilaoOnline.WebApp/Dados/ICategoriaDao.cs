using System.Collections.Generic;
using Alura.LeilaoOnline.WebApp.Models;

namespace Alura.LeilaoOnline.WebApp.Dados
{
    // Implementa a interface genérica, tendo todos seus métodos genéricos, mas agora, voltados para o objeto Categoria
    public interface ICategoriaDao : IQuery<Categoria> { }
}
