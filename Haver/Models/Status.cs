using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    //salesorder status list
    public enum Status
	{
		Draft,
		[Display(Name="In Progress")]
		InProgress,
		Archived,
		Completed
	}
}
