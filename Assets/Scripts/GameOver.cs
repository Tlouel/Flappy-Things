using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private Text scoreText;

    private void Awake() 
    {
        
        scoreText = transform.Find("scoreText").GetComponent<Text>();
        transform.Find("retryBtn").GetComponent<Button>();
        
      
       
    }

    private void Start() 
    {
         Hide();
         CharacterMovement.GetInstance().OnDied += CharacterMovement_OnDied;
    }

    private void CharacterMovement_OnDied(object sender, System.EventArgs e)
    {
        
        scoreText.text = Level.GetInstance().GetPipePassedCount().ToString();
        Show();
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
