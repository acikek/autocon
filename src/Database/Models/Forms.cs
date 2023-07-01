using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Forms;

namespace Database;

using FormResponseData = IEnumerable<FormResponseModel>;

public class FormResponseModel
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid? ResponseId { get; set; }
	public string Title { get; set; }
	public string OptionId { get; set; }
	public string Value { get; set; }
	public int Index { get; set; }

	// Foreign Keys
	public Guid AppId { get; set; } // to ApplicationModel

	public FormSectionResponse Revert()
		=> new FormSectionResponse(this.Title, this.OptionId, this.Value);
}

public class ApplicationModel
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid AppId { get; set; } 
	public uint CurrentQuery { get; set; }
	public bool InProgress { get; set; }
	public bool Accepted { get; set; }
	public ulong? MessageId { get; set; }
	
	// Foreign Keys
	public string FormId { get; set; } // to FormTypeModel
	public ulong UserId { get; set; } // to UserModel

	// Navigations
	public virtual List<FormResponseModel> Responses { get; } = new(); // to FormResponseModel

	public static ApplicationModel New(Form form, IUser user)
		=> new ApplicationModel {
			FormId = form.Id,
			UserId = user.Id,
			InProgress = true,
			Accepted = false
		};

	public FormResponseData GetResponseData(List<FormSectionResponse> responses)
	{
		int count = this.Responses.Count();
		return responses
			.AsEnumerable()
			.Select((x, i) => new FormResponseModel {
				Title = x.Title,
				Value = x.Value,
				OptionId = x.Id,
				Index = count + i
			});
	}

	public ICollection<FormSectionResponse> GetFormSectionResponses()
	{
		this.Responses.Sort((x, y) => x.Index.CompareTo(y.Index));
		return this.Responses.Select(x => x.Revert()).ToList();
	}

	public string GetCSVString(bool title)
	{
		var values = this.Responses
			.Select(r => title ? r.Title : r.Value)
			.Prepend(title ? "User ID" : this.UserId.ToString());
		return String.Join(",", values);
	}
}

public class FormTypeModel
{
	[Key]
	public string FormId { get; set; }

	// Navigations
	public virtual List<ApplicationModel> Applications { get; } = new(); // to ApplicationModel

	public ApplicationModel? FindResumable(ulong userId)
		=> this.Applications.Find(x => x.InProgress && x.UserId == userId);

	public Stream CreateExportStream()
	{
		List<string> lines = new();
		lines.Add(this.Applications.First().GetCSVString(true));
		lines.AddRange(this.Applications.Select(a => a.GetCSVString(false)));
		var data = String.Join("\n", lines);

		var stream = new MemoryStream();
		var writer = new StreamWriter(stream);
		writer.Write(data);
		writer.Flush();
		stream.Position = 0;
		
		return stream;
	}
}

public class UserModel
{
	[Key]
	public ulong UserId { get; set; }
	
	// Navigations
	public virtual List<ApplicationModel> Applications { get; } = new(); // to ApplicationModel
}
