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
}

public class FormTypeModel
{
	[Key]
	public string FormId { get; set; }

	// Navigations
	public virtual List<ApplicationModel> Applications { get; } = new(); // to ApplicationModel

	public ApplicationModel? FindResumable(ulong userId)
		=> this.Applications.Find(x => x.InProgress && x.UserId == userId);
}

public class UserModel
{
	[Key]
	public ulong UserId { get; set; }
	
	// Navigations
	public virtual List<ApplicationModel> Applications { get; } = new(); // to ApplicationModel
}
