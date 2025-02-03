using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
	public enum Status
	{
		Draft,
		[Display(Name="In Progress")]
		InProgress,
		Archived,
		Completed
	}
}
