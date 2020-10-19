using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {
    public TextAsset QuestionsFile;
    public TextMeshProUGUI QuestionText;
    public List<TextMeshProUGUI> ButtonTexts;
    public List<Button> AnswerButtons;
    public Image BackgroundImage;
    private Statement[] AllQuestions;
    public int currentQuestionIndex = 0;
    private bool currentTextAlreadyFilled;
    private AudioManager audioManager;
    private Statement CurrentQuestion => AllQuestions.First(x => x.Id == currentQuestionIndex);
    //A dictionary to store flags as they come
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private void Awake() {
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play("AmbianceMusic");
    }
    // Start is called before the first frame update
    void Start() {        
        Statements fileData = JsonUtility.FromJson<Statements>(QuestionsFile.text);
        AllQuestions = fileData.Questions;        
        HideButtons();
    }

    // Update is called once per frame
    void Update() {
        FillUITexts();
    }

    public void AnswerQuestion(int answerId) {
        audioManager.Play("buttonClick");
        if (CurrentQuestion.Failure) {
            Replay();
            return;
        }

        if (CurrentQuestion.Success) {
            SceneHelper.GoToMainScene();
            return;
        }
        
        ChangeQuestion(CurrentQuestion.Answers[answerId]);
    }

    public void Replay() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ChangeQuestion(Answer answer) {
        currentTextAlreadyFilled = false;
        HideButtons();
        //test for empty dictionary
        if (answer.ChangeFlag != null)
        {
            print("A");
            foreach(KeyValuePair<string,bool> flag in answer.ChangeFlag)
            {
                //check if flag was added to flags dictionary and chane it, else add it
                if (flags.ContainsKey(flag.Key))
                {
                    print("B");
                    flags[flag.Key] = flag.Value;
                }
                else
                {
                    print("C");
                    flags.Add(flag.Key, flag.Value);
                }
            }
        }
        currentQuestionIndex = answer.NextQuestionId;
    }

    private void HideButtons(){
        for (var i = 0; i < AnswerButtons.Count; i++) {
            AnswerButtons[i].gameObject.SetActive(false);
        }
    }

    private void FillUITexts() {
        if (!currentTextAlreadyFilled)
        {
            Statement CurrQuestion = CurrentQuestion;
            StopAllCoroutines();
            StartCoroutine(TypeSentence(CurrentQuestion.Question));
            for (var i = 0; i < CurrentQuestion.Answers.Length; i++) {
                if (CheckRequirment(CurrentQuestion.Answers[i]))
                {
                    ButtonTexts[i].text = CurrentQuestion.Answers[i].Label;
                    AnswerButtons[i].gameObject.SetActive(true);
                }
            }

            if (!string.IsNullOrWhiteSpace(CurrentQuestion.Image))
            {
                Sprite background = Resources.Load<Sprite>(CurrentQuestion.Image);
                BackgroundImage.sprite = background;
            }
            else
            {
                Sprite background = Resources.Load<Sprite>("Sprites/Backgrounds/black");
                BackgroundImage.sprite = background;
            }

            currentTextAlreadyFilled = true;
        }
    }

    private bool CheckRequirment(Answer answer)
    {
        if (answer.RequireFlag != null)
        {
            print(1);
            //check if key is present in dictionary
            foreach(KeyValuePair<string,bool> flag in answer.RequireFlag)
            {
                if (flags.ContainsKey(flag.Key))
                {
                    print(2);
                    //check if flag value matches required value
                    if (flags[flag.Key] != flag.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    print(3);
                    if (flag.Value == true)
                    {
                        return false;
                    }
                }
            }
        }
        if (answer.Label == "Feed the cat")
        {
            print(JsonUtility.ToJson(answer, true));
        }
        return true;
    }

    IEnumerator TypeSentence(string sentence) {
        QuestionText.text = string.Empty;
        foreach(char letter in sentence.ToCharArray()){                        
            QuestionText.text += letter;
            yield return null;
        }
    }
}