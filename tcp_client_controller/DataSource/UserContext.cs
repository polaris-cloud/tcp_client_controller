using Microsoft.EntityFrameworkCore;

namespace testMailValidate.data
{
    public class UserContext<T> : DbContext where T : class
    {
        //reflect - map  : Entity Discovery
        public DbSet<T> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=orders.db");


    }
}
