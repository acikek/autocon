using System.ComponentModel.DataAnnotations;
using Forms;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class FormDataResponse
{
	[Key]
	public string Title { get; set; }
	public string Value { get; set; }
}

public class FormUserData
{
	[Key]
	public long UserId { get; set; }
	public string FormId { get; set; }
	public uint CurrentQuery { get; set; }
	public List<FormDataResponse> Responses { get; set; }
}

public partial class AutoConEntities : DbContext 
{

	public const string SOURCE = "data/autocon.db";

	public DbSet<FormUserData> FormData { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connStrBuilder = new SqliteConnectionStringBuilder { DataSource = SOURCE };
		var connection = new SqliteConnection(connStrBuilder.ToString());
		optionsBuilder.UseSqlite(connection);
	}
}
