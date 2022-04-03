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
    internal static class AssemblyToSql
    {
        // Cria tabelas no banco de dados
        // Das classes do assembly atual
        // Que tenham o atributo Table ou TableAttribute
        public static void AssemblyClassesToSqlTables(Assembly assembly, SqlConnection conn)
        {
            // Itera sobre todas as classes do assembly atual
            foreach (Type type in assembly.GetTypes())
            {
                // Se classe nao for uma tabela SQL:
                // Verificar próxima classe do programa
                if (type.GetCustomAttribute<TableAttribute>() == null)
                    continue;

                // Se a tabela SQL já existe:
                // Constinuar para a próxima classe do assembly atual
                if (IsClassASqlTable(type, conn))
                    continue;

                // Cria a query de criãção de uma tabela SQL
                string CreateTable = CreateTableQueryFromClass(type);
                

                // Cria a tabela no banco de dados
                SqlCommand cmd = new SqlCommand(CreateTable, conn);
                cmd.ExecuteNonQuery();
            }
        }


        // Verifica se uma classe já é uma tabela SQL
        // Retorna 'true' se já existe e 'false' caso contrário
        private static bool IsClassASqlTable(Type type, SqlConnection conn)
        {
            // Seleciona todas as tabelas do servidor e insere em dt
            DataTable dt = new DataTable();
            SqlDataAdapter sys = new SqlDataAdapter($"SELECT * FROM sys.tables WHERE name = '{type.Name}'", conn);
            sys.Fill(dt);

            // Se tabela já existe:
            // Retorne 'true'
            if (dt.Rows.Count != 0)
                return true;

            // Caso contrário retorne 'false'
            return false;
        }


        // Dado uma classe, esta funcção cria uma query de criação de tabela SQL
        // Que reflete as propriedades e atributos da classe
        private static string CreateTableQueryFromClass(Type type)
        {
            // Cria a query de criar uma tabela
            string CreateTable = $"CREATE TABLE {type.Name}(\n\t";

            // Cria os campos da tabela (1 campo por iteração)
            foreach (PropertyInfo propriedade in type.GetProperties())
            {
                CreateTable += $"{CreateField(propriedade)}";
            }

            // Remove caracteres extras da string
            CreateTable = CreateTable.Remove(CreateTable.Length - 3, 3);

            // Adiciona o final da query de criação de tabela
            CreateTable += "\n);";

            // Retorna a query(CREATE TABLE)
            return CreateTable;
        }


        // Cria o campo a ser inserido em uma query de criação de tabela SQL
        private static string CreateField(PropertyInfo property)
        {
            string campo = "";

            // Adiciona o nome do campo
            campo += property.Name;

            // Adiciona o tipo do campo
            campo += AttachFieldType(property);

            // Adiciona os constraints do campo
            campo += AttachFieldConstraints(property);

            // Retorna o campo
            return campo;
        }


        // Retorna o tipo do atributo infromado
        private static string AttachFieldType(PropertyInfo property)
        {
            // Retorna um atributo que herde da classe 'TypeAttribute'
            TypeAttribute? atributo = property.GetCustomAttribute<TypeAttribute>();

            // Se o atributo não existir:
            // Levantar Exceção
            if (atributo == null)
                throw new Exception(); // TODO: Implementar excessão

            // Retornar atributo
            return $" {atributo.Type}";
        }


        // Retorna os constraints do atributo informado
        private static string AttachFieldConstraints(PropertyInfo property)
        {
            string attributeConstraints = "";

            // retorna uma lista de Constraints da propriedade
            IEnumerable<ConstraintAttribute> constraints = property.GetCustomAttributes<ConstraintAttribute>();

            // Para cada constraint, adicioná-lo a string de constraints
            foreach (ConstraintAttribute constraint in constraints)
            {
                attributeConstraints += $" {constraint.Constraint}";
            }

            // Retorna a string de constraints
            return $"{attributeConstraints},\n\t";
        }
    }
}
