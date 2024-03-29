﻿using System;
using UnityEngine;

namespace FunkyCode.Rendering.Lightmap.Sorting {
    public class SortList {
        public int Count { private set; get; }
        public SortObject[] List = new SortObject[1024];

        public SortList() {
            Count = 0;

            for (int i = 0; i < List.Length; i++)
                List[i] = new SortObject();
        }

        public void Add(object lightObject, float dist) {
            if (Count + 1 < List.Length) {
                List[Count] = new SortObject(dist, lightObject);
                Count++;
            }
            else {
                Debug.LogError("Collider Depth Overhead!");
            }
        }

        public void Reset() {
            Count = 0;
        }

        public void Sort() {
            Array.Sort<SortObject>(List, 0, Count, SortObject.Sort());
        }
    }
}