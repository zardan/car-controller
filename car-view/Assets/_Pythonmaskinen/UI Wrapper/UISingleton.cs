﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PM {

	public class UISingleton : MonoBehaviour {

		public static UISingleton instance;

		public Transform ideRoot { get { return transform.parent; } }

		[Header("PM")]
		public HelloCompiler compiler;
		public IDETextField textField;
		public CodeWalker walker;
		public GlobalSpeed speed;
		public Manus.ManusPlayer manusPlayer;
		[Header("UI")]
		public IDERowsLimit rowsLimit;
		public LevelHints levelHints;
		public RectTransform gameCameraRect;
		public SmartButtonController smartButtons;
		public WinScreen winScreen;
		public ProgressBar levelbar;
		public GameObject uiTooltipPrefab;
		public GameObject varTooltipPrefab;
		public RectTransform tooltipParent;
		public IDEPrintBubble printBubble;
		public IDEManusBubble manusBubble;
		public IDEGuideBubble guideBubble;
		public IDETaskDescription taskDescription;
		public CanvasGroup uiCanvasGroup;
		[Header("Misc")]
		public Camera uiCamera;
		public Camera popupCamera;

		[HideInInspector]
		public List<ManusSelectable> manusSelectables = new List<ManusSelectable>();
		
		public static string gameToken = null;

		[Serializable]
		public struct ManusSelectable {
			public Selectable selectable;
			public List<string> names;
		}

		private void Awake() {
			instance = this;
		}

		/// <summary>
		/// This function is made for finding objects. Similar to <seealso cref="UnityEngine.Object.FindObjectsOfType{T}"/> but also works for interfaces.
		/// <para>The catch is though that it can only search amoung classes that inherit from <see cref="UnityEngine.Object"/></para>
		/// </summary>
		public static T[] FindInterfaces<T>() where T : class {
			var list = new List<UnityEngine.Object>(FindObjectsOfType<UnityEngine.Object>()).ConvertAll(o => o as T);
			list.RemoveAll(o => o == null);
			return list.ToArray();
		}

	}

}