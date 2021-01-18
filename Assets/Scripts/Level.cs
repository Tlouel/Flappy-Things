using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const float CAMERA_SIZE = 50f;
    private const float PIPE_WIDTH = 10f;
    private const float PIPE_HEAD_HEIGHT = 1.41f;
    private const float PIPE_SPDMOV = 30f;
    private const float PIPE_SPAWN = +100f;
    private const float OUT_PIPE = -100f; // Carefull check on every game scale (16:9 / 5:4)
    private const float CHARA_POSITION = 0f;

    private static Level instance;
    
    public static Level GetInstance()
    {
        return instance;
    }
    
    private List<Pipe> pipeList;

    private int pipePassedCount;
    private int pipesSpawn;

    private float pipeSpawnTime;
    private float pipeSpawnTimeMax;
    private float spaceSize;

    private State state;


    public enum Difficulty
    {
        //Create a list of difficulties
        Easy,
        Medium,
        Hard,
        Impossible,
    
    }

    public enum State
    {
        //Create an order of Actions
        Waiting,
        Playing,
        Dead,
    }
    
    

     private void Awake() 
    {
        instance = this;
        pipeList = new List<Pipe>();
        pipeSpawnTimeMax = 1f;
        
        SetDifficulty(Difficulty.Easy);
        
        state = State.Waiting;  
    }

    private void Start()
    {
        CharacterMovement.GetInstance().OnDied += Level_OnDied;
        CharacterMovement.GetInstance().OnStart += Level_OnStart;

    }

    private void Level_OnStart(object sender, System.EventArgs e) 
    {
        //Debug.Log("Good");
        state = State.Playing;
    }

    private void Level_OnDied(object sender, System.EventArgs e)
    {
        //Debug.Log("Dead");
        state = State.Dead;

    }
    
    private void Update() 
    {
        if(state == State.Playing)
        {
            PipeMov();
            PipeSpawning();
        }
    }

#region Pipe Spawning and Destroying Outside Camera View
    private void PipeSpawning()
    {
        pipeSpawnTime -= Time.deltaTime;

        if(pipeSpawnTime < 0)
        {
            pipeSpawnTime += pipeSpawnTimeMax;

            float heightEdgeLimit = 10f;
            float minHeight = spaceSize * .5f + heightEdgeLimit;
            float totalHeight = CAMERA_SIZE * 2f;
            float maxHeight = totalHeight - spaceSize * .5f - heightEdgeLimit;

            float height = Random.Range(minHeight, maxHeight);
    
            CreateSpace(height, spaceSize, PIPE_SPAWN);
        }
    }
#endregion    

#region Pipe Passing By The Character & Score ++   
    private void PipeMov()
    {
        for(int i = 0; i < pipeList.Count; i++)
        {
            Pipe pipe = pipeList[i];
            bool isToTheRightOfChara = pipe.GetXPosition() > CHARA_POSITION;
            pipe.Mov();
            if(isToTheRightOfChara && pipe.GetXPosition() <= CHARA_POSITION && pipe.IsBottom())
            {
                pipePassedCount++;
                SoundManager.PlaySound(SoundManager.Sound.Score);
            }

           if(pipe.GetXPosition() < OUT_PIPE)
           {
            
                pipe.DestroyPipe();
                pipeList.Remove(pipe);
                i--;

           }
        }
    }
#endregion

#region Set Up Difficulty & Pipe Spawning Time & Gap Size.
    private void SetDifficulty(Difficulty difficulty)
    {   
        //Spawning Time Pipe & Space 
        switch (difficulty)
        {
            case Difficulty.Easy:
            spaceSize = 50f;
            pipeSpawnTimeMax = 1.2f;
            break;

            case Difficulty.Medium:
            spaceSize = 40f;
            pipeSpawnTimeMax = 1.1f;
            break;
            
            case Difficulty.Hard:
            spaceSize = 33f;
            pipeSpawnTimeMax = 1.0f;
            break;

            case Difficulty.Impossible:
            spaceSize = 24f;
            pipeSpawnTimeMax = .9f;
            break;
        }
    }

    private Difficulty GetDifficulty()
    {
        
        if(pipesSpawn >= 30) return Difficulty.Impossible;
        if(pipesSpawn >= 20) return Difficulty.Hard;
        if(pipesSpawn >= 10) return Difficulty.Medium;
        return Difficulty.Easy; 

    }
#endregion

    private void CreateSpace(float spaceY, float spaceSize, float xPosition)
    {
        CreatePipe(spaceY - spaceSize * .5f, xPosition, true);
        CreatePipe(CAMERA_SIZE * 2f - spaceY - spaceSize * .5f, xPosition, false);
        pipesSpawn++;
        SetDifficulty(GetDifficulty());
        
    }

#region Pipe Creation, Positioning, Colliding & Destroying
    private void CreatePipe(float height, float xPosition, bool createBottomPipe)
    {
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);
       
        float pipeHeadYPosition;

        if(createBottomPipe)
        {
            pipeHeadYPosition = -CAMERA_SIZE + height - PIPE_HEAD_HEIGHT * .5f;
        }
        else
        {
            pipeHeadYPosition = +CAMERA_SIZE - height + PIPE_HEAD_HEIGHT * .5f;
        }
        
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition );

        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);

        float pipeBodyYPosition;

        if(createBottomPipe)
        {
            pipeBodyYPosition = -CAMERA_SIZE;
        }
        else
        {
            pipeBodyYPosition = +CAMERA_SIZE;
            pipeBody.localScale = new Vector3(1, -1, 1);
            
        }

        pipeBody.position = new Vector3(xPosition, pipeBodyYPosition);


        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_WIDTH, height);

        BoxCollider2D pipeBodyBoxCollider = pipeBody.GetComponent<BoxCollider2D>();

        pipeBodyBoxCollider.size = new Vector2(PIPE_WIDTH, height);
        pipeBodyBoxCollider.offset = new Vector2(0f, height * .5f); 

        Pipe pipe = new Pipe(pipeHead, pipeBody, createBottomPipe);
        pipeList.Add(pipe);
    
    }


    public int GetPipesSpawn()
    {
        return pipesSpawn;
    }

    public int GetPipePassedCount()
    {
        return pipePassedCount;
    }

    private class Pipe
        {
            private Transform pipeHeadTransform;
            private Transform pipeBodyTransform;
            private bool isBottom;
            
            public Pipe(Transform pipeHeadTransform,Transform pipeBodyTransform, bool isBottom)
            {
                this.pipeHeadTransform = pipeHeadTransform;
                this.pipeBodyTransform = pipeBodyTransform;
                this.isBottom = isBottom;
            }


            public void Mov()
            {
                pipeHeadTransform.position += new Vector3(-1, 0, 0) * PIPE_SPDMOV * Time.deltaTime;
                pipeBodyTransform.position += new Vector3(-1, 0, 0) * PIPE_SPDMOV * Time.deltaTime;

            }

            public float GetXPosition()
            {
                return pipeHeadTransform.position.x;
            }

            public bool IsBottom()
            {
                return isBottom;
            }


            public void DestroyPipe() 
            {
                Destroy(pipeHeadTransform.gameObject);
                Destroy(pipeBodyTransform.gameObject);

            }
    
        }
#endregion

}
