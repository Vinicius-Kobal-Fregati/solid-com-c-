# Projeto para o curso de SOLID com CSharp

Esse projeto foi realizado em conjunto com o curso "SOLID com C#: princípios da programação orientada a objetos" da Alura, nele eu aprendi e apliquei os conceitos desse acrônomo, ele é composto pelos seguintes termos:

## S =  Single responsibility principle ##
Um método deve realizar apenas uma função (tendo uma única responsabilidade), uma classe deve apenas precisar ser alterada por conta de uma única pessoa, função ou área de negócio (também tendo uma única responsabilidade, dentro dos parâmetros de uma classe).

## O = Open Closed Principle ##
Minimize alterações no código pronto (que está fechado), mas permita que seu projeto continue extensível (aberto) para alterações, podemos respeitar esse princípio utilizando o padrão **Decorator**, ao invés de modificarmos nosso código, podimos criar uma nova que decora a existente e apenas altera o que for necessário, dessa forma expandimos nosso código sem prejudicar sua estabilidade.

## L = Liskov Substitution Principle ##
Esse princípio foi desenvolvido pensando em heranças, mas também pode ser aplicado na implementação de interfaces, a Barbara Liskov descobriu que uma hierarquia problemática é aquela que os descendentes não podem ser substituídos pelos ancestrais, dessa forma, devemos fazer com que todas as implementações cumpram as promessas de suas abstrações.

## I = Interface Segregation Principle ##
Notou-se que as abstrações também perdem coesão e acoplamento, é possível em um momento propagar promessas que não poderão ser cumpridas pelos descendentes e implementações, para evitar isso, deve-se separar as operações das interfaces em grupos menores, uma possibilidade é utilizando o padrão **CQRS** (Command and Query Responsibility Segregation), onde deve-se separar o código em operações de leitura e de escrita.

## D = Dependency Inversion Principle ##
Acoplamentos em um sistema orientado a objeto sempre vão existir, mas devemos prestar atenção na qualidade deles, os bons são aqueles que utilizam tipos estáveis (que tem baixa probabilidade de mudar), normalmente, os que fazem parte da plataforma .NET e de APIs amplamente utilizadas. Já os acoplamentos ruins são os de tipos instáveis, como os que criamos na nossa aplicação, principalmente implementações de mecanismos específicos. 
Com isso, deve-se criar abstrações e depender delas (não das implementações), assim melhoramos a qualidade do acoplamento, para isso, podemos a dependência de uma classe, passando como parâmetro no construtor (**injeção de dependência**), podemos também ter a **inversão de controle**, essa ocorre quando uma classe dependente deixa de resolver as dependências de forma direta do código, e passa esse controle para outra.

## Aplicação dos conceitos no código ##
### Single responsibility principle ###
Percebemos que alguns códigos eram repetidos várias vezes no nosso código (principalmente nos arquivos LeilaoController e LeilaoApiController), como esses:

```c#
_context.Leiloes.Include(l => l.Categoria);
_context.Leiloes.Find(id);
_context.Leiloes.Add(leilao);
_context.Leiloes.Update(leilao);
_context.Leiloes.Remove(leilao);
_context.SaveChanges();
```

Para aplicar DRY e o SRP, criou-se uma classe nomeada de LeilaoDao, ela tem a responsabilidade de criar métodos para essas operações específicas e garantir o reuso de código
```c#
public class LeilaoDao
{
    AppDbContext _context;

    public LeilaoDao()
    {
        _context = new AppDbContext();
    }

    public IEnumerable<Categoria> BuscarCategorias()
    {
       return _context.Categorias.ToList();
    }

    public Leilao BuscarPorId(int id)
    {
        return _context.Leiloes.Find(id);
    }
    ...
```

Perceba que no começo essa classe não implementava nenhuma interface, posteriormente ela começou a implementar e mudou de nome para LeilaoDaoComEfCore, ficando assim:
```c#
public class LeilaoDaoComEfCore : ILeilaoDao
{
    AppDbContext _context;

    public LeilaoDaoComEfCore(AppDbContext context)
    {
        _context = context;
    }

    public Leilao BuscarPorId(int id)
    {
        return _context.Leiloes.Find(id);
    }

    public IEnumerable<Leilao> BuscarTodos() => _context.Leiloes.Include(l => l.Categoria);

    public void Incluir(Leilao obj)
    {
        _context.Leiloes.Add(obj);
        _context.SaveChanges();
    }
    ...
```

### Open Closed Principle ###
Iniciamos criando interfaces sobre os serviços de admin e produto, sendo elas:
```c#
public interface IAdminService
{
    IEnumerable<Categoria> ConsultaCategorias();
    IEnumerable<Leilao> ConsultaLeiloes();
    Leilao ConsultaLeilaoPorId(int id);
    void CadastraLeilao(Leilao leilao);
    void ModificaLeilao(Leilao leilao);
    void RemoveLeilao(Leilao leilao);
    void IniciaPregaoDoLeilaoComId(int id);
    void FinalizaPregaoDoLeilaoComId(int id);
}
...
```

Posteriormente, foram criadas classes padrões que as implementem
```c#
public class DefaultAdminService : IAdminService
{
    readonly ILeilaoDao _dao;

    public DefaultAdminService(ILeilaoDao dao)
    {
        _dao = dao;
    }

    public IEnumerable<Categoria> ConsultaCategorias()
    {
        return _dao.BuscarTodasCategorias();
    }
...
```

Por fim, aplicamos elas no código, quando fosse necessário alterar o código, criamos uma nova classe que implemente a interface, utilizando o padrão Decorator.
```c#
public class ArquivamentoAdminService : IAdminService
{
    IAdminService _defaultService;

    public ArquivamentoAdminService(ILeilaoDao dao)
    {
        _defaultService = new DefaultAdminService(dao);
    }

    // Decorator
    public void CadastraLeilao(Leilao leilao)
    {
        _defaultService.CadastraLeilao(leilao);
    }
...
```
Dessa forma, repassamos o método ao padrão (quando não precisar alterar a funcionalidade) e modificamos apenas o que for necessário, mantendo a estabilidade do código e deixando ele aberto para mudanças.

### Liskov Substitution Principle ###
Nossas implementações devem cumprir corretamente todas as promessas das abstrações, é o que fazemos nessa classe
```c#
public class CategoriaDaoComEfCore : ICategoriaDao
{
    AppDbContext _context;
    public CategoriaDaoComEfCore(AppDbContext context)
    {
        _context = context;
    }

    public Categoria BuscarPorId(int id)
    {
        return _context.Categorias
            .Include(c => c.Leiloes)
            .First(c => c.Id == id);
    }
    
    public IEnumerable<Categoria> BuscarTodos()
    {
        return _context.Categorias
            .Include(c => c.Leiloes);
    }
    ...
```

```c#
public interface ICategoriaDao : IQuery<Categoria> { }
```

```c#
public interface IQuery<T>
{
    IEnumerable<T> BuscarTodos();
    T BuscarPorId(int id);
}
```
Perceba que em alguns casos isso pode ser impossível, nesse caso devemos aplicar ISP.


### Interface Segregation Principle ###
No começo do desenvolvimento, existia essa interface, perceba que ela tinha responsabilidades de grupos diferentes
```c#
public interface IDao<T>
{
    IEnumerable<T> BuscarTodos();
    T BuscarPorId(int id);
    void Incluir(T obj);
    void Alterar(T obj);
    void Excluir(T obj);
}
```
Por conta disso, a interface que ICategoria não podia implementar a IDao, pois ela não precisava dos métodos de escrita
```c#
public interface ICategoriaDao
{
    IEnumerable<Categoria> ConsultaCategorias();
    Categoria ConsultaCategoriaPorId(int id);
}
```

Com isso, dividimos a IDao em duas interfaces, sendo essa uma delas:
```c#
public interface IQuery<T>
{
    IEnumerable<T> BuscarTodos();
    T BuscarPorId(int id);
}
```
Dessa forma, a ICategoriaDao pode implementar a IQuery, tornando seus métodos mais genéricos
```c#
public interface ICategoriaDao : IQuery<Categoria> { }
```
Assim, nossa classe descendente pode ser substituída corretamente pela ancestral.

### Dependency Inversion Principle ###
Para melhorar o acoplamento do código, devemos depender de abstrações e não de implementações, fizemos isso nesse trecho de código
Antigo:
```c#
public class LeilaoApiController : ControllerBase
{
    AppDbContext _context;
    LeilaoDao _leilaoDao;
        
    public LeilaoApiController()
    {
        _context = new AppDbContext();
        _leilaoDao = new LeilaoDao();
    }
        
    [HttpGet]
    public IActionResult EndpointGetLeiloes()
    {
        var leiloes = _leilaoDao.BuscarLeiloes();
        return Ok(leiloes);
    }
    ...
```
Viu como dependíamos de implementação?
Olhe como ficou o novo código:
```c#
public class LeilaoApiController : ControllerBase
{
    ILeilaoDao _dao;
    
    public LeilaoApiController(ILeilaoDao dao)
    {
       _dao = dao;
    }

    [HttpGet]
    public IActionResult EndpointGetLeiloes()
    {
       var leiloes = _dao.BuscarLeiloes();
       return Ok(leiloes);
    }
     ...
 ```
 Agora dependemos apenas da abstração
