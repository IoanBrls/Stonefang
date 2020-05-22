using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowTextOnButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public GameObject textToShow;

	private void Start()
	{
		textToShow.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		textToShow.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		textToShow.SetActive(false);
	}
}
