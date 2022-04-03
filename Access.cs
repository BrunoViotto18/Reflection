using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;

namespace Reflection
{
    internal class Access<T>
        where T : new() // Deve ser um objeto com construtor
    {
        // Conexão SQL
        SqlConnection conn;


        // Construtor da classe Access (SqlConnection: Obrigatório)
        public Access(SqlConnection conn)
        {
            this.conn = conn;
        }


        // Faz um SELECT de uma tabela T inteira
        public List<T> SelectAll()
        {
            // Puxa todos os dados de uma tabela
            DataTable dt = new DataTable();
            SqlDataAdapter cmd = new SqlDataAdapter($"SELECT * FROM {typeof(T).Name}", conn);
            cmd.Fill(dt);

            // Cria uma lista de T que representa o SELECT realizado
            List<T> selectAll = DatatableToList(dt);

            // Retorna a lista contendo o SELECT * FROM T
            return selectAll;
        }


        // Faz um SELECT por um Id específico de uma tabela T
        public List<T> SelectById(object id)
        {
            // TODO: Possível excessão pode ser levantada, implementar TRY/EXCEPT
            PropertyInfo prop = GetClassPrimaryKey();

            // Select por ID
            DataTable dt = new DataTable();
            SqlDataAdapter cmd = new SqlDataAdapter($"SELECT * FROM {typeof(T).Name} WHERE {prop.Name} = @id;", conn);
            cmd.UpdateCommand.Parameters.AddWithValue("@id", id);
            cmd.Fill(dt);

            // Cria uma lista de T que representa o SELECT realizado
            List<T> selectById = DatatableToList(dt);

            // Retorna uma lista de T
            // Onde cada T representa uma linha retornada pelo SELECT
            return selectById;
        }


        // Faz um SELECT pelo valor de um campo/coluna específica de uma tabela T
        public List<T> SelectByField(string field, object value)
        {
            // Cria um SELECT por algum campo e valor específico
            DataTable dt = new DataTable();
            SqlDataAdapter cmd = new SqlDataAdapter($"SELECT * FROM {typeof(T).Name} WHERE {field} = @value;", conn);
            cmd.UpdateCommand.Parameters.AddWithValue("@value", value);
            cmd.Fill(dt);

            // Cria uma lista de T que representa o SELECT realizado
            List<T> SelectByField = DatatableToList(dt);

            // Retorna a lista de T
            return SelectByField;
        }
        
        
        // Insere dados no formato Lista de T no banco de dados
        public void Insert(List<T> data)
        {
            // Cria o comando INSERT com os locais para inserção de parâmetros
            string insert = ListToInsertQuery(data);

            // Cria o objeto comando SQL
            SqlCommand cmd = new SqlCommand(insert, conn);

            // Retrona todas as propriedades da classe, exceto aquelas com atributo <IdentityAttribute>
            List<PropertyInfo> props = new List<PropertyInfo>();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (prop.GetCustomAttribute<IdentityAttribute>() != null)
                    continue;

                props.Add(prop);
            }

            // Adiciona os valores providos para os parametros da query
            int i = 1;
            foreach (T t in data)
            {
                foreach(PropertyInfo prop in props)
                {
                    cmd.Parameters.AddWithValue($"@{i}", prop.GetValue(t));
                }
            }

            // Executa o comando
            cmd.ExecuteNonQuery();
        }
        
        
        // Deleta um registro do banco de dados
        // De acordo com um valor de Id/PrimaryKey provido
        public void DeleteById(object value)
        {
            // Retorna a propriedade com atributo <PrimaryKey>
            PropertyInfo prop = GetClassPrimaryKey();

            // Cria o comando DELETE
            SqlCommand cmd = new SqlCommand($"DELETE FROM {typeof(T).Name} WHERE {prop.Name} = @value", conn);
            cmd.Parameters.AddWithValue("@value", value);

            // Executa o comando DELETE
            cmd.ExecuteNonQuery();
        }


        // Deleta um registro do banco de dados
        // De acordo um campo e um valor provido
        public void DeleteByField(string field, object value)
        {
            // Cria o comando DELETE
            SqlCommand cmd = new SqlCommand($"DELETE FROM {typeof(T).Name} WHERE {field} = @value", conn);
            cmd.Parameters.AddWithValue("@value", value);

            // Executa o comando DELETE
            cmd.ExecuteNonQuery();
        }


        // Atualiza o valor de um registro a partir do Id/PrimaryKey
        public void UpdateById(string setField, object setValue, object whereId)
        {
            // Retorna a propriedade com atributo <PrimaryKey>
            PropertyInfo prop = GetClassPrimaryKey();

            // Cria o comando DELETE
            SqlCommand cmd = new SqlCommand(
                $"UPDATE {typeof(T).Name} " +
                $"SET {setField} = @value " +
                $"WHERE {prop.Name} = @id", conn);
            cmd.Parameters.AddWithValue("@value", setValue);
            cmd.Parameters.AddWithValue("@value", whereId);

            // Executa o comando DELETE
            cmd.ExecuteNonQuery();
        }


        // Atualiza o valor de um registro a partir do de um campo e valor provido
        public void UpdateByField(string setField, object setValue, string whereField, object whereValue)
        {
            // Cria o comando DELETE
            SqlCommand cmd = new SqlCommand(
                $"UPDATE {typeof(T).Name} " +
                $"SET {setField} = @value " +
                $"WHERE {whereField} = @id", conn);
            cmd.Parameters.AddWithValue("@value", setValue);
            cmd.Parameters.AddWithValue("@value", whereValue);

            // Executa o comando DELETE
            cmd.ExecuteNonQuery();
        }


        // == Private Methods ==


        // Retorna a propriedade <PrimaryKey> da classe T
        private PropertyInfo GetClassPrimaryKey()
        {
            // Busca pela propriedade com o atributo PrimaryKeyAttribute
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                // retorna a propriedade <PrimaryKey>
                if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                    return prop;
            }

            throw new Exception(); // TODO: Implement Exception
        }

        
        // Transforma uma DataTable em uma lista de T
        private List<T> DatatableToList(DataTable dt)
        {
            // Cria uma lista de T
            List<T> select = new List<T>();

            // Itera todas as linhas da DataTable
            foreach (DataRow row in dt.Rows)
            {
                T t = new T();

                // Obtém um array das propriedades de T
                PropertyInfo[] props = typeof(T).GetProperties();

                // Itera pelas colunas da linha atual
                // E atribui os valores para o objeto 't'
                for (int item = 0; item < row.ItemArray.Length; item++)
                {
                    props[item].SetValue(t, row.ItemArray[item]);
                }

                select.Add(t);
            }

            // Retorna a lista de T que representa os dados de um SELECT
            return select;
        }

        
        // Cria uma insert query a partir de uma lista de T
        private string ListToInsertQuery(List<T> data)
        {
            // Cria a primeira parte da query INSERT
            string insertQuery = $"INSERT INTO {typeof(T).Name}";

            // Adiciona os nomes dos parametros a serem inseridos
            insertQuery += AttachPropertyNames();

            // Adiciona a keyword 'VALUES'
            insertQuery += " VALUES ";

            // Retrona uma lista de todas as propriedades da classe
            // Exceto propriedades com atributo <IdentityAttribute>
            List<PropertyInfo> props = new List<PropertyInfo>();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (prop.GetCustomAttribute<IdentityAttribute>() != null)
                    continue;

                props.Add(prop);
            }

            // Itera os objetos de 'data'
            int i = 1;
            foreach (T t in data)
            {
                insertQuery += "\n(";

                // Adiciona os locais de inserção de parametros
                foreach (PropertyInfo prop in props)
                {
                    insertQuery += $"@{i},";
                    i++;
                }

                // Remove uma vírgula extra e adiciona um '),' no final da query
                insertQuery = insertQuery.Remove(insertQuery.Length - 1, 1);
                insertQuery += "),";
            }

            // Remove uma vírgula extra e adiciona um ';' no final da query
            insertQuery = insertQuery.Remove(insertQuery.Length - 1, 1);
            insertQuery += ";";

            // Retorna a string da query
            return insertQuery;
        }
        

        // Retorna uma string com o nome das propriedades de T
        // Dentro de parentesis, separados por vírgula
        private string AttachPropertyNames()
        {
            // Abre parentesis
            string properties = "(";

            // Insere o nome das propriedades separados por vírgula
            foreach(PropertyInfo prop in typeof(T).GetProperties())
            {
                // Se a propriedade tem como atributo <IdentityAttribute>:
                // Continue para a próxima propriedade
                if (prop.GetCustomAttribute<IdentityAttribute>() != null)
                    continue;

                // Adiciona o nome da propriedade
                properties += $"{prop.Name},";
            }

            // Remove a última vírgula extra e fecha o parentesis
            properties = properties.Remove(properties.Length - 1, 1);
            properties += ")";

            // Retorna os atributos
            return properties;
        }
    }
}
