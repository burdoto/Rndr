﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenGL_Util.Matrix
{
    public abstract class GridMatrix
    {
        public IGameObject? this[int x, int y]
        {
            get => this[x, y, 0];
            set => this[x, y, 0] = value;
        }

        public IGameObject? this[Vector2 vec]
        {
            get => this[new Vector3(vec, 0)];
            set => this[new Vector3(vec, 0)] = value;
        }

        public IGameObject? this[int x, int y, int z]
        {
            get => this[new Vector3(x, y, z)];
            set => this[new Vector3(x, y, z)] = value;
        }

        public abstract IGameObject? this[Vector3 vec] { get; set; }

        public abstract IEnumerable<IRenderObject?> GetVisibles(ITransform? camera);

        public abstract void Clear();

        public abstract IEnumerable<IGameObject> GetGameObjects();
    }

    public class ShortLongMatrix : GridMatrix
    {
        private readonly IDictionary<long, IGameObject?> _objs = new ConcurrentDictionary<long, IGameObject?>();

        public override IGameObject? this[Vector3 vec]
        {
            get => _objs[vec.CombineIntoLong(0)];
            set => _objs[vec.CombineIntoLong(value?.Metadata ?? 0)] = value!;
        }

        public override IEnumerable<IRenderObject?> GetVisibles(ITransform? camera)
        {
            return _objs.Values.SelectMany(it => it?.RenderObjects).Where(it => it != null);
        }

        public override void Clear()
        {
            _objs.Clear();
        }

        public override IEnumerable<IGameObject> GetGameObjects()
        {
            return _objs.Values
                .Where(it => it != null)
                .Select(it => it!);
        }
    }

    public class ListMatrix : GridMatrix
    {
        private readonly List<IGameObject> _objs = new List<IGameObject>();

        public override IGameObject? this[Vector3 vec]
        {
            get => _objs.First(it => it.Position == vec);
            set
            {
                int index = _objs.FindIndex(it => it.Position == vec);
                if (index != -1)
                    _objs[index] = value!;
                else _objs.Add(value!);
            }
        }

        public override IEnumerable<IRenderObject?> GetVisibles(ITransform? camera)
        {
            return _objs.SelectMany(it => it.RenderObjects);
        }

        public override void Clear()
        {
            _objs.Clear();
        }

        public override IEnumerable<IGameObject> GetGameObjects()
        {
            return _objs
                .Where(it => it != null)
                .Select(it => it!);
        }
    }

    public class MapMatrix : GridMatrix
    {
        private readonly
            ConcurrentDictionary<int, ConcurrentDictionary<int, ConcurrentDictionary<int, IGameObject?>>> _matrix =
                new ConcurrentDictionary<int, ConcurrentDictionary<int, ConcurrentDictionary<int, IGameObject?>>>();

        public override IGameObject? this[Vector3 vec]
        {
            get
            {
                _matrix.GetOrAdd((int)vec.X,
                        key => new ConcurrentDictionary<int, ConcurrentDictionary<int, IGameObject?>>())
                    .GetOrAdd((int)vec.Y, key => new ConcurrentDictionary<int, IGameObject?>())
                    .TryGetValue((int)vec.Z, out var draw);
                return draw;
            }
            set => _matrix.GetOrAdd((int)vec.X,
                    key => new ConcurrentDictionary<int, ConcurrentDictionary<int, IGameObject?>>())
                .GetOrAdd((int)vec.Y, key => new ConcurrentDictionary<int, IGameObject?>())
                .TryAdd((int)vec.Z, value);
        }

        public override IEnumerable<IRenderObject?> GetVisibles(ITransform? camera)
        {
            return _matrix.Values
                .SelectMany(x => x.Values)
                .SelectMany(y => y.Values)
                .SelectMany(it => it?.RenderObjects);
        }

        public override void Clear()
        {
            foreach (var xStage in _matrix.Values)
            {
                foreach (var yStage in xStage.Values)
                    yStage.Clear();
                xStage.Clear();
            }

            _matrix.Clear();
        }

        public override IEnumerable<IGameObject> GetGameObjects()
        {
            return _matrix.Values
                .SelectMany(x => x.Values)
                .SelectMany(y => y.Values)
                .Where(it => it != null)
                .Select(it => it!);
        }
    }
}