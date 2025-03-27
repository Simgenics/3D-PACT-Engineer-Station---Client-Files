using LMS.Models;
using TMPro;

namespace FileDialogSystem
{
	public class pb_SceneButton : pb_DialogButton<SceneDto>
	{
		public override void SetText()
		{
			var text = GetComponentInChildren<TextMeshProUGUI>();
			text.text = $"Id: {value.id} " +
				$"| {(value.name != "" ?  "Name: " + value.name : "") } " +
				$"| ({(value.versionNumber != "" ? value.versionNumber: "") }) " +
				$"| status: {value.status} " +
				$"| created: {value.creationTime} " +
				$"| facilityId :{value.facilityId}";
		}
	}
}
