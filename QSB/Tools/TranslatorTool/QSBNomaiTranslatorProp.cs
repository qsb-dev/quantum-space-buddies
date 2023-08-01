using QSB.Utility;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Tools.TranslatorTool;

[UsedInUnityProject]
public class QSBNomaiTranslatorProp : MonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;
	private static int s_propID_EmissionColor;

	public GameObject TranslatorProp;
	public MeshRenderer _leftPageArrowRenderer;
	public MeshRenderer _rightPageArrowRenderer;
	public Font _defaultPropFont;
	public Font _defaultPropFontDynamic;
	public float _defaultFontSpacing = 0.7f;
	public Text _textField;
	public Text _pageNumberTextField;

	private Font _fontInUse;
	private Font _dynamicFontInUse;
	private float _fontSpacingInUse;
	private Color _baseEmissionColor;
	private float _totalTranslateTime = 0.2f;
	private float _totalAlreadyTranslatedTime;
	private float _translationTimeElapsed;
	private float _perWordTranslationTime;
	private float _fullTextTranslationTime;
	private float _equipTime;
	private string _lastTextNodeToDisplay;
	private string _textNodeToDisplay;
	private string _translatedText;
	private TranslatorWordLengthComparer _lengthComparer;
	private List<TranslatorWord> _listDisplayWordsByLength;
	private List<TranslatorWord> _listDisplayWords;
	private int _numTranslatedWords;
	private Canvas _canvas;
	private ScrollRect _scrollRect;
	private StringBuilder _strBuilder;
	private StringBuilder _pageNumberStrBuilder;
	private NomaiText _nomaiTextComponent;
	private int _currentTextID;
	private TranslatorTargetBeam _targetBeam;
	private QSBTranslatorScanBeam[] _scanBeams;
	private PlayerAudioController _audioController;
	private ThrusterModel _jetpackModel;
	private bool _isTooCloseToTarget;
	private bool _isTargetingGhostText;
	private bool _isTranslating;
	private bool _hasUsedTranslator;
	private bool _isTimeFrozen;
	private QSBNomaiTranslator _nomaiTranslator;

	private void Awake()
	{
		_nomaiTranslator = base.GetComponentInParent<QSBNomaiTranslator>();
		this._listDisplayWords = new List<TranslatorWord>(1024);
		this._listDisplayWordsByLength = new List<TranslatorWord>(1024);
		this._lengthComparer = new TranslatorWordLengthComparer();
		this._textNodeToDisplay = null;
		this._strBuilder = new StringBuilder();
		this._pageNumberStrBuilder = new StringBuilder();
		this._canvas = base.transform.GetComponentInChildren<Canvas>();
		this._scrollRect = base.transform.GetComponentInChildren<ScrollRect>();

		_targetBeam = transform.GetComponentInChildren<TranslatorTargetBeam>();

		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
		}

		if (_rightPageArrowRenderer != null)
		{
			var sharedMaterial = _rightPageArrowRenderer.sharedMaterial;
			_baseEmissionColor = sharedMaterial.GetColor(NomaiTranslatorProp.s_propID_EmissionColor);
		}

		TurnOffArrowEmission();
		this._canvas.enabled = false;
		_scanBeams = transform.GetComponentsInChildren<QSBTranslatorScanBeam>();
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].enabled = false;
		}

		TranslatorProp.SetActive(false);
	}

	private void Start()
	{
		this.InitializeFont();
		TextTranslation.Get().OnLanguageChanged += this.InitializeFont;
		enabled = false;
	}

	private void OnDestroy()
	{
		TextTranslation.Get().OnLanguageChanged -= this.InitializeFont;
	}

	private void InitializeFont()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			this._fontInUse = this._defaultPropFont;
			this._dynamicFontInUse = this._defaultPropFontDynamic;
			this._fontSpacingInUse = this._defaultFontSpacing;
		}
		else
		{
			this._fontInUse = TextTranslation.GetFont(false);
			this._dynamicFontInUse = TextTranslation.GetFont(true);
			this._fontSpacingInUse = TextTranslation.GetDefaultFontSpacing();
		}
		this._textField.font = this._fontInUse;
		this._textField.lineSpacing = this._fontSpacingInUse;
	}

	private void TurnOffArrowEmission()
	{
		if (_leftPageArrowRenderer != null)
		{
			SetMaterialEmissionEnabled(_leftPageArrowRenderer, false);
		}

		if (_rightPageArrowRenderer != null)
		{
			SetMaterialEmissionEnabled(_rightPageArrowRenderer, false);
		}
	}

	private void SetMaterialEmissionEnabled(MeshRenderer emissiveRenderer, bool emissionEnabled)
	{
		if (emissionEnabled)
		{
			s_matPropBlock.SetColor(s_propID_EmissionColor, _baseEmissionColor * 1f);
			emissiveRenderer.SetPropertyBlock(s_matPropBlock);
			return;
		}

		s_matPropBlock.SetColor(s_propID_EmissionColor, _baseEmissionColor * 0f);
		emissiveRenderer.SetPropertyBlock(s_matPropBlock);
	}

	public void OnEquipTool()
	{
		enabled = true;
		_canvas.enabled = true;
		if (_targetBeam)
		{
			_targetBeam.Activate();
		}

		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].enabled = true;
		}

		TranslatorProp.SetActive(true);
	}

	public void OnUnequipTool()
	{
		enabled = false;
		StopTranslating();
		TurnOffArrowEmission();
	}

	public void OnFinishUnequipAnimation()
	{
		if (_targetBeam)
		{
			_targetBeam.Deactivate();
		}

		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].enabled = false;
		}

		_canvas.enabled = false;
		TranslatorProp.SetActive(false);
	}

	public void SetTooCloseToTarget(bool value)
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetTooCloseToTarget(value);
		}
	}

	public void SetNomaiTextLine(NomaiTextLine line)
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(line);
			_scanBeams[i].SetNomaiComputerRing(null);
			_scanBeams[i].SetNomaiVesselComputerRing(null);
		}
	}

	public void ClearNomaiTextLine()
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(null);
		}
	}

	public void SetNomaiComputerRing(NomaiComputerRing ring)
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(null);
			_scanBeams[i].SetNomaiComputerRing(ring);
			_scanBeams[i].SetNomaiVesselComputerRing(null);
		}
	}

	public void ClearNomaiComputerRing()
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiComputerRing(null);
		}
	}

	public void SetNomaiVesselComputerRing(NomaiVesselComputerRing ring)
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(null);
			_scanBeams[i].SetNomaiComputerRing(null);
			_scanBeams[i].SetNomaiVesselComputerRing(ring);
		}
	}

	public void ClearNomaiVesselComputerRing()
	{
		for (var i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiVesselComputerRing(null);
		}
	}

	public void SetNomaiText(NomaiText text, int textID)
	{
		if (_nomaiTextComponent != text || _currentTextID != textID)
		{
			_nomaiTextComponent = text;
			_currentTextID = textID;
			_textNodeToDisplay = _nomaiTextComponent.GetTextNode(_currentTextID);
			_scrollRect.verticalNormalizedPosition = 1f;
			TurnOffArrowEmission();
		}
	}

	public void SetNomaiText(NomaiText text)
	{
		if (_nomaiTextComponent != text)
		{
			_nomaiTextComponent = text;
			_currentTextID = -1;
			_textNodeToDisplay = _nomaiTextComponent.GetTextNode(_currentTextID);
			_scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	public void ClearNomaiText()
	{
		_nomaiTextComponent = null;
		_textNodeToDisplay = null;
	}

	public void SetTargetingGhostText(bool isTargetingGhostText)
	{
		_isTargetingGhostText = isTargetingGhostText;
	}

	private void StopTranslating()
	{
		if (_isTranslating)
		{
			_isTranslating = false;
		}
	}

	public void SetScroll(float scrollPos)
		=> _scrollRect.verticalNormalizedPosition = scrollPos;

	public void UpdateTranslating(bool translating)
	{
		if (translating)
		{
			_isTranslating = true;
			_audioController.PlayTranslateAudio();
		}
		else
		{
			StopTranslating();
		}
	}

	protected virtual void Update()
	{
		var textIsTranslated = _nomaiTextComponent != null && _nomaiTextComponent.IsTranslated(_currentTextID);
		var flag2 = !_isTargetingGhostText && !textIsTranslated && _textNodeToDisplay != null;
		if (_textNodeToDisplay != null && _textNodeToDisplay != _lastTextNodeToDisplay)
		{
			StopTranslating();
			SwitchTextNode(_textNodeToDisplay);
		}

		_lastTextNodeToDisplay = _textNodeToDisplay;
		if (_isTargetingGhostText)
		{
			_textField.text = UITextLibrary.GetString(_isTooCloseToTarget ? UITextType.TranslatorTooCloseWarning : UITextType.TranslatorUntranslatableWarning);
		}
		else if (_textNodeToDisplay == null)
		{
			_textField.text = "";
			_pageNumberTextField.text = "";
			StopTranslating();
		}
		else if (_isTooCloseToTarget)
		{
			_textField.text = UITextLibrary.GetString(UITextType.TranslatorTooCloseWarning);
		}
		else
		{
			DisplayTextNode();
		}

		_textField.rectTransform.sizeDelta = new Vector2(_textField.rectTransform.sizeDelta.x, _textField.preferredHeight);
		_scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, _textField.preferredHeight);

		if (_pageNumberTextField != null && _pageNumberTextField.text != "")
		{
			_pageNumberTextField.text = "";
		}
	}

	protected void DisplayTextNode()
	{
		if (!_nomaiTextComponent.IsTranslated(_currentTextID))
		{
			if (_nomaiTranslator.Player.IsTranslating)
			{
				_translationTimeElapsed += Time.deltaTime;
			}
		}
		else
		{
			_translationTimeElapsed += Time.deltaTime;
		}

		var flag = false;
		string text;
		if (_translationTimeElapsed == 0f && !_nomaiTextComponent.IsTranslated(_currentTextID))
		{
			text = UITextLibrary.GetString(UITextType.TranslatorUntranslatedWritingWarning);
		}
		else
		{
			if (_translationTimeElapsed > (float)(_numTranslatedWords + 1) * _perWordTranslationTime && _numTranslatedWords < _listDisplayWords.Count)
			{
				_listDisplayWordsByLength[_numTranslatedWords].BeginTranslation(_perWordTranslationTime);
				_numTranslatedWords++;
			}

			for (var i = 0; i < _listDisplayWords.Count; i++)
			{
				_listDisplayWordsByLength[i].UpdateDisplayText(Time.deltaTime);
			}

			_strBuilder.Length = 0;
			var flag2 = true;
			for (var j = 0; j < _listDisplayWords.Count; j++)
			{
				var translatorWord = _listDisplayWords[j];
				_strBuilder.Append(translatorWord.DisplayText);
				if (!translatorWord.IsTranslated())
				{
					flag2 = false;
				}
			}

			text = _strBuilder.ToString();
			if (flag2)
			{
				StopTranslating();
				_nomaiTextComponent.SetAsTranslated(_currentTextID);
				if (text.Contains("</i>") || text.Contains("</b>") || text.Contains("</u>") || text.Contains("</size>"))
				{
					flag = true;
				}
			}
		}

		if (flag && _textField.font != _dynamicFontInUse)
		{
			_textField.font = _dynamicFontInUse;
		}
		else if (!flag && _textField.font != _fontInUse)
		{
			_textField.font = _fontInUse;
		}

		_textField.text = text;
	}

	private void SwitchTextNode(string textNode)
	{
		_translationTimeElapsed = 0f;
		_fullTextTranslationTime = _nomaiTextComponent.IsTranslated(_currentTextID) ? _totalAlreadyTranslatedTime : _totalTranslateTime;
		_translatedText = CleanupText(textNode);
		_listDisplayWords.Clear();
		_listDisplayWordsByLength.Clear();
		var num = 0;
		while (num >= 0 && num < _translatedText.Length)
		{
			var num2 = _translatedText.IndexOf(' ', num);
			if (num2 == -1)
			{
				num2 = _translatedText.Length - 1;
			}

			var item = new TranslatorWord(_translatedText.Substring(num, num2 + 1 - num), num, num2 + 1, _nomaiTextComponent.IsTranslated(_currentTextID), _fullTextTranslationTime);
			_listDisplayWords.Add(item);
			_listDisplayWordsByLength.Add(item);
			num = num2 + 1;
		}

		_listDisplayWordsByLength.Sort(_lengthComparer);
		_numTranslatedWords = 0;
		_perWordTranslationTime = _fullTextTranslationTime / (float)_listDisplayWords.Count;
	}

	private string CleanupText(string text)
	{
		return text.Trim();
	}
}