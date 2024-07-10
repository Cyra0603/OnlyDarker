using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.GameProcess.SpriteClasses.Enemies;
using OnlyDarker.PlayerClasses;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OnlyDarker.GameProcess
{
    public class Room
    {
        public enum RoomExplorationState
        {
            Unexplored,
            CanBeExplored,
            Explored,
        }
        private readonly Point _roomTileSize;
        private readonly Texture2D _roomPresetImage;
        public readonly SpriteStandartTile[,] _tiles;
        public readonly SpriteStandartObstacle[,] _standartObstacles;
        public readonly SpriteStandartTile[] _tilesToDraw;
        private Node[,] _nodesAllocation;
        public List<IYSortable> ObjectsYSorted;
        public List<INonSortable> ObjectsNotSorted;
        public List<IInteractive> Interactives;
        public List<IDamageable> Damageables;
        public List<IMyUpdateable> Updateables;
        public Stack<object> EntitiesToSpawn;
        public List<RoomPortalSprite> Portals { get; private set; } = new();
        public readonly BackgroundSprite CurrentBackground;
        public readonly Level ParentLevelReference;
        public readonly RoomType InstanceRoomType;
        public RoomExplorationState ExplorationState = RoomExplorationState.Unexplored;
        private readonly static Dictionary<Vector4, string> _presetColorTranslator = new()
        {
            {new Vector4(255,0,0,255) , "Obstacle"},
            {new Vector4(0,255,0,255) , "Tile"},
            {new Vector4(0,0,255,255) , "Portal"},
            {new Vector4(60,60,60,255) , "TargetDummy" },
            {new Vector4(60,70,60,255) , "TargetDummyShooter" },
            {new Vector4(60,60,70,255) , "MobSummoner" },
            {new Vector4(150,100,100,255) , "WeaponStick" },
            {new Vector4(100,150,100,255) , "WeaponSword" },
            {new Vector4(100,100,150,255) , "WeaponLance" },
            {new Vector4(200,200,0,255) , "WoodenChest" },
            {new Vector4(70,60,60,255) , "Boss" },
        };
        public List<Rectangle> RoomColliders { get; private set; }
        public List<Rectangle> ObstaclesBounds { get; private set; }
        public List<Rectangle> TempRectDrawList;
        public const int MAX_ENTITIES = 25;
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
            _tiles = new SpriteStandartTile[_roomTileSize.Y, _roomTileSize.X];
            _nodesAllocation = new Node[_roomTileSize.Y, _roomTileSize.X];
            _tilesToDraw = new SpriteStandartTile[_tiles.Length];
            _standartObstacles = new SpriteStandartObstacle[_roomTileSize.Y, _roomTileSize.X];
            ObjectsYSorted = new();
            ObjectsNotSorted = new();
            RoomColliders = new();
            ObstaclesBounds = new();
            Damageables = new();
            Interactives = new();
            Updateables = new();
            TempRectDrawList = new();
            EntitiesToSpawn = new();
            GridCords = new(emptyRoom.X, emptyRoom.Y);
            List<Texture2D> tileTextures = ImportTileTextures(emptyRoom.FloorType, emptyRoom.RoomType);
            List<Texture2D> standartObstacleTextures = ImportStandartObstacleTextures(emptyRoom.FloorType, emptyRoom.RoomType);
            List<Texture2D> portalTextures = ImportPortalTextures(emptyRoom.FloorType, emptyRoom.RoomType);
            TileSize = new(tileTextures[0].Width, tileTextures[0].Height);
            RoomSize = new(TileSize.X * _roomTileSize.X, TileSize.Y * _roomTileSize.Y);
            FillRoom(tileTextures, standartObstacleTextures, portalTextures, presetData, emptyRoom);
            int i = 0;
            foreach (var tile in _tiles)
            {
                _tilesToDraw[i] = tile;
                i++;
            }
            foreach (var obstacle in _standartObstacles)
            {
                if (obstacle is not null)
                {
                    RoomColliders.Add(obstacle.MovementCollider);
                    ObstaclesBounds.Add(obstacle.Bounds);
                    ObjectsYSorted.Add(obstacle);
                }
            }
            CreateUnblockedTileGrid();
            ObjectsYSorted = ObjectsYSorted.OrderBy(obj => obj.Position.Y).ToList();
            ParentLevelReference = parentLevelReference;
            //local methods
            static Rectangle GetDeflatedRect(SpriteStandartTile tile)
            {
                return new Rectangle((int)tile.Position.X - (int)tile.GetTextureWidth() / 2 + 1, (int)tile.Position.Y - (int)tile.GetTextureHeight() / 2 + 1, (int)tile.GetTextureWidth() - 1, (int)tile.GetTextureHeight() - 1);
            }
            void CreateUnblockedTileGrid()
            {
                for (int y = 0; y < _tiles.GetLength(0); y++)
                {
                    for (int x = 0; x < _tiles.GetLength(1); x++)
                    {
                        _nodesAllocation[y, x] = new Node(x, y);
                        var testRect = GetDeflatedRect(_tiles[y, x]);
                        if (!RoomColliders.Any(rect => rect.Intersects(testRect)))
                        {
                            _nodesAllocation[y, x].IsBlocked = false;
                        }
                    }
                }
            }
        }
        public void Update(float elapsedMilliseconds)
        {
            for (int i = 0; i < EntitiesToSpawn.Count; i++)
            {
                SpawnEntity(EntitiesToSpawn.Pop());
            }
            foreach (var item in Updateables)
            {
                item.Update(elapsedMilliseconds);
            }
            UpdatePortals();
            Updateables.RemoveAll(item => item.IsExpired);
            Damageables.RemoveAll(entity => entity.IsExpired);
        }
        public void Draw()
        {
            Span<SpriteStandartTile> tilesAsSpan = _tilesToDraw;
            for (int i = 0; i < tilesAsSpan.Length; i++)
            {
                var tile = tilesAsSpan[i];
                tile?.Draw();
            }
            Span<RoomPortalSprite> portalsAsSpan = CollectionsMarshal.AsSpan(Portals);
            for (int i = 0; i < portalsAsSpan.Length; i++)
            {
                var obj = portalsAsSpan[i];
                obj.Draw();
            }
            Span<IYSortable> ySortedAsSpan = CollectionsMarshal.AsSpan(ObjectsYSorted);
            for (int i = 0; i < ySortedAsSpan.Length; i++)
            {
                var obj = ySortedAsSpan[i];
                obj.Draw();
            }
            Span<INonSortable> nonSortedAsSpan = CollectionsMarshal.AsSpan(ObjectsNotSorted);
            for (int i = 0; i < nonSortedAsSpan.Length; i++)
            {
                var obj = nonSortedAsSpan[i];
                obj.Draw();
            }
        }
        public void SortObjectsByY()
        {
            ObjectsYSorted = ObjectsYSorted.OrderBy(obj => obj.Position.Y).ToList();
        }
        private static Texture2D ImportPreset(Floor floorType, RoomType roomType)
        {
            var rng = new Random();
            string contentDir = $"Floor/{floorType.GetName()}/RoomType/{roomType.GetName()}/Presets";
            int contentDirCount = Directory.EnumerateFiles("Content/" + contentDir, "*.xnb").Count();
            return GlobalUse.Content.Load<Texture2D>(contentDir + $"/Preset{floorType.GetName()}{roomType.GetName()}{rng.Next(1, contentDirCount)}");
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
            for (int y = 0; y < _tiles.GetLength(0); y++)
            {
                for (int x = 0; x < _tiles.GetLength(1); x++)
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
                            var targetDummy = new TargetDummySprite(_tiles[y, x]);
                            ObjectsYSorted.Add(targetDummy);
                            Damageables.Add(targetDummy);
                            //RoomColliders.Add(targetDummy.MovementCollider);
                            ObstaclesBounds.Add(targetDummy.BodyHitbox);
                            break;
                        case "MobSummoner":
                            BuildTile(tileTextures, x, y);
                            var summoner = new MobSummonerSprite(GameBody.GetGameInstance().TextureMapper.TargetDummySpriteTexture, new WaspSprite(_tiles[y, x].Position, this), _tiles[y, x].Position, this, 2F, new Armor(ArmorType.Base), 30F, 15000F, 5);
                            ObjectsYSorted.Add(summoner);
                            Damageables.Add(summoner);
                            Updateables.Add(summoner);
                            RoomColliders.Add(summoner.BodyHitbox);
                            ObstaclesBounds.Add(summoner.BodyHitbox);
                            break;
                        case "TargetDummyShooter":
                            BuildTile(tileTextures, x, y);
                            var targetDummyShooter = new TargetDummyShooterSprite(_tiles[y, x], this);
                            ObjectsYSorted.Add(targetDummyShooter);
                            Damageables.Add(targetDummyShooter);
                            RoomColliders.Add(targetDummyShooter.MovementCollider);
                            ObstaclesBounds.Add(targetDummyShooter.BodyHitbox);
                            Updateables.Add(targetDummyShooter);
                            break;
                        case "WeaponStick":
                            BuildTile(tileTextures, x, y);
                            var stickTest = new WeaponSprite(new(x * _tiles[y, x].GetTextureWidth() - (_tiles[y, x].GetTextureWidth() / 2), y * _tiles[y, x].GetTextureHeight() - (_tiles[y, x].GetTextureHeight() / 2)), "Stick");
                            stickTest.WeaponInstance = new WeaponStick(stickTest);
                            ObjectsYSorted.Add(stickTest);
                            Interactives.Add(stickTest);
                            break;
                        case "WeaponSword":
                            BuildTile(tileTextures, x, y);
                            var swordTest = new WeaponSprite(new(x * _tiles[y, x].GetTextureWidth() - (_tiles[y, x].GetTextureWidth() / 2), y * _tiles[y, x].GetTextureHeight() - (_tiles[y, x].GetTextureHeight() / 2)), "Sword");
                            swordTest.WeaponInstance = new WeaponSword(swordTest);
                            ObjectsYSorted.Add(swordTest);
                            Interactives.Add(swordTest);
                            break;
                        case "WeaponLance":
                            BuildTile(tileTextures, x, y);
                            var lanceTest = new WeaponSprite(new(x * _tiles[y, x].GetTextureWidth() - (_tiles[y, x].GetTextureWidth() / 2), y * _tiles[y, x].GetTextureHeight() - (_tiles[y, x].GetTextureHeight() / 2)), "Lance");
                            lanceTest.WeaponInstance = new WeaponLance(lanceTest);
                            ObjectsYSorted.Add(lanceTest);
                            Interactives.Add(lanceTest);
                            break;
                        case "WoodenChest":
                            BuildTile(tileTextures, x, y);
                            var chestTest = new ChestSprite(new(x * _tiles[y, x].GetTextureWidth(), y * _tiles[y, x].GetTextureHeight()), this);
                            break;
                        case "Boss":
                            BuildTile(tileTextures, x, y);
                            var testBoss = new FloorOneBossSprite(GameBody.GetGameInstance().TextureMapper.TargetDummySpriteTexture, this, _tiles[y, x].Position, 50F, "the druid");
                            ObjectsYSorted.Add(testBoss);
                            Damageables.Add(testBoss);
                            Updateables.Add(testBoss);
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
        private void BuildObstacle(List<Texture2D> standartObstacleTextures, int x, int y)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, standartObstacleTextures.Count);
            _standartObstacles[y, x] = new SpriteStandartObstacle(standartObstacleTextures[i], _tiles[y, x]);
        }

        private void BuildTile(List<Texture2D> tileTextures, int x, int y)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, tileTextures.Count);
            _tiles[y, x] = new SpriteStandartTile(tileTextures[i], new Vector2(x * TileSize.X, y * TileSize.Y), x,y);
        }

        private void BuildPortal(List<Texture2D> portalTextures, int x, int y, Direction portalDirection)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, portalTextures.Count);
            Portals.Add(new RoomPortalSprite(portalTextures[i], new Vector2(x * TileSize.X, y * TileSize.Y), portalDirection, this));
        }
        public void SpawnEntity(object entity)
        {
            if (Damageables.Count >= MAX_ENTITIES)
                return;
            if (TryCast<IDamageable>(entity, out var damageable))
            {
                Damageables.Add(damageable);
            }
            if (TryCast<IMyUpdateable>(entity, out var updateable))
            {
                Updateables.Add(updateable);
            }
            if (TryCast<INonSortable>(entity, out var nonSortable))
            {
                ObjectsNotSorted.Add(nonSortable);
            }
            if (TryCast<IYSortable>(entity, out var ySortable))
            {
                ObjectsYSorted.Add(ySortable);
            }
            bool TryCast<T>(object obj, out T cast)
            {
                if (obj is T value)
                {
                    cast = value;
                    return true;
                }
                cast = default;
                Type t = typeof(T);
                Debug.WriteLine($"Spawn failed cast: object non {t}");
                return false;
            }
        }
        private static Vector4 ColorToVector4(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }
        private static List<Texture2D> ImportTileTextures(Floor floorType, RoomType roomType)
        {
            string dirContentPath = $"Content/Floor/{floorType.GetName()}/RoomType/{roomType.GetName()}/Tile";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> tileTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    tileTextures.Add(GlobalUse.Content.Load<Texture2D>($"Floor/{floorType.GetName()}/RoomType/{roomType.GetName()}/Tile/{roomType.GetName()}{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return tileTextures;
        }
        private static List<Texture2D> ImportStandartObstacleTextures(Floor floorType, RoomType roomType)
        {
            string dirContentPath = $"Content/Floor/{floorType.GetName()}/RoomType/{roomType.GetName()}/StandartObstacle";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> standartObstacleTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    standartObstacleTextures.Add(GlobalUse.Content.Load<Texture2D>($"Floor/{floorType.GetName()}/RoomType/{roomType.GetName()}/StandartObstacle/{roomType.GetName()}{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return standartObstacleTextures;
        }
        private static List<Texture2D> ImportPortalTextures(Floor floorType, RoomType roomType) //Temp
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
        public Vector2 GetPathDestination(Vector2 start, Vector2 finish)
        {
            int sx = (int)start.X / 64;
            int sy = (int)start.Y / 42;
            int fx = (int)finish.X / 64;
            int fy = (int)finish.Y / 42;
            int height = _nodesAllocation.GetLength(0);
            int width = _nodesAllocation.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (_nodesAllocation[y, x].IsBlocked)
                    {
                        _nodesAllocation[y, x].Weight = float.MaxValue;
                        continue;
                    }
                    _nodesAllocation[y, x].Weight = 0;
                    _nodesAllocation[y, x].IsMarked = false;
                    _nodesAllocation[y, x].AStarWeight = Math.Abs(Vector2.Distance(_tiles[y, x].Position, _tiles[fy, fx].Position));
                }
            }
            float leastWeight = float.MaxValue;
            int leastWeightX = sx;
            int leastWeightY = sy;
            if (sx != 0)//Left
            {
                if (!_nodesAllocation[sy, sx - 1].IsBlocked && _nodesAllocation[sy, sx - 1].TotalWeight < leastWeight)
                {
                    leastWeightX = sx - 1;
                    leastWeightY = sy;
                    leastWeight = _nodesAllocation[sy, sx - 1].TotalWeight;
                }
            }
            if (sx != width - 1)//Right
            {
                if (!_nodesAllocation[sy, sx + 1].IsBlocked && _nodesAllocation[sy, sx + 1].TotalWeight < leastWeight)
                {
                    leastWeightX = sx + 1;
                    leastWeightY = sy;
                    leastWeight = _nodesAllocation[sy, sx + 1].TotalWeight;
                }
            }
            if (sy != 0)//Up
            {
                if (!_nodesAllocation[sy - 1, sx].IsBlocked && _nodesAllocation[sy - 1, sx].TotalWeight < leastWeight)
                {
                    leastWeightX = sx;
                    leastWeightY = sy - 1;
                    leastWeight = _nodesAllocation[sy - 1, sx].TotalWeight;
                }
            }
            if (sy != height - 1)//Down
            {
                if (!_nodesAllocation[sy + 1, sx].IsBlocked && _nodesAllocation[sy + 1, sx].TotalWeight < leastWeight)
                {
                    leastWeightX = sx;
                    leastWeightY = sy + 1;
                    leastWeight = _nodesAllocation[sy + 1, sx].TotalWeight;
                }
            }
            if (sx != 0 && sy != 0 && !_nodesAllocation[sy, sx - 1].IsBlocked && !_nodesAllocation[sy - 1, sx].IsBlocked)//NW
            {
                if (!_nodesAllocation[sy - 1, sx - 1].IsBlocked && _nodesAllocation[sy - 1, sx - 1].TotalWeight < leastWeight)
                {
                    leastWeightX = sx - 1;
                    leastWeightY = sy - 1;
                    leastWeight = _nodesAllocation[sy - 1, sx - 1].TotalWeight;
                }
            }
            if (sx != width - 1 && sy != 0 && !_nodesAllocation[sy, sx + 1].IsBlocked && !_nodesAllocation[sy - 1, sx].IsBlocked)//NE
            {
                if (!_nodesAllocation[sy - 1, sx + 1].IsBlocked && _nodesAllocation[sy - 1, sx + 1].TotalWeight < leastWeight)
                {
                    leastWeightX = sx + 1;
                    leastWeightY = sy - 1;
                    leastWeight = _nodesAllocation[sy - 1, sx + 1].TotalWeight;
                }
            }
            if (sx != 0 && sy != height - 1 && !_nodesAllocation[sy, sx - 1].IsBlocked && !_nodesAllocation[sy + 1, sx].IsBlocked)//SW
            {
                if (!_nodesAllocation[sy + 1, sx - 1].IsBlocked && _nodesAllocation[sy + 1, sx - 1].TotalWeight < leastWeight)
                {
                    leastWeightX = sx - 1;
                    leastWeightY = sy + 1;
                    leastWeight = _nodesAllocation[sy + 1, sx - 1].TotalWeight;
                }
            }
            if (sx != width - 1 && sy != height - 1 && !_nodesAllocation[sy + 1, sx].IsBlocked && !_nodesAllocation[sy, sx + 1].IsBlocked)//SE
            {
                if (!_nodesAllocation[sy + 1, sx + 1].IsBlocked && _nodesAllocation[sy + 1, sx + 1].TotalWeight < leastWeight)
                {
                    leastWeightX = sx + 1;
                    leastWeightY = sy + 1;
                    leastWeight = _nodesAllocation[sy + 1, sx + 1].TotalWeight;
                }
            }
            return _tiles[leastWeightY, leastWeightX].Position;
        }
    }

    internal class Node
    {
        public bool IsMarked = false;
        public bool IsBlocked = true;
        public readonly int X;
        public readonly int Y;
        public float Weight;
        public float AStarWeight;
        public float TotalWeight => Weight + AStarWeight;
        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
