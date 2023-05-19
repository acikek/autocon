using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Structs;

namespace Database;

public partial class AutoConEntities : DbContext {

	public const string SOURCE = "data/autocon.db";

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connStrBuilder = new SqliteConnectionStringBuilder { DataSource = SOURCE };
		var connection = new SqliteConnection(connStrBuilder.ToString());
		optionsBuilder.UseSqlite(connection);
	}
}