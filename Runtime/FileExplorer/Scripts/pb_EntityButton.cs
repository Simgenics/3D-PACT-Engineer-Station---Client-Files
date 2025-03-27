using LMS.Models;
using TMPro;

namespace FileDialogSystem
{
	public class pb_EntityButton : pb_DialogButton<EntityDto>
	{
		public override void SetText()
		{
			var text = GetComponentInChildren<TextMeshProUGUI>();
			text.text = $"Id: {value.id} " +
				$"| {(value.name != "" ? "Name: " + value.name : "")} " +
				$"| created: {value.creationTime} ";
		}
	}
}
