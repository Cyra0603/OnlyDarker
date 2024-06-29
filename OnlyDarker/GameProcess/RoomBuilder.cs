using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class Room
    {
        private readonly Point _roomTileSize;
        private readonly Texture2D _roomPresetImage;
        public readonly SpriteStandartTile[,] _tiles;
        public readonly SpriteStandartObstacle[,] _standartObstacles;
        public List<IYSortable> ObjectsYSorted;
        public List<IInteractive> Interactives;
        public List<IDamageable> Damageables;
        public List<IMyUpdateable> Updateables;
        public List<RoomPortalSprite> Portals { get; private set; } = new();
        public readonly BackgroundSprite CurrentBackground;
        public readonly Level ParentLevelReference;
        public readonly RoomType InstanceRoomType;
        private readonly static Dictionary<Vector4, string> _presetColorTranslator = new()
        {
            {new Vector4(255,0,0,255) , "Obstacle"},
            {new Vector4(0,255,0,255) , "Tile"},
            {new Vector4(0,0,255,255) , "Portal"},
            {new Vector4(60,60,60,255) , "TargetDummy" },
            {new Vector4(60,70,60,255) , "TargetDummyShooter" },
            {new Vector4(150,100,100,255) , "WeaponStick" },
            {new Vector4(100,150,100,255) , "WeaponSword" },
            {new Vector4(100,100,150,255) , "WeaponLance" },
            {new Vector4(200,200,0,255) , "WoodenChest" },
        };
        public List<Rectangle> RoomColliders { get; private set; }
        public List<Rectangle> ObstaclesBounds { get; private set; }
        public List<Rectangle> TempRectDrawList;
        public Point TileSize { get; private set; }
        public Point RoomSize { get; private set; }
        public Point GridCords { get; private set; }
        public Room(RoomBlueprint emptyRoom, Level parentLevelReference)
        {
            _roomPresetImage = ImportPreset(emptyRoom.FloorType, emptyRoom.RoomType);
            InstanceRoomType = emptyRoom.RoomType;
            var presetData = TextureTo2DArray(_roomPresetImage);
            _roomTileSize.Y = presetData.GetLength(1);
            _roomTileSize.X = presetData.GetLength(0);
            CurrentBackground = new(emptyRoom.FloorType);
            _tiles = new SpriteStandartTile[_roomTileSize.X, _roomTileSize.Y];
            _standartObstacles = new SpriteStandartObstacle[_roomTileSize.X, _roomTileSize.Y];
            ObjectsYSorted = new();
            RoomColliders = new();
            ObstaclesBounds = new();
            Damageables = new();
            Interactives = new();
            Updateables = new();
            TempRectDrawList = new();
            GridCords = new(emptyRoom.X, emptyRoom.Y);
            List<Texture2D> tileTextures = ImportTileTextures(emptyRoom.FloorType, emptyRoom.RoomType);
            List<Texture2D> standartObstacleTextures = ImportStandartObstacleTextures(emptyRoom.FloorType, emptyRoom.RoomType);
            List<Texture2D> portalTextures = ImportPortalTextures(emptyRoom.FloorType, emptyRoom.RoomType);
            TileSize = new(tileTextures[0].Width, tileTextures[0].Height);
            RoomSize = new(TileSize.X * _roomTileSize.X, TileSize.Y * _roomTileSize.Y);
            FillRoom(tileTextures, standartObstacleTextures, portalTextures, presetData, emptyRoom);
            foreach (var obstacle in _standartObstacles)
            {
                if (obstacle is not null)
                {
                    RoomColliders.Add(obstacle.MovementCollider);
                    ObstaclesBounds.Add(obstacle.Bounds);
                    ObjectsYSorted.Add(obstacle);
                }
            }
            foreach (var portal in Portals)
            {
                ObjectsYSorted.Add(portal);
            }
            ObjectsYSorted = ObjectsYSorted.OrderBy(obj => obj.Position.Y).ToList();
            ParentLevelReference = parentLevelReference;
        }

        public void Draw()
        {
            foreach (var tile in _tiles)
            {
                tile?.Draw();
            }
            foreach (var obj in ObjectsYSorted)
            {
                obj.Draw();
            }
        }
        public void SortObjectsByY()
        {
            ObjectsYSorted = ObjectsYSorted.OrderBy(obj => obj.Position.Y).ToList();
        }
        private static Texture2D ImportPreset(Floor floor, RoomType roomType)
        {
            var rng = new Random();
            string contentDir = $"Floor/{floor}/RoomType/{roomType}/Presets";
            int contentDirCount = Directory.EnumerateFiles("Content/" + contentDir, "*.xnb").Count();
            return GlobalUse.Content.Load<Texture2D>(contentDir + $"/Preset{floor}{roomType}{rng.Next(1, contentDirCount)}");
        }
        private static Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }
        private void FillRoom(List<Texture2D> tileTextures, List<Texture2D> standartObstacleTextures, List<Texture2D> portalTextures, Color[,] presetData, RoomBlueprint emptyRoom)
        {
            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    switch (_presetColorTranslator[ColorToVector4(presetData[x, y])])
                    {
                        case "Tile":
                            BuildTile(tileTextures, x, y);
                            break;
                        case "Obstacle":
                            BuildTile(tileTextures, x, y);
                            BuildObstacle(standartObstacleTextures, x, y);
                            break;
                        case "Portal":
                            BuildTile(tileTextures, x, y);
                            var portalDirection = CheckPortalDirection(_roomPresetImage, x, y);
                            if (portalDirection == Direction.Left && emptyRoom.HasLeftNeighbour == true)
                                BuildPortal(portalTextures, x, y, Direction.Left);
                            else if (portalDirection == Direction.Right && emptyRoom.HasRightNeighbour == true)
                                BuildPortal(portalTextures, x, y, Direction.Right);
                            else if (portalDirection == Direction.Up && emptyRoom.HasUpNeighbour == true)
                                BuildPortal(portalTextures, x, y, Direction.Up);
                            else if (portalDirection == Direction.Down && emptyRoom.HasDownNeighbour == true)
                                BuildPortal(portalTextures, x, y, Direction.Down);
                            break;
                        case "TargetDummy":
                            BuildTile(tileTextures, x, y);
                            var targetDummy = new TargetDummySprite(_tiles[x, y]);
                            ObjectsYSorted.Add(targetDummy);
                            Damageables.Add(targetDummy);
                            RoomColliders.Add(targetDummy.MovementCollider);
                            ObstaclesBounds.Add(targetDummy.BodyHitbox);
                            {
                                var testWasp = new WaspSprite(targetDummy.Position, this);
                                ObjectsYSorted.Add(testWasp);
                                Damageables.Add(testWasp);
                                Updateables.Add(testWasp);
                            }
                            break;
                        case "TargetDummyShooter":
                            BuildTile(tileTextures, x, y);
                            var targetDummyShooter = new TargetDummyShooterSprite(_tiles[x, y], this);
                            ObjectsYSorted.Add(targetDummyShooter);
                            Damageables.Add(targetDummyShooter);
                            RoomColliders.Add(targetDummyShooter.MovementCollider);
                            ObstaclesBounds.Add(targetDummyShooter.BodyHitbox);
                            Updateables.Add(targetDummyShooter);
                            break;
                        case "WeaponStick":
                            BuildTile(tileTextures, x, y);
                            var stickTest = new WeaponSprite(new(x * _tiles[x, y].GetTextureWidth() - (_tiles[x, y].GetTextureWidth() / 2), y * _tiles[x, y].GetTextureHeight() - (_tiles[x, y].GetTextureHeight() / 2)), "Stick");
                            stickTest.WeaponInstance = new WeaponStick(stickTest);
                            ObjectsYSorted.Add(stickTest);
                            Interactives.Add(stickTest);
                            break;
                        case "WeaponSword":
                            BuildTile(tileTextures, x, y);
                            var swordTest = new WeaponSprite(new(x * _tiles[x, y].GetTextureWidth() - (_tiles[x, y].GetTextureWidth() / 2), y * _tiles[x, y].GetTextureHeight() - (_tiles[x, y].GetTextureHeight() / 2)), "Sword");
                            swordTest.WeaponInstance = new WeaponSword(swordTest);
                            ObjectsYSorted.Add(swordTest);
                            Interactives.Add(swordTest);
                            break;
                        case "WeaponLance":
                            BuildTile(tileTextures, x, y);
                            var lanceTest = new WeaponSprite(new(x * _tiles[x, y].GetTextureWidth() - (_tiles[x, y].GetTextureWidth() / 2), y * _tiles[x, y].GetTextureHeight() - (_tiles[x, y].GetTextureHeight() / 2)), "Lance");
                            lanceTest.WeaponInstance = new WeaponLance(lanceTest);
                            ObjectsYSorted.Add(lanceTest);
                            Interactives.Add(lanceTest);
                            break;
                        case "WoodenChest":
                            BuildTile(tileTextures, x, y);
                            var chestTest = new ChestSprite(new(x * _tiles[x, y].GetTextureWidth(), y * _tiles[x, y].GetTextureHeight()), this);
                            break;
                        default:
                            BuildTile(tileTextures, x, y);
                            break;
                    }
                }
            }
        }

        private static Direction CheckPortalDirection(Texture2D roomPresetImage, int x, int y)
        {
            Point center = new((roomPresetImage.Width - 1) / 2, (roomPresetImage.Height - 1) / 2);
            if (x > center.X) { return Direction.Right; }
            else if (x < center.X) { return Direction.Left; }
            else if (y > center.Y) { return Direction.Down; }
            else return Direction.Up;
        }
        public void DeactivatePortals()
        {
            foreach (var portal in Portals)
            {
                portal.DeactivatePortal();
            }
        }
        public async void ActivatePortals(int milliseconds) //REWORK TO INGAME TIME
        {
            await Task.Delay(milliseconds);
            foreach (var portal in Portals)
            {
                portal.ActivatePortal();
            }
        }
        private void UpdatePortals()
        {
            foreach (var portal in Portals)
            {
                portal.Update();
            }
        }
        public void Update(float elapsedMilliseconds)
        {
            UpdatePortals();
            foreach (var item in Updateables)
            {
                item.Update(elapsedMilliseconds);
            }
            Updateables.RemoveAll(item => item.IsExpired);
        }
        private void BuildObstacle(List<Texture2D> standartObstacleTextures, int x, int y)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, standartObstacleTextures.Count);
            _standartObstacles[x, y] = new SpriteStandartObstacle(standartObstacleTextures[i], _tiles[x, y]);
        }

        private void BuildTile(List<Texture2D> tileTextures, int x, int y)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, tileTextures.Count);
            _tiles[x, y] = new SpriteStandartTile(tileTextures[i], new Vector2(x * TileSize.X, y * TileSize.Y));
        }

        private void BuildPortal(List<Texture2D> portalTextures, int x, int y, Direction portalDirection)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, portalTextures.Count);
            Portals.Add(new RoomPortalSprite(portalTextures[i], new Vector2(x * TileSize.X, y * TileSize.Y), portalDirection, this));
        }
        private static Vector4 ColorToVector4(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }
        private static List<Texture2D> ImportTileTextures(Floor floor, RoomType roomType)
        {
            string dirContentPath = $"Content/Floor/{floor}/RoomType/{roomType}/Tile";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> tileTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    tileTextures.Add(GlobalUse.Content.Load<Texture2D>($"Floor/{floor}/RoomType/{roomType}/Tile/{roomType}{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return tileTextures;
        }
        private static List<Texture2D> ImportStandartObstacleTextures(Floor floor, RoomType roomType)
        {
            string dirContentPath = $"Content/Floor/{floor}/RoomType/{roomType}/StandartObstacle";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> standartObstacleTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    standartObstacleTextures.Add(GlobalUse.Content.Load<Texture2D>($"Floor/{floor}/RoomType/{roomType}/StandartObstacle/{roomType}{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return standartObstacleTextures;
        }
        private static List<Texture2D> ImportPortalTextures(Floor floor, RoomType roomType) //Temp
        {
            string dirContentPath = $"Content/RoomGates";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> portalTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    portalTextures.Add(GlobalUse.Content.Load<Texture2D>($"RoomGates/RoomGate{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return portalTextures;
        }
        public void UpdateObstaclesTransparency(float elapsedMilliseconds)
        {
            foreach (var obstacle in _standartObstacles)
            {
                obstacle?.UpdateTransparencyTimer(elapsedMilliseconds);
            }
        }
        public void AddTempDrawableRect(Rectangle rect)
        {
            TempRectDrawList.Add(rect);
        }
        public void ClearTempDrawables()
        {
            if (TempRectDrawList.Any())
                TempRectDrawList.Clear();
        }
    }
}
