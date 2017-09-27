using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

    public Text foodText;
    public float restartLevelDelay = 1f;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public int wallDamage = 1;

    private Animator animator;
    private int food;
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
	private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif

	//Sounds
	public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
	public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
	public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
	public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
	public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
	public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
	public AudioClip gameOverSound;				//Audio clip to play when player dies.

    // Use this for initialization
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        foodText.text = "Food: " + food;
        base.Start();
    }

    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update() {
        if (!GameManager.instance.playersTurn) return;

        //so that the last + or - food doesn't continue to show.
        foodText.text = "Food: " + food;

        int horizontal = 0;
        int vertical = 0;

		//if you're using a regular computer for a standalone application:
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

        horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        vertical = (int)(Input.GetAxisRaw("Vertical"));

        if (horizontal != 0)
            vertical = 0;


		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		//Check if Input has registered more than zero touches
		if (Input.touchCount > 0)
		{
			//Store the first touch detected.
			Touch myTouch = Input.touches[0];

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began)
			{
				//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
			}

			//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
			{
				//Set touchEnd to equal the position of this touch
				Vector2 touchEnd = myTouch.position;

				//Calculate the difference between the beginning and end of the touch on the x axis.
				float x = touchEnd.x - touchOrigin.x;

				//Calculate the difference between the beginning and end of the touch on the y axis.
				float y = touchEnd.y - touchOrigin.y;

				//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
				touchOrigin.x = -1;

				//Check if the difference along the x axis is greater than the difference along the y axis.
				if (Mathf.Abs(x) > Mathf.Abs(y))
					//If x is greater than zero, set horizontal to 1, otherwise set it to -1
					horizontal = x > 0 ? 1 : -1;
				else
					//If y is greater than zero, set horizontal to 1, otherwise set it to -1
					vertical = y > 0 ? 1 : -1;
			}
		}

#endif //End of mobile platform dependendent compilation section started above with #elif
		//Check if we have a non-zero value for horizontal or vertical
		if (horizontal != 0 || vertical != 0)
            AttemptMove<Wall>(horizontal, vertical);
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        food--;
        //Update food text display to reflect current score.
        foodText.text = "Food: " + food;

        base.AttemptMove <T> (xDir,yDir);
        RaycastHit2D hit;
        
		//If Move returns true, meaning Player was able to move into an empty space.
		if (Move (xDir, yDir, out hit)) 
		{
			//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
		}

        CheckIfGameOver();
        GameManager.instance.playersTurn = false;
    }

    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("PlayerChop");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
			//Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
			SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
            other.gameObject.SetActive(false);
            
        }
        else if(other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;
			//Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the eating sound effect.
			SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
			other.gameObject.SetActive(false);
            
        }
    }
    
    private void Restart()
    {
        //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
        //and not load all the scene object in the current scene.
       // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        SceneManager.LoadScene(0);
    }

    public void LoseFood(int loss)
    {
        animator.SetTrigger("PlayerHit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
			//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
			SoundManager.instance.PlaySingle (gameOverSound);
			//Stop the background music.
			SoundManager.instance.musicSource.Stop();
			//Call the GameOver function of GameManager.
			GameManager.instance.GameOver();
        }
    }


}
