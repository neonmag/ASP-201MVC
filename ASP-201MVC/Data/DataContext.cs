using Microsoft.EntityFrameworkCore;

namespace ASP_201MVC.Data
{
	public class DataContext : DbContext
	{
		public DbSet<Entity.User> Users { get; set; } 
		public DbSet<Entity.EmailConfirmToken> EmailConfirmToken { get; set; }
        public DataContext(DbContextOptions options) : base(options)
		{

		}
	}
}
