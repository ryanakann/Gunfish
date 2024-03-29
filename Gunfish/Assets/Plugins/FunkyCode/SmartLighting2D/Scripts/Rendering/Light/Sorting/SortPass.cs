﻿using FunkyCode.LightSettings;
using FunkyCode.LightTilemapCollider;
using UnityEngine;

namespace FunkyCode.Rendering.Light.Sorting {
    public class SortPass {
        public SortList sortList = new SortList();
        public SortObject sortObject;
        public Pass pass;

        public void Clear() {
            sortList.Reset();
        }

        public void SortObjects() {
            if (pass == null) {
                return;
            }

            sortList.Reset();

            AddColliders();

            AddTilemaps();

            sortList.Sort();
        }

        void AddColliders() {
            for (int id = 0; id < pass.colliderList.Count; id++) {
                var collider = pass.colliderList[id];
                if (collider.shadowLayer != pass.layerID && collider.maskLayer != pass.layerID) {
                    continue;
                }

                if (!collider.InLight(pass.light)) {
                    continue;
                }

                switch (pass.layer.sorting) {
                    case LightLayerSorting.Isometric:
                        Vector2 lightPos = pass.light.transform.position;
                        Vector2 colliderPos = collider.mainShape.GetPivotPoint();
                        Vector2 position = colliderPos - lightPos;

                        float x = position.y + position.x / 2;
                        float y = -(position.x / 2 - position.y);

                        if (x > 0) {
                            sortList.Add(collider, x);
                        }
                        else {
                            sortList.Add(collider, -x + 10000);
                        }

                        break;

                    case LightLayerSorting.SortingLayerAndOrder:
                        sortList.Add(collider, collider.mainShape.GetSortingOrder() + collider.mainShape.GetSortingLayer() * 1000);
                        break;

                    case LightLayerSorting.ZAxisLower:

                        if (pass.layer.sortingIgnore == LightLayerSortingIgnore.IgnoreAbove) {
                            if (collider.transform.position.z >= pass.light.transform.position.z) {
                                sortList.Add(collider, -collider.transform.position.z);
                            }
                        }
                        else {
                            sortList.Add(collider, -collider.transform.position.z);
                        }

                        break;

                    case LightLayerSorting.ZAxisHigher:

                        if (pass.layer.sortingIgnore == LightLayerSortingIgnore.IgnoreAbove) {
                            if (collider.transform.position.z <= pass.light.transform.position.z) {
                                sortList.Add(collider, collider.transform.position.z);
                            }
                        }
                        else {
                            sortList.Add(collider, collider.transform.position.z);
                        }

                        break;

                    case LightLayerSorting.YAxisLower:

                        sortList.Add(collider, -collider.transform.position.y);

                        break;

                    case LightLayerSorting.YAxisHigher:

                        sortList.Add(collider, collider.transform.position.y);

                        break;

                    case LightLayerSorting.DistanceToLight:

                        sortList.Add(collider, -Vector2.Distance(collider.transform.position, pass.light.transform.position));

                        break;

                    case LightLayerSorting.YDistanceToLight:

                        sortList.Add(collider, -Mathf.Abs(collider.transform.position.y - pass.light.transform.position.y));

                        break;
                }
            }
        }

        void AddTilemaps() {
            for (int id = 0; id < pass.tilemapList.Count; id++) {
                var tilemap = pass.tilemapList[id];
                if (tilemap.shadowLayer != pass.layerID && tilemap.maskLayer != pass.layerID) {
                    continue;
                }

                //if (tilemap.IsNotInRange(pass.light)) {
                //   continue;
                //}

                bool shadowsDisabled = tilemap.ShadowsDisabled();
                bool masksDisabled = tilemap.MasksDisabled();

                if (shadowsDisabled && masksDisabled) {
                    continue;
                }

                //AddTiles(tilemap);
                AddTileMap(tilemap);
            }
        }

        public void AddTileMap(LightTilemapCollider2D id) {
            switch (id.mapType) {
                case MapType.UnityRectangle:

                    switch (pass.layer.sorting) {
                        case LightLayerSorting.SortingLayerAndOrder:
                            sortList.Add(id, id.lightingTransform.sortingOrder + id.lightingTransform.sortingLayerID * 1000);
                            break;

                        case LightLayerSorting.ZAxisLower:
                            sortList.Add(id, -id.transform.position.z);
                            break;

                        case LightLayerSorting.ZAxisHigher:
                            sortList.Add(id, id.transform.position.z);
                            break;

                        case LightLayerSorting.YAxisLower:
                            sortList.Add(id, -id.transform.position.y);
                            break;

                        case LightLayerSorting.YAxisHigher:
                            sortList.Add(id, id.transform.position.y);
                            break;

                        case LightLayerSorting.DistanceToLight:
                            sortList.Add(id, -Vector2.Distance(id.transform.position, pass.light.transform.position));
                            break;

                        case LightLayerSorting.YDistanceToLight:
                            //     sortList.Add(id, tile,  -Mathf.Abs(tilePosition.y - pass.light.transform.position.y));
                            break;
                    }

                    break;
            }

        }

        public void AddTiles(LightTilemapCollider2D id) {
            Vector2 lightPosition = -pass.light.transform2D.position;

            var tilemapBase = id.GetCurrentTilemap();

            foreach (var tile in id.GetTileList()) {
                if (tile.GetSprite() == null) {
                    return;
                }

                Vector2 tilePosition = tile.GetWorldPosition(tilemapBase);
                if (tile.NotInRange(tilePosition + lightPosition, pass.light.size)) {
                    continue;
                }

                switch (pass.layer.sorting) {
                    case LightLayerSorting.SortingLayerAndOrder:
                        sortList.Add(id, tile, id.lightingTransform.sortingOrder + id.lightingTransform.sortingLayerID * 1000);
                        break;

                    case LightLayerSorting.ZAxisLower:
                        sortList.Add(id, tile, -id.transform.position.z);
                        break;

                    case LightLayerSorting.ZAxisHigher:
                        sortList.Add(id, tile, id.transform.position.z);
                        break;

                    case LightLayerSorting.YAxisLower:
                        sortList.Add(id, tile, -tilePosition.y);
                        break;

                    case LightLayerSorting.YAxisHigher:
                        sortList.Add(id, tile, tilePosition.y);
                        break;

                    case LightLayerSorting.DistanceToLight:
                        sortList.Add(id, tile, -Vector2.Distance(tilePosition, pass.light.transform.position));
                        break;

                    case LightLayerSorting.YDistanceToLight:
                        sortList.Add(id, tile, -Mathf.Abs(tilePosition.y - pass.light.transform.position.y));
                        break;

                }
            }
        }
    }
}