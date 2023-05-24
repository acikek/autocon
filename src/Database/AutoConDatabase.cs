using System.ComponentModel.DataAnnotations;
using Forms;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class FormResponseModel
{
	[Key]
	public string Title { get; set; }
	public string Value { get; set; }

	public ulong? UserId { get; set; }
	public virtual ApplicationModel? Application { get; set; }

	public FormSectionResponse Revert()
		=> new FormSectionResponse(this.Title, this.Value);
}

public class ApplicationModel
{
	[Key]
	public ulong UserId { get; set; }
	public string FormId { get; set; }
	public uint CurrentQuery { get; set; }
	
	public virtual ICollection<FormResponseModel> Responses { get; set; }

	public FormResponseModel CreateResponse(FormSectionResponse response)
		=> new FormResponseModel {
			Title = response.Title,
			Value = response.Value,
			UserId = this.UserId,
			Application = this
		};

	public static ApplicationModel Empty(ulong userId, string formId)
		=> new ApplicationModel {
			UserId = userId,
			FormId = formId,
			CurrentQuery = 0
		};
}

public partial class AutoConDatabase : DbContext 
{

	public const string SOURCE = "data/autocon.db";

	public DbSet<ApplicationModel> Applications { get; set; }
	public DbSet<FormResponseModel> Responses { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connStrBuilder = new SqliteConnectionStringBuilder { DataSource = SOURCE };
		var connection = new SqliteConnection(connStrBuilder.ToString());
		optionsBuilder.UseSqlite(connection);
	}
}
