using System.Reflection;
using static System.Console;
using System.Data.SqlClient;
using Reflection;
using static Reflection.AssemblyToSql;


// Cria e abre uma conexão com o banco de dados
using SqlConnection conn = new SqlConnection("Data Source=JVLPC0524;Initial Catalog=Reflection;Integrated Security=True");
conn.Open();

// Retorna o Assembly atual
Assembly assembly = Assembly.GetExecutingAssembly();

// Cria tabelas com as classes do assembly atual
AssemblyClassesToSqlTables(assembly, conn);


List<Cliente> lista = new List<Cliente>();

Cliente c1 = new Cliente();
Cliente c2 = new Cliente();

c1.cliente_id = 1;
c1.name = "Mary";
c2.cliente_id = 2;
c2.name = "Emilia";

lista.Add(c1);
lista.Add(c2);

Access<Cliente> acesso = new Access<Cliente>(conn);
acesso.Insert(lista);


// Tabelas
[Table]
public class Cliente
{
    [PrimaryKey]
    [Identity]
    [NotNull]
    [Int]
    public int cliente_id { get; set; }
    [NotNull]
    [Varchar(100)]
    public string? name { get; set; }
}
