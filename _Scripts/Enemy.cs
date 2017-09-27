using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

    public int playerDamage;
    
    public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
	public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.

    private Animator animator;
    private Transform target;
    private bool skipMove;

	// Use this for initialization
	protected override void Start ()
    {
        GameManager.instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
	}
	
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        if (skipMove)
        {
            skipMove = false;
            return;
        }
        base.AttemptMove<T>(xDir, yDir);
        skipMove = true;
    }

    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            yDir = target.position.y > transform.position.y ? 1 : -1;
        else
            xDir = target.position.x > transform.position.x ? 1 : -1;

        AttemptMove <Player> (xDir,yDir);
    }

    protected override void OnCantMove<T>(T component)
    {
        Player hitPlayer = component as Player;        
        hitPlayer.LoseFood(playerDamage);
        animator.SetTrigger("EnemyAttack");
        
        //Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
		SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
         
    }
}//end class Enemy.
