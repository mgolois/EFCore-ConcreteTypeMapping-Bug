using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2
{
    public class MyObject
    {
        public int ObjectID { get; set; }
        public string ObjectName { get; set; }
    }

    //[Table("Table1")]
    //public class Table1
    //{
    //    [Key]
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public int? NameId { get; set; }
    //}
    //public class MyContext : DbContext
    //{
    //    public MyContext()
    //    {

    //    }
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    {
    //        optionsBuilder.UseSqlServer("Server=.;Database=Database1;Integrated Security=true;");
    //        base.OnConfiguring(optionsBuilder);
    //    }

    //    public DbSet<Table1> Table1s { get; set; }
    //}
}
