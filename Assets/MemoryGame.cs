using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MemoryGame : MonoBehaviour
{
    public TMP_Text TimerText;
    public TMP_Text MatchFindedText;
    public TMP_Text GameOverText;
    public TMP_Text EndGameTimerText;
    public TMP_Text EndGameNameText;
    public TMP_Text EndGameScoreText;
    public TMP_Text NameText;
    public TMP_Text CounterText;
    public TMP_Text AlertText;

    public GameObject buttonPrefab; // Referência ao prefab do botão
    public int gridSizeX = 4; // Número de colunas da grade
    public int gridSizeY = 4; // Número de linhas da grade
    public Transform gridTransform;
    public List<GameObject> ButtonsList = new List<GameObject>();

    public List<Sprite> MemorySprites = new List<Sprite>();

    public ElementController card1; //Primeira carta escolhida no turno
    public ElementController card2; //Segunda carta escolhida no turno

    public int ObjectsFlipped = 0;

    private int lines; //linhas da matriz
    private int columns; // colunas da matriz
    int score; //pontuacao do jogador
    public float gameTime; //tempo para fim de jogo
    public float startGameTime; //tempo total do jogo
    int timeToStart; // Tempo antes de começar o jogo
    public bool gameStarted = false; //Verifica se o jogo começou
    bool pause = false; //Verifica se o jogo está pausado

    public static MemoryGame Instance;

    public bool CanClick = true;

    public int Difficulty = 0;

    public List<Button> DifficultyButtons = new List<Button>();

    public GameObject PopupDifficulty;

    public GridLayoutGroup GameGrid;

    public float PlayedTime;


    public int MatchFinded = 0;

    public GameObject EndGameObject;
    public GameObject RankingObject;

    int totalScore; //pontuacao do jogador
    int RoundIndex = 1;
    private List<Sprite> remainingMemorySprites = new List<Sprite>();

    public RankingManager rankingManager;
    public List<GameObject> LevelObjects = new List<GameObject>();

    void Awake()
    {
        NameText.text = PlayerPrefs.GetString("CurrentUser");
        Instance = this;
        MatchFindedText.text = "x" + AddLeadingZeroIfNeeded(MatchFinded);
    }

    public void SendMessage(string message, bool animated)
    {
        CancelInvoke("DisableMessage");

        if (message.Length > 3)
        {
            AlertText.fontSize = 80;
        }

        if (!AlertText.gameObject.activeSelf)
        {
            AlertText.gameObject.SetActive(true);
        }
            AlertText.text = message;
            if (animated)
            {
                AlertText.GetComponent<Animator>().SetTrigger("Animation");
            }
            Invoke("DisableMessage", 2);
    }

    private bool IsAnimationPlaying(string animationName)
    {
        if (AlertText.TryGetComponent<Animator>(out Animator animator))
        {
            // Check if the specified animation is currently playing
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        }

        // If no Animator component is found, assume the animation is not playing
        return false;
    }

    void DisableMessage()
    {
        AlertText.gameObject.SetActive(false);
    }

    public void FlipAll()
    {
        foreach (GameObject cardObject in ButtonsList)
        {
            ElementController card = cardObject.GetComponent<ElementController>();
            StartCoroutine(card.GirarBotao(card.versoImage));
        }
    }

    public void UnflipAll()
    {
        foreach (GameObject cardObject in ButtonsList)
        {
            ElementController card = cardObject.GetComponent<ElementController>();
            card.Unflip();
        }
    }

    public IEnumerator StartCountdown()
    {
        //FlipAll();
        CancelInvoke("DisableMessage");
        AlertText.fontSize = 150;
        AlertText.gameObject.SetActive(false);
        CounterText.gameObject.SetActive(true);
        int timer = 4;
        while (timer > 0)
        {
            timer--;
            CounterText.text = timer.ToString();
            yield return new WaitForSeconds(1);
        }
        //UnflipAll();
        //comenta ate aqui pra tirar o timer
        yield return new WaitForSeconds(0.1f);
        gameStarted = true;
        CounterText.gameObject.SetActive(false);
    }
    public string AddLeadingZeroIfNeeded(int inputInt)
    {
        string inputString = inputInt.ToString();
        if (inputString.Length == 1)
        {
            return "0" + inputString;
        }
        else
        {
            return inputString;
        }
    }
    public void SetGameTime(int time)
    {
        gameTime = time;
    }
    public string ConvertToTimeFormat(int value)
    {
        int minutes = value / 60;
        int seconds = value % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void InitConfig()
    {
        //Difficulty = 0;
        //PopupDifficulty.SetActive(false);

        CreateGrid();

    }

    void Update()
    {
        if (gameStarted && !pause)
        {
            gameTime -= Time.deltaTime;
            PlayedTime += Time.deltaTime;
            //GUIHandleMemory.instance.UpdateGameTime(PlayedTime);

            //if (gameTime <= 0)
            //{
            //    //Caso o jogador não tenha pegado todas as cartas, fim de jogo
            //    GameOver();
            //}

            TimerText.text = ConvertToTimeFormat((int)PlayedTime);
        }
    }

    void GameOver()
    {
        int scoreTotal = (lines * columns) / 2;
        Debug.Log("encontrou apenas " + scoreTotal + "  pares");

        gameStarted = false;
        gameTime = 0;

        string playerName = PlayerPrefs.GetString("CurrentUser");
        int playerScore = (int)PlayedTime; // Você pode ajustar isso para calcular a pontuação real do jogador.

        rankingManager.AddPlayerScore(playerName, playerScore);

        //EndGameNameText.text = playerName;
        //EndGameScoreText.text = playerScore.ToString();
        RankingObject.SetActive(true);
    }


    private void Start()
    {
        InitConfig();
    }

    public void InitGame()
    {
        StartCoroutine(StartCountdown());
        //gameStarted = true;
    }

    public void GameStart() //Use para começar/recomeçar o jogo
    {
        //allCardsInGame = GameObject.FindGameObjectsWithTag("Card");      
        //DisableBarrier();
        score = 0;
        //GUIHandle.instance.UpdateScore(score);        
        gameTime = startGameTime + 1;
        pause = false;
        gameStarted = false;

        InitGame();
    }

    public void ResetCanClick()
    {
        CanClick = false;
        StartCoroutine(ActiveCanClick());
    }

    private IEnumerator ActiveCanClick()
    {
        yield return new WaitForSeconds(0.3f);
        CanClick = true;
    }

    public void AddFlipped(ElementController pickedElement)
    {
        ObjectsFlipped++;
        if (ObjectsFlipped == 1)
        {
            card1 = pickedElement;
            card1.Flipped = true;
        }
        else if (ObjectsFlipped == 2)
        {
            card2 = pickedElement;
            card2.Flipped = true;
            ObjectsFlipped = 0;
            CheckMatch();
        }
    }

    void CheckMatch() //Use para verificar se as cartas deram match
    {
        //Verificar se código das cartas são iguais

        //Reseta cartas
        if (card1.Id == card2.Id && card1.versoImage.name == card2.versoImage.name)
        {
            //Debug.Log("Par encontrado " + card2.versoImage.name);
            //gameTime += 5f;
            score++;
            if (Difficulty > 0 && card1.versoImage.name == "Frame 36")
            {
                PlayedTime -= 15;
                CheckScore(true);
            } else
            {
                CheckScore(false);
            }
            
            card1.Finded = true;
            card2.Finded = true;
            ResetCards();
            MatchFinded++;
            MatchFindedText.text = "x" + AddLeadingZeroIfNeeded(MatchFinded);

        }
        else
        {
            StartCoroutine(UnflipCards());
        }

    }

    IEnumerator UnflipCards()
    {
        yield return new WaitForSeconds(1);
        card1.Reset();
        card2.Reset();

        ResetCards();
    }

    IEnumerator WaitToEnd()
    {
        yield return new WaitForSeconds(1);
        EndGameObject.SetActive(true);

    }
    void CheckScore(bool bonus) //Verifica se o jogador conseguiu a pontuacao para vencer
    {
        int scoreTotal = (gridSizeX * gridSizeY) / 2;
        if (score >= scoreTotal)
        {
            LevelObjects[RoundIndex - 1].SetActive(false);
            if (RoundIndex <= 2)
            {
                LevelObjects[RoundIndex].SetActive(true);
            }
            RoundIndex++;
            SendMessage( "Level " + RoundIndex, true);
            foreach (GameObject bt in ButtonsList)
            {
                Destroy(bt);
            }
            ButtonsList.Clear();
            totalScore += score;
            score = 0;

            CreateGrid();
        } else
        {
            AlertText.fontSize = 150;
            if (bonus)
            {
                SendMessage("Bonus + 00:15s", true);
            } else
            {
                SendMessage("+1", true);
            }
        }
    }

    string GetKeyByLevel()
    {
        if (Difficulty == 0)
        {
            return "score_easy";
        }
        if (Difficulty == 1)
        {
            return "score_medium";
        }
        if (Difficulty == 2)
        {
            return "score_hard";
        }

        return "score_expert";
    }

    int GetSecconds()
    {
        return (int)PlayedTime;
    }

    string GetLevel()
    {
        if (Difficulty == 0)
        {
            return "Fácil";
        }
        if (Difficulty == 1)
        {
            return "Médio";
        }
        if (Difficulty == 2)
        {
            return "Difícil";
        }

        return "Expert";
    }

    void ResetCards() //Use para resetar as cartas
    {
        card1.Flipped = false;
        card2.Flipped = false;
        card1 = null;
        card2 = null;
    }

    private void CreateGrid()
    {
        if (Difficulty == -1)
        {
            Difficulty = 0;
        }
        else if (Difficulty == 0)
        {
            Difficulty = 1;
        }
        else if (Difficulty == 1)
        {
            Difficulty = 2;
        }
        else {
            GameOver();
        }

        if (Difficulty == 0)
        {
            GameGrid.spacing = new Vector2(45, GameGrid.spacing.y);
            gridSizeX = 2;
            gridSizeY = 4;
        }
        else if (Difficulty == 1)
        {
            GameGrid.spacing = new Vector2(40, GameGrid.spacing.y);
            gridSizeX = 4;
            gridSizeY = 4;
        }
        else if (Difficulty == 2)
        {
            GameGrid.spacing = new Vector2(25, 25);
            GameGrid.cellSize = new Vector2(183, 183);
            gridSizeX = 5;
            gridSizeY = 6;
        }
        else
        {
            GameGrid.spacing = new Vector2(115, 112);
            gridSizeX = 5;
            gridSizeY = 6;
        }

        // Loop para criar cada botão
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                // Cria uma nova instância do prefab do botão
                GameObject button = Instantiate(buttonPrefab);

                // Posiciona o botão na grade
                //button.transform.localPosition = new Vector3(x, y, 0);
                ButtonsList.Add(button);
            }
        }
        SetMemorySprites();
        MemoryGame myClassInstance = GetComponent<MemoryGame>();
        MemoryGame.ShuffleListStatic(ButtonsList, myClassInstance);

        int i = 0;
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {

                // Posiciona o botão na grade
                ButtonsList[i].transform.SetParent(gridTransform);
                //if (Application.isEditor)
                //{
                //    ButtonsList[i].transform.localScale = Vector3.one;
                //}

                ButtonsList[i].transform.localPosition = new Vector3(x, y, 0);
                i++;
            }
        }

    }



    public void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static void ShuffleListStatic<T>(List<T> list, MemoryGame myClassInstance)
    {
        myClassInstance.ShuffleList(list);
    }

    public void GoToQuiz()
    {
        SceneManager.LoadScene("Quiz");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene("JogoDaMemoria");
    }
    public void SetMemorySprites()
    {
        List<Sprite> availableSprites = new List<Sprite>(MemorySprites);

        int id = 0;
        int duplicatedId = Random.Range(0, availableSprites.Count - 1);
        foreach (GameObject o in ButtonsList)
        {
            id++;
            ElementController element = o.GetComponent<ElementController>();
            element.Id = duplicatedId + "memory";
            element.versoImage = availableSprites[duplicatedId];
            if (id == 2)
            {
                id = 0;
                availableSprites.RemoveAt(duplicatedId);
                duplicatedId = Random.Range(0, availableSprites.Count - 1);
            }
        }

        // Após criar os elementos de memória, a lista availableSprites conterá os sprites não utilizados
        remainingMemorySprites = availableSprites;
    }
}