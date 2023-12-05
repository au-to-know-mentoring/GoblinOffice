using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PathfindingManager : MonoBehaviour
{

    [Header("Lists")]
    public List<PathfindingObject> UnassignedEnemyList = new List<PathfindingObject>();
    public List<PathfindingObject> AssignedEnemyList = new List<PathfindingObject>(); // TODO
    public List<PathfindingObject> DeadEnemyList = new List<PathfindingObject>();
    public List<String> StringBeatList= new List<String>();
    public UIImageSpawner myUIImageSpawner;


    // List of beat events, excluding movement;
    public GameObject Player;
    public Tilemap obstacleTilemap;
    private TileBase[] obstacleTiles;
    private Vector3 offset;
    public Dictionary<Vector3Int, Node> nodeDictionary = new Dictionary<Vector3Int, Node>();
    public float myTimer;

    public float VulnerableDuration = 1.99f;

    [Header("Beats")]
    public int RangedBeats;
    public int BeatToLoop;
    private bool HasLoopedBefore = false;
    public AnimationSettings myAnimationSettings;

    [Header("Player Positions")]
    public Vector3Int LeftOfPlayerPosition;
    public Vector3Int RightOfPlayerPosition;
    public Vector3Int TopOfPlayerPosition;
    public Vector3Int PlayerPosition;

    public Text youWinText;
    public Text youLoseText;
    public SettingsData GlobalSettingsObject;
    public TimeManager GlobalTimeManager;
    public enum BeatEvent
    {
        RangedAttack,
        MeleeAttack,
        Vulnerable
    }
    public List<BeatEvent?> beatEvents;
    public List<BeatEventWithEnemy?> beatEventWithEnemies = new List<BeatEventWithEnemy?>(new BeatEventWithEnemy?[0]);
    private void Start()
    {
        youWinText.enabled = false;
        // Register all pathfinding objects in the scene
        RegisterPathfindingObjects();
        myUIImageSpawner = FindObjectOfType<UIImageSpawner>();
        myUIImageSpawner.setBeatLoop(BeatToLoop);
        // Set Positions adjacent to player.
        SetPositionsAroundPlayer();
        

        // Get the obstacle tiles from the tilemap
        obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);

        // Calculate the offset based on the tilemap's anchor
        offset = new Vector3(0.5f, 0.5f, 0f);

        // Initialize the grid
        InitializeGrid();
        DrawDebugLines();
    }




    public Dictionary<Vector3Int, Node> GetNodeDictionary()
    {
        return nodeDictionary;
    }
    private void Update()
    {
        GlobalTimeManager.Timer += Time.deltaTime * GlobalSettingsObject.BeatsPerSecondBPM;
        // Set Paths to surround Player.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q is pressed");
            AssignPathsToSurroundPlayer();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R is pressed");
            AssignRangedAttacks();
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("C is pressed");
            //AdjustSpeedOfEnemies();
            AssignRangedAttacksByRandomBeat();
            foreach (var pathFindingObject in AssignedEnemyList)
            {
                pathFindingObject.CreateStringListOfActions();
            }
            CreateStringListOfBeats();
        }

        // Update the paths for all pathfinding objects
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var pathFindingObject in UnassignedEnemyList)
            {
                pathFindingObject.SetCurrentPath(FindPath(pathFindingObject.startPos, pathFindingObject.endPos));
                pathFindingObject.StartMovement();
            }
        }
        //UpdatePaths();
        LoopBeat();
    }

    private void AdjustSpeedOfEnemies()
    {
        //VulnerableDuration *= GlobalSettingsObject.BeatsPerSecondBPM;
        //foreach (var pathFindingObject in UnassignedEnemyList)
        //{
        //    Debug.Log("Global BPS: " + GlobalSettingsObject.BeatsPerSecondBPM);

        //    pathFindingObject.VulnerableDuration *= GlobalSettingsObject.BeatsPerSecondBPM;
        //    pathFindingObject.projectileSpeed *= GlobalSettingsObject.BeatsPerSecondBPM;
        //   Debug.Log("P-Speed = " + pathFindingObject.projectileSpeed);
        //}
    }

    private void LoopBeat()
    {
        if (GlobalTimeManager.Timer >= BeatToLoop)
        {
            GlobalTimeManager.Timer -= BeatToLoop;
            if(Player.GetComponent<Player>().Health == 0)
            {
                //LevelComplete(false);
            }
            foreach (var pathFindingObject in AssignedEnemyList)
            {
                if (pathFindingObject.Dead == true)
                {
                    DeadEnemyList.Add(pathFindingObject);
                    // dont add to unassigned because dead.
                }
                else
                {
                    pathFindingObject.ResetAttackList(GlobalTimeManager.Timer);
                    pathFindingObject.isVulnerable = false;
                    //[TODO]Add lost attacks to current alive enemies?

                    UnassignedEnemyList.Add(pathFindingObject);
                }
            }
            AssignedEnemyList.Clear();

            if (UnassignedEnemyList.Count > 0)
            {
                foreach (var pathFindingObject in UnassignedEnemyList)
                {
                    pathFindingObject.SetTimer(GlobalTimeManager.Timer);
                }
                    AssignRangedAttacksByRandomBeat();
                    //AssignMeleeAttacksByRandomBeat();
                /// This is currently set in RangedAttackByBeat
                // int RandomEnemy = UnityEngine.Random.Range(0, AssignedEnemyList.Count);
                //AssignedEnemyList[RandomEnemy].VulnerableBeat = BeatToLoop - 2;
            }
            else
            {
                beatEventWithEnemies.Clear();
                StringBeatList.Clear();
                //Reloads Scene.
                LevelComplete(true);
            }
            foreach (var pathFindingObject in AssignedEnemyList)
            {
               pathFindingObject.CreateStringListOfActions();
            }
            CreateStringListOfBeats();
        }
    }

    private void AssignMeleeAttacksByRandomBeat()
    {
        //**[Optimize] Could be optimized by placing all pathFindingObjects into a list, and taking objects out after assigning them.
        //**[Improvement] Current priority order is (Left, Right, Top). Could be changed to help equal out the arrival times.
        //**[Limitations] Cannot Assign more then 3 enemies atm. Cannot Assign more then 1 enemy to 1 spot.

        //Funtionalise each portion.
        //Location as an argument.
        //Beatmanager assign event WITHOUT choice of enemy.

        PathfindingObject EnemyForLeftPosition = null;
        PathfindingObject EnemyForRightPosition = null;
        PathfindingObject EnemyForTopPosition = null;

        int DistanceToLeftPos = 999;
        int DistanceToRightPos = 999;
        int DistanceToTopPos = 999;
        //LEFT
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            AssignMeleePosition(ref EnemyForLeftPosition, ref DistanceToLeftPos, LeftOfPlayerPosition);
        }

        //foreach (var pathFindingObject in UnassignedEnemyList)
        //{
        //    AssignMeleePosition(ref EnemyForRightPosition, ref DistanceToRightPos, RightOfPlayerPosition);
        //}
        //foreach (var pathFindingObject in UnassignedEnemyList)
        //{
        //    AssignMeleePosition(ref EnemyForTopPosition, ref DistanceToTopPos, TopOfPlayerPosition);
        //}
        //RIGHT
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            Debug.Log(GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]));

            if (GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]) < DistanceToRightPos)
            {
                if (pathFindingObject != EnemyForLeftPosition)
                {
                    EnemyForRightPosition = pathFindingObject;
                    DistanceToRightPos = GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]);
                }
            }
        }
        Debug.Log("The closest Enemy to the position Right of the player is: " + EnemyForRightPosition.name);
        EnemyForRightPosition.endPos = Vector3Int.FloorToInt(RightOfPlayerPosition);
        //TOP
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            Debug.Log(GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]));

            if (GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]) < DistanceToTopPos)
            {
                if (pathFindingObject != EnemyForLeftPosition & pathFindingObject != EnemyForRightPosition)
                {
                    EnemyForTopPosition = pathFindingObject;
                    DistanceToTopPos = GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]);
                }
            }
        }
        Debug.Log("The closest Enemy to the position Top of the player is: " + EnemyForTopPosition.name);
        EnemyForLeftPosition.endPos = LeftOfPlayerPosition;
        if (EnemyForRightPosition != null)
        {
            EnemyForRightPosition.endPos = RightOfPlayerPosition;
            if (EnemyForTopPosition != null)
            {
                EnemyForTopPosition.endPos = TopOfPlayerPosition;
            }
        }
        throw new NotImplementedException();
    }

    public void LevelComplete(bool win)
    {
        if (win == true)
        {
            //Runs when all enemies are dead.
            youWinText.enabled = true;
            Invoke("ReloadScene", 4f);
        }
        else
        {
            youLoseText.enabled = true;
            Invoke("ReloadScene", 4f);
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void AssignRangedAttacks() // CHANGE TO BEAT EVENTS><
    {
        beatEvents = new List<BeatEvent?>(new BeatEvent?[BeatToLoop]); // creating the list here prevents both methods from doubling up
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            if (pathFindingObject.rangedAttackQuantity >= 1)
            {
                int distance = GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(PlayerPosition)]);
                while (pathFindingObject.rangedAttackQuantity > 0)
                {
                    bool EventChosen = false;
                    int RandomNumber = 0;
                    while (EventChosen == false)
                    {
                        RandomNumber = UnityEngine.Random.Range(5, 15); // change to enemys not random beats.
                        if (beatEvents[RandomNumber] == null)
                        {
                            beatEvents[RandomNumber] = BeatEvent.RangedAttack;
                            float BeatEventTimeToStart = pathFindingObject.SetRangedAttack(RandomNumber, distance, GlobalSettingsObject.BeatsPerSecondBPM);
                            beatEventWithEnemies[RandomNumber] = new BeatEventWithEnemy(BeatEvent.RangedAttack, pathFindingObject, BeatEventTimeToStart);
                            EventChosen = true;
                            pathFindingObject.rangedAttackQuantity--;

                        }
                    }
                }
                AssignedEnemyList.Add(pathFindingObject);
                pathFindingObject.Active = true;
                pathFindingObject.CloneAttackList();
            }
        }
        foreach (var pathFindingObject in AssignedEnemyList)
        {
            UnassignedEnemyList.Remove(pathFindingObject);
        }
    }
    private void AssignRangedAttacksByRandomBeat()
    {
        beatEventWithEnemies.Clear();
        for (int i = 0; i < BeatToLoop; ++i)
        {
            beatEventWithEnemies.Add(null);
        }


        beatEvents = new List<BeatEvent?>(new BeatEvent?[BeatToLoop]);// creating the list here prevents both methods from doubling up
        for (int i = 0; i < RangedBeats; i++)
        {
            int x = 0;
            bool BeatSet = false;
            while (BeatSet == false)
            {

                int randomBeat = UnityEngine.Random.Range(3, BeatToLoop - 2);
                if (beatEvents[randomBeat] == null)
                {
                    beatEvents[randomBeat] = BeatEvent.RangedAttack;
                    BeatSet = true;
                    int RandomEnemy;
                    float distance; //Not sure why this was ever an int.
                    float BeatEventTimeToStart;
                    if (UnassignedEnemyList.Count > 0)
                    {
                        
                        RandomEnemy = UnityEngine.Random.Range(0, UnassignedEnemyList.Count);
                        distance = GetDistanceFloat(nodeDictionary[UnassignedEnemyList[RandomEnemy].startPos], nodeDictionary[Vector3Int.FloorToInt(PlayerPosition)]);
                        BeatEventTimeToStart = UnassignedEnemyList[RandomEnemy].SetRangedAttack(randomBeat, distance, GlobalSettingsObject.BeatsPerSecondBPM);
                        beatEventWithEnemies[randomBeat] = new BeatEventWithEnemy(BeatEvent.RangedAttack, UnassignedEnemyList[RandomEnemy], BeatEventTimeToStart);
                        //Assign Random enemy for the event here. Add to beateventsWithEnenmys list
                        if (!AssignedEnemyList.Contains(UnassignedEnemyList[RandomEnemy]))
                        {
                            //Add enemy to assigned list and activate it.
                            AssignedEnemyList.Add(UnassignedEnemyList[RandomEnemy]);
                            UnassignedEnemyList[RandomEnemy].Active = true;
                        }
                        //UnassignedEnemyList[RandomEnemy].CloneAttackList();
                        UnassignedEnemyList.RemoveAt(RandomEnemy);
                    }
                    else
                    {
                        RandomEnemy = UnityEngine.Random.Range(0, AssignedEnemyList.Count);
                        distance = GetDistanceFloat(nodeDictionary[AssignedEnemyList[RandomEnemy].startPos], nodeDictionary[Vector3Int.FloorToInt(PlayerPosition)]);
                        BeatEventTimeToStart = AssignedEnemyList[RandomEnemy].SetRangedAttack(randomBeat, distance, GlobalSettingsObject.BeatsPerSecondBPM);
                        beatEventWithEnemies[randomBeat] = new BeatEventWithEnemy(BeatEvent.RangedAttack, AssignedEnemyList[RandomEnemy], BeatEventTimeToStart);
                    }   
                }
                //debug stuff
                else if (x == 30)
                {
                    Debug.Log("beat couldn't be set");
                    break;
                }
                x++;

            }
        }

        int randomEnemy = UnityEngine.Random.Range(0, AssignedEnemyList.Count);
        AssignedEnemyList[randomEnemy].VulnerableBeat = BeatToLoop - 2;
        AssignedEnemyList[randomEnemy].VulnerableDuration = VulnerableDuration;
        beatEventWithEnemies[BeatToLoop - 2] = new BeatEventWithEnemy(BeatEvent.Vulnerable, AssignedEnemyList[randomEnemy], BeatToLoop - 2);
        Debug.Log("Enemy: " + AssignedEnemyList[randomEnemy] + "Assigned to be vulnerable on beat: " + AssignedEnemyList[randomEnemy].VulnerableBeat);
        foreach(var Enemy in AssignedEnemyList) 
        {
            //Enemy.CloneAttackList(); // Clone old attacks? might need to set new ones.
        }
    }

    private void AssignPathsToSurroundPlayer()
    {
        //**[Optimize] Could be optimized by placing all pathFindingObjects into a list, and taking objects out after assigning them.
        //**[Improvement] Current priority order is (Left, Right, Top). Could be changed to help equal out the arrival times.
        //**[Limitations] Cannot Assign more then 3 enemies atm. Cannot Assign more then 1 enemy to 1 spot.

        //Funtionalise each portion.
        //Location as an argument.
        //Beatmanager assign event WITHOUT choice of enemy.

        PathfindingObject EnemyForLeftPosition = null;
        PathfindingObject EnemyForRightPosition = null;
        PathfindingObject EnemyForTopPosition = null;

        int DistanceToLeftPos = 999;
        int DistanceToRightPos = 999;
        int DistanceToTopPos = 999;
        //LEFT
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            AssignMeleePosition(ref EnemyForLeftPosition, ref DistanceToLeftPos, LeftOfPlayerPosition);
        }

        //foreach (var pathFindingObject in UnassignedEnemyList)
        //{
        //    AssignMeleePosition(ref EnemyForRightPosition, ref DistanceToRightPos, RightOfPlayerPosition);
        //}
        //foreach (var pathFindingObject in UnassignedEnemyList)
        //{
        //    AssignMeleePosition(ref EnemyForTopPosition, ref DistanceToTopPos, TopOfPlayerPosition);
        //}
        //RIGHT
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            Debug.Log(GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]));

            if (GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]) < DistanceToRightPos)
            {
                if (pathFindingObject != EnemyForLeftPosition)
                {
                    EnemyForRightPosition = pathFindingObject;
                    DistanceToRightPos = GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]);
                }
            }
        }
        Debug.Log("The closest Enemy to the position Right of the player is: " + EnemyForRightPosition.name);
        EnemyForRightPosition.endPos = Vector3Int.FloorToInt(RightOfPlayerPosition);
        //TOP
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            Debug.Log(GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]));

            if (GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]) < DistanceToTopPos)
            {
                if (pathFindingObject != EnemyForLeftPosition & pathFindingObject != EnemyForRightPosition)
                {
                    EnemyForTopPosition = pathFindingObject;
                    DistanceToTopPos = GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]);
                }
            }
        }
        Debug.Log("The closest Enemy to the position Top of the player is: " + EnemyForTopPosition.name);
        EnemyForLeftPosition.endPos = LeftOfPlayerPosition;
        if (EnemyForRightPosition != null)
        {
            EnemyForRightPosition.endPos = RightOfPlayerPosition;
            if (EnemyForTopPosition != null)
            {
                EnemyForTopPosition.endPos = TopOfPlayerPosition;
            }
        }
    }

    private void AssignMeleePosition(ref PathfindingObject EnemyForPosition, ref int DistanceToLeftPos, Vector3Int Position)
    {
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            Debug.Log(GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(Position)]));

            if (GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(Position)]) < DistanceToLeftPos)
            {
                EnemyForPosition = pathFindingObject;
                DistanceToLeftPos = GetDistanceInt(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(Position)]);
            }
        }

        EnemyForPosition.endPos = Vector3Int.FloorToInt(Position);
    }

    private void RegisterPathfindingObjects()
    {
        // Find all pathfinding objects in the scene and add them to the list
        PathfindingObject[] objects = FindObjectsOfType<PathfindingObject>();
        UnassignedEnemyList.AddRange(objects);
        foreach (var pathFindingObject in UnassignedEnemyList)
        {
            pathFindingObject.setObstacleTilemap(obstacleTilemap);
            pathFindingObject.GlobalSettingsObject = GlobalSettingsObject;
            pathFindingObject.GlobalTimeManager = GlobalTimeManager;
        }
    }
    private void SetPositionsAroundPlayer()
    {
        LeftOfPlayerPosition = Vector3Int.FloorToInt(Player.transform.position + Vector3.left);
        RightOfPlayerPosition = Vector3Int.FloorToInt(Player.transform.position + Vector3.right);
        TopOfPlayerPosition = Vector3Int.FloorToInt(Player.transform.position + Vector3.up);
    }


    //private void UpdatePaths()
    //{
    //    // Update the paths for each pathfinding object
    //    foreach (PathfindingObject pathfindingObject in UnassignedEnemyList)
    //    {
    //        pathfindingObject.UpdatePath();
    //    }
    //}
    private void InitializeGrid()
    {
        // Loop through each cell in the tilemap
        foreach (var position in obstacleTilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int cellPosition = new Vector3Int(position.x, position.y, position.z);

            // Create a new node for the cell
            Node node = new Node(cellPosition);

            // Check if the cell contains an obstacle tile
            if (obstacleTiles != null && obstacleTilemap.HasTile(cellPosition))
            {
                node.isWalkable = false;
            }

            // Add the node to the dictionary
            nodeDictionary.Add(cellPosition, node);
        }
    }
    private void DrawDebugLines()
    {
        // Draw debug lines for the grid
        foreach (var node in nodeDictionary.Values)
        {
            Vector3Int cellPosition = node.position;
            Vector3 worldPosition = obstacleTilemap.CellToWorld(cellPosition);
            Vector3 cellSize = obstacleTilemap.cellSize;
            Vector3 nodeCenter = worldPosition + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
            float radius = Mathf.Min(cellSize.x, cellSize.y) * 0.1f;

            Debug.DrawRay(nodeCenter, Vector3.forward, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.up * radius, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.down * radius, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.left * radius, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.right * radius, Color.yellow, 9999f);



            Vector3 topLeft = worldPosition + new Vector3(-cellSize.x / 2f + .5f, -cellSize.y / 2f + .5f);
            Vector3 topRight = worldPosition + new Vector3(cellSize.x / 2f + .5f, -cellSize.y / 2f + .5f);
            Vector3 bottomLeft = worldPosition + new Vector3(-cellSize.x / 2f + .5f, cellSize.y / 2f + .5f);
            Vector3 bottomRight = worldPosition + new Vector3(cellSize.x / 2f + .5f, cellSize.y / 2f + .5f);

            Debug.DrawLine(topLeft, topRight, Color.green, 100000f);
            Debug.DrawLine(topRight, bottomRight, Color.green, 100000f);
            Debug.DrawLine(bottomRight, bottomLeft, Color.green, 100000f);
            Debug.DrawLine(bottomLeft, topLeft, Color.green, 100000f);
        }

        // Draw debug lines for obstacles
        foreach (var position in obstacleTilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int cellPosition = new Vector3Int(position.x, position.y, position.z);

            if (obstacleTilemap.HasTile(cellPosition))
            {
                Vector3 worldPosition = obstacleTilemap.CellToWorld(cellPosition);
                Vector3 cellSize = obstacleTilemap.cellSize;

                Vector3 topLeft = worldPosition + new Vector3(-cellSize.x / 2f, -cellSize.y / 2f);
                Vector3 topRight = worldPosition + new Vector3(cellSize.x / 2f, -cellSize.y / 2f);
                Vector3 bottomLeft = worldPosition + new Vector3(-cellSize.x / 2f, cellSize.y / 2f);
                Vector3 bottomRight = worldPosition + new Vector3(cellSize.x / 2f, cellSize.y / 2f);

                Debug.DrawLine(topLeft, topRight, Color.red, 10000f);
                Debug.DrawLine(topRight, bottomRight, Color.red, 10000f);
                Debug.DrawLine(bottomRight, bottomLeft, Color.red, 10000f);
                Debug.DrawLine(bottomLeft, topLeft, Color.red, 10000f);
            }
        }
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3Int startCell = obstacleTilemap.WorldToCell(startPos);
        Vector3Int targetCell = obstacleTilemap.WorldToCell(targetPos);
        if(startCell == targetCell)
        {
            Debug.Log("Can't find path when Start and target cell are the same Object: " + this.name);
            return null;
        }    
        if (!nodeDictionary.ContainsKey(startCell) || !nodeDictionary.ContainsKey(targetCell))
        {
            Debug.LogWarning("Invalid start or target position!");
            return null;
        }

        Node startNode = nodeDictionary[startCell];
        Node targetNode = nodeDictionary[targetCell];

        // Run the A* algorithm to find the path
        List<Node> path = AStar(startNode, targetNode);
        
        // Draw the debug line
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 startPoint = obstacleTilemap.CellToWorld(path[i].position) + obstacleTilemap.cellSize * 0.5f;
                Vector3 endPoint = obstacleTilemap.CellToWorld(path[i + 1].position) + obstacleTilemap.cellSize * 0.5f;
                Debug.DrawLine(startPoint, endPoint, Color.blue, 10000f);
                //Disable Path?
                //path[i].isWalkable = false;

            }
        }
        
        return path;
    }

    private List<Node> AStar(Node startNode, Node targetNode)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + GetDistanceInt(currentNode, neighbor);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistanceInt(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("No valid path found!");
        return null;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        Vector3Int[] neighborOffsets =
        {
            new Vector3Int(-1, 0, 0),  // Left
            new Vector3Int(1, 0, 0),   // Right
            new Vector3Int(0, -1, 0),  // Down
            new Vector3Int(0, 1, 0)    // Up
        };

        foreach (var offset in neighborOffsets)
        {
            Vector3Int neighborPos = node.position + offset;

            if (nodeDictionary.TryGetValue(neighborPos, out Node neighborNode))
            {
                neighbors.Add(neighborNode);
            }
        }

        return neighbors;
    }

    private int GetDistanceInt(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
       
        return dstX + dstY;
    }

    private float GetDistanceFloat(Node nodeA, Node nodeB)
    {
        
        float dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        float dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
        if(dstX <= 0 || dstY <= 0)
            return dstX + dstY; //[Optimise]
        
        else
            return MathF.Sqrt(dstX* dstX + dstY * dstY); // gets diagnoal length.
    }
    public void CreateStringListOfBeats()
    {
        StringBeatList.Clear();
        for (int i = 0; i < beatEventWithEnemies.Count; i++)
        {
            StringBeatList.Add(null);
        }
        for (int i = 0; i < beatEventWithEnemies.Count; i++)
        {
            if (beatEventWithEnemies[i] != null)
            {
                StringBeatList[i] = new string("Beat: " + beatEventWithEnemies[i].TimeToStart + " Type: " + beatEventWithEnemies[i].BeatEventType.ToString() + " Enemy: " + beatEventWithEnemies[i].Enemy.name);
            }
        }
        //if (VulnerableBeat != 999)
        //{
        //    StringBeatList.Add("Vulnerable Beat: " + VulnerableBeat.ToString());
        //}
    }
    public class Node
    {
        public Vector3Int position;
        public bool isWalkable;
        public int gCost;
        public int hCost;
        public Node parent;

        public int fCost => gCost + hCost;

        public Node(Vector3Int pos)
        {
            position = pos;
            isWalkable = true;
            gCost = int.MaxValue;
            hCost = 0;
            parent = null;
        }
    }

    public class BeatEventWithEnemy
    {
        public BeatEvent BeatEventType;
        public PathfindingObject Enemy;
        public float TimeToStart;
        public BeatEventWithEnemy(BeatEvent beateventType, PathfindingObject pathfindingObject, float timeToStart)
        {
            BeatEventType= beateventType;
            Enemy= pathfindingObject;
            TimeToStart= timeToStart;
        }
    }
}