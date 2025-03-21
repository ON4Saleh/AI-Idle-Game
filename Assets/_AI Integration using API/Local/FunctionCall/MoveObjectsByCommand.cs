using System.Collections.Generic;
using System.Reflection;
using LLMUnity;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
// Main class that handles the movement of objects based on text commands
public class MoveObjectsByCommand : MonoBehaviour
{
    // References to required components
    public LLMCharacter llmCharacter;        // Reference to the AI language model
    /*public InputField playerText;       */     // Input field where player types commands
    
    // References to the UI objects that can be moved
    public RectTransform blueSquare;
    public RectTransform redSquare;
    public PlayerSkills playerSkills;
    public RectTransform moveableObject;
    public GameObject targetEnemy;
    //void Start()
    //{
    //    // Set up the input field to listen for submissions
    //    playerText.onSubmit.AddListener(onInputFieldSubmit);
    //    playerText.Select();  // Automatically focus the input field
    //}
    void Start()
    {
        // ??? ?????? ????????
        InvokeRepeating("AutoAttack", 0, 1f); // ???? ?? ?????
    }

    private void AutoAttack()
    {
        if (targetEnemy != null)
        {
            Debug.Log("Attacking enemy: " + targetEnemy.name);
            // ??????? ?????? ?? ??????? ?????
            PAttack attackScript = moveableObject.GetComponent<PAttack>();
            if (attackScript != null)
            {
                attackScript.StartAttacking();
            }
        }
    }

    // Helper method to get all function names from a class using reflection
    string[] GetFunctionNames<T>()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) 
            functionNames.Add(function.Name);
        return functionNames.ToArray();
    }

    // Constructs the prompt for the AI to understand direction commands
    string ConstructDirectionPrompt(string message)
    {
        string prompt = "From the input, which direction is mentioned? Choose from the following options:\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames<DirectionFunctions>()) 
            prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice, focusing only on direction";
        return prompt;
    }

    // Constructs the prompt for the AI to understand color commands
    string ConstructColorPrompt(string message)
    {
        string prompt = "From the input, which color is mentioned? Choose from the following options:\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames<ColorFunctions>()) 
            prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice, focusing only on color";
        return prompt;
    }

    // Called when the player submits text in the input field
    public async Task<bool> onInputFieldSubmit(string message, Action callback = null)
    {
        if (playerSkills == null)
        {
            playerSkills = FindFirstObjectByType<PlayerSkills>();
        }

        bool commandProcessed = await ProcessCommand(message);

        // Ask the AI to interpret the direction from the input
        string getDirection = await llmCharacter.Chat(ConstructDirectionPrompt(message));

        // Convert the AI's response into actual Vector3 direction using reflection
        Vector3 direction = (Vector3)typeof(DirectionFunctions).GetMethod(getDirection).Invoke(null, null);

        // Log the result for debugging
        Debug.Log($"Direction function called: {getDirection}, returned: {direction}");

        if (callback != null)
        {
            if (commandProcessed)
            {
                callback.Invoke();
            }
        }

        // Move the object in the specified direction (multiplied by 100 for visible movement)
        if (moveableObject != null)
        {
            moveableObject.anchoredPosition += (Vector2)direction * 100f;
        }

        return commandProcessed;
    }
    private async Task<bool>ProcessCommand(string command)
    {
        if (command.Contains("Stop") || command.Contains("Cancel")) // ????? ?????
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                PAttack attackScript = playerObject.GetComponent<PAttack>();
                if (attackScript != null)
                {
                    attackScript.StopAttacking();
                    return true;
                }
                else
                {
                    Debug.LogError("PAttack script not found on Player!");
                    return false;
                }
            }
            else
            {
                Debug.LogError("Player object not found with tag 'Player'!");
                return false;
            }
         }
            else if (command.Contains("Shield"))
         {
            if (playerSkills != null)
            {
                playerSkills.UseShieldSkill(); 
                return true;
            }
            else
            {
                Debug.LogError("PlayerSkills script not found!");
                return false;
            }
        }
        else if (command.Contains("Target first enemy")) 
        {
            targetEnemy = GameObject.FindGameObjectWithTag("Enemy");
            if (targetEnemy != null)
            {
                moveableObject.anchoredPosition += Vector2.up * 100f; 
                return true;
            }
            else
            {
                Debug.LogError("Enemy object not found with tag 'Enemy'!");
                return false;
            }

        }
        else if (command.Contains("Target second enemy")) 
        {
            targetEnemy = GameObject.FindGameObjectWithTag("Enemy2");
            if (targetEnemy != null)
            {
                moveableObject.anchoredPosition += Vector2.down * 100f;
                return true;
            }
            else
            {
                Debug.LogError("Enemy object not found with tag 'Enemy2'!");
                return false;
            }
        }
        else if (command.Contains("blue") || command.Contains("red"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

  

    // Cancels any pending AI requests
    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
    }

    // Quits the application (called by UI button)
    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    // Editor-only validation to ensure the AI model is properly set up
    bool onValidateWarning = true;
    void OnValidate()
    {
        if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
        {
            Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
            onValidateWarning = false;
        }
    }
}
