using System;
using LMS.Models;
using UnityEngine.UI;

namespace FileDialogSystem
{
	public abstract class pb_DialogButton<T> : Button where T : EntityDto
	{
		public T value;

		public virtual void SetDelegateAndValue(Action<T> del, T buttonValue) 
		{
			value = buttonValue;
			onClick.AddListener(() => del(buttonValue));
			SetText();
		}

		public abstract void SetText();
	}
}
