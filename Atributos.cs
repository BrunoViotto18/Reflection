using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflection
{
    // Table

    // Define se uma classe é uma tabela SQL
    public class TableAttribute : Attribute
    {

    }



    // Constraints

    // Classe abstrata para definir constraints
    public abstract class ConstraintAttribute : Attribute
    {
        public abstract string Constraint { get; set; }
    }

    // Primary Key
    public class PrimaryKeyAttribute : ConstraintAttribute
    {
        public override string Constraint { get; set; } = "PRIMARY KEY";
    }

    // Not Null
    public class NotNullAttribute : ConstraintAttribute
    {
        public override string Constraint { get; set; } = "NOT NULL";
    }

    // Identity()
    public class IdentityAttribute : ConstraintAttribute
    {
        public int Init { get; set; }
        public int Step { get; set; }
        public override string Constraint { get; set; } = "IDENTITY";

        public IdentityAttribute(int init = 1, int step = 1)
        {
            this.Init = init;
            this.Step = step;
            if (this.Init != 1 || this.Step != 1)
            {
                this.Constraint += $"({this.Init},{this.Step})";
            }
        }
    }



    // Types

    // Classe abstrata para definir tipos
    public abstract class TypeAttribute : Attribute
    {
        public abstract string Type { get; set; }
    }

    // INT
    public class IntAttribute : TypeAttribute
    {
        public override string Type { get; set; } = "INT";
    }

    // CHAR
    public class CharAttribute : TypeAttribute
    {
        public override string Type { get; set; } = "CHAR";
    }

    // VARCHAR()
    public class VarcharAttribute : TypeAttribute
    {
        public int Size { get; set; }
        public override string Type { get; set; }

        public VarcharAttribute(int size)
        {
            this.Size = size;
            this.Type = $"VARCHAR({size})";
        }
    }

    // DATE
    public class DateAttribute : TypeAttribute
    {
        public override string Type { get; set; } = "DATE";
    }

    // BOOL
    public class BoolAttribute : TypeAttribute
    {
        public override string Type { get; set; } = "BOOL";
    }
}
