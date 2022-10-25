using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum  GameState{
    wait,
    move
}

public class Board : Singleton<Board>
{
    public GameState currentState=GameState.move;
    public int width;
    public int height;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue=1;
    private ScoreManager scoreManager;

    public override void Awake() {
        MakeSingleton(false);
    }

    public override void Start()
    {
        base.Start();
        scoreManager=FindObjectOfType<ScoreManager>();
        findMatches=FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, height];
        allDots=new GameObject[width,height];
    }

    public void PlayGame(){

        GameGUIManager.Ins.ShowGameGui(true);
        SetUp();
        if(IsDeadlocked()){
            Debug.Log("Deadlocked!!!");
            GameGUIManager.Ins.gameoverDialog.Show(true);
            GameGUIManager.Ins.CurDialog=GameGUIManager.Ins.gameoverDialog;
            AudioController.Ins.PlaySound(AudioController.Ins.lose);
        }
    }

    private void SetUp()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j= 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent=this.transform;
                backgroundTile.name="( "+ i + ", "+j+" )";
                int dotToUse=Random.Range(0,dots.Length);
                int maxIterations=0;
                while (MatchesAt(i,j,dots[dotToUse])&&maxIterations<100)
                {
                    dotToUse=Random.Range(0,dots.Length);
                    maxIterations++;
                    Debug.Log(maxIterations);
                }
                maxIterations=0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.GetComponent<Dot>().row=j;
                dot.GetComponent<Dot>().column=i;

                dot.transform.parent=this.transform;
                dot.name="( "+ i + ", "+j+" )";
                allDots[i,j]=dot;
            }
        }
    }

    private bool MatchesAt(int column,int row,GameObject piece){
        if(column>1&& row>1){
            if(allDots[column-1,row].tag==piece.tag&&allDots[column-2,row].tag==piece.tag){
                return true;
            }
            if(allDots[column,row-1].tag==piece.tag&&allDots[column,row-2].tag==piece.tag){
                return true;
            }
        }
        else if(column<=1||row<=1){
            if(row>1){
                if(allDots[column,row-1].tag==piece.tag&&allDots[column,row-2].tag==piece.tag){
                    return true;
                }
            }
            if(column>1){
                if(allDots[column-1,row].tag==piece.tag&&allDots[column-2,row].tag==piece.tag){
                    return true;
                }
            }
        }
        return false;
    }

    private void DestroyMatchesAt(int column,int row){
        if(allDots[column,row].GetComponent<Dot>().isMatched){
            findMatches.currentMatches.Remove(allDots[column,row]);
            GameObject particle = Instantiate(destroyEffect,allDots[column,row].transform.position,Quaternion.identity);
            Destroy(particle,.5f);
            Destroy(allDots[column,row]);
            scoreManager.IncreaseScore(basePieceValue*streakValue);
            allDots[column,row]=null;
        }
    }

    public void DeytroyMatches(){
        for(int i=0;i<width;i++){
            for(int j=0;j<height;j++){
                if(allDots[i,j]!=null){
                    DestroyMatchesAt(i,j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo(){
        int nullCount=0;
        for(int i=0;i<width;i++){
            for(int j=0;j<height;j++){
                if(allDots[i,j]==null){
                    nullCount++;
                }
                else if(nullCount>0){
                    allDots[i,j].GetComponent<Dot>().row-=nullCount;
                    allDots[i,j]=null;
                }
            }
            nullCount=0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard(){
        for(int i=0;i<width;i++){
            for(int j=0;j<height;j++){
                if(allDots[i,j]==null){
                    Vector2 tempPosition=new Vector2(i,j+offSet);
                    int dotToUse=Random.Range(0,dots.Length);
                    GameObject piece =Instantiate(dots[dotToUse],tempPosition,Quaternion.identity);
                    allDots[i,j]=piece;
                    piece.GetComponent<Dot>().row=j;
                    piece.GetComponent<Dot>().column=i;
                }
            }
        }
    }

    private bool MatchesOnBoard(){
        for(int i=0;i<width;i++){
            for(int j=0;j<height;j++){
                if(allDots[i,j]!=null){
                    if(allDots[i,j].GetComponent<Dot>().isMatched){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo(){
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            streakValue++;

            yield return new WaitForSeconds(.5f);
            DeytroyMatches();
        }
        yield return new WaitForSeconds(.5f);

        if(IsDeadlocked()){
            Debug.Log("Deadlocked!!!");
            GameGUIManager.Ins.gameoverDialog.Show(true);
            GameGUIManager.Ins.CurDialog=GameGUIManager.Ins.gameoverDialog;
            AudioController.Ins.PlaySound(AudioController.Ins.lose);
        }
        currentState=GameState.move;
        streakValue=1;
    }

    private void SwitchPieces(int column,int row,Vector2 direction){
        //Take the second piece and save it in a holder
        GameObject holder = allDots[column + (int)direction.x,row+(int)direction.y]as GameObject;
        //Switching the first dot to be the second position
        allDots[column+(int)direction.x,row +(int)direction.y]=allDots[column,row];
        //Set the first dot to be the second dot
        allDots[column,row]=holder;
    }

    private bool CheckForMatches(){
        for(int i=0;i<width;i++){
            for(int j=0;j<height;j++){
                if(allDots[i,j]!=null){
                    //Make sure that one and two to the right are in the board
                    if(i<width-2){
                        //Check if the dots to the right and two to the right exist
                        if(allDots[i+1,j]!=null&&allDots[i+2,j]!=null){
                            if(allDots[i+1,j].tag==allDots[i,j].tag
                                && allDots[i+2,j].tag==allDots[i,j].tag){
                                    return true;
                                }
                        }
                    }
                    if(j<height-2){
                        //Check if the dots above exist
                        if(allDots[i,j+1]!=null&&allDots[i,j+2]!=null){
                            if(allDots[i,j+1].tag==allDots[i,j].tag
                                && allDots[i,j+2].tag==allDots[i,j].tag){
                                    return true;
                                }
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool SwitchAndCheck(int column,int row,Vector2 direction){
        SwitchPieces(column,row,direction);
        if(CheckForMatches()){
            SwitchPieces(column,row,direction);
            return true;
        }
        SwitchPieces(column,row,direction);
        return false;
    }

    private bool IsDeadlocked(){
        for(int i=0;i<width;i++){
            for(int j =0;j<height;j++){
                if(allDots[i,j]!=null){
                    if(i<width-1){
                        if(SwitchAndCheck(i,j,Vector2.right)){
                            return false;
                        }
                    }
                    if(j<height-1){
                        if(SwitchAndCheck(i,j,Vector2.up)){
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }
}
