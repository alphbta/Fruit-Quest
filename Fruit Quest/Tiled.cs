﻿using System.Collections.Generic;

namespace Fruit_Quest
{
    //Некоторые свойства имеют такие названия, чтобы спарсить json
    public class TiledMap
    {
        public int width {  get; set; }
        public int height { get; set; }
        public int tilewidth { get; set; }
        public int tileheight { get; set; }
        public List<Tileset> tilesets { get; set; }
        public List<Layer> layers { get; set; }

    }

    public class Tileset
    {
        public int firstgid { get; set; }
        public string image { get; set; }
        public int tilewidth { get; set; }
        public int tileheight { get; set; }
        public int columns { get; set; }
        public int rows { get; set; }
    }

    public class Layer
    {
        public string name { get; set; }
        public string type { get; set; }
        
        // тайловые слои
        public int[] data { get; set; }
        public int width { get; set; }
        public int height {  get; set; }
        
        // слои объектов
        public List<TiledObject> objects { get; set; }
    }

    public class TiledObject
    {
        public int gid { get; set; }
        public string name { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
