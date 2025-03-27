using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpawnableObjectEntry : MonoBehaviour
{
    [SerializeField]
	private Image icon;
	[SerializeField]
	private TextMeshProUGUI spawnableName;
	[SerializeField]
	private Button button;

	private SpawnableObject spawnable;
	private Action<SpawnableObject> onClick;

	private void Awake()
	{
		button.onClick.AddListener(OnClick);
	}

	public void Setup(SpawnableObject spawnable, Action<SpawnableObject> onClick)
	{
		icon.sprite = spawnable.Icon;
		spawnableName.text = spawnable.Name;
		this.spawnable = spawnable;
		this.onClick = onClick;
	}

	void OnClick()
	{
		onClick?.Invoke(spawnable);
	}
}
