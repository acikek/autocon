using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Database;

public partial class AutoConDatabase : DbContext 
{

	public const string SOURCE = "data/autocon.db";

	public DbSet<UserModel> Users { get; set; }
	public DbSet<FormTypeModel> Forms { get; set; }
	public DbSet<ApplicationModel> Applications { get; set; }
	public DbSet<FormResponseModel> Responses { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connStrBuilder = new SqliteConnectionStringBuilder { DataSource = SOURCE };
		var connection = new SqliteConnection(connStrBuilder.ToString());
		optionsBuilder
			.UseSqlite(connection)
			.UseLazyLoadingProxies();
	}

	private async Task<T> AddIfNotPresent<T, P>(DbSet<T> set, P context, Func<P, T> mapper) where T: class 
	{
		var existing = await set.FindAsync(context);
		if (existing is not null)
			return existing;
		var value = mapper(context);
		await set.AddAsync(value);
		await this.SaveChangesAsync();
		return value;
	}

	public async Task<UserModel> AddUserIfNotPresent(ulong userId)
		=> await this.AddIfNotPresent(this.Users, userId, id => new UserModel { UserId = id });

	public async Task<FormTypeModel> AddFormIfNotPresent(string formId)
		=> await this.AddIfNotPresent(this.Forms, formId, id => new FormTypeModel { FormId = id });

	public FormTypeModel? FindForm(string formId)
		=> this.Forms
			.Where(x => x.FormId == formId)
			.Include(x => x.Applications)
			.ThenInclude(x => x.Responses)
			.FirstOrDefault();
}
