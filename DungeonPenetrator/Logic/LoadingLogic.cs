﻿using Model;
using Model.Active;
using Model.Passive;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logic
{
    public class LoadingLogic : ILoadingLogic
    {
        Random rnd = new Random();
        IGameModel gameModel;
        ISaveGameRepository saveGameRepository;
        IHighscoreRepository highscoreRepository;

        public LoadingLogic(IGameModel gameModel, ISaveGameRepository saveGameRepository, IHighscoreRepository highscoreRepository)
        {
            this.gameModel = gameModel;
            this.saveGameRepository = saveGameRepository;
            this.highscoreRepository = highscoreRepository;
        }

        public void EndGame(string name)
        {
            Highscore h = new Highscore();
            h.Level = gameModel.LevelCounter;
            h.Name = name;
            highscoreRepository.Insert(h);
            saveGameRepository.Insert(default);
        }

        public void GenerateMap()
        {
            if (gameModel.Equals(default)) // static starting values of a newly created map
            {
                gameModel.MyPlayer.Damage = 5;
                gameModel.MyPlayer.Health = 100;
                gameModel.LevelCounter = 0; // Gets raised to one, must be zero
            }
            gameModel.GameAreaChar = new char[(int)(gameModel.GameWidth / gameModel.TileSize), (int)(gameModel.GameHeight / gameModel.TileSize)];
            gameModel.LevelCounter++;
            gameModel.MyPlayer.Cords = new Point(
                (int)gameModel.GameWidth / gameModel.TileSize,
                (int)(gameModel.GameHeight / gameModel.TileSize) - 1);
            gameModel.LevelFinished = false;

            GenerateInitializedEmptyMap();
            GenerateProps();
            GenerateBasicEnemiesAndCollectables();
            for (int y = 0; y < gameModel.GameAreaChar.GetLength(1); y++)
            {
                for (int x = 0; x < gameModel.GameAreaChar.GetLength(0); x++)
                {
                    switch (gameModel.GameAreaChar[x,y]) // Some parts here shall not be hardcoded.
                    {
                        case 'W':
                            gameModel.Walls.Add(new WallProp { Cords = new Point(x, y) });
                            break;
                        case 'L':
                            gameModel.Lavas.Add(new LavaProp { Cords = new Point(x, y), Damage = 5 }); 
                            break;
                        case 'P':
                            gameModel.Waters.Add(new WaterProp { Cords = new Point(x, y)});
                            break;
                        case 'H':
                            gameModel.Powerups.Add(new Powerups { Cords = new Point(x, y), Type = PowerupType.Health});
                            break;
                        case 'D':
                            gameModel.Powerups.Add(new Powerups { Cords = new Point(x, y), Type = PowerupType.Damage});
                            break;
                        case 'R':
                            gameModel.Powerups.Add(new Powerups { Cords = new Point(x, y), Type = PowerupType.FiringSpeed});
                            break;
                        case 'F':
                            gameModel.FlyingMonsters.Add(new FlyingEnemy { Cords = new Point(x, y), Damage = 10, Health = 30 });
                            break;
                        case 'T':
                            gameModel.TrackingMonsters.Add(new TrackingEnemy { Cords = new Point(x, y), Damage = 5, Health = 60 });
                            break;
                        case 'S':
                            gameModel.ShootingMonsters.Add(new ShootingEnemy { Cords = new Point(x, y), Damage = 5, Health = 40 });
                            break;
                        case 'B':
                            gameModel.Boss = new BossEnemy { Cords = new Point(x, y), Damage = 10, Health = 100 };
                            break;
                    }
                }
            }

        }

        private void GenerateInitializedEmptyMap()
        {
            for (int y = 0; y < gameModel.GameAreaChar.GetLength(1); y++)
            {
                for (int x = 0; x < gameModel.GameAreaChar.GetLength(0); x++)
                {
                    gameModel.GameAreaChar[x, y] = 'E'; // Generates Empty Cell
                }
            }
            gameModel.GameAreaChar[(int)gameModel.MyPlayer.Cords.X,(int)gameModel.MyPlayer.Cords.Y] = 'C'; // Sets Character->Player pos
            gameModel.GameAreaChar[(int)gameModel.LevelExit.X, (int)gameModel.LevelExit.Y] = 'G'; // Sets Goal->LevelExit pos
        }
        private void GenerateBasicEnemiesAndCollectables()
        {
            int rndObjectNum = rnd.Next(rnd.Next(0, (int)(gameModel.GameWidth / gameModel.TileSize) * (int)(gameModel.GameHeight / gameModel.TileSize)));
            for (int i = 0; i < rndObjectNum; i++)
            {
                Tuple<int, int> rndCord = new Tuple<int, int>((int)(gameModel.GameWidth / gameModel.TileSize), (int)(gameModel.GameHeight / gameModel.TileSize));
                if (!(rndCord.Item1 == gameModel.MyPlayer.Cords.X && rndCord.Item2 == gameModel.MyPlayer.Cords.Y)
                    && !(rndCord.Item1 == gameModel.LevelExit.X && rndCord.Item2 == gameModel.LevelExit.Y))
                {
                    int randomObject = rnd.Next(0, 6);
                    switch (randomObject)
                    {
                        case 0:
                            gameModel.GameAreaChar[rndCord.Item1, rndCord.Item2] = 'H'; // Generates HP powerup
                            break;
                        case 1:
                            gameModel.GameAreaChar[rndCord.Item1, rndCord.Item2] = 'D'; // Generates Damage powerup
                            break;
                        case 2:
                            gameModel.GameAreaChar[rndCord.Item1, rndCord.Item2] = 'R'; // Generates "Reload speed" firing powerup
                            break;
                        case 3:
                            gameModel.GameAreaChar[rndCord.Item1, rndCord.Item2] = 'F'; // Generates Flying monster
                            break;
                        case 4:
                            gameModel.GameAreaChar[rndCord.Item1, rndCord.Item2] = 'T'; // Generates Tracking monster
                            break;
                        case 5:
                            gameModel.GameAreaChar[rndCord.Item1, rndCord.Item2] = 'S'; // Generates shooting monster
                            break;
                    }
                }
            }
        }

        private void GenerateProps()
        {
            for (int y = 1; y <= gameModel.GameAreaChar.GetLength(1) - 1; y += 2)
            {
                int[] EmptySpaceCords = GenerateEmptySpacesForRow();
                for (int x = 0; x < gameModel.GameAreaChar.GetLength(0); x++)
                {
                    if (!EmptySpaceCords.Contains(x))
                    {
                        int randomProp = rnd.Next(0, 3);
                        switch (randomProp)
                        {
                            case 0:
                                gameModel.GameAreaChar[x, y] = 'W'; // Generates Wall
                                break;
                            case 1:
                                gameModel.GameAreaChar[x, y] = 'P'; // Generates "Puddle" (Water)
                                break;
                            case 2:
                                gameModel.GameAreaChar[x, y] = 'L'; // Generates Lava
                                break;
                        }
                    }
                }
            }
        }

        private int[] GenerateEmptySpacesForRow()
        {
            int[] EmptySpaceCords = new int[rnd.Next(0, (int)(gameModel.GameWidth / gameModel.TileSize) - 1)];
            for (int i = 0; i < EmptySpaceCords.Length; i++)
            {
                EmptySpaceCords[i] = rnd.Next(0, (int)(gameModel.GameWidth / gameModel.TileSize) - 1);
            }

            return EmptySpaceCords;
        }

        public IQueryable<Highscore> GetHighscores()
        {
            return highscoreRepository.GetAll().OrderBy(x => x.Level);
        }

        public void NextLevel()
        {
            GenerateMap();
        }

        public IGameModel Play()
        {
            gameModel = saveGameRepository.GetSaveGame();
            if (gameModel.Equals(default))
            {
                GenerateMap();
                return gameModel;
            }
            return gameModel;
        }
    }
}
