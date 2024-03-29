﻿using FunkyCode.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode.Rendering.Light.Shadow {
    public class Tile {
        static public void Draw(Light2D light, LightTile tile, LightTilemapCollider2D tilemap) {
            LightTilemapCollider.Base tilemapCollider = tilemap.GetCurrentTilemap();

            List<Polygon2> polygons = tile.GetWorldPolygons(tilemapCollider);

            ShadowEngine.Draw(polygons, 0, 0, 0);
        }
    }
}