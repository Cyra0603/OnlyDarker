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
using System.Security.Cryptography;
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
        public const int MAX_ENTITIES = 250;
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
            FillRoom(tileTextures, standartObstacleTextures, presetData, emptyRoom);
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
            CreateNodeGrid();
            ObjectsYSorted = ObjectsYSorted.OrderBy(obj => obj.Position.Y).ToList();
            ParentLevelReference = parentLevelReference;

            //local functions
            static Rectangle GetDeflatedRect(SpriteStandartTile tile)
            {
                return new Rectangle((int)tile.Position.X - (int)tile.GetTextureWidth() / 2 + 1, (int)tile.Position.Y - (int)tile.GetTextureHeight() / 2 + 1, (int)tile.GetTextureWidth() - 1, (int)tile.GetTextureHeight() - 1);
            }
            void CreateNodeGrid()
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
            Interactives.RemoveAll(interactive => !interactive.IsInteractive);
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
        private void FillRoom(List<Texture2D> tileList, List<Texture2D> standartObstacleTextures, Color[,] presetData, RoomBlueprint emptyRoom)
        {
            var rng = new Random();

            for (int y = 0; y < _tiles.GetLength(0); y++)
            {
                for (int x = 0; x < _tiles.GetLength(1); x++)
                {
                    var randT = rng.Next(0, tileList.Count);
                    switch (_presetColorTranslator[ColorToVector4(in presetData[x, y])])
                    {
                        case "Tile":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            break;
                        case "Obstacle":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            BuildObstacle(standartObstacleTextures, x, y);
                            break;
                        case "Portal":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var portalDirection = CheckPortalDirection(_roomPresetImage, x, y);
                            if (portalDirection == Direction.Left && emptyRoom.HasLeftNeighbour == true)
                                BuildPortal(x, y, Direction.Left);
                            else if (portalDirection == Direction.Right && emptyRoom.HasRightNeighbour == true)
                                BuildPortal(x, y, Direction.Right);
                            else if (portalDirection == Direction.Up && emptyRoom.HasUpNeighbour == true)
                                BuildPortal(x, y, Direction.Up);
                            else if (portalDirection == Direction.Down && emptyRoom.HasDownNeighbour == true)
                                BuildPortal(x, y, Direction.Down);
                            break;
                        case "TargetDummy":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var targetDummy = new TargetDummySprite(_tiles[y, x]);
                            ObjectsYSorted.Add(targetDummy);
                            Damageables.Add(targetDummy);
                            //RoomColliders.Add(targetDummy.MovementCollider);
                            ObstaclesBounds.Add(targetDummy.BodyHitbox);
                            break;
                        case "MobSummoner":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var summoner = new MobSummonerSprite(GameBody.GetGameInstance().TextureMapper.TargetDummySpriteTexture, new WaspSprite(_tiles[y, x].Position, this, true), _tiles[y, x].Position, this, 2F, new BaseArmor(), 30F, 15000F, 5);
                            ObjectsYSorted.Add(summoner);
                            Damageables.Add(summoner);
                            Updateables.Add(summoner);
                            RoomColliders.Add(summoner.BodyHitbox);
                            ObstaclesBounds.Add(summoner.BodyHitbox);
                            break;
                        case "TargetDummyShooter":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var targetDummyShooter = new TargetDummyShooterSprite(_tiles[y, x], this);
                            ObjectsYSorted.Add(targetDummyShooter);
                            Damageables.Add(targetDummyShooter);
                            Updateables.Add(targetDummyShooter);
                            break;
                        case "WeaponStick":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var stickTest = PremadeWeaponSprites.GetInstance().GetNewSprite("Stick", new(x * _tiles[y, x].GetTextureWidth() - (_tiles[y, x].GetTextureWidth() / 2), y * _tiles[y, x].GetTextureHeight() - (_tiles[y, x].GetTextureHeight() / 2)));
                            ObjectsYSorted.Add(stickTest);
                            Interactives.Add(stickTest);
                            break;
                        case "WeaponSword":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var swordTest = PremadeWeaponSprites.GetInstance().GetNewSprite("Sword", new(x * _tiles[y, x].GetTextureWidth() - (_tiles[y, x].GetTextureWidth() / 2), y * _tiles[y, x].GetTextureHeight() - (_tiles[y, x].GetTextureHeight() / 2)));
                            ObjectsYSorted.Add(swordTest);
                            Interactives.Add(swordTest);
                            break;
                        case "WeaponLance":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var lanceTest = PremadeWeaponSprites.GetInstance().GetNewSprite("Lance", new(x * _tiles[y, x].GetTextureWidth() - (_tiles[y, x].GetTextureWidth() / 2), y * _tiles[y, x].GetTextureHeight() - (_tiles[y, x].GetTextureHeight() / 2)));
                            ObjectsYSorted.Add(lanceTest);
                            Interactives.Add(lanceTest);
                            break;
                        case "WoodenChest":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var chestTest = new ChestSprite(new(x * _tiles[y, x].GetTextureWidth(), y * _tiles[y, x].GetTextureHeight()), this);
                            break;
                        case "Boss":
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            var testBoss = new FloorOneBossSprite(GameBody.GetGameInstance().TextureMapper.TargetDummySpriteTexture, this, _tiles[y, x].Position, 50F, "the druid");
                            ObjectsYSorted.Add(testBoss);
                            Damageables.Add(testBoss);
                            Updateables.Add(testBoss);
                            break;
                        default:
                            BuildTile(GetNewSpriteSheet(tileList[randT]), x, y);
                            break;
                    }
                }
            }
            //temp
            static SpriteSheet GetNewSpriteSheet(Texture2D texture)
            {
                return new SpriteSheet(texture, texture.Width, texture.Height, 1);
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
        public void BlockPortals()
        {
            foreach (var portal in Portals)
            {
                portal.BlockPortal();
            }
        }
        private void UpdatePortals()
        {
            foreach (var portal in Portals)
            {
                portal.Update();
            }
        }
        private void BuildObstacle(List<Texture2D> standartObstacleTextures, in int x, in int y)
        {
            int i = GlobalUse.SeededStandartRNG.Next(0, standartObstacleTextures.Count);
            _standartObstacles[y, x] = new SpriteStandartObstacle(standartObstacleTextures[i], _tiles[y, x]);
        }

        private void BuildTile(SpriteSheet spriteSheet, in int x, in int y)
        {
            _tiles[y, x] = new SpriteStandartTile(spriteSheet, new Vector2(x * TileSize.X, y * TileSize.Y), x, y);
        }

        private void BuildPortal(int x, int y, Direction portalDirection)
        {
            Portals.Add(new RoomPortalSprite(new Vector2(x * TileSize.X, y * TileSize.Y), portalDirection, this));
        }
        private void SpawnEntity(object entity)
        {
            if (Damageables.Count >= MAX_ENTITIES)
                return;
            if (TryCast<IDamageable>(entity, out var damageable) && damageable is not null)
            {
                Damageables.Add(damageable);
            }
            if (TryCast<IMyUpdateable>(entity, out var updateable) && updateable is not null)
            {
                Updateables.Add(updateable);
            }
            if (TryCast<INonSortable>(entity, out var nonSortable) && nonSortable is not null)
            {
                ObjectsNotSorted.Add(nonSortable);
            }
            if (TryCast<IYSortable>(entity, out var ySortable) && ySortable is not null)
            {
                ObjectsYSorted.Add(ySortable);
            }
            if (TryCast<IInteractive>(entity, out var interactive) && interactive is not null)
            {
                Interactives.Add(interactive);
                interactive.IsInteractive = true;
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
        private static Vector4 ColorToVector4(in Color color)
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
            if (TempRectDrawList.Count > 0)
                TempRectDrawList.Clear();
        }
        public bool LineCastIsCollidingObstacles(in Vector2 start, in Vector2 finish)
        {
            var halfTile = 1;
            Vector2 difference = finish - start;
            int iterations;
            if ((int)difference.Length() > 1)
            {
                iterations = (int)difference.Length() / halfTile;
            }
            else
            {
                iterations = 2;
            }
            Vector2 direction = difference / iterations;
            Vector2 currentPosition = start;
            for (int i = 0; i < iterations; i++)
            {
                var tile = GetTileByPosition(currentPosition);
                if (_standartObstacles[tile.GridY, tile.GridX] is not null && _standartObstacles[tile.GridY, tile.GridX].MovementCollider.Contains(currentPosition))
                    return true;
                currentPosition += direction;
            }
            return false;
        }
        public Vector2 GetPathDestination(in Vector2 start, in Vector2 finish)
        {
            var startTile = GetTileByPosition(in start);
            var finishTile = GetTileByPosition(in finish);
            int sx = startTile.GridX;
            int sy = startTile.GridY;
            int fx = finishTile.GridX;
            int fy = finishTile.GridY;
            int height = _nodesAllocation.GetLength(0);
            int width = _nodesAllocation.GetLength(1);
            Span<Point> safeNodes = stackalloc Point[height * width];
            int safeNodesI = 0;
            safeNodes[0] = new Point(fx, fy);
            foreach (var node in _nodesAllocation)
            {
                node.IsMarked = false;
                node.Weight = float.MaxValue;
                node.AStarWeight = float.MaxValue;
            }
            _nodesAllocation[fy, fx].Weight = 0;
            bool done = false;
            while (!done)
            {
                int lx = fx;
                int ly = fy;
                int blocks = 0;
                float leastWeight = float.MaxValue;
                if (fx != 0 && !_nodesAllocation[fy, fx - 1].IsBlocked && !_nodesAllocation[fy, fx - 1].IsMarked) //Left
                {

                    if (_nodesAllocation[fy, fx - 1].Weight == float.MaxValue)
                    {
                        _nodesAllocation[fy, fx - 1].Weight = _nodesAllocation[fy, fx].Weight + 1;
                        _nodesAllocation[fy, fx - 1].AStarWeight = Vector2.Distance(_tiles[fy, fx - 1].Position, _tiles[sy, sx].Position);
                    }
                    if (_nodesAllocation[fy, fx - 1].AStarWeight < leastWeight)
                    {
                        leastWeight = _nodesAllocation[fy, fx - 1].AStarWeight;
                        ly = fy;
                        lx = fx - 1;
                    }
                }
                else
                    blocks++;
                if (fx != width - 1 && !_nodesAllocation[fy, fx + 1].IsBlocked && !_nodesAllocation[fy, fx + 1].IsMarked) //Right
                {

                    if (_nodesAllocation[fy, fx + 1].Weight == float.MaxValue)
                    {
                        _nodesAllocation[fy, fx + 1].Weight = _nodesAllocation[fy, fx].Weight + 1;
                        _nodesAllocation[fy, fx + 1].AStarWeight = Vector2.Distance(_tiles[fy, fx + 1].Position, _tiles[sy, sx].Position);
                    }
                    if (_nodesAllocation[fy, fx + 1].AStarWeight < leastWeight)
                    {
                        leastWeight = _nodesAllocation[fy, fx + 1].AStarWeight;
                        ly = fy;
                        lx = fx + 1;
                    }
                }
                else
                    blocks++;
                if (fy != 0 && !_nodesAllocation[fy - 1, fx].IsBlocked && !_nodesAllocation[fy - 1, fx].IsMarked) //Up
                {

                    if (_nodesAllocation[fy - 1, fx].Weight == float.MaxValue)
                    {
                        _nodesAllocation[fy - 1, fx].Weight = _nodesAllocation[fy, fx].Weight + 1;
                        _nodesAllocation[fy - 1, fx].AStarWeight = Vector2.Distance(_tiles[fy - 1, fx].Position, _tiles[sy, sx].Position);
                    }
                    if (_nodesAllocation[fy - 1, fx].AStarWeight < leastWeight)
                    {
                        leastWeight = _nodesAllocation[fy - 1, fx].AStarWeight;
                        ly = fy - 1;
                        lx = fx;
                    }
                }
                else
                    blocks++;
                if (fy != height - 1 && !_nodesAllocation[fy + 1, fx].IsBlocked && !_nodesAllocation[fy + 1, fx].IsMarked) //Down
                {

                    if (_nodesAllocation[fy + 1, fx].Weight == float.MaxValue)
                    {
                        _nodesAllocation[fy + 1, fx].Weight = _nodesAllocation[fy, fx].Weight + 1;
                        _nodesAllocation[fy + 1, fx].AStarWeight = Vector2.Distance(_tiles[fy + 1, fx].Position, _tiles[sy, sx].Position);
                    }
                    if (_nodesAllocation[fy + 1, fx].AStarWeight < leastWeight)
                    {
                        leastWeight = _nodesAllocation[fy + 1, fx].AStarWeight;
                        ly = fy + 1;
                        lx = fx;
                    }
                }
                else
                    blocks++;
                _nodesAllocation[fy, fx].IsMarked = true;
                if (fx == lx && fy == ly)
                {
                    fx = safeNodes[safeNodesI].X;
                    fy = safeNodes[safeNodesI].Y;
                    safeNodesI--;
                    if (safeNodesI < 0)
                    {
                        return Vector2.Zero;
                    }
                }
                else
                {
                    fx = lx;
                    fy = ly;
                    if (blocks < 3)
                    {
                        safeNodes[safeNodesI + 1] = new Point(fx, fy);
                        safeNodesI++;
                    }
                }
                if (fx == sx && fy == sy)
                {
                    done = true;
                }
            }
            int x = sx;
            int y = sy;
            float finalWeight = float.MaxValue;
            if (sx != 0 && !_nodesAllocation[sy, sx - 1].IsBlocked && _nodesAllocation[sy, sx - 1].Weight <= finalWeight)//left
            {
                finalWeight = _nodesAllocation[sy, sx - 1].Weight;
                y = sy;
                x = sx - 1;
            }
            if (sx != width - 1 && !_nodesAllocation[sy, sx + 1].IsBlocked && _nodesAllocation[sy, sx + 1].Weight <= finalWeight)//right
            {
                finalWeight = _nodesAllocation[sy, sx + 1].Weight;
                y = sy;
                x = sx + 1;
            }
            if (sy != 0 && !_nodesAllocation[sy - 1, sx].IsBlocked && _nodesAllocation[sy - 1, sx].Weight <= finalWeight)//up
            {
                finalWeight = _nodesAllocation[sy - 1, sx].Weight;
                y = sy - 1;
                x = sx;
            }
            if (sy != height - 1 && !_nodesAllocation[sy + 1, sx].IsBlocked && _nodesAllocation[sy + 1, sx].Weight <= finalWeight)//down
            {
                finalWeight = _nodesAllocation[sy + 1, sx].Weight;
                y = sy + 1;
                x = sx;
            }
            if (sx != 0 && sy != 0 && !_nodesAllocation[sy - 1, sx - 1].IsBlocked && !_nodesAllocation[sy, sx - 1].IsBlocked && !_nodesAllocation[sy - 1, sx].IsBlocked && _nodesAllocation[sy - 1, sx - 1].Weight <= finalWeight)//NW
            {
                finalWeight = _nodesAllocation[sy - 1, sx - 1].Weight;
                y = sy - 1;
                x = sx - 1;
            }
            if (sx != width - 1 && sy != 0 && !_nodesAllocation[sy - 1, sx + 1].IsBlocked && !_nodesAllocation[sy, sx + 1].IsBlocked && !_nodesAllocation[sy - 1, sx].IsBlocked && _nodesAllocation[sy - 1, sx + 1].Weight <= finalWeight)//NE
            {
                finalWeight = _nodesAllocation[sy - 1, sx + 1].Weight;
                y = sy - 1;
                x = sx + 1;
            }
            if (sx != 0 && sy != height - 1 && !_nodesAllocation[sy + 1, sx - 1].IsBlocked && !_nodesAllocation[sy, sx - 1].IsBlocked && !_nodesAllocation[sy + 1, sx].IsBlocked && _nodesAllocation[sy + 1, sx - 1].Weight <= finalWeight)//SW
            {
                finalWeight = _nodesAllocation[sy + 1, sx - 1].Weight;
                y = sy + 1;
                x = sx - 1;
            }
            if (sx != width - 1 && sy != height - 1 && !_nodesAllocation[sy + 1, sx + 1].IsBlocked && !_nodesAllocation[sy, sx + 1].IsBlocked && !_nodesAllocation[sy + 1, sx].IsBlocked && _nodesAllocation[sy + 1, sx + 1].Weight <= finalWeight)//SE
            {
                finalWeight = _nodesAllocation[sy + 1, sx + 1].Weight;
                y = sy + 1;
                x = sx + 1;
            }
            return _tiles[y, x].Position;
        }

        private SpriteStandartTile GetTileByPosition(in Vector2 position)
        {
            int lx = (int)position.X / 64;
            int ly = (int)position.Y / 42;
            int maxHeight = _tiles.GetLength(0) - 1;
            int maxWidth = _tiles.GetLength(1) - 1;
            var estimate = _tiles[ly, lx];
            if (estimate.Bounds.Contains(position))
                return estimate;
            if (lx != 0 && _tiles[ly, lx - 1].Bounds.Contains(position))//left
                return _tiles[ly, lx - 1];
            if (lx != maxWidth && _tiles[ly, lx + 1].Bounds.Contains(position))//right
                return _tiles[ly, lx + 1];
            if (ly != 0 && _tiles[ly - 1, lx].Bounds.Contains(position))//up
                return _tiles[ly - 1, lx];
            if (ly != maxHeight && _tiles[ly + 1, lx].Bounds.Contains(position))//down
                return _tiles[ly + 1, lx];
            if (lx != 0 && ly != 0 && _tiles[ly - 1, lx - 1].Bounds.Contains(position))//NW
                return _tiles[ly - 1, lx - 1];
            if (lx != maxWidth && ly != 0 && _tiles[ly - 1, lx + 1].Bounds.Contains(position))//NE
                return _tiles[ly - 1, lx + 1];
            if (lx != 0 && ly != maxHeight && _tiles[ly + 1, lx - 1].Bounds.Contains(position))//SW
                return _tiles[ly + 1, lx - 1];
            if (lx != maxWidth && ly != maxHeight && _tiles[ly + 1, lx + 1].Bounds.Contains(position))//SE
                return _tiles[ly + 1, lx + 1];
            Debug.WriteLine("Pathfinder couldn't find the tile {GetTileByPosition}");
            return estimate;
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
