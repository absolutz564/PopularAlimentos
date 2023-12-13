using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

public class MenuScript : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField phoneInputField;
    public TMP_InputField emailInputField;
    public Button startButton; // Referência ao botão que leva para a nova cena
    private string playerNameKey = "CurrentUser";

    private User userData;

    private const string url = "https://popular-alimentos.dilisgs.com.br/users.json"; // URL do JSON
    private void Start()
    {
        startButton.interactable = false;

        //// Verifica se já existe um nome de jogador salvo em PlayerPrefs
        //if (PlayerPrefs.HasKey(playerNameKey))
        //{
        //    string savedPlayerName = PlayerPrefs.GetString(playerNameKey);
        //    nameInputField.text = savedPlayerName;
        //    startButton.interactable = true;
        //}

        // Adicione um listener de evento para o campo de entrada
        nameInputField.onValueChanged.AddListener(delegate { UpdateStartButton(); });
        emailInputField.onValueChanged.AddListener(delegate { UpdateStartButton(); });
        phoneInputField.onValueChanged.AddListener(delegate { UpdateStartButton(); });

        phoneInputField.onValueChanged.AddListener(delegate { FormatPhoneNumber(); });
    }


    [System.Serializable]
    public class User
    {
        public string nome;
        public string email;
        public string telefone;
    }
    public class UserListContainer
    {
        public List<User> users;
    }

    public void AddUser(string nome, string email, string telefone)
    {
        StartCoroutine(AddUserCoroutine(nome, email, telefone));
    }
    private const string serverURL = "https://popular-alimentos.dilisgs.com.br/update_data.php"; // URL do JSON

    IEnumerator AddUserCoroutine(string nome, string email, string telefone)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro ao carregar dados do servidor: " + www.error);
            yield break;
        }
        //REMOVER
        //SceneManager.LoadScene(1);

        UserListContainer userListContainer = JsonUtility.FromJson<UserListContainer>(www.downloadHandler.text);

        if (userListContainer == null)
        {
            userListContainer = new UserListContainer();
            userListContainer.users = new List<User>();
        }

        User newUser = new User
        {
            nome = nome,
            email = email,
            telefone = telefone
        };

        userListContainer.users.Add(newUser);

        // Convertendo a lista de usuários diretamente para JSON
        string updatedJson = JsonUtility.ToJson(userListContainer);

        using (UnityWebRequest uploadRequest = UnityWebRequest.Put(serverURL, updatedJson))
        {
            uploadRequest.method = "PUT";
            uploadRequest.SetRequestHeader("Content-Type", "application/json");

            yield return uploadRequest.SendWebRequest();

            if (uploadRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao enviar dados atualizados para o servidor: " + uploadRequest.error);
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.Log("JSON atualizado com sucesso!");
                SceneManager.LoadScene(1);
            }
        }
    }





    private string previousText = "";
    private void FormatPhoneNumber()
    {
        int comprimentoAnterior = phoneInputField.text.Length;
        string svalue = phoneInputField.text;
        string snum = System.Text.RegularExpressions.Regex.Replace(svalue, @"[^0-9]", string.Empty) ?? string.Empty;

        // Verifica se o campo está vazio
        if (string.IsNullOrEmpty(snum))
        {
            // Se estiver vazio, limpa o campo
            phoneInputField.text = "";
            return;
        }

        // Agora, ajustamos para o formato (00) 00000-0000
        if (snum.Length > 11) snum = snum.Substring(0, 11);

        if (snum.Length < 2)
        {
            // Apenas dois dígitos, apenas parênteses
            snum = $"({snum})";
        }
        else if (snum.Length <= 7)
        {
            // Formato (00) 0000
            snum = $"({snum.Substring(0, 2)}) {snum.Substring(2)}";
        }
        else if (snum.Length <= 11)
        {
            // Formato (00) 00000-0000
            snum = $"({snum.Substring(0, 2)}) {snum.Substring(2, 5)}-{snum.Substring(7)}";
        }

        phoneInputField.text = snum;

        if (phoneInputField.text.Length > 0 && phoneInputField.text[phoneInputField.text.Length - 1] == ' ')
        {


            if (previousText.Length > snum.Length)
            {
                phoneInputField.text = "";
            }

        }

        // Move o cursor para o fim do texto se o comprimento mudou
        if (snum.Length != svalue.Length)
        {
            phoneInputField.ForceLabelUpdate();
            phoneInputField.MoveTextEnd(false);
        }
        else if (svalue.Length > snum.Length)
        {
            // Se estamos apagando, ajusta o cursor para a posição correta
            int caretPosition = phoneInputField.caretPosition;
            if (caretPosition > 0 && (snum.Length == 7 || snum.Length == 2))
            {

                // Se estamos apagando a posição onde há um espaço ou um parêntese, ajusta a posição do cursor
                caretPosition--;
            }

            phoneInputField.caretPosition = caretPosition;
        }
        // Atualiza o texto anterior
        previousText = snum;
    }


    private void UpdateStartButton()
    {
        // Habilita o botão de início apenas se o campo de entrada não estiver vazio
        startButton.interactable = (!string.IsNullOrEmpty(nameInputField.text) && !string.IsNullOrEmpty(emailInputField.text) && !string.IsNullOrEmpty(phoneInputField.text));
    }

    public void SavePlayerName()
    {
        string playerName = nameInputField.text;
        string playerEmail = emailInputField.text;
        string playerPhone = phoneInputField.text;

        // Verifique se o campo de entrada não está vazio
        if (!string.IsNullOrEmpty(playerName))
        {
            // Salve o nome do jogador em PlayerPrefs
            PlayerPrefs.SetString(playerNameKey, playerName);
            PlayerPrefs.Save();
            Debug.Log("Nome do jogador salvo: " + playerName);

            // Aqui você pode fazer a transição para a cena do ranking
            // SceneManager.LoadScene("CenaDoRanking");
        }
        else
        {
            Debug.LogWarning("Nome do jogador não pode estar vazio.");
        }
    }

    public void LoadNextScene()
    {
        string playerName = nameInputField.text;
        string playerEmail = emailInputField.text;
        string playerPhone = phoneInputField.text;

        // Verifica se os campos estão preenchidos antes de adicionar o usuário
        if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(playerEmail) && !string.IsNullOrEmpty(playerPhone))
        {
            // Chama a função AddUser para adicionar o novo usuário no servidor
            StartCoroutine(AddUserCoroutine(playerName, playerEmail, playerPhone));
        }
        else
        {
            Debug.LogWarning("Preencha todos os campos para adicionar o usuário.");
        }
    }
}
