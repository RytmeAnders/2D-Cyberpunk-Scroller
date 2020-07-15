using UnityEngine;
using System.Collections;

public class ScriptMain : MonoBehaviour {

    //Textures from Unity
    public Texture2D bgOriginal, player, obstacle, bgDeath, bgOpening;

    //Pixel arrays
    Color[] pixelsOriginal, pixelsPlayer, pixelsObstacle,pixelsBinary, pixelsBgDeath, pixelsOpening;

    //Behind the scenes
    Texture2D bgNew, binary;
    int caseSwitch = 3;

    //Player variables
    public int playerX, playerSpeed;
    int playerY;

    //Obstacle variables
    int obstacleX, obstacleY;
    public int obstacleSpeed;

    int obstacleX1, obstacleY1;
    public int obstacleSpeed1;

	void Start () {
        // Create a new Texture2D with the same size as original
        bgNew = new Texture2D(bgOriginal.width, bgOriginal.height);
        binary = new Texture2D(bgOriginal.width, bgOriginal.height);

        //  Change the Quads texture so it uses 'showOnQuad'
        GetComponent<Renderer>().material.mainTexture = bgNew;

        // Read out all the pixels from original and save them in pix
        pixelsPlayer = player.GetPixels();
        pixelsObstacle = obstacle.GetPixels();
        pixelsBinary = binary.GetPixels();
        pixelsBgDeath = bgDeath.GetPixels();
        pixelsOpening = bgOpening.GetPixels();

        //Define positions
        playerY = (bgOriginal.height - player.height) / 2;

        obstacleX = bgOriginal.width-obstacle.width;
        obstacleY = (bgOriginal.height - obstacle.height) / 2;

        obstacleX1 = bgOriginal.width - obstacle.width;
        obstacleY1 = bgOriginal.height - 50;

        /*-----------------BINARY MAP---------------------
        This creates a binary map with black background and white objects. This way, if
        an object tries to overwrite a pixel that is white and NOT black, we know that a
        collision has happened. This map is not shown, it is simply there in the
        background to check for collisions*/
        for (int y = 0; y < binary.height; y++) {
            for (int x = 0; x < binary.width; x++) {
                pixelsBinary[y * bgOriginal.width + x] = Color.black;
            }
        }

        binary.SetPixels(pixelsBinary);
        binary.Apply();
    }

    void Update () {

        switch (caseSwitch) {
            case 1: //Game
                //Reset pix
                pixelsOriginal = bgOriginal.GetPixels();
                pixelsBinary = binary.GetPixels();

                //-----------GRAPHICS--------------------------------
                //Draw spaceship
                for (int y = 0; y < player.height; y++) {
                    for (int x = 0; x < player.width; x++) {
                        if (pixelsPlayer[y * player.width + x].a > 0.9f) {
                            pixelsOriginal[(playerY + y) * bgOriginal.width + playerX + x] = pixelsPlayer[y * player.width + x];
                        }
                    }
                }

                //Draw spaceship in binary
                for (int y = 0; y < player.height; y++) {
                    for (int x = 0; x < player.width; x++) {
                        if (pixelsPlayer[y * player.width + x].a > 0.9f) {
                            pixelsBinary[(playerY + y) * bgOriginal.width + playerX + x] = Color.white;
                        }
                    }
                }

                //Draw obstacles
                CreateObstacle(obstacleX, obstacleY);
                CreateObstacle(obstacleX1, obstacleY1);

                //Draw obstacle in binary
                //if the pixel it is trying to overwrite is white, we know we have a collision
                CreateObstacleBinary(obstacleX, obstacleY);
                CreateObstacleBinary(obstacleX1, obstacleY1);

                //--------------COLLISION CHECKS---------------------------

                /*If the obstacle goes outside the screen, the X gets reset (starts over), and Y
                gets different (so its starts in a different height). The first obstacle gets a
                new Y in the top half of the screen, the second in the bottom half. This way the
                two obstacles never meet, because then the collision would trigger*/
                if (obstacleX < 0) {
                    obstacleX = bgOriginal.width - obstacle.width;
                    obstacleY = Random.Range(0, (bgOriginal.height/2) - obstacle.height);
                    obstacleSpeed = Random.Range(4, 6);
                }

                if (obstacleX1 < 0) {
                    obstacleX1 = bgOriginal.width - obstacle.width;                    //-20 so it stays 'in frame'
                    obstacleY1 = Random.Range(bgOriginal.height/2, bgOriginal.height - obstacle.height - 20);
                    obstacleSpeed1 = Random.Range(4, 6);
                }

                //If the spaceships moves to the edge of the screen
                if (playerY > bgOriginal.height - player.height) {
                    playerY -= playerSpeed;
                }

                if (playerY < 0) {
                    playerY += playerSpeed;
                }

                //--------MOVEMENT----------

                if (Input.GetKey(KeyCode.W))
                    playerY += playerSpeed;

                if (Input.GetKey(KeyCode.S))
                    playerY -= playerSpeed;

                if (Input.GetKeyDown(KeyCode.T))
                    caseSwitch = 3;

                //Make the obstacles move left
                obstacleX -= obstacleSpeed;
                obstacleX1 -= obstacleSpeed1;
                //--------------------------


                bgNew.SetPixels(pixelsOriginal);
                bgNew.Apply();
                break;
            case 2: //Death
                //Apply the death screen texture
                bgNew.SetPixels(pixelsBgDeath);
                bgNew.Apply();

                //Reset positions of all objects
                playerY = (bgOriginal.height - player.height) / 2;

                obstacleX = bgOriginal.width - obstacle.width;
                obstacleY = (bgOriginal.height - obstacle.height) / 2;

                obstacleX1 = bgOriginal.width - obstacle.width;
                obstacleY1 = bgOriginal.height - 50;

                obstacleSpeed = 2;
                obstacleSpeed1 = 2;

                //If the user presses [space], the game starts over
                if (Input.GetKeyDown(KeyCode.Space))
                    caseSwitch = 1;

                if (Input.GetKeyDown(KeyCode.T))
                    caseSwitch = 3;
                break;
            case 3: //Opening
                pixelsOpening = bgOpening.GetPixels();

                for (int y = 0; y < player.height; y++) {
                    for (int x = 0; x < player.width; x++) {
                        if (pixelsPlayer[y * player.width + x].a > 0.9f) {
                                                //Cosine of time to make it move up and down
                            pixelsOpening[(90 + y + (int)(Mathf.Cos(Time.time*2) * 12)) * bgOriginal.width + 100 + x] =
                                pixelsPlayer[y  * player.width + x];
                        }
                    }
                }

                //Apply the opening screen texture
                bgNew.SetPixels(pixelsOpening);
                bgNew.Apply();

                //If the user presses [space], the game starts
                if (Input.GetKeyDown(KeyCode.Space))
                    caseSwitch = 1;
                break;
        }
    }

    void CreateObstacle(int obX, int obY) {
        for (int y = 0; y < obstacle.height; y++) {
            for (int x = 0; x < obstacle.width; x++) {
                if (pixelsObstacle[y * obstacle.width + x].a > 0.9f)
                    pixelsOriginal[(obY + y) * bgOriginal.width + obX + x] = pixelsObstacle[y * obstacle.width + x];
            }
        }
    }

    void CreateObstacleBinary(int obX, int obY) {
        for (int y = 0; y < obstacle.height; y++) {
            for (int x = 0; x < obstacle.width; x++) {
                if (pixelsObstacle[y * obstacle.width + x].a > 0.9f) {
                    if (pixelsBinary[(obY + y) * bgOriginal.width + obX + x] == Color.black) {
                        pixelsBinary[(obY + y) * bgOriginal.width + obX + x] = Color.white;
                    }
                    else {
                        caseSwitch = 2;
                    }
                }
            }
        }
    }
}
